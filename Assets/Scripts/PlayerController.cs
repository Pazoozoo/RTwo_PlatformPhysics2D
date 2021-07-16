using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    public Text jumpText;
    [SerializeField, Range(0f, 50f)] float maxSpeed = 12f;
    [SerializeField, Range(0f, 800f)] float maxAcceleration = 400f;
    [SerializeField, Range(0f, 800f)] float maxAirAcceleration = 400f;
    [SerializeField, Range(0f, -50f)] float gravity = -8f;
    [SerializeField, Range(0f, 1f)] float wallSlideMultiplier = 0.2f;
    [SerializeField, Range(0f, 0.3f)] float startWallSlideGraceTime = 0.08f;
    [SerializeField, Range(0f, 0.3f)] float stopWallSlideGraceTime = 0.04f;
    [SerializeField, Range(0f, 0.3f)] float wallJumpFlyTime = 0.5f;
    [SerializeField, Range(0f, 100f)] float jumpForce = 25f;
    [SerializeField, Range(0f, 200f)] float jumpDecay = 50f;
    [SerializeField] LayerMask groundLayers;
    Vector3 _velocity;
    Vector3 _movement;
    int _facingDirection = 1;
    Vector3 _jumpVelocity;
    float _wallSlideStartTime;
    float _wallSlideStopTime;
    const float VerticalRays = 3f;
    const float HorizontalRays = 5f;
    const float RaycastOffset = 0.05f;
    bool _isGrounded;
    bool _hasJumped;
    bool _hasWallJumped;
    float _wallJumpTime;
    bool _onWall;
    Collider2D _col;
    Bounds _bounds;
    SpriteRenderer _spriteRenderer;

    bool WallSlideStartGraceTime => Time.time < _wallSlideStartTime + startWallSlideGraceTime;
    bool WallSlideStopGraceTime => Time.time < _wallSlideStopTime + stopWallSlideGraceTime;
    bool WallJumpedRecently => Time.time < _wallJumpTime + wallJumpFlyTime;
    //TODO replace bools with state enum

    void Awake() {
        _col = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _wallJumpTime -= wallJumpFlyTime;
    }

    void Update() {
        float desiredVelocity = Input.GetAxisRaw("Horizontal") * maxSpeed;
        float maxSpeedChange = _isGrounded || _onWall ? maxAcceleration : maxAirAcceleration;
        maxSpeedChange *= Time.deltaTime;

        _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity, maxSpeedChange);
        _velocity.y = _isGrounded ? 0f : gravity;

        if (_velocity.x > 0)
            _facingDirection = 1;
        else if (_velocity.x < 0)
            _facingDirection = -1;
        
        switch (_onWall) {
            case true: {
                _jumpVelocity.y = _hasWallJumped ? _jumpVelocity.y : 0f;

                if (WallSlideStartGraceTime) {
                    _velocity.y = 0f;
                }
                else
                    _velocity.y *= wallSlideMultiplier;
                break;
            }
            case false when WallSlideStopGraceTime:
                _velocity.y = 0f;
                break;
        }
        
        if (Input.GetButton("Jump")) {
            if (!_hasJumped && _isGrounded) {
                _jumpVelocity.y = jumpForce;
                _jumpVelocity.x = 0f;
                _hasJumped = true;
            } else if (!_hasWallJumped && _onWall) {
                _facingDirection *= -1;
                _velocity.x = 0f;
                _jumpVelocity.y = jumpForce;
                _jumpVelocity.x = jumpForce * _facingDirection * 0.2f;
                _onWall = false;
                _hasWallJumped = true;
                _wallJumpTime = Time.time;
            }
        }

        if (_hasJumped || _hasWallJumped) {
            _velocity += _jumpVelocity;
            
            if (_jumpVelocity.y > 0f)
                _jumpVelocity.y -= jumpDecay * Time.deltaTime;
            
            if (_velocity.y < gravity) {
                _velocity.y = gravity;
                _jumpVelocity.y = 0f;
            }
            
            if (_jumpVelocity.x > 0f)
                _jumpVelocity.x -= jumpDecay * Time.deltaTime;
            else if (_jumpVelocity.x < 0f)
                _jumpVelocity.x += jumpDecay * Time.deltaTime;
            //TODO stop x at 0, use facing direction?
        }
        
        _movement = _velocity * Time.deltaTime;
        bool moving = _movement != Vector3.zero;

        if (!moving) return;
        
        _bounds = _col.bounds;
        bool movingHorizontally = _movement.x != 0;
        bool movingDown = _movement.y < 0;
        bool movingUp = _movement.y > 0;
        bool movingOnGround = _movement.y == 0 && movingHorizontally;

        if (movingHorizontally || _onWall || _hasWallJumped) 
            WallCheck();
        
        if (movingDown || movingOnGround)
            GroundCheck();
        else if (movingUp)
            CeilingCheck();
    }
    
    void FixedUpdate() {
        jumpText.text = _jumpVelocity.ToString("F");
    }

    void LateUpdate() {
        if (_isGrounded)
            _spriteRenderer.color = Color.red;
        else if (_onWall)
            _spriteRenderer.color = Color.gray;
        else 
            _spriteRenderer.color = Color.yellow;

        transform.position += _movement;
    }

    //TODO refactor collision checks (DRY), maybe move them to a new class 
    void GroundCheck() {
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_movement.y);
        Vector3 direction = Vector3.down;

        for (int i = 0; i < VerticalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (VerticalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);

            if (hit.collider == null) {
                _isGrounded = false;
                continue;
            }

            transform.position += direction * (hit.distance - _bounds.extents.y);
            _movement.y = 0f;
            _isGrounded = true;
            _hasJumped = false;
            _hasWallJumped = false;
            break;
        }
    }

    void CeilingCheck() {
        _isGrounded = false;
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_movement.y);
        Vector3 direction = Vector3.up;

        for (int i = 0; i < VerticalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (VerticalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, Vector3.up, Color.cyan, 1f);

            if (hit.collider == null) continue;
            
            transform.position += direction * (hit.distance - _bounds.extents.y);
            _movement.y = 0f;
            _jumpVelocity = Vector3.zero;
            break;
        }
    }

    void WallCheck() {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_movement.x);
        int horizontalInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var direction = new Vector3(_facingDirection, 0, 0);
        var collision = false;
        var hit = new RaycastHit2D();

        for (int i = 0; i < HorizontalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (HorizontalRays - 1));
            hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);

            if (hit.collider == null) continue;
            collision = true;
            break;
        }
        
        if (!collision) {
            _onWall = false;
            return;
        }
        
        transform.position += direction * (hit.distance - _bounds.extents.x);
        _movement.x = 0f;
        _jumpVelocity.x = 0f;

        if (!_onWall)
            _wallSlideStartTime = Time.time;

        if (_isGrounded) {
            _onWall = false;
            return;
        }

        if (horizontalInput == _facingDirection) 
            _onWall = true;
        else {
            if (_onWall)
                _wallSlideStopTime = Time.time;
            _onWall = false;
        }
        
        if (WallJumpedRecently) {
            _onWall = false;
        }
    }
}

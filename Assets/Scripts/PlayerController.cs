using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    public Text jumpText;
    [SerializeField, Range(0f, 50f)] float maxSpeed = 12f;
    [SerializeField, Range(0f, 800f)] float maxAcceleration = 400f;
    [SerializeField, Range(0f, -50f)] float gravity = -8f;
    [SerializeField, Range(0f, 1f)] float wallSlideMultiplier = 0.2f;
    [SerializeField, Range(0f, 0.3f)] float startWallSlideGraceTime = 0.08f;
    [SerializeField, Range(0f, 0.3f)] float stopWallSlideGraceTime = 0.04f;
    [SerializeField, Range(0f, 100f)] float jumpForce = 25f;
    [SerializeField, Range(0f, 200f)] float jumpDecay = 50f;
    [SerializeField] LayerMask groundLayers;
    Vector3 _velocity;
    Vector3 _movement;
    int _facingDirection = 1;
    float _jumpVelocity;
    float _wallSlideStartTime;
    float _wallSlideStopTime;
    const float VerticalRays = 3f;
    const float HorizontalRays = 5f;
    const float RaycastOffset = 0.05f;
    bool _isGrounded;
    bool _hasJumped;
    bool _onWall;
    Collider2D _col;
    Bounds _bounds;
    //TODO replace bools with state enum

    void Awake() {
        _col = GetComponent<Collider2D>();
    }

    void Update() {
        float desiredVelocity = Input.GetAxisRaw("Horizontal") * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        
        _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity, maxSpeedChange);
        _velocity.y = _isGrounded ? 0f : gravity;

        if (_velocity.x > 0)
            _facingDirection = 1;
        else if (_velocity.x < 0)
            _facingDirection = -1;
        
        switch (_onWall) {
            case true: {
                _jumpVelocity = 0f;

                if (Time.time < _wallSlideStartTime + startWallSlideGraceTime)
                    _velocity.y = 0f;
                else
                    _velocity.y *= wallSlideMultiplier;
                break;
            }
            case false when Time.time < _wallSlideStopTime + stopWallSlideGraceTime:
                _velocity.y = 0f;
                break;
        }
        
        if (Input.GetButton("Jump") && !_hasJumped && _isGrounded) {
            _jumpVelocity = jumpForce;
            _hasJumped = true;
        }

        if (_hasJumped && !_onWall) {
            if (_jumpVelocity > 0f)
                _jumpVelocity -= jumpDecay * Time.deltaTime;
            
            _velocity.y += _jumpVelocity;
            
            if (_velocity.y < gravity) {
                _velocity.y = gravity;
                _jumpVelocity = 0f;
            }
        }
        
        bool moving = _velocity != Vector3.zero;
        
        if (!moving) return;
        
        _movement = _velocity * Time.deltaTime;
        _bounds = _col.bounds;
        bool movingHorizontally = _movement.x != 0;
        bool movingDown = _movement.y < 0;
        bool movingUp = _movement.y > 0;
        bool movingOnGround = _movement.y == 0 && movingHorizontally;

        if (movingHorizontally || _onWall) 
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
            _jumpVelocity = 0f;
            break;
        }
    }

    void WallCheck() {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_movement.x);
        int horizontalInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var direction = new Vector3(_facingDirection, 0, 0);

        for (int i = 0; i < HorizontalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (HorizontalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);
            
            if (hit.collider == null) {
                _onWall = false;
                continue;
            }

            if (!_onWall)
                _wallSlideStartTime = Time.time;
            
            transform.position += direction * (hit.distance - _bounds.extents.x);
            _movement.x = 0f;

            if (_isGrounded) {
                _onWall = false;
                return;
            }

            if (horizontalInput == _facingDirection) 
                _onWall = true;
            else {
                _onWall = false;
                _wallSlideStopTime = Time.time;
            }
            break;
        }
    }
}

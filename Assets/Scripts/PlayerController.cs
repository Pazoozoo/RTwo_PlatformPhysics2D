using System;
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
    [SerializeField, Range(0f, 0.5f)] float jumpInputLeeway = 0.1f;
    [SerializeField, Range(0f, 0.5f)] float jumpOffPlatformLeeway = 0.1f;
    [SerializeField, Range(0f, 100f)] float jumpForce = 25f;
    [SerializeField, Range(0f, 200f)] float jumpDecay = 50f;
    [SerializeField, Range(0f, 5f)] int wallJumps = 2;    
    [SerializeField] Vector2 wallJumpForce;
    [SerializeField] LayerMask groundLayers;
    
    Vector3 _velocity;
    Vector3 _jumpVelocity;
    Vector3 _movement;
    
    int _faceDirection = 1;
    int _jumpDirection;
    int _maxWallJumps;
    
    float _wallSlideStartTime;
    float _wallSlideStopTime;
    float _jumpInputTime;
    float _leaveGroundTime;
    float _leaveWallTime;

    bool _onGround;
    bool _onWall;
    bool _wallSliding;
    
    const int Right = 1;
    const int Left = -1;
    const float VerticalRays = 3f;
    const float HorizontalRays = 5f;
    const float RaycastOffset = 0.05f;
    
    Collider2D _col;
    Bounds _bounds;
    SpriteRenderer _spriteRenderer;

    bool WallSlideStartGraceTime => Time.time < _wallSlideStartTime + startWallSlideGraceTime;
    bool WallSlideStopGraceTime => Time.time < _wallSlideStopTime + stopWallSlideGraceTime;
    bool Jumping => _jumpVelocity != Vector3.zero;
    bool CanJump => _onGround || CloseToGround;
    bool CanWallJump => wallJumps > 0 && (_onWall || CloseToWall);
    bool JumpingRight => _jumpVelocity.x > 0f && _jumpDirection == Right;
    bool JumpingLeft => _jumpVelocity.x < 0f && _jumpDirection == Left;

    bool JumpInput => Time.time - _jumpInputTime < jumpInputLeeway;
    bool CloseToGround => Time.time - _leaveGroundTime < jumpOffPlatformLeeway;
    bool CloseToWall => Time.time - _leaveWallTime < jumpOffPlatformLeeway;
                      

    //TODO replace bools with state enum

    void Awake() {
        _col = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _maxWallJumps = wallJumps;
    }

    void Update() {
        float desiredVelocity = Input.GetAxisRaw("Horizontal") * maxSpeed;
        float maxSpeedChange = _onGround || _onWall ? maxAcceleration : maxAirAcceleration;
        maxSpeedChange *= Time.deltaTime;

        _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity, maxSpeedChange);
        _velocity.y = _onGround ? 0f : gravity;

        if (_velocity.x > 0)
            _faceDirection = Right;
        else if (_velocity.x < 0)
            _faceDirection = Left;
        
        switch (_wallSliding) {
            case true: {
                _jumpVelocity.y = 0f;

                if (WallSlideStartGraceTime) 
                    _velocity.y = 0f;
                else
                    _velocity.y *= wallSlideMultiplier;
                break;
            }
            case false when WallSlideStopGraceTime:
                _velocity.y = 0f;
                break;
        }
        
        if (Input.GetButtonDown("Jump"))
            _jumpInputTime = Time.time;
        
        if (JumpInput) {
            //TODO jumpForce * normalized jumpDirectionVector (??)
            if (CanJump) {
                _jumpVelocity.y = jumpForce;
                _jumpVelocity.x = 0f;
            } else if (CanWallJump) {
                wallJumps -= 1;
                _faceDirection *= -1;
                _jumpDirection = _faceDirection;
                _velocity.y = 0f;
                _velocity.x = 0f;
                _jumpVelocity.y = wallJumpForce.y;
                _jumpVelocity.x = wallJumpForce.x * _faceDirection;
                _onWall = false;
                _wallSliding = false;
            }
        }

        if (Jumping) {
            _velocity += _jumpVelocity;
            
            if (_jumpVelocity.y > 0f)
                _jumpVelocity.y -= jumpDecay * Time.deltaTime;
            
            if (_velocity.y < gravity) {
                _velocity.y = gravity;
                _jumpVelocity.y = 0f;
            }
            
            if (_jumpVelocity.x != 0f) {
                if (JumpingRight) {
                    _jumpVelocity.x -= jumpDecay * Time.deltaTime;
                    _jumpVelocity.x = Mathf.Max(_jumpVelocity.x, 0f);
                }
                else if (JumpingLeft) {
                    _jumpVelocity.x += jumpDecay * Time.deltaTime;
                    _jumpVelocity.x = Mathf.Min(_jumpVelocity.x, 0f);
                }
            }
        }
        
        _movement = _velocity * Time.deltaTime;
        bool moving = _movement != Vector3.zero;
        
        if (!moving) return;
        
        _bounds = _col.bounds;
        bool movingHorizontally = _movement.x != 0;
        bool movingDown = _movement.y < 0;
        bool movingUp = _movement.y > 0;
        bool movingOnGround = _movement.y == 0 && movingHorizontally;

        if (movingHorizontally || !_onGround) 
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
        if (_onGround)
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
        var collision = false;
        var hit = new RaycastHit2D();

        for (int i = 0; i < VerticalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (VerticalRays - 1));
            hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);

            if (hit.collider == null) continue;
            collision = true;
            break;
        }

        if (!collision) {
            if (_onGround)
                _leaveGroundTime = Time.time;
            _onGround = false;
            return;
        }
        
        transform.position += direction * (hit.distance - _bounds.extents.y);
        _movement.y = 0f;
        wallJumps = _maxWallJumps;
        _onGround = true;
        _wallSliding = false;
    }

    void CeilingCheck() {
        _onGround = false;
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
        _wallSliding = false;
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_movement.x);
        int horizontalInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var direction = new Vector3(_faceDirection, 0, 0);
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
            if (_onWall)
                _leaveWallTime = Time.time;
            _onWall = false;
            return;
        }

        if (horizontalInput == _faceDirection) {
            if (!_onWall)
                _wallSlideStartTime = Time.time;
            _wallSliding = true;
        }
        else if (_wallSliding) {
            _wallSlideStopTime = Time.time;
            _wallSliding = false;
        }
        
        transform.position += direction * (hit.distance - _bounds.extents.x);
        _movement.x = 0f;
        _jumpVelocity.x = 0f;
        _onWall = !_onGround;
    }
}

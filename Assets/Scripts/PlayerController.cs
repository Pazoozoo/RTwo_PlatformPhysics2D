using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    [SerializeField, Range(0f, 50)] int maxSpeed = 12;
    [SerializeField, Range(0f, 800)] int maxAcceleration = 400;
    [SerializeField, Range(0f, 800)] int maxAirAcceleration = 400;
    [SerializeField, Range(0f, -50)] int gravity = -20;
    
    [Header("Jump Input")]
    [SerializeField, Range(0f, 0.5f)] float jumpInputLeeway = 0.1f;
    [SerializeField, Range(0f, 0.5f)] float jumpOffPlatformLeeway = 0.1f;
    [SerializeField, Range(0f, 1f)] float minTimeBetweenJumps = 0.2f; 
    
    [Header("Jump")]
    [SerializeField, Range(0f, 100)] int jumpForce = 40;
    [SerializeField, Range(0f, 200)] int verticalJumpResistance = 100; 
    [SerializeField, Range(0f, 5f)] int airJumps = 1;
    
    [Header("Wall Jump")]
    [SerializeField] Vector2 wallJumpForce = new Vector2(5, 40);
    [SerializeField, Range(0f, 50)] int horizontalJumpResistance = 12;
    [SerializeField] bool unlimitedWallJumps; 
    [SerializeField, Range(0f, 5f)] int wallJumps = 2;
    
    [Header("Wall Slide")]
    [SerializeField, Range(0f, 30)] int wallSlideSpeed = 10;
    [SerializeField, Range(0f, 0.3f)] float wallSlideStartLeeway = 0.08f;
    [SerializeField, Range(0f, 0.3f)] float wallSlideStopLeeway = 0.09f;
    
    [Header("Other")]
    [SerializeField, Range(0f, 5f)] float respawnDelay = 1f;
    [SerializeField] LayerMask groundLayers;
    
    Vector3 _velocity;
    Vector3 _jumpVelocity;
    Vector3 _movement;
    Vector3 _respawnPosition;
    
    int _faceDirection = 1;
    int _jumpDirection;
    int _maxJumps;
    int _maxWallJumps;
    
    float _wallSlideStartTime;
    float _wallSlideStopTime;
    float _jumpInputTime = -10f;
    float _jumpTime;
    float _leaveGroundTime;
    float _leaveWallTime;

    bool _onGround;
    bool _onWall;
    bool _wallSliding;
    bool _respawning;
    
    const int Right = 1;
    const int Left = -1;
    const float VerticalRays = 3f;
    const float HorizontalRays = 5f;
    const float RaycastOffset = 0.05f;
    
    Collider2D _col;
    Bounds _bounds;
    SpriteRenderer _spriteRenderer;

    bool OnGround => _onGround || CloseToGround;
    bool OnWall => _onWall || CloseToWall;
    bool InAir => !OnGround && !OnWall;

    bool JumpInput => Time.time < _jumpInputTime + jumpInputLeeway;
    bool JumpReady => Time.time > _jumpTime + minTimeBetweenJumps;
    bool CanAirJump => airJumps > 0 && JumpReady && InAir;
    bool CanWallJump => wallJumps > 0 && JumpReady && OnWall && !OnGround;
    bool Jumping => _jumpVelocity != Vector3.zero;
    bool JumpingRight => _jumpVelocity.x > 0f && _jumpDirection == Right;
    bool JumpingLeft => _jumpVelocity.x < 0f && _jumpDirection == Left;
    bool WallJumping => _jumpVelocity.x != 0f && !_onGround;
    
    bool CloseToGround => Time.time < _leaveGroundTime + jumpOffPlatformLeeway;
    bool CloseToWall => Time.time < _leaveWallTime + jumpOffPlatformLeeway;
    bool WallSlideStartLeeway => Time.time < _wallSlideStartTime + wallSlideStartLeeway;
    bool WallSlideStopLeeway => Time.time < _wallSlideStopTime + wallSlideStopLeeway;

    enum Axis {
        Horizontal,
        Vertical
    }
    
    //TODO replace bools with state enum

    void Awake() {
        _col = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _maxJumps = airJumps;
        _maxWallJumps = wallJumps;
        _respawnPosition = transform.position;
    }

    void OnEnable() {
        EventBroker.Instance.OnDeath += RespawnPlayer;
        EventBroker.Instance.OnCheckpointUpdate += UpdateRespawnPosition;
    }

    void OnDisable() {
        EventBroker.Instance.OnDeath -= RespawnPlayer;
        EventBroker.Instance.OnCheckpointUpdate -= UpdateRespawnPosition;
    }

    #region Update

    void Update() {
        if (_respawning)
            return;
        
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
                if (!WallJumping)
                    _jumpVelocity.y = 0f;

                _velocity.y = WallSlideStartLeeway ? 0f : -wallSlideSpeed;
                break;
            }
            case false when WallSlideStopLeeway:
                _velocity.y = 0f;
                break;
        }
        
        if (Input.GetButtonDown("Jump"))
            _jumpInputTime = Time.time;
        
        if (JumpInput) {
            if (OnGround) 
                Jump();
            else if (CanAirJump)
                AirJump();
            else if (CanWallJump) 
                WallJump();
        }

        if (Jumping) {
            _velocity += _jumpVelocity;
            ReduceJumpVelocity();
        }

        _velocity.x = Mathf.Clamp(_velocity.x, -maxSpeed, maxSpeed);
        _movement = _velocity * Time.deltaTime;
        bool moving = _movement != Vector3.zero;
        
        if (moving)
            CheckForCollisions();
    }

    void LateUpdate() {
        if (_onGround)
            _spriteRenderer.color = Color.green;
        else if (_onWall)
            _spriteRenderer.color = Color.gray;
        else 
            _spriteRenderer.color = Color.yellow;

        transform.position += _movement;
    }
    
    #endregion

    #region Jumps
    
    void Jump() {
        _jumpTime = Time.time;
        _jumpVelocity.y = jumpForce;
        _jumpVelocity.x = 0f;
        _jumpInputTime = 0f;
    }

    void AirJump() {
        airJumps -= 1;
        Jump();
    }
    
    void WallJump() {
        _jumpTime = Time.time;
        _velocity.y = 0f;
        _velocity.x = 0f;
        _jumpVelocity.y = wallJumpForce.y;
        _jumpVelocity.x = wallJumpForce.x * _jumpDirection;
        _onWall = false;
        _wallSliding = false;
        
        if (unlimitedWallJumps) return;
        wallJumps -= 1;
    }
    
    void ReduceJumpVelocity() {
        if (_jumpVelocity.y > 0f)
            _jumpVelocity.y -= verticalJumpResistance * Time.deltaTime;

        if (_velocity.y < gravity) {
            _velocity.y = gravity;
            _jumpVelocity.y = 0f;
        }

        if (_jumpVelocity.x == 0f) return;
        
        if (JumpingRight) {
            _jumpVelocity.x -= horizontalJumpResistance * Time.deltaTime;
            _jumpVelocity.x = Mathf.Max(_jumpVelocity.x, 0f);
        }
        else if (JumpingLeft) {
            _jumpVelocity.x += horizontalJumpResistance * Time.deltaTime;
            _jumpVelocity.x = Mathf.Min(_jumpVelocity.x, 0f);
        }
    }
    
    #endregion

    void RespawnPlayer() {
        StartCoroutine(ResetPlayerPosition());
    }
    
    IEnumerator ResetPlayerPosition() {
        _spriteRenderer.enabled = false;
        _velocity = Vector3.zero;
        _movement = Vector3.zero;
        _jumpVelocity = Vector3.zero;
        _respawning = true;
        
        yield return new WaitForSeconds(respawnDelay);

        transform.position = _respawnPosition;
        _respawning = false;
        _spriteRenderer.enabled = true;
    }

    void UpdateRespawnPosition(Vector3 newPosition) {
        newPosition.y += _bounds.extents.y;
        _respawnPosition = newPosition;
    }
    
    #region Collisions
    
    void CheckForCollisions() {
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
    
    void WallCheck() {
        var direction = new Vector3(_faceDirection, 0, 0);
        
        HorizontalCollisionCheck(direction, out bool collision);
        
        if (!collision) {
            if (_onWall) {
                _leaveWallTime = Time.time;
            }
            _jumpDirection = _faceDirection;
            _wallSliding = false;
            _onWall = false;
            return;
        }

        _jumpDirection = -_faceDirection;
        int horizontalInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));

        if (horizontalInput == _faceDirection) {
            if (!_onWall)
                _wallSlideStartTime = Time.time;
            _wallSliding = true;
        } 
        else {
            if (_wallSliding) 
                _wallSlideStopTime = Time.time;
            _wallSliding = false;
        }
        
        _onWall = !_onGround;
    }
    
    void GroundCheck() {
        VerticalCollisionCheck(Vector3.down, out bool collision);

        if (!collision) {
            if (_onGround)
                _leaveGroundTime = Time.time;
            _onGround = false;
            return;
        }

        airJumps = _maxJumps;
        wallJumps = _maxWallJumps;
        _jumpVelocity.x = 0f;
        _jumpVelocity.y = 0f;
        
        _onGround = true;
        _wallSliding = false;
    }

    void CeilingCheck() {
        _onGround = false;
        VerticalCollisionCheck(Vector3.up, out bool collision);

        if (collision) 
            _jumpVelocity = Vector3.zero;
    }

    void VerticalCollisionCheck(Vector3 direction, out bool collision) {
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_movement.y);
        
        collision = DoCollisionCheck(startPoint, endPoint, direction, rayLength, VerticalRays, out RaycastHit2D hit);
        
        if (collision)
            AdjustPosition(direction, hit.distance, Axis.Vertical);
    }
    
    void HorizontalCollisionCheck(Vector3 direction, out bool collision) {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_movement.x);
        
        collision = DoCollisionCheck(startPoint, endPoint, direction, rayLength, HorizontalRays, out RaycastHit2D hit);
        
        if (collision)
            AdjustPosition(direction, hit.distance, Axis.Horizontal);
    }
    
    bool DoCollisionCheck(Vector2 startPoint, Vector2 endPoint, Vector3 direction, float rayLength, float rayAmount, out RaycastHit2D hit) {
        hit = new RaycastHit2D();
        
        for (int i = 0; i < rayAmount; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (rayAmount - 1));
            hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);

            if (hit.collider == null) continue;
            return true;
        }
        return false;
    }

    void AdjustPosition(Vector3 direction, float hitDistance, Axis axis) {
        float extents = 0f;
        
        switch (axis) {
            case Axis.Vertical:
                extents = _bounds.extents.y;
                _movement.y = 0f;
                break;
            case Axis.Horizontal:
                extents = _bounds.extents.x;
                if (_movement.x > 0f)
                    _movement.x = Mathf.Clamp(_movement.x, _movement.x, 0f);
                else if (_movement.x < 0f)
                    _movement.x = Mathf.Clamp(_movement.x, 0f, _movement.x);
                break;
        }
        
        transform.position += direction * (hitDistance - extents);
    }
    
    #endregion
}

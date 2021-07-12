using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    public Text jumpText;
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -8f;
    [SerializeField] float wallSlideMultiplier = 0.2f;
    [SerializeField] float startWallSlideGraceTime = 0.08f;
    [SerializeField] float stopWallSlideGraceTime = 0.04f;
    [SerializeField] float jumpForce = 25f;
    [SerializeField] float jumpDecay = 50f;
    [SerializeField] LayerMask groundLayers;
    Vector3 _move = Vector3.zero;
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
        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        _move.y = _isGrounded ? 0f : gravity * Time.deltaTime;

        if (_move.x > 0)
            _facingDirection = 1;
        else if (_move.x < 0)
            _facingDirection = -1;
        
        switch (_onWall) {
            case true: {
                _jumpVelocity = 0f;

                if (Time.time < _wallSlideStartTime + startWallSlideGraceTime)
                    _move.y = 0f;
                else
                    _move.y *= wallSlideMultiplier;
                break;
            }
            case false when Time.time < _wallSlideStopTime + stopWallSlideGraceTime:
                _move.y = 0f;
                break;
        }
        
        if (Input.GetButton("Jump") && !_hasJumped && _isGrounded) {
            _jumpVelocity = jumpForce;
            _hasJumped = true;
        }

        if (_hasJumped && !_onWall) {
            if (_jumpVelocity > 0f)
                _jumpVelocity -= jumpDecay * Time.deltaTime;
            
            _move.y += _jumpVelocity * Time.deltaTime;
            float maxFallSpeed = gravity * Time.deltaTime;
            
            if (_move.y < maxFallSpeed) {
                _move.y = maxFallSpeed;
                _jumpVelocity = 0f;
            }
        }

        bool moving = _move != Vector3.zero;
        
        if (!moving) return;
        
        _bounds = _col.bounds;
        bool movingHorizontally = _move.x != 0;
        bool movingDown = _move.y < 0;
        bool movingUp = _move.y > 0;
        bool movingOnGround = _move.y == 0 && movingHorizontally;

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
        transform.position += _move;
    }

    //TODO refactor collision checks (DRY), maybe move them to a new class 
    void GroundCheck() {
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_move.y);
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
            _move.y = 0f;
            _isGrounded = true;
            _hasJumped = false;
            break;
        }
    }

    void CeilingCheck() {
        _isGrounded = false;
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_move.y);
        Vector3 direction = Vector3.up;

        for (int i = 0; i < VerticalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (VerticalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, Vector3.up, Color.cyan, 1f);

            if (hit.collider == null) continue;
            
            transform.position += direction * (hit.distance - _bounds.extents.y);
            _move.y = 0f;
            _jumpVelocity = 0f;
            break;
        }
    }

    void WallCheck() {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_move.x);
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
            _move.x = 0f;

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

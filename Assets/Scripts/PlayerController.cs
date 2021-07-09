using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    public Text jumpText;
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -8f;
    [SerializeField] float jumpForce = 25f;
    [SerializeField] float jumpDecay = 50f;
    [SerializeField] float maxFallSpeed = -10f;
    [SerializeField] float fallSpeedMultiplier = 1.1f;
    [SerializeField] float fallSpeedThreshold = 0f;
    [SerializeField] LayerMask groundLayers;
    Vector3 _move = Vector3.zero;
    Vector3 _playerExtents;
    float _jumpVelocity;
    const float VerticalRays = 4f;
    const float HorizontalRays = 6f;
    const float RaycastOffset = 0.05f;
    bool _isGrounded;
    bool _hasJumped;
    Collider2D _col;
    Bounds _bounds;

    void Awake() {
        _col = GetComponent<Collider2D>();
        _playerExtents = _col.bounds.extents;
    }

    void Update() {
        _bounds = _col.bounds;
        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        _move.y = _isGrounded ? 0f : gravity * Time.deltaTime;

        if (Input.GetButton("Jump") && !_hasJumped) {
            _jumpVelocity = jumpForce;
            _hasJumped = true;
        }

        if (_hasJumped) {
            _jumpVelocity -= jumpDecay * Time.deltaTime;

            if (_jumpVelocity < fallSpeedThreshold)
                _jumpVelocity *= _jumpVelocity > 0 ? -fallSpeedMultiplier : fallSpeedMultiplier;

            if (_jumpVelocity <= maxFallSpeed)
                _jumpVelocity = maxFallSpeed;

            _move.y += _jumpVelocity * Time.deltaTime;
        }

        bool falling = _move.y < 0;
        bool rising = _move.y > 0;
        bool movingOnGround = _move.y == 0 && _move.x != 0;
        bool movingHorizontally = _move.x != 0;

        if (falling || movingOnGround)
            GroundCheck();
        
        if (movingHorizontally) 
            WallCheck();
    }
    
    void FixedUpdate() {
        jumpText.text = _jumpVelocity.ToString("F");
    }

    void LateUpdate() {
        transform.position += _move;
    }

    void GroundCheck() {
        var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
        var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
        float rayLength = _bounds.extents.y + Mathf.Abs(_move.y);
        Vector3 direction = Vector3.down;

        for (int i = 0; i < VerticalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (VerticalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, Vector3.down, Color.cyan, 1f);

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

    void WallCheck() {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_move.x);
        Vector3 direction = _move.x > 0 ? Vector3.right : Vector3.left;

        for (int i = 0; i < HorizontalRays; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (HorizontalRays - 1));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);
            
            if (hit.collider == null) continue;
            
            transform.position += direction * (hit.distance - _bounds.extents.x);
            _move.x = 0f;
            break;
        }
    }
}

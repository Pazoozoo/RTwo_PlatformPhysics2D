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
    const float RaycastOffset = 0.2f;
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
        //TODO raycast ground check
        _isGrounded = true;
        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        _move.y = _isGrounded ? 0f : gravity * Time.deltaTime;

        if (_move.x != 0) 
            HorizontalColliderCheck();

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

        // if (_isBlockedLeft && _move.x < 0)
        //     _move.x = 0;
    }

    void HorizontalColliderCheck() {
        var startPoint = new Vector2(_bounds.center.x, _bounds.min.y + RaycastOffset);
        var endPoint = new Vector2(_bounds.center.x, _bounds.max.y - RaycastOffset);
        float rayLength = _bounds.extents.x + Mathf.Abs(_move.x);
        Vector3 direction = _move.x > 0 ? Vector3.right : Vector3.left;

        for (int i = 0; i < HorizontalRays; i++) {
            float lerpAmount = i / (HorizontalRays - 1);
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, lerpAmount);
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
            Debug.DrawRay(origin, direction, Color.cyan, 1f);
            
            if (hit.collider == null) continue;
            
            transform.position += direction * (hit.distance - _bounds.extents.x);
            _move.x = 0f;
            break;
        }
    }

    void FixedUpdate() {
        jumpText.text = _jumpVelocity.ToString("F");
    }

    void LateUpdate() {
        transform.position += _move;
    }


    void GetContactPoints(Vector2 position) {
        Vector2 originalPosition = position;
        position.x += _playerExtents.x * 0.9f;

        for (int i = 0; i < 3; i++) {
            RaycastHit2D downHit = Physics2D.Raycast(position, Vector2.down, _playerExtents.y, groundLayers);
            Debug.DrawRay(position, Vector3.down, Color.cyan, 2f);
            if (downHit) {
                Debug.Log("downHit: " + downHit.collider.gameObject.name);
                _isGrounded = true;
                _hasJumped = false;
                
                Bounds ground = downHit.collider.bounds;
                float groundPositionY = ground.center.y + ground.extents.y;
                Vector2 updatedPosition = originalPosition;
                updatedPosition.y = groundPositionY + _playerExtents.y;
                transform.position = updatedPosition;
                break;
            }
            position.x -= _playerExtents.x * 0.9f;
            _isGrounded = false;
        }

        position = originalPosition;
        position.y -= _playerExtents.y * 0.9f;
        
        for (int i = 0; i < 3; i++) {
            RaycastHit2D leftHit = Physics2D.Raycast(position, Vector2.left, _playerExtents.x, groundLayers);
            Debug.DrawRay(position, Vector3.left, Color.cyan, 2f);
            if (leftHit) {
                Debug.Log("leftHit: " + leftHit.collider.gameObject.name);
                // _isBlockedLeft = true;
                break;
            }
            position.y += _playerExtents.y * 0.9f;
            // _isBlockedLeft = false;
        }
    }
}

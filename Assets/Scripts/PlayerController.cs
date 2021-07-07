using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -8f;
    [SerializeField] float jumpForce = 25f;
    [SerializeField] float jumpDecay = 50f;
    Vector3 _move = Vector3.zero;
    Vector3 _playerExtents;
    float _jumpVelocity;
    bool _isGrounded;
    bool _hasJumped;
    Collider2D _col;

    void Awake() {
        _col = GetComponent<Collider2D>();
        _playerExtents = _col.bounds.extents;
    }

    void Update() {
        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        _move.y = _isGrounded ? 0f : gravity * Time.deltaTime;
        
        if (Input.GetButton("Jump") && !_hasJumped) {
            _jumpVelocity = jumpForce;
            _hasJumped = true;
        }
        
        if (_hasJumped) {
            _jumpVelocity = _jumpVelocity > 0f ? _jumpVelocity - jumpDecay * Time.deltaTime : 0f;
            _move.y += _jumpVelocity * Time.deltaTime;
        }

        transform.position += _move;
    }

    void OnTriggerEnter2D(Collider2D other) {
        Vector3 playerPosition = transform.position;
        _isGrounded = GroundCheck(other, playerPosition);
        
        if (_isGrounded) {
            Bounds ground = other.bounds;
            float groundHeight = ground.center.y + ground.extents.y;
            
            playerPosition.y = groundHeight + _playerExtents.y;
            transform.position = playerPosition;
            _hasJumped = false;
        }
    }

    bool GroundCheck(Collider2D other, Vector3 position) {
        position.y -= _playerExtents.y;
        var bottomLeftCorner = new Vector3(position.x - _playerExtents.x, position.y, 0);
        var bottomRightCorner = new Vector3(position.x + _playerExtents.x, position.y, 0);
        return other.OverlapPoint(bottomLeftCorner) || other.OverlapPoint(bottomRightCorner);
    }

    void OnTriggerExit2D(Collider2D other) {
        _isGrounded = false;
    }
}

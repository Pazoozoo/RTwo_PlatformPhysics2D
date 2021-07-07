using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -10f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpDecay = 1f;
    Vector3 _move = Vector3.zero;
    float _jumpVelocity;
    bool _isGrounded;
    bool _hasJumped;
    Collider2D _col;

    void Awake() {
        _col = GetComponent<Collider2D>();
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
        Vector3 extents = _col.bounds.extents;
        Vector3 position = transform.position;
        
        GroundCheck(other, position, extents);
    }

    void GroundCheck(Collider2D other, Vector3 position, Vector3 extents, float offset = 0.05f) {
        position.y -= extents.y + offset;
        var bottomLeftCorner = new Vector3(position.x - extents.x, position.y, 0);
        var bottomRightCorner = new Vector3(position.x + extents.x, position.y, 0);
        _isGrounded = other.OverlapPoint(bottomLeftCorner) || other.OverlapPoint(bottomRightCorner);
        if (_isGrounded)
            _hasJumped = false;
    }

    void OnTriggerExit2D(Collider2D other) {
        _isGrounded = false;
    }
}

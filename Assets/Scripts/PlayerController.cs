using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -10f;
    Vector3 _move = Vector3.zero;
    bool _isGrounded;
    Collider2D _col;

    void Awake() {
        _col = GetComponent<Collider2D>();
    }

    void Update() {

        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        
        if (!_isGrounded)
            _move.y = gravity * Time.deltaTime;
        else 
            _move.y = 0;

        transform.position += _move;
    }

    void OnTriggerEnter2D(Collider2D other) {
        Vector3 extents = _col.bounds.extents;
        Vector3 position = transform.position;
        
        GroundCheck(other, position, extents);
    }

    void GroundCheck(Collider2D other, Vector3 position, Vector3 extents) {
        position.y -= extents.y + 0.05f;
        var bottomLeftCorner = new Vector3(position.x - extents.x, position.y, 0);
        var bottomRightCorner = new Vector3(position.x + extents.x, position.y, 0);
        _isGrounded = other.OverlapPoint(bottomLeftCorner) || other.OverlapPoint(bottomRightCorner);
    }

    void OnTriggerExit2D(Collider2D other) {
        _isGrounded = false;
    }
}

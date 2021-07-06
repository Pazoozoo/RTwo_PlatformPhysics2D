using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -10f;
    Vector3 _move = Vector3.zero;
    bool _isGrounded;

    void Update() {

        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        
        if (!_isGrounded)
            _move.y = gravity * Time.deltaTime;
        else 
            _move.y = 0;

        transform.position += _move;
    }

    void OnTriggerEnter2D(Collider2D other) {
        _isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other) {
        _isGrounded = false;
    }
}

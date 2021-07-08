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
    bool _isGrounded;
    bool _isBlockedLeft;
    bool _hasJumped;
    Collider2D _col;

    void Awake() {
        _col = GetComponent<Collider2D>();
        Bounds bounds = _col.bounds;
        _playerExtents = bounds.extents;
    }

    void Update() {
        GetContactPoints(transform.position);
        
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

        if (_isBlockedLeft && _move.x < 0)
            _move.x = 0;
        
        transform.position += _move;
        
        // var position = transform.position;
        // Debug.DrawLine(position, new Vector3(position.x, position.y - _playerExtents.y, 0), Color.magenta);
        // Debug.DrawLine(new Vector3(position.x - _playerExtents.x, position.y, 0), 
        //     new Vector3(position.x - _playerExtents.x, position.y - _playerExtents.y, 0), Color.magenta);
        // Debug.DrawLine(new Vector3(position.x + _playerExtents.x, position.y, 0), 
        //     new Vector3(position.x + _playerExtents.x, position.y - _playerExtents.y, 0), Color.magenta);
    }

    void FixedUpdate() {
        jumpText.text = _jumpVelocity.ToString("F");
    }

    // void OnTriggerEnter2D(Collider2D other) {
    //     Vector3 playerPosition = transform.position;
    //     GetContactPoints(playerPosition);
    //     // Debug.Log($"Enter = {other.gameObject.name}: grounded = {_isGrounded}");
    //     
    //     // if (_isGrounded) {
    //     //     Bounds ground = other.bounds;
    //     //     float groundHeight = ground.center.y + ground.extents.y;
    //     //
    //     //     playerPosition.y = groundHeight + _playerExtents.y;
    //     //     transform.position = playerPosition;
    //     //     _hasJumped = false;
    //     // }
    //
    //     //TODO wall check
    // }

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
                _isBlockedLeft = true;
                break;
            }
            position.y += _playerExtents.y * 0.9f;
            _isBlockedLeft = false;
        }
    }

    // void OnTriggerExit2D(Collider2D other) {
    //     GetContactPoints(transform.position);
    //     // Debug.Log($"Exit: grounded = {_isGrounded}");
    // }
}

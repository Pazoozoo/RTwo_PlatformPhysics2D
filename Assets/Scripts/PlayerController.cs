using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity = -10f;
    Vector3 _move = Vector3.zero;

    void Update() {

        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        _move.y = gravity * Time.deltaTime;
        
        transform.position += _move;
    }
}

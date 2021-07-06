using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] float speed = 12f;
    [SerializeField] float gravity;
    Vector3 _move;

    void Update() {

        _move.x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime; 
        
        transform.position += _move;
    }
}

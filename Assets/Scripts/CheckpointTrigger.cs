using System;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour {
    [SerializeField] LayerMask groundLayer;

    void Start() {
        EventBroker.Instance.OnCheckpointUpdate += Activate;
    }

    void OnDestroy() {
        EventBroker.Instance.OnCheckpointUpdate -= Activate;
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        var col = GetComponent<BoxCollider2D>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, col.size.y, groundLayer);

        if (hit.collider != null) {
            EventBroker.Instance.OnCheckpointUpdate?.Invoke(hit.point);
            gameObject.SetActive(false);
        }
        else 
            Debug.LogWarning($"{name}: Collider is not close enough to ground!");
    }
    
    void Activate(Vector3 v) {
        gameObject.SetActive(true);
    }
}

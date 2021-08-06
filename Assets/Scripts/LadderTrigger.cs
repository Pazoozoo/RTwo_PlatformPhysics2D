using UnityEngine;

public class LadderTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        
        EventBroker.Instance.OnLadderUpdate?.Invoke(true);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        
        EventBroker.Instance.OnLadderUpdate?.Invoke(false);
    }
}

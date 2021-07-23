using UnityEngine;

public class DeathTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
            EventBroker.Instance.OnDeath?.Invoke();
    }
}

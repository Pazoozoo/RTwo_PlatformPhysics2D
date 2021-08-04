using UnityEngine;

public class DestroyDustEffect : MonoBehaviour {
    
    /// <summary>
    /// Animation Event
    /// </summary>
    public void OnDestroyEffect() {
        Destroy(gameObject);
    }
}

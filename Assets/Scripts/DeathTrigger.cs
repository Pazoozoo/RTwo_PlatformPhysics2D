using System;
using UnityEngine;

public class DeathTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponent<PlayerController>();
        
        if (player != null) 
            StartCoroutine(player.Respawn());
    }
}

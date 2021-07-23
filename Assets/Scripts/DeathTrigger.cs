using System;
using UnityEngine;

public class DeathTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Trying to respawn");
        var player = other.GetComponent<PlayerController>();
        
        if (player != null) 
            player.Respawn();
    }

    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log("col");
    }
}

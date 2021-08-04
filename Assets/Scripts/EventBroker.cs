using System;
using UnityEngine;

public class EventBroker {
    static EventBroker _instance;
    EventBroker() { }
    
    public static EventBroker Instance => _instance ??= new EventBroker();
    
    public Action OnDeath;
    public Action<Vector3> OnCheckpointUpdate;
    public Action<PlayerController.PlayerState> OnPlayerStateUpdate;
    public Action<int> OnDirectionChange;
    public Action<int> OnMovementImpact;
}
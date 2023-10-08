using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLaneTrigger : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private BoxCollider col;
    
    public Action<WordBlock> OnBlockDetected;

    private void Awake() {
        col.enabled = false;
    }
    
    public void SetEnabled(bool b) {
        col.enabled = b;
    }
    
    private void OnTriggerEnter(Collider other) {
        var block = other.GetComponent<WordBlock>();
        if (block != null) {
            OnBlockDetected?.Invoke(block);
        }
    }
}

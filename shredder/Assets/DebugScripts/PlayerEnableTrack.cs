using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnableTrack : MonoBehaviour
{
    public GameObject playerTouchControls;
    private void OnEnable()
    {
        playerTouchControls.SetActive(true);
    }

    private void OnDisable()
    {
        
    }
}

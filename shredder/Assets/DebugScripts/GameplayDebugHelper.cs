using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayDebugHelper : MonoBehaviour
{
    public Laser playerOneLaser;

    private bool enableKeyboardEvents = false;
    private void OnEnable()
    {
        enableKeyboardEvents = true;
    }
    public void InvokeButtonPress(int id)
    {
        playerOneLaser.DebugFireLaser(id);
    }

    private void Update()
    {
        if (enableKeyboardEvents)
        {
            //update the below api to new input system to work with keyboard
            /*
            if (Input.GetKey(KeyCode.Alpha1))
            {
                playerOneLaser.DebugFireLaser(0);
            }
            if (Input.GetKey(KeyCode.Alpha1))
            {
                playerOneLaser.DebugFireLaser(1);
            }
            if (Input.GetKey(KeyCode.Alpha1))
            {
                playerOneLaser.DebugFireLaser(2);
            }
            */
        }
    }
}

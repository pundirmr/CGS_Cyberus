using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FaceTransformTowardsCamera : MonoBehaviour
{
    [SerializeField] private bool flip;
    
    private bool _cameraFound;
    private Camera cam;


    private void Start()
    {
        if (StaticCamera.Main == null)
        {
            cam = MainMenuStaticCamera.Main;
        }
        else
        {
            cam = StaticCamera.Main;
        }
    }
    private void Update()
    {
        
        var dir = cam.transform.position - transform.position;
        // dir = new Vector3(0, dir.y, 0);
        Quaternion rot = quaternion.LookRotation(dir.normalized, Vector3.up);
        var finalRot = new Vector3(0, flip ? rot.eulerAngles.y + 180 : rot.eulerAngles.y, 0);
        transform.localRotation = Quaternion.Euler(finalRot);
    }
}

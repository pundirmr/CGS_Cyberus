using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStaticCamera : MonoBehaviour
{
    public static Camera Main { get; private  set;}
    private void Awake()
    {
        Main = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        Main = null;
    }
}

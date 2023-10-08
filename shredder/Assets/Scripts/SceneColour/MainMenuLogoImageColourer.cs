using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogoImageColourer : MonoBehaviour
{
    [SerializeField] private float intensityStrength = 1;

    private Image _image;

    private Material _mat;
    private int _tintID;

    private void Start()
    {
        SceneColourer.OnUpdateSceneColours += UpdateColours;
        _image = GetComponent<Image>();
        _mat = _image.materialForRendering;
        _tintID = _mat.shader.GetPropertyNameId(_mat.shader.FindPropertyIndex("_Tint"));
        var colour = SceneColourer.currentColourScheme.Find(SceneColourType.logoColour);
        // colour *= intensityStrength;
        _mat.SetColor(_tintID, colour);
    }

    private void UpdateColours(SceneColourScheme scheme)
    {
        var colour = scheme.Find(SceneColourType.logoColour);
        // colour *= intensityStrength;
        _mat.SetColor(_tintID, colour);
    }

    private void OnDestroy()
    {
        SceneColourer.OnUpdateSceneColours -= UpdateColours;
    }
}

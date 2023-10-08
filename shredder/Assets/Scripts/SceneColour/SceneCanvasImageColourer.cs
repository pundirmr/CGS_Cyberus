using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneCanvasImageColourer : MonoBehaviour
{
    private Image _image;

    private void Start()
    {
        SceneColourer.OnUpdateSceneColours += UpdateColours;

        _image = GetComponent<Image>();
        var scheme = SceneColourer.currentColourScheme;
        _image.color = scheme.Find(SceneColourType.BuildingFacade);
    }

    private void OnDestroy()
    {
        SceneColourer.OnUpdateSceneColours -= UpdateColours;
    }

    private void UpdateColours(SceneColourScheme scheme)
    {
        _image.color = scheme.Find(SceneColourType.BuildingFacade);
    }
}

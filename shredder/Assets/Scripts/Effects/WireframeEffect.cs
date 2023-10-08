using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WireframeEffect : MonoBehaviour
{
    [SerializeField] private Material wireframeMaterialTemplate;
    [SerializeField] private bool isTextureWireframe;
    [SerializeField][Range(0, 0.5f)] private float thickness;
    [SerializeField][Range(0, 10)] private float intensity;
    [SerializeField][Range(0, 1)] private float baseColourAlpha = 0.25f;

    public Material MaterialInstance { get; private set;}
    //Shader IDs
    private int _colourID;
    private int _baseColourID;
    private int _thicknessID;

    public float BaseColourAlpha => baseColourAlpha;

    private void Awake()
    {
        MaterialInstance = new Material(wireframeMaterialTemplate);
        Shader shader = MaterialInstance.shader;
        _colourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Colour"));
        _baseColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColour"));
        if (isTextureWireframe)
        {
            return;
        }
        _thicknessID = shader.GetPropertyNameId(shader.FindPropertyIndex("_WireframeVal"));
        SetThickness(thickness);
    }

    public void SetColour(Color colour)
    {
        float power = maths.Pow2(intensity);
        colour *= power;
        colour.a = 1;
        MaterialInstance.SetColor(_colourID, colour);
    }
    
    /// <summary>
    /// Can be HDR.
    /// </summary>
    /// <param name="colour"></param>
    public void SetBaseColour(Color colour)
    {
        colour.a = baseColourAlpha;
        MaterialInstance.SetColor(_baseColourID, colour);
    }

    //////////////////
    /// Varient functions to allow for editing of alpha values
    public void SetColourAndAlpha(Color colour)
    {
        // we cache the alpha value of the colour here so that we can reapply the correct alpha below
        float a     = colour.a;
        float power = maths.Pow2(intensity);
        
        colour  *= power;
        colour.a = a; // apply the cached alpha value
        
        MaterialInstance.SetColor(_colourID, colour);
    }
    
    public void SetBaseColourAndAlpha(Color colour)
    {
        MaterialInstance.SetColor(_baseColourID, colour);
    }

    public void SetHDRColour(Color hdrColour)
    {
        MaterialInstance.SetColor(_colourID, hdrColour);
    }

    public void SetThickness(float value)
    {
        if (isTextureWireframe)
        {
            return;
        }
        MaterialInstance.SetFloat(_thicknessID, value);
    }
}

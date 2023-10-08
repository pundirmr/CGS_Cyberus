using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AberrationEffect : MonoBehaviour
{
    [SerializeField] private Material aberrationMaterialTemplate;
    [SerializeField] private bool useTint;
    [HelpBox("Set this to true if the effect is on a masked image, so that material references will be set properly see: (https://forum.unity.com/threads/masked-ui-elements-shader-not-updating.371542/) for more details.")]
    [SerializeField] private bool isUiMasked;
    [Space]
    public Vector3 maxAmount;
    public Vector3 minAmount;

    private Image imgRef;
    private Material _matInst;
    private Material _maskedMat; // NOTE(Zack): this seems to get cleaned up on the object/image being disabled
    
    //Shader IDs
    private int _amountXID;
    private int _amountYID;
    private int _useTint;
    
    private bool _isUiMasked;
    private bool setup = false;
    
    private void Awake()
    {
        imgRef   = GetComponent<Image>();
        _matInst = new Material(aberrationMaterialTemplate);

        Shader shader = _matInst.shader;
        _amountXID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountX"));
        _amountYID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountY"));
        _useTint      = shader.GetPropertyNameId(shader.FindPropertyIndex("_UseTint"));
        
        SetUseTint(useTint);
    }
    
    public void Setup()
    {
        _maskedMat = imgRef.materialForRendering;

        Shader shader = _maskedMat.shader;
        _amountXID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountX"));
        _amountYID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountY"));
        _useTint      = shader.GetPropertyNameId(shader.FindPropertyIndex("_UseTint"));

        SetUseTint(useTint);
        _isUiMasked = true;
        setup = true;
    }
    
    private void Start()
    {
        if (setup || _maskedMat != null) return;
        
        if (isUiMasked)
        {
            _maskedMat = imgRef.materialForRendering;

            Shader shader = _maskedMat.shader;
            _amountXID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountX"));
            _amountYID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountY"));
            _useTint      = shader.GetPropertyNameId(shader.FindPropertyIndex("_UseTint"));

            SetUseTint(useTint);
            _isUiMasked = true;
        }
    }

    // NOTE(Zack): [_maskedMat] seems to get cleaned up by the GC or Unity when the [Image] or [Entity] this
    // script is attached to gets disabled
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckMaskMatIsNotNull() {
        if (_maskedMat != null) return;
        _maskedMat = imgRef.materialForRendering;
    }
    
    public void SetUseTint(bool value)
    {
        if (_isUiMasked)
        {
            CheckMaskMatIsNotNull();
            _maskedMat.SetInteger(_useTint, value.ToInt());
            return;
        }
        if (!_matInst.HasProperty(_useTint))
        {
            _matInst.SetInteger(_useTint, value.ToInt());
        }
    }

    public void SetAmount(Vector3 valueX, Vector3 valueY)
    {
        if (_isUiMasked)
        {
            CheckMaskMatIsNotNull();
            _maskedMat.SetVector(_amountXID, valueX);
            _maskedMat.SetVector(_amountYID, valueY);
            
            return;
        }
        _matInst.SetVector(_amountXID, valueX);
        _matInst.SetVector(_amountYID, valueY);
    }

    public Material GetMaterialInstance()
    {
        if (isUiMasked)
        {
            CheckMaskMatIsNotNull();
            return _maskedMat;
        }

        return _matInst;
    }

    public Vector3 RandomAmount()
    {
        float x = Random.Range(minAmount.x, maxAmount.x);
        float y = Random.Range(minAmount.y, maxAmount.y);
        float z = Random.Range(minAmount.z, maxAmount.z);

        return new (x, y, z);
    }

    public Vector3 RandomAmount(float multiplier) {
        float x = Random.Range(minAmount.x * multiplier, maxAmount.x * multiplier);
        float y = Random.Range(minAmount.y * multiplier, maxAmount.y * multiplier);
        float z = Random.Range(minAmount.z * multiplier, maxAmount.z * multiplier);

        return new (x, y, z);
    }
}

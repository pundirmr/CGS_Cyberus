using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneTMP_TextColourer : MonoBehaviour
{
    [SerializeField] private TextType type;
    
    private enum TextType
    {
        TitleText = 0,
        Other,
    }
    
    private TMP_Text _text;
    private TMP_FontAsset _fontAsset;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        switch (type)
        {
            case TextType.TitleText:
                _text.fontMaterial = SceneColourer.TextMaterial;
                break;
            case TextType.Other:
                _text.fontMaterial = SceneColourer.OtherTextMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }
}
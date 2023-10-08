using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameSceneCharacterUI : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [Header("Aberration Effect Values")]
    [SerializeField] private int rndAberrationCount;
    [SerializeField] private Vector3 minAberrationAmount;
    [SerializeField] private Vector3 maxAberrationAmount;
    [Header("Animation Effect Values")]
    [SerializeField] private float animTime;
    [SerializeField] private float aberrationTime;
    
    private Material _matInst;
    private int _aberrationAmountXID;
    private int _aberrationAmountYID;
    private int _glowColourID;
    
    private List<Vector3> aberrationAmounts;

    public delegate IEnumerator LerpOnUiCharacter(Sprite image, Color imageColour, Color glowColor);
    public LerpOnUiCharacter ShowUiCharacter;

    private void Awake()
    {
        _matInst = characterImage.materialForRendering;
        Shader shader = _matInst.shader;
        _aberrationAmountXID = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountX"));
        _aberrationAmountYID = shader.GetPropertyNameId(shader.FindPropertyIndex("_AmountY"));
        _glowColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_GlowColour"));
        
        ShowUiCharacter = ShowUiCharacterAnim;
        
        aberrationAmounts = new List<Vector3>(rndAberrationCount);
        for (int i = 0; i < rndAberrationCount; i++)
        {
            Vector3 amount = new Vector3(Random.Range(minAberrationAmount.x, maxAberrationAmount.x),
                                         Random.Range(minAberrationAmount.y, maxAberrationAmount.y),
                                         Random.Range(minAberrationAmount.z, maxAberrationAmount.z));
            aberrationAmounts.Add(amount);
        }
    }

    private IEnumerator ShowUiCharacterAnim(Sprite image, Color imageColour, Color glowColor)
    {
        _matInst = characterImage.materialForRendering;
        Color spriteColour = imageColour;
        // Color startColour = new Color(spriteColour.r, spriteColour.g, spriteColour.b, 0);
        Color startColour = new Color(1, 1, 1, 0);
        characterImage.sprite = image;
        characterImage.color = new Color(1,1,1, 0);
        
        float time = 0;
        float abTime = aberrationTime;
        while (time < animTime)
        {
            characterImage.color = Color.Lerp(startColour, spriteColour, EaseInUtil.Exponential(time / animTime));
            // Log.Print($"{Color.Lerp(new Color(1,1,1, 0), new Color(1,1,1, 1), time / animTime)}");
            if (abTime >= aberrationTime)
            {
                _matInst.SetVector(_aberrationAmountXID, aberrationAmounts[0]);
                _matInst.SetVector(_aberrationAmountYID, aberrationAmounts[1]);
                ArrayUtil.Shuffle(aberrationAmounts, aberrationAmounts.Count);
                abTime = 0;
            }
            
            time += Time.deltaTime;
            abTime += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        _matInst.SetVector(_aberrationAmountXID, Vector4.zero);
        _matInst.SetVector(_aberrationAmountYID, Vector4.zero);
        characterImage.color = spriteColour;
        
        
        _matInst.SetColor(_glowColourID, glowColor);
    }
}

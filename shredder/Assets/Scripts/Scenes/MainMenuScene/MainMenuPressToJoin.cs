using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainMenuPressToJoin : MonoBehaviour
{
  [Header("Glitch")]
  [SerializeField] private Image[] glitchImages;
  [SerializeField] private Sprite[] glitchSprites;
  
  [Header("Aberration Effects")]
  [SerializeField] private RangedFloat3 aberrationAmountRange = new (new (-0.3f, -0.3f, -0.3f), new (0.3f, 0.3f, 0.3f));
  [SerializeField] private RangedFloat aberrationWaitRange    = new (0.1f, 0.15f);
  [SerializeField] private RangedInt aberrationChangesRange   = new (10, 15);
  [SerializeField] private RangedInt aberrationImageCount     = new RangedInt(2, 4);
  [SerializeField] private RangedFloat2 aberrationMoveRange;
  [SerializeField] private RangedFloat textMoveWaitRange      = new (0.1f, 0.15f);
  [SerializeField] private RangedInt textChangesRange         = new (10, 15);
  [SerializeField] private RangedFloat2 textMoveRange;
  
  [Header("Timing Settings")]
  [SerializeField] private RangedFloat waitBetweenEffect;
  
  private AberrationEffect[] _aberrationEffects;
  private float3[] _aberrationAmounts;
  private float[] _aberrationWaits;
  private int _aberrationChanges;
  
  private int[] _aberrationImageIndices;
  
  private delegate IEnumerator ImageAberrationDel(int index);
  private ImageAberrationDel ImageAberration;

  private RectTransform _rectTransform => (RectTransform)transform;
  private Sprite _randomGlitchSprite   => glitchSprites[Random.Range(0, glitchSprites.Length)];

  private void Awake()
  {
    ImageAberration = __ImageAberration;

    _aberrationEffects      = new AberrationEffect[glitchImages.Length];
    _aberrationImageIndices = new int[glitchImages.Length];
    for (int i = 0; i < glitchImages.Length; i++)
    {
      _aberrationEffects[i]      = glitchImages[i].GetComponent<AberrationEffect>();
      glitchImages[i].enabled    = false;
      _aberrationImageIndices[i] = i;
    }
    
    _aberrationChanges = aberrationChangesRange.Random();
    _aberrationAmounts = new float3[_aberrationChanges];
    _aberrationWaits   = new float[_aberrationChanges];

    for (int i = 0; i < _aberrationChanges; ++i) 
    {
      _aberrationAmounts[i] = aberrationAmountRange.Random();
      _aberrationWaits[i]   = aberrationWaitRange.Random();
    }
  }

  private IEnumerator Start()
  {
    // Set up aberration effects
    for (int i = 0; i < _aberrationEffects.Length; i++)
    {
      glitchImages[i].material = _aberrationEffects[i].GetMaterialInstance();
      _aberrationEffects[i].Setup();
    }

    while (true)
    {
      yield return CoroutineUtil.Wait(waitBetweenEffect.Random());

      ArrayUtil.Shuffle(_aberrationImageIndices, _aberrationImageIndices.Length);
      
      int imageCount = aberrationImageCount.Random();
      for (int i = 0; i < imageCount; i++)
      {
        int effectIndex = _aberrationImageIndices[i];
        StartCoroutine(ImageAberration(effectIndex));
      }
      
      Vector3 startPos = _rectTransform.localPosition;
      int textChanges  = textChangesRange.Random();
      for (int i = 0; i < textChanges; ++i)
      {
        Vector3 newPos = startPos + textMoveRange.Random().ToFloat3().ToVector();
        _rectTransform.localPosition = newPos;
      
        float wait = textMoveWaitRange.Random();
        yield return CoroutineUtil.Wait(wait);
      }
      
      _rectTransform.localPosition = startPos;
    }
  }

  private IEnumerator __ImageAberration(int index)
  {
    glitchImages[index].sprite  = _randomGlitchSprite;
    glitchImages[index].enabled = true;
    
    Vector3 startPos = glitchImages[index].rectTransform.localPosition;

    for (int i = 0; i < _aberrationChanges; ++i)
    {
      _aberrationEffects[index].SetAmount(_aberrationAmounts[0], _aberrationAmounts[1]);
      ArrayUtil.Shuffle(_aberrationAmounts, _aberrationChanges);
      
      Vector3 newPos = startPos + aberrationMoveRange.Random().ToFloat3().ToVector();
      glitchImages[index].rectTransform.localPosition = newPos;
      
      yield return CoroutineUtil.Wait(_aberrationWaits[i]);
    }
    
    glitchImages[index].enabled                     = false;
    glitchImages[index].rectTransform.localPosition = startPos;
  }
}
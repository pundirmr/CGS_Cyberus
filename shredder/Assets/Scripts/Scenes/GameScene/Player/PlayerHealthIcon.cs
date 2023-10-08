using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthIcon : MonoBehaviour
{
  [SerializeField] private Image healthIcon;
  [SerializeField] private Color fullHealthCol;
  [SerializeField] private Color emptyHealthCol;
  [Space]
  [SerializeField] private Image aberrationImage;
  [SerializeField] private AberrationEffect aberrationEffect;
  
  [Header("Animation Settings")]
  [SerializeField] private RangedInt aberrationChangesRange;
  [SerializeField] private float lerpColAnimTime = 0.25f;
  [SerializeField] private RangedFloat3 aberrationAmountRange;
  [SerializeField] private RangedFloat aberrationWaitRange;

  private bool _healthFull = false;
  private int _aberrationChanges;
  private float3[] _aberrationAmounts;
  private float[] _aberrationWaits;

  // Delegates
  private delegate IEnumerator LerpHeartColourDel(Color endCol);
  private LerpHeartColourDel LerpHeartColour;

  private DelegateUtil.EmptyCoroutineDel AnimateEmpty;
  private Coroutine _animCoroutine;

  private void Awake()
  {
    // Assign func delegates
    AnimateEmpty    = __AnimateEmpty;
    LerpHeartColour = __LerpHeartColour;
    
    // Set heart to full on startup
    SetFull();
    
    // Init settings
    GenerateStaticAberrationData();
  }

  private void Start()
  {
    aberrationImage.material = aberrationEffect.GetMaterialInstance();
  }
  
  private void GenerateStaticAberrationData()
  {
    _aberrationChanges = maths.Min(2, aberrationChangesRange.Random());
    
    _aberrationAmounts = new float3[_aberrationChanges];
    for (int i = 0; i < _aberrationChanges; i++)
    {
      _aberrationAmounts[i] = aberrationAmountRange.Random();
    }
    
    _aberrationWaits = new float[_aberrationChanges];
    for (int i = 0; i < _aberrationChanges; i++)
    {
      _aberrationWaits[i] = aberrationWaitRange.Random();
    }
  }

  [ContextMenu("set full")]
  public void SetFull()
  {
    if (_healthFull) return;
    healthIcon.color = fullHealthCol;
    _healthFull = true;
  }

  [ContextMenu("set empty")]
  public void SetEmpty()
  {
    if (!_healthFull) return;
    CoroutineUtil.StartSafelyWithRef(this, ref _animCoroutine, AnimateEmpty());
    _healthFull = false;
  }

  private IEnumerator __AnimateEmpty()
  {
    healthIcon.color = emptyHealthCol;
    
    aberrationImage.enabled = true;

    for (int i = 0; i < _aberrationChanges; i++)
    {
      aberrationEffect.SetAmount(_aberrationAmounts[i], _aberrationAmounts[i]);
      yield return CoroutineUtil.Wait(_aberrationWaits[i]);
    }
    
    aberrationImage.enabled = false;
    
    // NOTE(WSWhitehouse): Reshuffle arrays so its a different sequence next time
    ArrayUtil.Shuffle(_aberrationAmounts, _aberrationChanges);
    ArrayUtil.Shuffle(_aberrationWaits,   _aberrationChanges);
  }

  private IEnumerator __LerpHeartColour(Color endCol)
  {
    float time = 0;
    Color startFillAmount = healthIcon.color;
    while (time < lerpColAnimTime)
    {
      healthIcon.color = Colour.Lerp(startFillAmount, endCol, time / lerpColAnimTime);
      time += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
  }
}
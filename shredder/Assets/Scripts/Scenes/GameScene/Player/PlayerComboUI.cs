using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerComboUI : MonoBehaviour
{
  [Header("UI Elements")]
  [SerializeField] private PlayerID playerID;
  [SerializeField] private TMP_Text comboText;
  [Space]
  [SerializeField] private int maxComboNumber = 99;

  [Header("UI Elements Effects")]
  [SerializeField] private float uiScaleEffectPower  = 0.75f;
  [SerializeField] private float scaleEffectTime     = 1f;
  [SerializeField] private float scaleEffectWaitTime = 0.1f;
  [SerializeField] private float maxComboIntensity   = 3.0f;
    
  // NOTE(WSWhitehouse): Pre-allocating delegates in awake to increase performance when starting coroutines
  private delegate IEnumerator EffectCoroutineDelegate(Transform transform, float amount);
  private EffectCoroutineDelegate UpScaleEffectCo;
  private EffectCoroutineDelegate DownScaleEffectCo;
  private Coroutine _scaleEffectCo;
  
  private int _prevCombo = -1;
  private int _faceColourID;
  private Color _baseColour;
  private Vector3 _startScale;
  
  private void Awake()
  {
    // delegate allocations
    UpScaleEffectCo   = __UpScaleEffectCo;
    DownScaleEffectCo = __DownScaleEffectCo;
  
    comboText.text = "0";
        
    Shader shader = comboText.fontMaterial.shader;
    _faceColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_FaceColor"));
    _baseColour = comboText.fontMaterial.GetColor(_faceColourID);
    _startScale = transform.localScale;
  }
  
  private IEnumerator Start()
  {
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    playerID.PlayerData.OnComboUpdated += UpdateUI;
  }

  private void OnDestroy()
  {
    if (PlayerManager.PlayerData[playerID.ID] == null) return;
    playerID.PlayerData.OnComboUpdated -= UpdateUI;
  }

  private void UpdateUI()
  {
    int combo      = maths.Clamp(playerID.PlayerData.CurrentCombo, 0, maxComboNumber);
    comboText.text = StaticStrings.Nums[combo];
    
    if (combo < _prevCombo || combo == 0)
    {
      DownScaleEffect(comboText.transform, uiScaleEffectPower);
    }
    else if (combo > _prevCombo || combo == maxComboNumber)
    {
      UpScaleEffect(comboText.transform, uiScaleEffectPower);
    }
    
    _prevCombo = combo;
  }

  private void UpScaleEffect(Transform transform, float effectPower = 0.25f)
  {
    if (_scaleEffectCo != null) { StopCoroutine(_scaleEffectCo); }
    _scaleEffectCo = StartCoroutine(UpScaleEffectCo(transform, effectPower));
  }

  private void DownScaleEffect(Transform transform, float effectPower = 0.25f)
  {
    if (_scaleEffectCo != null) { StopCoroutine(_scaleEffectCo); }
    _scaleEffectCo = StartCoroutine(DownScaleEffectCo(transform, effectPower));
  }

  private IEnumerator __UpScaleEffectCo(Transform transform, float amount)
  {
    float combo = maths.Clamp(playerID.PlayerData.CurrentCombo, 0, maxComboNumber);
    float time  = 0;
    
    // NOTE(WSWhitehouse): We're doing something strange here to calculate the colours intensity (ask Felix).
    // The 400 value *should* be max combo but its unlikely anyone will reach that number so we have used something
    // lower instead.
    Color targetColour = _baseColour * maths.Clamp(maths.Pow2(combo / 400), 1.0f, maxComboIntensity);

    // set the target scale
    float3 scale = transform.localScale;
    scale.x += amount;
    scale.y += amount;
    scale.z += amount;

    // scale text up
    while (time < scaleEffectTime)
    {
      time   += Time.deltaTime;
      float t = time / scaleEffectTime;
      
      comboText.fontMaterial.SetColor(_faceColourID, Color.Lerp(_baseColour, targetColour, t));
      
      transform.localScale = float3Util.Lerp(_startScale, scale, t);
      yield return CoroutineUtil.WaitForUpdate;
    }

    transform.localScale = scale;
    
    // we wait before we scale back down again    
    yield return CoroutineUtil.Wait(scaleEffectWaitTime);

    // scale text down
    time = 0;
    while (time < scaleEffectTime)
    {
      time   += Time.deltaTime;
      float t = time / scaleEffectTime;
      
      comboText.fontMaterial.SetColor(_faceColourID, Color.Lerp(targetColour, _baseColour, t));
      
      transform.localScale = float3Util.Lerp(scale, _startScale, t);
      yield return CoroutineUtil.WaitForUpdate;
    }

    transform.localScale = _startScale;
  }

  private IEnumerator __DownScaleEffectCo(Transform transform, float amount)
  {
    float3 scale = transform.localScale;
    scale.x -= amount;
    scale.y -= amount;
    scale.z -= amount;
    yield return LerpUtil.LerpScale(transform, scale, scaleEffectTime);
    yield return CoroutineUtil.Wait(scaleEffectWaitTime);
    yield return LerpUtil.LerpScale(transform, Vector3.one, scaleEffectTime);
  }
}

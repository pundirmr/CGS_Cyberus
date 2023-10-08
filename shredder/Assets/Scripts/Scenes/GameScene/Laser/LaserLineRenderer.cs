using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// This is the line renderer that shows the laser on screen.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserLineRenderer : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Laser laser;
  [SerializeField] private Vector3 laserStartPos;
  [SerializeField] private Vector3 laserEndPos;
  
  [Header("Laser Renderer Values")]
  // [SerializeField] private Material laserMaterialTemplate;
  [SerializeField] private LaserEffect laserEffect;
  [SerializeField] private float laserFireDuration;
  [SerializeField] private int lineRendererStartPointIndex = 0;
  [SerializeField] private int lineRendererEndPointIndex = 1;
  
  public bool IsLaserActive => _lineRenderer.enabled;

  private float3 _startPointPosition
  {
    get => _lineRenderer.GetPosition(lineRendererStartPointIndex);
    set => _lineRenderer.SetPosition(lineRendererStartPointIndex, value);
  }
  
  private float3 _endPointPosition
  {
    get => _lineRenderer.GetPosition(lineRendererEndPointIndex);
    set => _lineRenderer.SetPosition(lineRendererEndPointIndex, value);
  }
  
  private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
  
  private LineRenderer _lineRenderer;
  private Coroutine _laserAnimCoroutine;
  // private Material _laserMaterial;
  
  // Assigning func ptr on start up to reduce delegate allocations
  private delegate IEnumerator LaserAnimDel();
  private LaserAnimDel _showLaserAnim;
  private LaserAnimDel _hideLaserAnim;

  private void Awake()
  {
    _lineRenderer = GetComponent<LineRenderer>();
    _lineRenderer.useWorldSpace = false;
    _showLaserAnim = ShowLaserAnim;
    _hideLaserAnim = HideLaserAnim;
    
    // Set up line renderer
    _startPointPosition   = laserStartPos;
    _endPointPosition     = laserStartPos;
    _lineRenderer.enabled = false;
    
    // Set up material
    // _laserMaterial = new Material(laserMaterialTemplate);
    _lineRenderer.material = laserEffect.LaserEffectMaterial;
    
    laser.OnLaserFireStarted += FireLaser;
    laser.OnLaserFireStopped += HideLaser;
  }

  private void OnDestroy()
  {
    laser.OnLaserFireStarted -= FireLaser;
    laser.OnLaserFireStopped -= HideLaser;
  }

  // private void ChangeLaserMaterialColour(Color colour)
  // {
  //   _laserMaterial.color = colour;
  //   _laserMaterial.SetColor(Emission, colour);
  // }

  private void FireLaser(Laser.LaserInfo info)
  {
    // ChangeLaserMaterialColour(info.colour);
    _lineRenderer.enabled = true;
    CoroutineUtil.StartSafelyWithRef(this, ref _laserAnimCoroutine, _showLaserAnim());
  }

  private void HideLaser(Laser.LaserInfo info)
  {
    CoroutineUtil.StartSafelyWithRef(this, ref _laserAnimCoroutine, _hideLaserAnim());
  }
  
  private IEnumerator ShowLaserAnim()
  {
    _lineRenderer.enabled = true;
    
    float timeElapsed = 0.0f;
    float3 startValue = _endPointPosition;

    while (timeElapsed < laserFireDuration)
    {
      _endPointPosition = float3Util.Lerp(startValue, laserEndPos, timeElapsed / laserFireDuration);
      timeElapsed   += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    _endPointPosition = laserEndPos;
    _laserAnimCoroutine = null;
  }
  
  private IEnumerator HideLaserAnim()
  {
    float timeElapsed = 0.0f;
    float3 startValue = _endPointPosition;

    while (timeElapsed < laserFireDuration)
    {
      _endPointPosition = float3Util.Lerp(startValue, laserStartPos, timeElapsed / laserFireDuration);
      timeElapsed   += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    _endPointPosition = laserStartPos;
    _laserAnimCoroutine = null;
    _lineRenderer.enabled = false;
  }
}
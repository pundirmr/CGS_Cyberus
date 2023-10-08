using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public class TimingTextColour
{
  public LaserHitTiming Timing;
  [ColorUsage(false, true)] public Color Colour;
}

public class LaserTimingText : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Laser laser;
  [SerializeField] private ColourLane[] colourLanes;

  [Header("Text")]
  [SerializeField] private float duration;
  [SerializeField] private Transform startTransform;
  [SerializeField] private float verticalSpeed = 1.0f;
  
  [Header("Timing Colours")]
  [SerializeField] private TimingTextColour[] timingTextColours;
  
  private List<Vector3> _textSpawnPositions;

  // Shader Variable IDs
  private int _faceColourID;
  private int _glowColourID;

  // NOTE(WSWhitehouse): Caching timing text function to reduce runtime delegate alloc
  private delegate IEnumerator TimingTextDel(LaserTimingTextPool.TextInstance instance, int lane);
  private TimingTextDel _timingTextFunc;

  private void Awake()
  {
    // Cache TimingText function
    _timingTextFunc = TimingText;
    
    // Cache spawn positions
    int spawnPosCount   = colourLanes.Length;
    _textSpawnPositions = new List<Vector3>(spawnPosCount);
    Vector3 spawnPos    = startTransform.localPosition; // BUG(Zack): the position is not being set correctly, we have an offset of -1.5 in the y direction for some reason
    
    for (int i = 0; i < spawnPosCount; i++)
    {
      // NOTE(WSWhitehouse): Using local position here as we want the local
      // offset of the sub-lane within the players lane
      Vector3 spawnOffset = colourLanes[i].transform.position;
      
      Vector3 pos = new Vector3(spawnOffset.x, spawnPos.y, spawnOffset.z);
      _textSpawnPositions.Add(pos);
    }
    
    // Subscribe to laser events
    laser.OnLaserHit  += OnLaserHit;
    laser.OnLaserMiss += OnLaserHit;
  }

  private void Start()
  {
    var inst = LaserTimingTextPool.GetText(LaserHitTiming.INVALID);
    Shader shader = inst.Text.fontMaterial.shader;
    _faceColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_FaceColor"));
    _glowColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_GlowColor"));
  }

  private void OnDestroy()
  {
    laser.OnLaserHit  -= OnLaserHit;
    laser.OnLaserMiss -= OnLaserHit;
  }

  private void OnLaserHit(Laser.LaserHitInfo info)
  {
    // NOTE(WSWhitehouse): Starting the coroutine on the text instance to ensure it continues
    // if this object gets disabled.
    LaserTimingTextPool.TextInstance instance = LaserTimingTextPool.GetText(info.Timing);
    var colour = GetTextColourFromTiming(info);
    colour.a = 1;
    instance.Text.fontMaterial.SetColor(_faceColourID, colour);
    instance.Text.fontMaterial.SetColor(_glowColourID, colour);
    instance.Text.StartCoroutine(_timingTextFunc(instance, info.WordBlock.ColourIndex));
  }
  
  private Color GetTextColourFromTiming(Laser.LaserHitInfo info)
  {
    if (info.Timing == LaserHitTiming.WRONG_COLOUR)
    {
      return info.WordBlock.Colour;
    }
    
    foreach (TimingTextColour timingTextColour in timingTextColours)
    {
      if (timingTextColour.Timing == info.Timing)
      {
        return timingTextColour.Colour;
      }
    }
    return Color.white;
  }

  private IEnumerator TimingText(LaserTimingTextPool.TextInstance instance, int lane)
  {
    // BUG(Zack): offset of -1.5 in the y position for some reason for the spawn point
    instance.Canvas.enabled     = true;
    instance.Transform.position = _textSpawnPositions[lane];
    instance.Transform.rotation = Quaternion.identity;
    
    Vector3 startPos = _textSpawnPositions[lane];
    float timer      = 0.0f;
    while (true)
    {
      timer += Time.deltaTime;
      if (timer >= duration) break;
      
      // Update instance position
      float newYPos = instance.Transform.position.y - (verticalSpeed * Time.deltaTime);

      instance.Transform.position = new Vector3(startPos.x, newYPos, startPos.z);

      yield return CoroutineUtil.WaitForUpdate;
    }
    
    LaserTimingTextPool.ReturnToPool(instance);
  }
}

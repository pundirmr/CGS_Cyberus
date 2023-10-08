using System;
using Unity.Mathematics;
using UnityEngine;

public class NoteLine : MonoBehaviour
{
  public SpriteRenderer image;
  
  [Header("Colour")]
  public Color inactiveCol = new Color(1.0f, 1.0f, 1.0f, 0.0f);

  [Header("Beat Type Colours")]
  public Color mainBeatColour = Color.white;
  public Color defaultColour  = Color.grey;
  
  [Header("Size")]
  public float2 mainBeatSize;
  public float2 defaultSize;

  [NonSerialized] public Color activeCol   = Color.white;
}

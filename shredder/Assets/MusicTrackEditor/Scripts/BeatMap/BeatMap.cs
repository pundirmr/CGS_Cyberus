using System;
using UnityEngine;

public class BeatMap : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private RectTransform scrollContent;
  
  public float contentStartOffset = 20f;
  public float minusContentStartOffset => -maths.Abs(contentStartOffset);
  public float beatOffset = 5f;

  public float noteOffset => beatOffset * ViewScale;

  [Header("Scale")] 
  [SerializeField] private float viewScale = 3f;
  [SerializeField] private RangedFloat viewScaleClamp = new RangedFloat(1.25f, 5f);

  public float ViewScale
  {
    get => viewScale;
    set => viewScale = viewScaleClamp.Clamp(value);
  }

  public float LaneHeight
  {
    get
    {
      float height = contentStartOffset;
      height += noteOffset * TrackEditor.MusicTrack.TotalNotesInTrack;
      return height;
    }
  }

  private void Awake()
  {
    // Make sure to clamp view scale
    ViewScale = viewScale;
  }

  private void Update()
  {
    scrollContent.SetHeight(LaneHeight);
  }
}
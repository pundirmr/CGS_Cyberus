using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class Timeline : MonoBehaviour, IPointerDownHandler
{
  [SerializeField] private BeatMap beatMap;
  [SerializeField] private ConfigurableScrollRect scrollRect;
  
  [Header("Timeline")]
  [SerializeField] private RectTransform timeline;
  [SerializeField] private TimeMarker timeMarkerPrefab;
  [SerializeField] private int timeIntervals = 10;
  
  [Header("Play Head")]
  [SerializeField] private PlayHead playHead;
  [SerializeField] private PlayMarker playMarker;
  
  private List<TimeMarker> _timeMarkers;

  private void Awake()
  {
    TrackEditor.OnMusicTrackChanged += OnMusicTrackChanged;
  }

  private void OnDestroy()
  {
    TrackEditor.OnMusicTrackChanged -= OnMusicTrackChanged;
  }

  private void OnMusicTrackChanged()
  {
    ClearTimeline(); 
    
    // NOTE(WSWhitehouse): Don't want to recreate timeline if track is null
    if (TrackEditor.MusicTrack == null) return;
    
    CreateTimeline();
  }

  private void CreateTimeline()
  {
    int timeMarkersCount = (int)maths.Ceil(TrackEditor.MusicTrack.TrackDuration * (float)timeIntervals);
    _timeMarkers         = new List<TimeMarker>(timeMarkersCount);
    
    // NOTE(WSWhitehouse): Using decimal type here rather than floating point so I dont have to deal with
    // rounding errors. Using decimal is slightly less memory efficient, but that shouldn't matter here...
    decimal timeOffset = 1.0m / (decimal)timeIntervals;
    decimal time       = 0.0m;
    for (int i = 0; i < timeMarkersCount; i++)
    {
      TimeMarker marker = Instantiate(timeMarkerPrefab, timeline);
      marker.Init(time);
      _timeMarkers.Add(marker);
      time += timeOffset;
    }
    
    ScaleTimeline();
  }

  private void ClearTimeline()
  {
    if (_timeMarkers == null) return;
    
    foreach (TimeMarker timeMarker in _timeMarkers)
    {
      Destroy(timeMarker.gameObject);
    }
    
    _timeMarkers = null;
  }

  private void ScaleTimeline()
  {
    if (_timeMarkers == null) return;
    
    float yPos             = beatMap.minusContentStartOffset;
    float timeOffset       = 1.0f / (float)timeIntervals;
    float timeMarkerOffset = (beatMap.LaneHeight / TrackEditor.MusicTrack.TrackDuration) * timeOffset;
    for (int i = 0; i < _timeMarkers.Count; i++)
    {
      TimeMarker marker       = _timeMarkers[i];
      marker.RectTransform.localPosition = new Vector3(0.0f, yPos, 0.0f);
      yPos -= timeMarkerOffset;
    }
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    UpdateScrollRectDrag(false);
    
    if (eventData.button == PointerEventData.InputButton.Left)
    {
      playHead.pointerDown = true;
    }

    if (eventData.button == PointerEventData.InputButton.Right)
    {
      playMarker.pointerDown = true;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void UpdateScrollRectDrag(bool value)
  {
    if (Tool.Mode != ToolMode.MOVE) return;
    scrollRect.enableDragging = value;
  }
}
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayHead : MonoBehaviour, IPointerDownHandler
{
  private RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private BeatMap beatMap;
  [SerializeField] private RectTransform content;
  [SerializeField] private ConfigurableScrollRect scrollRect;
  
  [NonSerialized] public bool pointerDown = false;
  
  private void Update()
  {
    MoveToCursor();

    float songPercentage = maths.Clamp((float)MusicTrackPlayerEditor.TrackPlaybackTime, 0.0f, TrackEditor.MusicTrack.TrackDuration) / TrackEditor.MusicTrack.TrackDuration;
      
    float x = RectTransform.localPosition.x;
    float y = -maths.Abs((beatMap.LaneHeight - beatMap.contentStartOffset) * songPercentage);
    float z = RectTransform.localPosition.z;
    RectTransform.localPosition = new Vector3(x, y - beatMap.contentStartOffset, z);
  }

  private void MoveToCursor()
  {
    if (!pointerDown) return;
    
    // NOTE(WSWhitehouse): Force the pointer down to be false if the button is released or the track has started playing
    if (Mouse.current.leftButton.wasReleasedThisFrame || TrackEditor.IsPlayingTrack)
    {
      pointerDown = false;
      UpdateScrollRectDrag(true);
      return;
    }
      
    RectTransformUtility.ScreenPointToLocalPointInRectangle(content, Mouse.current.position.ReadValue(), null, out Vector2 canvasSpacePos);
    float heightPercentage                   = maths.Clamp((canvasSpacePos.y + beatMap.contentStartOffset) / beatMap.LaneHeight, -1.0f, 0.0f);
    MusicTrackPlayerEditor.TrackPlaybackTime = maths.Abs(TrackEditor.MusicTrack.TrackDuration * heightPercentage);
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if (TrackEditor.IsPlayingTrack) return;
    if (eventData.button != PointerEventData.InputButton.Left) return;
    pointerDown = true;
    UpdateScrollRectDrag(false);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void UpdateScrollRectDrag(bool value)
  {
    if (Tool.Mode != ToolMode.MOVE) return;
    scrollRect.enableDragging = value;
  }
}
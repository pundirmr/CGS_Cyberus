using System;
using UnityEngine;

public class PlaybackSpeedSlider : MonoBehaviour
{
  [SerializeField] private float playbackSpeed = MusicTrackPlayerEditor.PlaybackSpeedDefault;

  private void Update()
  {
    if (maths.FloatCompare(playbackSpeed, MusicTrackPlayerEditor.PlaybackSpeed, 0.0001f)) return;
    
    MusicTrackPlayerEditor.PlaybackSpeed = playbackSpeed;
  }
}
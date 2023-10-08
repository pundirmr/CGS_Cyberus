using System;
using TMPro;
using UnityEngine;

public class PlaybackTime : MonoBehaviour
{
  [SerializeField] private TMP_Text currentTime;
  [SerializeField] private TMP_Text totalTime;

  private const string TIME_FORMAT = "{0}:{1}:{2}";

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
    if (TrackEditor.MusicTrack == null)
    {
      totalTime.text = "00:00:000";
      return;
    }
    
    TimeSpan span  = TimeSpan.FromSeconds(TrackEditor.MusicTrack.TrackDuration);
    totalTime.text = $"{span.Minutes:00}:{span.Seconds:00}:{span.Milliseconds:000}";
  }

  private void Update()
  {
    if (TrackEditor.MusicTrack == null)
    {
      currentTime.text = "00:00:000";
      return;
    }
    
    TimeSpan span    = TimeSpan.FromSeconds(MusicTrackPlayerEditor.TrackPlaybackTime);
    currentTime.text = string.Format(TIME_FORMAT, span.Minutes.ToString("00"), span.Seconds.ToString("00"),span.Milliseconds.ToString("000"));
  }
}
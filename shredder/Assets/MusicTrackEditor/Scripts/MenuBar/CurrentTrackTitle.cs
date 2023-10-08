using TMPro;
using UnityEngine;

public class CurrentTrackTitle : MonoBehaviour
{
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private string noValidTrackText;
  
  private const string UnknownStr = "UNKNOWN";
  
  private void Awake()
  {
    TrackEditor.OnMusicTrackChanged += OnMusicTrackChanged;
    OnMusicTrackChanged();
  }

  private void OnDestroy()
  {
    TrackEditor.OnMusicTrackChanged -= OnMusicTrackChanged;
  }

  private void OnMusicTrackChanged()
  {
    if (TrackEditor.MusicTrack == null)
    {
      titleText.text = noValidTrackText;
      return;
    }
    
    // NOTE(WSWhitehouse): Checking for no name or artist and replacing with unknown
    string name    = string.IsNullOrEmpty(TrackEditor.MusicTrack.TrackName)   ? UnknownStr : TrackEditor.MusicTrack.TrackName;
    string artist  = string.IsNullOrEmpty(TrackEditor.MusicTrack.TrackArtist) ? UnknownStr : TrackEditor.MusicTrack.TrackArtist;
    titleText.text = $"{name} by {artist}";
  }
}
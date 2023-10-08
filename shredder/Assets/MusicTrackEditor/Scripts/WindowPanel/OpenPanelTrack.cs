using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenPanelTrack : MonoBehaviour
{
  [Header("Track Data")]
  [SerializeField] private TMP_Text trackNumber;
  [SerializeField] private Image trackIcon;
  [SerializeField] private TMP_Text trackName;
  [SerializeField] private TMP_Text artistName;
  [SerializeField] private TMP_Text trackDuration;
  
  [Header("Button")]
  [SerializeField] private Button button;

  private Color _defaultCol;
  private Color _selectedCol;
  private int _trackNumber;
  
  public const int INVALID_TRACK_INDEX     = -1;
  public static int CurrentlySelectedTrack = INVALID_TRACK_INDEX;
  public static Action OnCurrentlySelectedTrackUpdated;


  public void Create(int trackIndex, MusicTrack track)
  {
    _trackNumber = trackIndex;
    
    // NOTE(WSWhitehouse): Adding 1 so the track number starts at 1 not 0
    trackNumber.text = (_trackNumber + 1).ToString("00");
    trackIcon.sprite = track.Sprite;
    trackName.text   = track.TrackName;
    artistName.text  = track.TrackArtist;
    
    int min, sec;
    maths.TimeToMinutesAndSeconds(track.TrackDuration, out min, out sec);
    trackDuration.text = $"{min:00}:{sec:00}";
    
    _defaultCol  = button.colors.normalColor;
    _selectedCol = button.colors.pressedColor;
    
    OnCurrentlySelectedTrackUpdated += UpdateSelectedTrackUI;
    button.onClick.AddListener(OnButtonPressed);
  }

  private void OnDestroy()
  {
    OnCurrentlySelectedTrackUpdated -= UpdateSelectedTrackUI;
    button.onClick.RemoveListener(OnButtonPressed);
  }

  private void UpdateSelectedTrackUI()
  {
    ColorBlock colBlock = new ColorBlock()
    {
      colorMultiplier  = button.colors.colorMultiplier,
      disabledColor    = button.colors.disabledColor,
      fadeDuration     = button.colors.fadeDuration,
      highlightedColor = button.colors.highlightedColor,
      normalColor      = CurrentlySelectedTrack == _trackNumber ? _selectedCol : _defaultCol,
      pressedColor     = button.colors.pressedColor,
      selectedColor    = button.colors.selectedColor
    };
    
    button.colors = colBlock;
  }

  private void OnButtonPressed()
  {
    if (CurrentlySelectedTrack == _trackNumber)
    {
      CurrentlySelectedTrack = INVALID_TRACK_INDEX;
      OnCurrentlySelectedTrackUpdated?.Invoke();
      return;
    }
    
    CurrentlySelectedTrack = _trackNumber;
    OnCurrentlySelectedTrackUpdated?.Invoke();
  }
}
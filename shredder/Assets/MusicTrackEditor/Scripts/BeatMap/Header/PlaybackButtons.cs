using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaybackButtons : MonoBehaviour
{
  [Header("Play / Pause Button")]
  [SerializeField] private Button playPauseButton;
  [SerializeField] private Image playPauseImage;
  [SerializeField] private Sprite pauseSprite;
  [SerializeField] private Sprite playSprite;
  
  [Header("To Start Button")]
  [SerializeField] private Button toStartButton;
  
  [Header("To Marker Button")]
  [SerializeField] private Button toMarkerButton;

  private void Awake()
  {
    TrackEditor.OnPlayStateChanged += OnPlayStateChanged;
    playPauseButton.onClick.AddListener(OnPlay); 
    toStartButton.onClick.AddListener(OnToStart); 
    toMarkerButton.onClick.AddListener(OnToMarker); 
  }

  private void OnDestroy()
  {
    TrackEditor.OnPlayStateChanged -= OnPlayStateChanged;
    playPauseButton.onClick.RemoveListener(OnPlay); 
    toStartButton.onClick.RemoveListener(OnToStart); 
    toMarkerButton.onClick.RemoveListener(OnToMarker); 
  }
  
  private void OnPlayStateChanged()
  {
    playPauseImage.sprite = TrackEditor.IsPlayingTrack ? pauseSprite : playSprite;
  }
  
  private void OnPlay()
  {
    TrackEditor.IsPlayingTrack = !TrackEditor.IsPlayingTrack;
  }

  private void OnToStart()
  {
    SetPlaybackTimeWithoutInterruption(0.0);
    
    if (Keyboard.current.shiftKey.isPressed)
    {
      PlayMarker.Time = 0.0;
    }
  }

  private void OnToMarker()
  {
    SetPlaybackTimeWithoutInterruption(PlayMarker.Time);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void SetPlaybackTimeWithoutInterruption(double time)
  {
    if (TrackEditor.IsPlayingTrack)
    {
      TrackEditor.IsPlayingTrack               = false;
      MusicTrackPlayerEditor.TrackPlaybackTime = time;
      TrackEditor.IsPlayingTrack               = true;
    }
    else
    {
      MusicTrackPlayerEditor.TrackPlaybackTime = time;
    }
  }
  
  private void Update()
  {
    if (Keyboard.current.spaceKey.wasPressedThisFrame)
    {
      if (!TrackEditor.IsPlayingTrack && Keyboard.current.shiftKey.isPressed)
      {
        MusicTrackPlayerEditor.TrackPlaybackTime = PlayMarker.Time;
      }
      
      TrackEditor.IsPlayingTrack = !TrackEditor.IsPlayingTrack;
    }
  }
}
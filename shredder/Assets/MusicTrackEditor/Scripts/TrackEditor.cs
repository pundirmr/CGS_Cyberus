using System;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackEditor : MonoBehaviour
{
  public MusicTrack track;
  
  [SerializeField] private bool convertBeatDifficulties;

  public static Action OnMusicTrackChanged;
  
  private static MusicTrack _musicTrack;
  public static MusicTrack MusicTrack
  {
    get => _musicTrack;
    set
    {
      if (_musicTrack == value) return;
      
#if UNITY_EDITOR
      if (_musicTrack != null)
      {
        EditorUtility.SetDirty(_musicTrack);
        AssetDatabase.SaveAssetIfDirty(_musicTrack);
      }
#endif
      
      IsPlayingTrack                           = false;
      MusicTrackPlayerEditor.TrackPlaybackTime = 0.0;
      _musicTrack                              = value;
      if (_musicTrack != null) MusicTrack.SetUpBeatMap();
      OnMusicTrackChanged?.Invoke();
    }
  }

  public static Action OnPlayStateChanged;
  
  private static bool _isPlayingTrack = false;
  public static bool IsPlayingTrack
  {
    get => _isPlayingTrack;
    set
    {
      if (MusicTrack == null) return;
      if (_isPlayingTrack == value) return;
      
      _isPlayingTrack = value;
      OnPlayStateChanged?.Invoke();

      if (_isPlayingTrack)
      {
        MusicTrackPlayerEditor.Play();
      }
      else
      {
        MusicTrackPlayerEditor.Stop();
      }
    }
  }
  
  public static Action OnCursorDifficultyChanged;
  
  private static NoteDifficulty _cursorDifficulty = NoteDifficulty.EASY;
  public static NoteDifficulty CursorDifficulty
  {
    get => _cursorDifficulty;
    set
    {
      if (_cursorDifficulty == value) return;
      _cursorDifficulty = value;
      OnCursorDifficultyChanged?.Invoke();
    }
  }
  
  public delegate void OnBeatMapUpdatedDelegate();
  public static OnBeatMapUpdatedDelegate OnBeatMapUpdated; 
  
  private void Start()
  {
    MusicTrack = track;
  }

  private void OnDestroy()
  {
    MusicTrack = null;
  }

  private void Update()
  {
    if (!convertBeatDifficulties) return;
    convertBeatDifficulties = false;

    for (int i = 0; i < MusicTrack.BeatMap.Length; i++)
    {
      for (int j = 0; j < MusicTrack.BeatMap[i].Length; j++)
      {
        MusicTrack.BeatMap[i][j].Difficulty = ((int)MusicTrack.BeatMap[i][j].Difficulty) switch
        {
          0 => NoteDifficulty.HARD | NoteDifficulty.MEDIUM | NoteDifficulty.EASY,
          1 => NoteDifficulty.HARD | NoteDifficulty.MEDIUM,
          2 => NoteDifficulty.HARD,
          _ => MusicTrack.BeatMap[i][j].Difficulty
        };
      }
    }
    
    OnBeatMapUpdated?.Invoke();
  }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackEditorKeyboardControls : MonoBehaviour
{
  private TrackEditorInput input;

  private void Awake()
  {
    input = new TrackEditorInput();
    input.Enable();
    
    input.Track.ToggleLane01Beat.performed += OnToggleLane01BeatPerformed;
    input.Track.ToggleLane02Beat.performed += OnToggleLane02BeatPerformed;
    input.Track.ToggleLane03Beat.performed += OnToggleLane03BeatPerformed;
  }

  private void OnDestroy()
  {
    input.Track.ToggleLane01Beat.performed -= OnToggleLane01BeatPerformed;
    input.Track.ToggleLane02Beat.performed -= OnToggleLane02BeatPerformed;
    input.Track.ToggleLane03Beat.performed -= OnToggleLane03BeatPerformed;
  }

  private void OnToggleLane01BeatPerformed(InputAction.CallbackContext obj) => OnToggleLaneBeatPerformed(0);
  private void OnToggleLane02BeatPerformed(InputAction.CallbackContext obj) => OnToggleLaneBeatPerformed(1);
  private void OnToggleLane03BeatPerformed(InputAction.CallbackContext obj) => OnToggleLaneBeatPerformed(2);

  private void OnToggleLaneBeatPerformed(int laneIndex)
  {
    if (TrackEditor.MusicTrack == null) return;

    int noteIndex = MusicTrackPlayerEditor.CurrentNoteIndex;

    // NOTE(WSWhitehouse): Doing this check here so you can only disable a beat if its the same difficulty as the 
    // cursor. If its not, we force the beat to true and set it to the current cursor difficulty.
    if (TrackEditor.MusicTrack.BeatMap[laneIndex][noteIndex].Difficulty == TrackEditor.CursorDifficulty)
    {
      TrackEditor.MusicTrack.BeatMap[laneIndex][noteIndex].IsActive = !TrackEditor.MusicTrack.BeatMap[laneIndex][noteIndex].IsActive;
    }
    else
    {
      TrackEditor.MusicTrack.BeatMap[laneIndex][noteIndex].IsActive   = true;
      TrackEditor.MusicTrack.BeatMap[laneIndex][noteIndex].Difficulty = TrackEditor.CursorDifficulty;
    }

    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
}
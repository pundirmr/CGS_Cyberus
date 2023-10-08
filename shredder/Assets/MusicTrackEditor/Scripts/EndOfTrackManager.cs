public static class EndOfTrackManager
{
  private static MusicTrack MusicTrack => TrackEditor.MusicTrack;
  private static BeatMapLane[] BeatMap => MusicTrack.BeatMap;
  
  public static void SetSelectedBeatAsEndOfTrack()
  {
    if (MusicTrack == null)            return;
    if (Beat.SelectedBeats.Count <= 0) return;
    
    RemoveEndOfTrack();
    Beat.SortSelectedBeats();
    
    int noteIndex = Beat.SelectedBeats[0].Note.NoteIndex;
    for (int i = 0; i < MusicTrack.LaneCount; i++)
    {
      BeatMap[i].Notes[noteIndex].IsEndOfTrack = true;
    }
    
    TrackEditor.OnBeatMapUpdated?.Invoke();
  }

  public static void RemoveEndOfTrack()
  {
    if (MusicTrack == null) return;

    foreach (Beat beat in Beat.Beats)
    {
      beat.Note.IsEndOfTrack = false;
    }
    
    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
}
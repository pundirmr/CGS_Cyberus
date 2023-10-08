using System;
using UnityEngine;


public class TrackEditorNoteOnEveryBeat : MonoBehaviour
{
  [SerializeField] private bool go;

  private void Update()
  {
    if (!go) return;
    go = false;
    
    
    int numOfNotesToMove = (int)maths.Ceil(TrackEditor.MusicTrack.LaneDuration / TrackEditor.MusicTrack.SecondsPerNote);
    
    for (int i = 0; i < TrackEditor.MusicTrack.BeatMap.Length; i++)
    {
      for (int j = TrackEditor.MusicTrack.BeatMap[i].Notes.Length - 1; j >= 0; j--)
      {
        int index = j - numOfNotesToMove;
        if (index < 0) continue;
        if (index >= TrackEditor.MusicTrack.BeatMap[i].Notes.Length) continue;
        
        TrackEditor.MusicTrack.BeatMap[i].Notes[j] = TrackEditor.MusicTrack.BeatMap[i].Notes[index];
      }
    
    }

    // for (int i = 0; i < TrackEditor.MusicTrack.BeatMap[1].Notes.Length; i++)
    // {
    //   TrackEditor.MusicTrack.BeatMap[1].Notes[i] = i % 4 == 0;
    // }

    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
}
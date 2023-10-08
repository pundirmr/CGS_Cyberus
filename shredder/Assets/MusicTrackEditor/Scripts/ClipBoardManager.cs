using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ClipBoardManager
{
  public class EditableBeatMapLane
  {
    public const int InitialCapacity = 30;
    public List<BeatMapNote> Notes   = new List<BeatMapNote>(InitialCapacity);
    public int Length                => Notes.Count;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Notes.Clear();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(BeatMapNote note) => Notes.Add(note);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, BeatMapNote note) => Notes.Insert(index, note);
  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertNull(int index) => Insert(index, null);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(BeatMapNote note) => Notes.Remove(note);
  
    // NOTE(WSWhitehouse): Overloading [] operator for easy access to 
    // note array. Mainly used in the BeatMap as a simple 2D array.
    public BeatMapNote this[int i]
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Notes[i];
      
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Notes[i] = value;
    }
  }
  
  public static List<EditableBeatMapLane> ClipBoard;

  static ClipBoardManager()
  {
    // Create beat map lane array
    ClipBoard = new List<EditableBeatMapLane>(MusicTrack.LaneCount);

    // Crate each lane
    // for (int i = 0; i < ClipBoard.Capacity; i++)
    // {
    //   ClipBoard[i] = new EditableBeatMapLane();
    // }
  }

  public static void Copy()
  {
    ClearClipBoard();
    
    // Add all selected beats to clipboard
    for (int i = 0; i < Beat.SelectedBeatCount; i++)
    {
      Beat beat = Beat.SelectedBeats[i];
      ClipBoard[beat.LaneIndex].Add(beat.Note.GetNewDeepCopy());
    }
    
    // Sort added beats and calculate how many lanes are empty
    int startEmptyLanes = 0;
    bool foundLane = false;
    for (int i = 0; i < ClipBoard.Count; i++)
    {
      if (ClipBoard[i].Length <= 0)
      {
        if (!foundLane) startEmptyLanes++;
        continue;
      }
      
      foundLane = true;
      SortClipBoardLane(i);
    }
    
    int endEmptyLanes = 0; 
    foundLane = false;
    for (int i = ClipBoard.Count - 1; i >= 0; i--)
    {
      if (ClipBoard[i].Length <= 0)
      {
        if (!foundLane) endEmptyLanes++;
        continue;
      }
      
      foundLane = true;
    }

    for (int i = 0; i < startEmptyLanes; i++)
    {
      ClipBoard.RemoveAt(0);
    }
    
    for (int i = 0; i < endEmptyLanes; i++)
    {
      ClipBoard.RemoveAt(ClipBoard.Count - 1);
    }
    
    return;
  }

  private static BeatMapLane[] BeatMap
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => TrackEditor.MusicTrack.BeatMap;
  }

  public static void Paste()
  {
    if (TrackEditor.MusicTrack == null) return;
    
    // Get first selected beat - this is considered the top beat on the left-most lane 
    Beat beat = GetFirstSelectedBeat();
    if (beat == null) return;

    // Paste clipboard
    for (int i = 0; i < ClipBoard.Count; i++)
    {
      int laneIndex = beat.LaneIndex + i;
      if (laneIndex >= MusicTrack.LaneCount) break;

      for (int j = 0; j < ClipBoard[i].Length; j++)
      {
        int noteIndex = beat.NoteNumber + j;
        if (noteIndex >= BeatMap[laneIndex].Length) break;
        
        BeatMapNote note = ClipBoard[i].Notes[j];
        if (note == null) continue;
        
        note.CopyTo(BeatMap[laneIndex].Notes[noteIndex]);
      }
    }
    
    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void ClearClipBoard()
  {
    ClipBoard.Clear();

    for (int i = 0; i < MusicTrack.LaneCount; i++)
    {
      ClipBoard.Add(new EditableBeatMapLane());
    }
  }
  
  // NOTE(WSWhitehouse): A small little struct used to insert null elements in the SortClipBoardLane func
  private struct InsertNullElements
  {
    public InsertNullElements(int index, int amount)
    {
      this.index  = index;
      this.amount = amount;
    }
    
    public int index;
    public int amount;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void SortClipBoardLane(int beatMapIndex)
  {
    EditableBeatMapLane beatMap = ClipBoard[beatMapIndex];
    
    // Put notes in order
    for (int i = 0; i < beatMap.Length; i++)
    {
      for (int j = i + 1; j < beatMap.Length; j++)
      {
        if (beatMap[i].NoteIndex > beatMap[j].NoteIndex)
        {
          ArrayUtil.Swap(beatMap.Notes, i, j);
        }
      }
    }
    
    // Calculate the null elements 
    List<InsertNullElements> nullElements = new List<InsertNullElements>(beatMap.Length);
    for (int i = 0; i < beatMap.Length - 1; i++)
    {
      BeatMapNote thisNote = beatMap[i];
      BeatMapNote nextNote = beatMap[i + 1];
      
      // NOTE(WSWhitehouse): Minus 1 from diff so it starts at 0 rather than 1
      int noteDiff = nextNote.NoteIndex - thisNote.NoteIndex - 1;
      if (noteDiff <= 0) continue;
      
      // NOTE(WSWhitehouse): Adding 1 to index as we want to insert at the index after this one
      nullElements.Add(new InsertNullElements(i + 1, noteDiff));
    }

    // Insert null elements
    int totalInserted = 0;
    foreach (InsertNullElements nullElement in nullElements)
    {
      for (int j = 0; j < nullElement.amount; j++)
      {
        beatMap.InsertNull(totalInserted + nullElement.index);
      }
      
      totalInserted += nullElement.amount;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Beat GetFirstSelectedBeat()
  {
    Beat firstSelectedBeat = null;
    for (int i = 0; i < Beat.SelectedBeatCount; i++)
    {
      Beat beat = Beat.SelectedBeats[i];

      // If selected beats is null choose this one
      if (firstSelectedBeat == null)
      {
        firstSelectedBeat = beat;
        continue;
      }

      // Check if the lane index of this beat is before the selected beat
      if (firstSelectedBeat.LaneIndex < beat.LaneIndex) continue;
      
      // Check if the note number is greater than the selected beat
      if (firstSelectedBeat.NoteNumber < beat.NoteNumber) continue;
      
      firstSelectedBeat = beat;
    }
    
    return firstSelectedBeat;
  }
}
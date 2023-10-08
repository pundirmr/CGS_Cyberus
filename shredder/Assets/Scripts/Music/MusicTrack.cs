using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Flags]
public enum NoteDifficulty
{
  NONE   = (int)Bits.Bit1,//1 << 0, // 1
  EASY   = (int)Bits.Bit2,//1 << 1, // 2
  MEDIUM = (int)Bits.Bit3,//1 << 2, // 4
  HARD   = (int)Bits.Bit4,//1 << 3  // 8
}

[Serializable]
public class BeatMapNote
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public BeatMapNote(int laneIndex, int noteIndex)
  {
    LaneIndex = laneIndex;
    NoteIndex = noteIndex;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public BeatMapNote GetNewDeepCopy()
  {
    BeatMapNote copy = new BeatMapNote(LaneIndex, NoteIndex);
    
    copy.IsActive     = this.IsActive;
    copy.Difficulty   = this.Difficulty;
    copy.IsEndOfTrack = this.IsEndOfTrack;
    
    return copy;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(BeatMapNote note)
  {
    note.IsActive   = IsActive;
    note.Difficulty = Difficulty;
  }
  
  public bool IsActive             = false;
  public bool IsEndOfTrack         = false;
  public NoteDifficulty Difficulty = NoteDifficulty.NONE;

  public int LaneIndex;
  public int NoteIndex;
}

[Serializable]
public class BeatMapLane
{
  public BeatMapNote[] Notes = Array.Empty<BeatMapNote>();
  public int Length => Notes.Length;
  
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

[CreateAssetMenu(fileName = "New Track", menuName = "MusicTrack", order = 0)]
public class MusicTrack : ScriptableObject 
{
  [Header("In-Game Info")]
  public Sprite Sprite;
  public string TrackName;
  public string TrackArtist;
  public SpamMessage SpamMessage;
  public SceneColourScheme SceneColourScheme;
  [Range(1, 5)] public int TrackOverallDifficulty = 1;
  
  [Header("Stream Deck UI")]
  public ButtonTexture DeckPressed;
  public ButtonTexture DeckUnpressed;

  [Header("Track Info")]
  public AudioClip AudioClip;
  public double FirstBeatOffset;
  public int BeatsPerMinute;
  public double LaneDuration;
  public int NotesPerBeat = 4;
  
  public float TrackDuration => AudioClip.length;

  public double BeatsPerSecond => BeatsPerMinute / 60.0;
  public double SecondsPerBeat => 60.0 / BeatsPerMinute;
  public int TotalBeatsInTrack => (int)maths.Floor(BeatsPerSecond * AudioClip.length);
  
  public double NotesPerMinute => BeatsPerMinute * NotesPerBeat;
  public double NotesPerSecond => NotesPerMinute / 60.0;
  public double SecondsPerNote => 60.0 / NotesPerMinute;
  public int TotalNotesInTrack => (int)maths.Floor(NotesPerSecond * AudioClip.length);

  // BeatMap containing all lanes
  public const int LaneCount = 3;
  public BeatMapLane[] BeatMap;

  // NOTE(WSWhitehouse): Number of blocks in Beat Map. Total num is the count over all lanes and
  // difficulties, the dictionary is the total count for each difficulty across all lanes.
  public int TotalNumOfBlocks                        { get; private set; }
  public Dictionary<NoteDifficulty, int> NumOfBlocks { get; private set; }
  
  public readonly NoteDifficulty[] NoteDifficulties = Enum.GetValues(typeof(NoteDifficulty)) as NoteDifficulty[];

  public void Init()
  {
    // Perform checks to ensure track is valid
    if (AudioClip == null)   { Log.Error($"AudioClip is null on {TrackName}");               return; }
    if (BeatMap == null)     { Log.Error($"BeatMap is null on {TrackName}");                 return; }
    if (BeatMap.Length != 3) { Log.Error($"BeatMap doesn't contain 3 lanes on {TrackName}"); return; }
    if (SpamMessage == null) { Log.Error($"Spam Message is null on {TrackName}");            return; }
    // if (SceneColourScheme == null) { Log.Error($"Scene Colour Scheme is null on {TrackName}"); return; }
    
    // Reset total num of blocks
    TotalNumOfBlocks = 0;
    
    // Reset num of blocks, and add all difficulties to dictionary
    NumOfBlocks = new Dictionary<NoteDifficulty, int>(NoteDifficulties.Length);
    foreach (NoteDifficulty difficulty in NoteDifficulties)
    {
      NumOfBlocks.Add(difficulty, 0);     
    }
    
    // Calculate total number of blocks in beat map
    foreach (BeatMapLane lane in BeatMap)
    {
      for (int i = 0; i < lane.Length; i++)
      {
        if (!lane.Notes[i].IsActive) continue;

        TotalNumOfBlocks += 1;

        foreach (NoteDifficulty difficulty in NoteDifficulties)
        {
          if ((lane.Notes[i].Difficulty & difficulty) == 0) continue;
          NumOfBlocks[difficulty] += 1;
        }
      }
    }

    for (int i = 0; i < BeatMap.Length; i++)
    {
      for (int j = 0; j < BeatMap[i].Length; j++)
      {
        BeatMap[i][j].LaneIndex = i;
        BeatMap[i][j].NoteIndex = j;
      }
    }
  }

  /// <summary>
  /// Resets the beat map completely - removes any existing data.
  /// </summary>
  public void ResetBeatMap()
  {
    if (AudioClip == null)
    {
      Log.Warning("No audio clip in Music Track. Cannot reset beat map");
      return;
    }
    
    // Set up BeatMap array
    if (BeatMap == null || BeatMap.Length != LaneCount)
    {
      BeatMap = new BeatMapLane[LaneCount];
    }

    // Set up lane arrays
    for (int i = 0; i < BeatMap.Length; i++)
    {
      ref BeatMapLane beatMapLane = ref BeatMap[i];
      if (beatMapLane == null) beatMapLane = new BeatMapLane();
      
      SetupNoteArray(ref beatMapLane, i);
    }
  }
  
  /// <summary>
  /// Sets up the beat map - keeps any existing data.
  /// </summary>
  public void SetUpBeatMap()
  {
    if (AudioClip == null)
    {
      Log.Warning("No audio clip in Music Track. Cannot reset beat map");
      return;
    }
    
    // If there is no beatmap then might as well call reset
    if (BeatMap == null)
    {
      ResetBeatMap();
      return;
    }

    if (BeatMap.Length != GameManager.NumberOfSublanes)
    {
      // TODO(WSWhitehouse): Instead of calling reset, add/remove unused lanes but keep any valid data
      ResetBeatMap();
      return;
    }
    
    for (int i = 0; i < BeatMap.Length; i++)
    {
      ref BeatMapLane beatMapLane = ref BeatMap[i];
      
      if (beatMapLane == null)
      {
        beatMapLane = new BeatMapLane 
        {
          Notes = new BeatMapNote[TotalNotesInTrack]
        };
        
        continue;
      }
      
      if (beatMapLane.Length != TotalNotesInTrack)
      {
        SetupNoteArray(ref beatMapLane, i);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetupNoteArray(ref BeatMapLane array, int laneIndex)
  {
    array.Notes = new BeatMapNote[TotalNotesInTrack];

    for (int i = 0; i < array.Notes.Length; i++)
    {
      array.Notes[i] = new BeatMapNote(laneIndex, i);
    }
  }
}

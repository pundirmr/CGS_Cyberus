using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Beat : MonoBehaviour, IPointerClickHandler
{
  // Static array of all beats in the scene and the ones selected
  public static List<Beat> Beats;
  public static List<Beat> SelectedBeats;
  public static int SelectedBeatCount => SelectedBeats.Count;
  private static bool InitStatic = false;

  private static void OnMusicTrackChanged()
  {
    if (TrackEditor.MusicTrack == null) return;
    
    const int ExtraBeats = 100; // NOTE(WSWhitehouse): Adding a few extra to ensure we dont resize the list
    int totalBeatCount   = (TrackEditor.MusicTrack.BeatMap[0].Length * MusicTrack.LaneCount) + ExtraBeats;
    
    Beats         = new List<Beat>(totalBeatCount);
    SelectedBeats = new List<Beat>(totalBeatCount);
  }

  public static void SortSelectedBeats()
  {
    // Sort notes by lane index
    for (int i = 0; i < SelectedBeatCount; i++)
    {
      for (int j = i + 1; j < SelectedBeatCount; j++)
      {
        if (SelectedBeats[i].Note.LaneIndex > SelectedBeats[j].Note.LaneIndex)
        {
          ArrayUtil.Swap(SelectedBeats, i, j);
        }
      }
    }

    // Find final indices of each lane index
    System.Span<int> lastLaneIndices = stackalloc int[MusicTrack.LaneCount];
    for (int i = 0; i < MusicTrack.LaneCount; i++) { lastLaneIndices[i] = 0; }
    
    for (int i = 0; i < SelectedBeatCount; i++)
    {
      int laneIndex = SelectedBeats[i].Note.LaneIndex;

      if (i > lastLaneIndices[laneIndex])
      {
        lastLaneIndices[laneIndex] = i;
      }
    }

    // Sort each lane
    for (int i = 0; i < MusicTrack.LaneCount; i++)
    {
      int startIndex = i == 0 ? 0 : lastLaneIndices[i - 1];
      int endIndex   = lastLaneIndices[i];
      for (int j = startIndex; j < endIndex; j++)
      {
        for (int k = j + 1; k < endIndex; k++)
        {
          if (SelectedBeats[j].Note.LaneIndex > SelectedBeats[k].Note.LaneIndex)
          {
            ArrayUtil.Swap(SelectedBeats, j, k);
          }
        }
      }
    }
  }

  public RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private Image image;
  [SerializeField] private Image selectedImage;
  [SerializeField] private Vector2 mainBeatSize;
  [SerializeField] private Vector2 defaultSize;
  [SerializeField] private Vector2 playedSize;
  
  [Header("Difficulty Indicators")]
  [SerializeField] private GameObject easyDifficulty;
  [SerializeField] private GameObject mediumDifficulty;
  [SerializeField] private GameObject hardDifficulty;
  
  [Header("End of Track Indicators")]
  [SerializeField] private GameObject endOfTrackIndicator;

  public int LaneIndex   { get; private set; }
  public int NoteNumber  { get; private set; }
  public bool IsMainBeat { get; private set; }
  public BeatMapNote Note => TrackEditor.MusicTrack.BeatMap[LaneIndex][NoteNumber];
  
  private readonly NoteDifficulty[] NoteDifficulties = Enum.GetValues(typeof(NoteDifficulty)) as NoteDifficulty[];
  
  private bool _selected  = false;
  public bool Selected
  { 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _selected;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set
    {
      // If the value coming in is the same, ignore it
      if (_selected == value) return;
      
      // Update the internal value and UI
      _selected = value;
      UpdateUI();
      
      if (_selected)
      {
        // Add this beat to selected beats
        if (!SelectedBeats.Contains(this))
        {
          SelectedBeats.Add(this);
        }
      }
      else
      {
        // Remove this beat from selected beats
        if (SelectedBeats.Contains(this))
        {
          SelectedBeats.Remove(this);
        }
      }
    }
  }

  private bool init;

  private void Awake()
  {
    if (!InitStatic)
    {
      InitStatic = true;
      TrackEditor.OnMusicTrackChanged += OnMusicTrackChanged;
      
      // NOTE(WSWhitehouse): Calling the music track changed function here as its probably already
      // been called and we need to initialise the lists. It should be called automatically from the 
      // event from now on.
      OnMusicTrackChanged();
    }
    
    TrackEditor.OnBeatMapUpdated += UpdateUI;
    Tool.OnModeChanged           += OnModeChanged;
  }

  private void OnDestroy()
  {
    TrackEditor.OnBeatMapUpdated  -= UpdateUI;
    Tool.OnModeChanged            -= OnModeChanged;
    MusicTrackPlayerEditor.OnNote -= OnNote;
  }
  
  private void OnEnable()
  {
    Beats.Add(this);
  }

  private void OnDisable()
  {
    Beats.Remove(this);
    if (Selected) Selected = false;
  }

  private void OnModeChanged()
  {
    if (Selected) Selected  = false;
  }

  public void Init(int laneIndex, int noteNumber)
  {
    this.LaneIndex  = laneIndex;
    this.NoteNumber = noteNumber;
    this.init       = true;
    
    IsMainBeat = noteNumber % TrackEditor.MusicTrack.NotesPerBeat == 0;
    image.rectTransform.SetSize(IsMainBeat ? mainBeatSize : defaultSize);

    MusicTrackPlayerEditor.OnNote += OnNote;

    UpdateUI(); 
  }

  private void OnNote(NoteData noteData)
  {
    if (noteData.noteIndex != NoteNumber)
    {
      image.rectTransform.SetSize(IsMainBeat ? mainBeatSize : defaultSize);
      return;
    }
    
    image.rectTransform.SetSize(playedSize);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (Tool.Mode != ToolMode.EDIT) return;
    
    if (eventData.button == PointerEventData.InputButton.Right)
    {
      Note.IsActive   = false;
      Note.Difficulty = NoteDifficulty.NONE;
      UpdateUI();
      return;
    }

    Note.IsActive = true;

    if ((Note.Difficulty & TrackEditor.CursorDifficulty) == 0)
    {
       Note.Difficulty |= TrackEditor.CursorDifficulty;
    }
    else
    {
      Note.Difficulty &= ~TrackEditor.CursorDifficulty;
    }
    
    UpdateUI();
  }

  private void UpdateUI()
  {
    if (!init) return;
    if (TrackEditor.MusicTrack == null) return;
    
    endOfTrackIndicator.SetActive(Note.IsEndOfTrack);
    selectedImage.enabled = Selected;
    
    easyDifficulty.SetActive(false);
    mediumDifficulty.SetActive(false);
    hardDifficulty.SetActive(false);
    
    if (!Note.IsActive) return;
    
    if ((Note.Difficulty & NoteDifficulty.EASY) != 0)
    {
      easyDifficulty.SetActive(true);
    }
    
    if ((Note.Difficulty & NoteDifficulty.MEDIUM) != 0)
    {
      mediumDifficulty.SetActive(true);
    }
    
    if ((Note.Difficulty & NoteDifficulty.HARD) != 0)
    {
      hardDifficulty.SetActive(true);
    }
    
  }
}
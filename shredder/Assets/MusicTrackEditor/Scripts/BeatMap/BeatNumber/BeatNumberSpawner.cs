using System.Collections.Generic;
using UnityEngine;

public class BeatNumberSpawner : MonoBehaviour
{
  [SerializeField] private BeatMap beatMap;

  [Header("Timeline")]
  [SerializeField] private RectTransform beatNumberParent;
  [SerializeField] private BeatNumber beatNumberPrefab;
  
  private List<BeatNumber> _beatNumbers;

  private void Awake()
  {
    TrackEditor.OnMusicTrackChanged += OnMusicTrackChanged;
  }

  private void OnDestroy()
  {
    TrackEditor.OnMusicTrackChanged -= OnMusicTrackChanged;
  }

  private void OnMusicTrackChanged()
  {
    ClearBeatNumbers();

    // NOTE(WSWhitehouse): Don't want to recreate timeline if track is null
    if (TrackEditor.MusicTrack == null) return;

    CreateBeatNumbers();
  }

  private void CreateBeatNumbers()
  {
    int beatNumberCount = TrackEditor.MusicTrack.TotalBeatsInTrack;
    _beatNumbers = new List<BeatNumber>(beatNumberCount);
    for (int i = 0; i < beatNumberCount; i++)
    {
      BeatNumber number = Instantiate(beatNumberPrefab, beatNumberParent);
      number.Init(i);
      _beatNumbers.Add(number);
    }
    
    ScaleBeatNumbers();
  }

  private void ClearBeatNumbers()
  {
    if (_beatNumbers == null) return;

    foreach (BeatNumber number in _beatNumbers)
    {
      Destroy(number.gameObject);
    }

    _beatNumbers = null;
  }

  private void ScaleBeatNumbers()
  {
    if (_beatNumbers == null) return;

    float offset = (beatMap.LaneHeight / ((float)TrackEditor.MusicTrack.TotalNotesInTrack / (float)TrackEditor.MusicTrack.NotesPerBeat));
    float yPos   = beatMap.minusContentStartOffset;
    for (int i = 0; i < _beatNumbers.Count; i++)
    {
      BeatNumber number = _beatNumbers[i];
      number.RectTransform.localPosition = new Vector3(0.0f, yPos, 0.0f);
      yPos -= offset;
    }
  }
}
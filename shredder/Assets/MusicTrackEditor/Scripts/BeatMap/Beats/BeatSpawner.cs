using System.Collections.Generic;
using UnityEngine;

public class BeatSpawner : MonoBehaviour
{
  [SerializeField] private BeatMap beatMap;

  [Header("Timeline")]
  [SerializeField] private RectTransform[] playerLanes;
  [SerializeField] private Beat beatPrefab;

  private List<Beat>[] _beats;

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
    ClearBeats();

    // NOTE(WSWhitehouse): Don't want to recreate timeline if track is null
    if (TrackEditor.MusicTrack == null) return;

    CreateBeats();
  }

  private void CreateBeats()
  {
    int notesCount = TrackEditor.MusicTrack.TotalNotesInTrack;
    _beats = new List<Beat>[playerLanes.Length];
    for (int i = 0; i < playerLanes.Length; i++)
    {
      ref RectTransform playerLane = ref playerLanes[i];
      
      _beats[i] = new List<Beat>(notesCount);
      
      for (int j = 0; j < notesCount; j++)
      {
        Beat beat = Instantiate(beatPrefab, playerLane);
        beat.Init(i, j);
        _beats[i].Add(beat);
      }
    }
    
    ScaleBeats();
  }

  private void ClearBeats()
  {
    if (_beats == null) return;

    foreach (List<Beat> beats in _beats)
    {
      foreach (Beat beat in beats)
      {
        Destroy(beat.gameObject);
      }
    }

    _beats = null;
  }

  private void ScaleBeats()
  {
    if (_beats == null) return;
    
    float offset = (beatMap.LaneHeight / TrackEditor.MusicTrack.TotalNotesInTrack);

    for (int i = 0; i < playerLanes.Length; i++)
    {
      ref List<Beat> laneBeats = ref _beats[i];
      
      float y = beatMap.minusContentStartOffset;
      for (int j = 0; j < laneBeats.Count; j++)
      {
        Beat beat = laneBeats[j];
        beat.RectTransform.localPosition = new Vector3(0.0f, y, 0.0f);
        y -= offset;
      }
    }
  }
}
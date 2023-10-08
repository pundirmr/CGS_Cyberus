using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPanel : MonoBehaviour
{
  [SerializeField] private WindowPanel windowPanel;
  
  [Header("Open Panel Track")]
  [SerializeField] private OpenPanelTrack trackPrefab;
  [SerializeField] private RectTransform trackParent;
  
  [Header("Scene References")]
  [SerializeField] private Button openButton;
  [SerializeField] private Button cancelButton;

  private List<OpenPanelTrack> _tracks;
  private DelegateUtil.EmptyCoroutineDel OpenTrackCoroutine;

  private void Awake()
  {
    OpenTrackCoroutine = __OpenTrackCoroutine;
    
    windowPanel.OnOpen += OnOpen;

    openButton.onClick.AddListener(OpenTrack);
    cancelButton.onClick.AddListener(windowPanel.Close);
    
    OpenPanelTrack.OnCurrentlySelectedTrackUpdated += OnCurrentlySelectedTrackUpdated;
    
    SpawnTracks();
  }

  private void OnDestroy()
  {
    windowPanel.OnOpen -= OnOpen;
    
    openButton.onClick.RemoveListener(OpenTrack);
    cancelButton.onClick.RemoveListener(windowPanel.Close);
    
    OpenPanelTrack.OnCurrentlySelectedTrackUpdated -= OnCurrentlySelectedTrackUpdated;
  }

  private void OnOpen()
  {
    // Reset currently selected track as the tracks have had a refresh
    OpenPanelTrack.CurrentlySelectedTrack = OpenPanelTrack.INVALID_TRACK_INDEX;
    OpenPanelTrack.OnCurrentlySelectedTrackUpdated?.Invoke();
  }

  private void SpawnTracks()
  {
    ClearTracks();
    
    _tracks = new List<OpenPanelTrack>(StaticData.MusicTracks.Length);
    for (int i = 0; i < StaticData.MusicTracks.Length; i++)
    {
      OpenPanelTrack track = Instantiate(trackPrefab, trackParent);
      track.Create(i, StaticData.MusicTracks[i]);
      _tracks.Add(track);
    }

    // Reset currently selected track as the tracks have had a refresh
    OpenPanelTrack.CurrentlySelectedTrack = OpenPanelTrack.INVALID_TRACK_INDEX;
    OpenPanelTrack.OnCurrentlySelectedTrackUpdated?.Invoke();
  }

  private void ClearTracks()
  {
    if (_tracks == null) return;

    foreach (OpenPanelTrack track in _tracks)
    {
      Destroy(track.gameObject);
    }
    
    _tracks.Clear();
  }

  private void OpenTrack()
  {
    windowPanel.Close();
    StartCoroutine(OpenTrackCoroutine());
  }

  private IEnumerator __OpenTrackCoroutine()
  {
    LoadingPopup.ShowLoadingPopup();
    yield return CoroutineUtil.Wait(0.5f);
    
    TrackEditor.MusicTrack = StaticData.MusicTracks[OpenPanelTrack.CurrentlySelectedTrack];

    yield return CoroutineUtil.Wait(0.5f);
    LoadingPopup.HideLoadingPopup();
  }

  private void OnCurrentlySelectedTrackUpdated()
  {
    openButton.interactable = OpenPanelTrack.CurrentlySelectedTrack != OpenPanelTrack.INVALID_TRACK_INDEX;
  }
}
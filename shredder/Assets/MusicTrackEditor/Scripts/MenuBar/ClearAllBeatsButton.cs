using UnityEngine;
using UnityEngine.UI;

public class ClearAllBeatsButton : MonoBehaviour
{
  [SerializeField] private Button button;
  
  private void Awake()
  {
    button.onClick.AddListener(ClearAllBeats);
  }

  private void OnDestroy()
  {
    button.onClick.RemoveListener(ClearAllBeats);
  }
  
  private void ClearAllBeats()
  {
    if (TrackEditor.MusicTrack == null) return;
    
    void ResetBeatMap()
    {
      if (TrackEditor.MusicTrack == null) return;
      TrackEditor.MusicTrack.ResetBeatMap();
      TrackEditor.OnBeatMapUpdated?.Invoke();
    }

    PopupCanvas.Show("Are you sure you want to clear the Beat Map? This CANNOT be undone.", ResetBeatMap, null);
  }
}
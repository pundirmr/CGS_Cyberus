using UnityEngine;
using UnityEngine.UI;

public class EndOfTrackButton : MonoBehaviour
{
  [SerializeField] private Button button;

  private void Awake()
  {
    button.onClick.AddListener(EndOfTrackManager.SetSelectedBeatAsEndOfTrack);
  }

  private void OnDestroy()
  {   
    button.onClick.RemoveListener(EndOfTrackManager.SetSelectedBeatAsEndOfTrack);
  }
}
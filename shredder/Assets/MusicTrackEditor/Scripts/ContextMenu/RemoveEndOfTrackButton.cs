using UnityEngine;
using UnityEngine.UI;

public class RemoveEndOfTrackButton : MonoBehaviour
{
  [SerializeField] private Button button;

  private void Awake()
  {
    button.onClick.AddListener(EndOfTrackManager.RemoveEndOfTrack);
  }

  private void OnDestroy()
  {   
    button.onClick.RemoveListener(EndOfTrackManager.RemoveEndOfTrack);
  }
}
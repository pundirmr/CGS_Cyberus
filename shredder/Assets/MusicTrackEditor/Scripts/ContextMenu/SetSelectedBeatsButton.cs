using UnityEngine;
using UnityEngine.UI;

public class SetSelectedBeatsButton : MonoBehaviour
{
  [SerializeField] private Button button;
  [SerializeField] private bool setActive;

  private void Awake()
  {
    button.onClick.AddListener(SetBeats);
  }

  private void OnDestroy()
  {
    button.onClick.RemoveListener(SetBeats);
  }

  private void SetBeats()
  {
    for (int i = 0; i < Beat.SelectedBeatCount; i++)
    {
      Beat.SelectedBeats[i].Note.IsActive = setActive;
    }
    
    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
}
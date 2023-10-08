using System;
using UnityEngine;
using UnityEngine.UI;

public class SetDifficultyOnSelectedBeats : MonoBehaviour
{
  [SerializeField] private Button button;
  [SerializeField] private NoteDifficulty difficulty;

  private void Awake()
  {
    button.onClick.AddListener(OnButtonClick);
  }
  
  private void OnDestroy()
  {
    button.onClick.RemoveListener(OnButtonClick);
  }

  private void OnButtonClick()
  {
    foreach (Beat beat in Beat.Beats)
    {
      if (!beat.Selected) continue;
      beat.Note.Difficulty = difficulty;
    }
    
    TrackEditor.OnBeatMapUpdated?.Invoke();
  }
}
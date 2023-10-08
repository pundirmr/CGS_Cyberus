using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultySelection : MonoBehaviour, IPointerDownHandler
{
  [SerializeField] private NoteDifficulty difficulty;
  
  [Header("Scene References")]
  [SerializeField] private Image backgroundImage;
  [SerializeField] private Color selectedColour;
  [SerializeField] private Color unselectedColour;
  

  private void Awake()
  {
    TrackEditor.OnCursorDifficultyChanged += OnCursorDifficultyChanged;
    OnCursorDifficultyChanged();
  }

  private void OnDestroy()
  {
    TrackEditor.OnCursorDifficultyChanged -= OnCursorDifficultyChanged;
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if ((TrackEditor.CursorDifficulty & difficulty) == 0)
    {
      TrackEditor.CursorDifficulty |= difficulty;
    }
    else
    {
      TrackEditor.CursorDifficulty &= ~difficulty;
    }
  }

  private void OnCursorDifficultyChanged()
  {
    if ((TrackEditor.CursorDifficulty & difficulty) != 0)
    {
      backgroundImage.color = selectedColour;
    }
    else
    {
      backgroundImage.color = unselectedColour;
    }
  }
}
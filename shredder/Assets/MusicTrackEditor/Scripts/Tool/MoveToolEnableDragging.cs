using System;
using UnityEngine;

public class MoveToolEnableDragging : MonoBehaviour
{
  [SerializeField] private ConfigurableScrollRect scrollRect;

  private void Awake()
  {
    Tool.OnModeChanged += OnModeChanged;
  }

  private void OnDestroy()
  {
    Tool.OnModeChanged -= OnModeChanged;
  }

  private void OnModeChanged()
  {
    scrollRect.enableDragging = Tool.Mode == ToolMode.MOVE;
  }
}
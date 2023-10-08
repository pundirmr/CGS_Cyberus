using System;
using TMPro;
using UnityEngine;

public class CurrentToolMode : MonoBehaviour
{
  [SerializeField] private TMP_Text text;
  [SerializeField] private string textFormat = "{0} MODE";
  
  private readonly string[] _toolModeStrings = Enum.GetNames(typeof(ToolMode));

  private void Awake()
  {
    Tool.OnModeChanged += OnModeChanged;
    OnModeChanged();
  }

  private void OnDestroy()
  {
    Tool.OnModeChanged -= OnModeChanged;
  }

  private void OnModeChanged() => text.text = string.Format(textFormat, _toolModeStrings[(int)Tool.Mode]);
}
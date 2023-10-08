using System;
using UnityEngine;
using UnityEngine.UI;

public class NoSelectedBeatsButtonDisable : MonoBehaviour
{
  [SerializeField] private ContextMenu menu;
  [SerializeField] private Button button;

  private void Awake()
  {
    menu.OnOpen += OnOpen;
  }

  private void OnDestroy()
  {
    menu.OnOpen -= OnOpen;
  }

  private void OnOpen()
  {
    button.interactable = Beat.SelectedBeatCount > 0;
  }
}
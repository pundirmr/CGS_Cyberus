using UnityEngine;
using UnityEngine.UI;

public class PasteButton : MonoBehaviour
{
  [SerializeField] private ContextMenu menu;
  [SerializeField] private Button button;

  private void Awake()
  {
    menu.OnOpen += OnOpen;
    button.onClick.AddListener(ClipBoardManager.Paste);
  }

  private void OnDestroy()
  {
    menu.OnOpen -= OnOpen;
    button.onClick.RemoveListener(ClipBoardManager.Paste);
  }

  private void OnOpen()
  {
    button.interactable = ClipBoardManager.ClipBoard.Count > 0;
  }
}
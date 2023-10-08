using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MusicTrackEditorZOrder : MonoBehaviour
{
  public enum ZOrder : short
  {
    BEATMAP,
    MENUBAR,
    PANEL,
    POPUP,
    CONTEXT_MENU,
    TOOLTIP = short.MaxValue
  }
  
  [SerializeField] private ZOrder zOrder;

  private void Awake()
  {
    Canvas canvas       = GetComponent<Canvas>();
    canvas.sortingOrder = (int)zOrder;
  }
}
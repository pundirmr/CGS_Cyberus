using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuBarDropdown : MonoBehaviour, IPointerExitHandler
{
  [field: SerializeField] public Canvas Canvas { get; private set; }
  public RectTransform CanvasRT => (RectTransform) Canvas.transform;
  
  public Action<PointerEventData> OnMouseExit;
  public void OnPointerExit(PointerEventData eventData) => OnMouseExit?.Invoke(eventData);
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuBarDropdownButton : MonoBehaviour, IPointerEnterHandler
{
  [SerializeField] private Button button;
  
  public Action OnButtonClick;
  public Action<PointerEventData> OnMouseEnter;
  
  private void Awake()
  {
    button.onClick.AddListener(OnClick);
  }
  
  private void OnDestroy()
  {
    button.onClick.RemoveListener(OnClick);
  }

  private void OnClick() => OnButtonClick?.Invoke();
  public void OnPointerEnter(PointerEventData eventData) => OnMouseEnter?.Invoke(eventData);
}
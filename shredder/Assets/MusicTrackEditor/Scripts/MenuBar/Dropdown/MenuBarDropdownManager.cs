using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuBarDropdownManager : MonoBehaviour
{
  [SerializeField] private MenuBarDropdownButton button;
  [SerializeField] private MenuBarDropdown dropdown;

  private bool thisDropdownOpen = false;
  
  private static MenuBarDropdownManager CurrentOpenDropdown;
  private static bool DropdownOpen = false;
  private static Action UpdateDropdown;

  private void Awake()
  {
    UpdateDropdown += UpdateUI;
    
    button.OnButtonClick += OnButtonClick;
    button.OnMouseEnter  += ButtonOnMouseEnter;
    dropdown.OnMouseExit += DropdownOnMouseExit;
    
    dropdown.Canvas.enabled = false;
  }

  private void OnDestroy()
  {
    UpdateDropdown -= UpdateUI;
    
    button.OnButtonClick -= OnButtonClick;
    button.OnMouseEnter  -= ButtonOnMouseEnter;
    dropdown.OnMouseExit -= DropdownOnMouseExit;
  }

  private void OnButtonClick()
  {
    if (DropdownOpen && CurrentOpenDropdown != this)
    {
      CurrentOpenDropdown.thisDropdownOpen = false;
      thisDropdownOpen = true;
      UpdateDropdown?.Invoke();
      return;
    }
    
    thisDropdownOpen    = !thisDropdownOpen;
    DropdownOpen        = thisDropdownOpen;
    CurrentOpenDropdown = this;
    
    UpdateDropdown?.Invoke();
  }

  private void ButtonOnMouseEnter(PointerEventData eventData)
  {
    if (!DropdownOpen) return;
    
    thisDropdownOpen = true;
    CurrentOpenDropdown.thisDropdownOpen = false;
    CurrentOpenDropdown = this;
    
    UpdateDropdown?.Invoke();
  }

  private void DropdownOnMouseExit(PointerEventData eventData)
  {
    if (!DropdownOpen) return;
    
    CurrentOpenDropdown.thisDropdownOpen = false;
    thisDropdownOpen = false;
    DropdownOpen     = false;
    
    UpdateDropdown?.Invoke();
  }

  private void UpdateUI()
  {
    dropdown.Canvas.enabled = thisDropdownOpen;
  }
}
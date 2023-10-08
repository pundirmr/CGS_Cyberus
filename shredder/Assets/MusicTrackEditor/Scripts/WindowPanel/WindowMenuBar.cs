using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WindowMenuBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  public RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private WindowPanel windowPanel;
  
  private bool _mouseInsidePanel = false;
  private bool _movingPanel      = false;
  private Vector2 _mouseOffset   = Vector2.zero;

  private void Update()
  {
    if (!windowPanel.IsOpen)
    {
      _movingPanel = false;
      _mouseOffset = Vector2.zero;
      return;
    }

    if (_movingPanel)
    {
      MovePanel();
      return;
    }
    
    if (_mouseInsidePanel && Mouse.current.leftButton.wasPressedThisFrame)
    {
      _movingPanel = true;
      
      Vector2 mousePos = Mouse.current.position.ReadValue();
      _mouseOffset     = (Vector2)windowPanel.RectTransform.position - mousePos;
    }
  }

  private void MovePanel()
  {
    if (Mouse.current.leftButton.wasReleasedThisFrame)
    {
      _movingPanel = false;
      _mouseOffset = Vector2.zero;
      return;
    }
    
    Vector2 mousePos = float2Util.Clamp(Mouse.current.position.ReadValue(), float2.zero, new float2(Screen.width, Screen.height));
    windowPanel.RectTransform.position = mousePos + _mouseOffset;
  }

  public void OnPointerEnter(PointerEventData eventData) => _mouseInsidePanel = true;
  public void OnPointerExit(PointerEventData eventData)  => _mouseInsidePanel = false;
}
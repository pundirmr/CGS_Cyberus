using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContextMenu : MonoBehaviour
{
  [SerializeField] private ToolMode toolMode;
  [Space]
  [SerializeField] private Canvas canvas;
  [SerializeField] private RectTransformUtil.Anchor defaultAnchor;
  
  private RectTransform RectTransform => (RectTransform)transform;

  private TrackEditorInput _input;
  private DelegateUtil.EmptyCoroutineDel WaitAndClose;
  private Coroutine _waitAndCloseCoroutine;
  
  public bool IsOpen { get; private set; }
  public Action OnOpen;
  public Action OnClose;

  private void Awake()
  {
    WaitAndClose = __WaitAndClose;
    
    _input = new TrackEditorInput();
    _input.Enable();
    
    _input.Mouse.LeftClick.canceled   += OnLeftClickPerformed;
    _input.Mouse.RightClick.performed += OnRightClickPerformed;
    
    CloseImmediately();
  }

  private void OnDestroy()
  {
    _input.Mouse.LeftClick.canceled   -= OnLeftClickPerformed;
    _input.Mouse.RightClick.performed -= OnRightClickPerformed;
  }

  private void OnLeftClickPerformed(InputAction.CallbackContext obj)
  {
    Close();
  }

  private void OnRightClickPerformed(InputAction.CallbackContext obj)
  {
    if (Tool.Mode != toolMode) return;
    Open();
  }

  public void Open()
  {
    IsOpen = true;
    OnOpen?.Invoke();
    
    float2 mousePos = Mouse.current.position.ReadValue();
    
    RectTransformUtil.Anchor anchor = RectTransformUtil.Anchor.NONE;
    
    float width  = RectTransform.GetScaledWidth();
    float height = RectTransform.GetScaledHeight();

    if (mousePos.y + height > Screen.height)
    {
      if (mousePos.y + (height * 0.5f) > Screen.height)
      { 
        anchor |= RectTransformUtil.Anchor.TOP;
      }
      else
      {
        anchor |= RectTransformUtil.Anchor.MIDDLE;
      }
    }
    else
    {
      anchor |= RectTransformUtil.Anchor.BOTTOM;
    }
    
    if (mousePos.x + width > Screen.width)
    {
      if (mousePos.x + (width * 0.5f) > Screen.width)
      { 
        anchor |= RectTransformUtil.Anchor.RIGHT;
      }
      else
      {
        anchor |= RectTransformUtil.Anchor.MIDDLE;
      }
    }
    else
    {
      anchor |= RectTransformUtil.Anchor.LEFT;
    }
    
    RectTransform.SetPivotAndAnchors(anchor);
    RectTransform.position = mousePos.ToVector();

    canvas.enabled = true;
  }

  public void Close()
  {
    CoroutineUtil.StartSafelyWithRef(this, ref _waitAndCloseCoroutine, WaitAndClose());
  }

  public void CloseImmediately()
  {
    IsOpen = false;
    OnClose?.Invoke();
    
    canvas.enabled = false;
  }

  private IEnumerator __WaitAndClose()
  {
    // NOTE(WSWhitehouse): Wait a few frames so the context menu buttons can register a click
    yield return CoroutineUtil.WaitForEndOfFrame;
    CloseImmediately();
  }
}
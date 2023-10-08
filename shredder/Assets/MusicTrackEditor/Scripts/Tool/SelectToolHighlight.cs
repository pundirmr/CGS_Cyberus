using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectToolHighlight : MonoBehaviour
{
  private RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private Canvas canvas;
  [Tooltip("How big the selection size has to be before creating a selection")] 
  [SerializeField] private float2 minSelectionSize;
  [Tooltip("The maximum time between starting a click and ending it to be counted as a click rather than a drag")]
  [SerializeField] private float maxClickTime;
  [SerializeField] private float2 clickRectSize;
  [Space]
  [SerializeField] private ContextMenu menu;
   
  private TrackEditorInput _input;
  
  private float _clickTime;
  private bool _clickActive     = false;
  private bool _selectionActive = false;
  private float2 _mouseClickPos;
  private RectTransformUtil.Anchor _currentAnchor = RectTransformUtil.Anchor.NONE;

  private void Awake()
  {
    _input = new TrackEditorInput();
    _input.Enable();
    
    canvas.enabled = false;
    
    _input.Mouse.LeftClick.performed += OnLeftClickPerformed;
    _input.Mouse.LeftClick.canceled  += OnLeftClickCancelled;
  }

  private void OnDestroy()
  {
    _input.Mouse.LeftClick.performed -= OnLeftClickPerformed;
    _input.Mouse.LeftClick.canceled  -= OnLeftClickCancelled;
  }

  private void OnLeftClickPerformed(InputAction.CallbackContext obj)
  {
    if (Tool.Mode != ToolMode.SELECT) return;
    if (menu.IsOpen) return;
    
    _clickActive     = true;
    _clickTime       = Time.unscaledTime;
    _selectionActive = false;
    _mouseClickPos   = Mouse.current.position.ReadValue();
  }

  private void OnLeftClickCancelled(InputAction.CallbackContext obj)
  {
    if (Tool.Mode != ToolMode.SELECT) return;
    if (menu.IsOpen) return;
    
    _clickActive     = false;
    _selectionActive = false;
    canvas.enabled   = false;

    // If the time of this click is less than max click time we are counting this interaction as a click rather than drag
    if (Time.unscaledTime - _clickTime <= maxClickTime)
    {
      float2 mousePos = Mouse.current.position.ReadValue();
      Rect clickRect  = new Rect(mousePos.x, mousePos.y, clickRectSize.x, clickRectSize.y);
      
      // Remove all hovered selectables if ctrl isn't down
      if (!Keyboard.current.ctrlKey.isPressed)
      {
        foreach (Beat beat in Beat.Beats)
        {
          if (beat.Selected)
          {
            beat.Selected = false;
          }
        }
      }
      
      // Check for selectable under mouse
      foreach (Beat beat in Beat.Beats)
      {
        if (!clickRect.Overlaps(beat.RectTransform.GetWorldRect(), true)) continue;
        
        beat.Selected = !beat.Selected;
        break;
      }
    }
  }
  
  private void Update()
  {
    if (!_clickActive) return;
    
    // NOTE(WSWhitehouse): Remove highlight selection if tool has been changed 
    if (Tool.Mode != ToolMode.SELECT)
    {
      _clickActive     = false;
      _selectionActive = false;
      canvas.enabled   = false;
      return;
    }
    
    float2 mousePos = Mouse.current.position.ReadValue();
    float2 size     = float2Util.Abs(mousePos - _mouseClickPos) * 0.5f;

    if (!_selectionActive)
    {
      if (!(size >= minSelectionSize).AllTrue()) return;
      
      _selectionActive = true;
      canvas.enabled   = true;
    }
    
    // NOTE(WSWhitehouse): One corner of the highlight selection must remain at the mouse press point while the 
    // selection grows/shrinks. An easy way to achieve this is change the anchor point on the rect transform (i.e.
    // where the transform will be locked too when it grows and shrinks). We check which direction the mouse is at 
    // compared to the original press point and calculate which corner of the transform should be the new anchor.
    RectTransformUtil.Anchor anchor = RectTransformUtil.Anchor.NONE;
    if (mousePos.x < _mouseClickPos.x)
    {
      anchor |= RectTransformUtil.Anchor.LEFT;
    }
    else
    {
      anchor |= RectTransformUtil.Anchor.RIGHT;
    }

    if (mousePos.y < _mouseClickPos.y)
    {
      anchor |= RectTransformUtil.Anchor.BOTTOM;
    }
    else
    {
      anchor |= RectTransformUtil.Anchor.TOP;
    }
    
    SetAnchor(anchor);
    RectTransform.position = mousePos.ToVector();
    RectTransform.SetSize(size.ToVector());

    foreach (Beat beat in Beat.Beats)
    {
      if (!RectTransform.Overlaps(beat.RectTransform))
      {
        if (beat.Selected)
        {
          beat.Selected = false;
        }
        
        continue;
      }
      
      if (beat.Selected) continue;
      beat.Selected = true;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetAnchor(RectTransformUtil.Anchor anchor)
  {
    if (anchor == RectTransformUtil.Anchor.NONE) return;
    if (anchor == _currentAnchor) return;
    
    _currentAnchor = anchor;
    RectTransform.SetPivotAndAnchors(anchor);
  }
}
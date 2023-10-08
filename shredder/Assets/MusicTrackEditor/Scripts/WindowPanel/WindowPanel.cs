using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WindowPanel : MonoBehaviour, IPointerDownHandler
{
  public RectTransform RectTransform => (RectTransform)transform;
  
  [Header("Scene References")]
  [SerializeField] private Canvas canvas;
  [SerializeField] private Button closeButton;
  
  [Header("Panel Settings")]
  [SerializeField] [Tooltip("Should the window panel automatically close when the user clicks outside the window pane?")]
  private bool autoCloseOnOutsideClick = true;
  [Space]
  [Tooltip("Is the window panel resizeable by the user? min/max size are ignored if this is set to false.")]
  [SerializeField] private bool resizable = false;
  [SerializeField] private Vector2 minSize;
  [SerializeField] private Vector2 maxSize;
  [SerializeField] private WindowResizePoint[] resizePoints;
  
  private WindowResizePoint _currentResizePoint = null;
  private Vector2 _windowStartPos;
  private Vector2 _resizeMouseStartPos;
  private Vector2 _initialSize;
  private bool _isResizing = false;

  public bool IsOpen { get; private set; } = false;
  
  public Action OnOpen;
  public Action OnClose;

  private void Awake()
  {
    closeButton.onClick.AddListener(Close);

    Close();
  }

  private void OnDestroy()
  {
    closeButton.onClick.RemoveListener(Close);
  }

  public void Open()
  {
    IsOpen         = true;
    canvas.enabled = true;
    SetAsActivePanel();
    OnOpen?.Invoke();
  }

  public void Close()
  {
    IsOpen         = false;
    canvas.enabled = false;
    OnClose?.Invoke();
  }

  private void Update()
  {
    if (!IsOpen) return;
    
    AutoCloseOnOutsideClick();
    ResizePanel();
  }

  private void AutoCloseOnOutsideClick()
  {
    if (!autoCloseOnOutsideClick) return;

    bool mouseInsidePanel = RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Mouse.current.position.ReadValue());
    if (!mouseInsidePanel && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
    {
      Close();
    }
  }

  private void ResizePanel()
  {
    if (!resizable) return;
    
    Vector2 mousePos = Mouse.current.position.ReadValue();

    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      foreach (WindowResizePoint resizePoint in resizePoints)
      {
        bool insidePoint = RectTransformUtility.RectangleContainsScreenPoint(resizePoint.RectTransform, mousePos);
        
        if (insidePoint)
        {
          _isResizing          = true;
          _currentResizePoint  = resizePoint;
          _resizeMouseStartPos = mousePos;
          _initialSize         = RectTransform.GetSize();
          _windowStartPos      = (Vector2)resizePoint.RectTransform.position;
          break;
        }
      }
    }
    
    if (Mouse.current.leftButton.wasReleasedThisFrame)
    {
      _isResizing         = false;
      _currentResizePoint = null;
      return;
    }
    
    if (!_isResizing) return;
    
    Vector2 windowPos = _windowStartPos;
    
    if (_currentResizePoint.movementAxis.x)
    {
      float width = maths.Clamp(_initialSize.x - ((_resizeMouseStartPos.x - mousePos.x) * 0.5f), minSize.x, maxSize.x);
      
      RectTransform.SetWidth(width);
      windowPos += new Vector2(width - (_initialSize.x * 2.0f), 0.0f);
    }
    
    if (_currentResizePoint.movementAxis.y)
    {
      float height = maths.Clamp(_initialSize.y + ((_resizeMouseStartPos.y - mousePos.y) * 0.5f), minSize.y, maxSize.y);
      
      RectTransform.SetHeight(height);
      windowPos -= new Vector2(0.0f, height - (_initialSize.y * 2.0f));
    }
    
    RectTransform.position = windowPos;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetAsActivePanel()
  {
    RectTransform.SetAsLastSibling();
  }

  public void OnPointerDown(PointerEventData eventData) => SetAsActivePanel();
}
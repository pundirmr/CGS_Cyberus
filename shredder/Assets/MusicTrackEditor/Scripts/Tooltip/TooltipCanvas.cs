using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipCanvas : MonoBehaviour
{
  [SerializeField] private Canvas tooltipCanvas;
  [Space]
  [SerializeField] private RectTransform tooltipArea;
  [SerializeField] private RectTransform tooltip;
  [SerializeField] private TMP_Text tooltipText;
  
  // NOTE(WSWhitehouse): Classic singleton - suck it up buddy!
  private static TooltipCanvas Instance { get; set; }
  private static bool IsActive => Instance != null;
  
  public static bool ShowingTooltip { get; private set; } = false;
  
  private RectTransform canvasRectTransform => (RectTransform)tooltipCanvas.transform;

  private void Awake()
  {
    Debug.Assert(Instance == null, "Another Tooltip Canvas singleton instance already exists! Please remove it!", this);
    Instance = this;
  }

  private void OnDestroy()
  {
    if (Instance != this) return;
    Instance = null;
  }

  private void Update()
  {
    if (!ShowingTooltip) return;
    SetToolTipPos();
  }

  private void SetToolTipPos()
  {
    Vector2 areaSize    = tooltipArea.GetSize() * 0.5f;
    Vector2 tooltipSize = tooltip.GetSize();
    
    RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipArea, Mouse.current.position.ReadValue(), null, out Vector2 tooltipPos);
    
    // Make sure tooltip cannot go over the top of the area
    float tooltipTop = tooltipPos.y + tooltipSize.y;
    if (tooltipTop > areaSize.y)
    {
      tooltipPos = new Vector2(tooltipPos.x, areaSize.y - tooltipSize.y);
    }
    
    // Make sure tooltip cannot go over the right hand side of the area
    float tooltipRight = tooltipPos.x + tooltipSize.x;
    if (tooltipRight > areaSize.x)
    {
      tooltipPos = new Vector2(areaSize.x - tooltipSize.x, tooltipPos.y);
    }
    
    tooltip.localPosition = tooltipPos;
  }

  private void Impl_Show(string tooltipStr)
  {
    if (ShowingTooltip)
    {
      Log.Warning("Trying to show more than one tooltip at a time!");
      return;
    }
    
    ShowingTooltip        = true;
    tooltipText.text      = tooltipStr;
    
    SetToolTipPos();
    tooltipCanvas.enabled = true;
  }
  
  private void Imp_Hide()
  {
    if (!ShowingTooltip) return;
    ShowingTooltip        = false;
    tooltipCanvas.enabled = false;
  }
  
  public static void Show(string tooltipStr)
  {
    if (!IsActive) return;
    Instance.Impl_Show(tooltipStr);
  }
  
  public static void Hide()
  {
    if (!IsActive) return;
    Instance.Imp_Hide();
  }
}
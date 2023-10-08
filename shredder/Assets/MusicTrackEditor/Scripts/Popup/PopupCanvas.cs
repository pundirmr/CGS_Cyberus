using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCanvas : MonoBehaviour
{
  [SerializeField] private Canvas popupCanvas;
  [Space]
  [SerializeField] private TMP_Text popupText;
  [SerializeField] private Button yesButton;
  [SerializeField] private Button noButton;
  [Space]
  [SerializeField] private GraphicRaycaster[] rayCastersToDisable = Array.Empty<GraphicRaycaster>();
  
  // NOTE(WSWhitehouse): Classic singleton - suck it up buddy!
  private static PopupCanvas Instance { get; set; }
  private static bool IsActive => Instance != null;
  
  public static bool ShowingPopup { get; private set; } = false;
  
  public delegate void OnPopupButtonPerformedDelegate();
  private OnPopupButtonPerformedDelegate onYesButtonPerformed;
  private OnPopupButtonPerformedDelegate onNoButtonPerformed;

  private void Awake()
  {
    Debug.Assert(Instance == null, "Another Popup Canvas singleton instance already exists! Please remove it!", this);
    Instance = this;
    
    yesButton.onClick.AddListener(OnYesButtonPerformed);
    noButton.onClick.AddListener(OnNoButtonPerformed);
  }

  private void OnYesButtonPerformed()
  {
    onYesButtonPerformed?.Invoke();
    Imp_Hide();
  }

  private void OnNoButtonPerformed()
  {
    onNoButtonPerformed?.Invoke();
    Imp_Hide();
  }

  private void OnDestroy()
  {
    if (Instance != this) return;
    Instance = null;
  }

  private void Impl_Show(string popupTextStr, OnPopupButtonPerformedDelegate onYesCallback, OnPopupButtonPerformedDelegate onNoCallback)
  {
    if (ShowingPopup)
    {
      Log.Warning("Trying to show more than one popup at a time!");
      return;
    }
    
    ShowingPopup         = true;
    popupText.text       = popupTextStr;
    popupCanvas.enabled  = true;
    onYesButtonPerformed = onYesCallback;
    onNoButtonPerformed  = onNoCallback;

    foreach (GraphicRaycaster raycaster in rayCastersToDisable)
    {
      raycaster.enabled = false;
    }
  }
  
  private void Imp_Hide()
  {
    if (!ShowingPopup) return;
    ShowingPopup        = false;
    popupCanvas.enabled = false;
    
    foreach (GraphicRaycaster raycaster in rayCastersToDisable)
    {
      raycaster.enabled = true;
    }
  }
  
  public static void Show(string popupTextStr, OnPopupButtonPerformedDelegate onYesCallback, OnPopupButtonPerformedDelegate onNoCallback)
  {
    if (!IsActive) return;
    Instance.Impl_Show(popupTextStr, onYesCallback, onNoCallback);
  }
  
  public static void Hide()
  {
    if (!IsActive) return;
    Instance.Imp_Hide();
  }
}
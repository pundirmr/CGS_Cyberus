using System;
using UnityEngine;

public class LoadingPopup : MonoBehaviour
{
  [SerializeField] private Canvas loadingCanvas;
  
  private static LoadingPopup Instance;

  private void Awake()
  {
    Instance = this;
    HideLoadingPopup();
  }

  public static void ShowLoadingPopup()
  {
    Instance.loadingCanvas.enabled = true;
  }

  public static void HideLoadingPopup()
  {
    Instance.loadingCanvas.enabled = false;
  }
}
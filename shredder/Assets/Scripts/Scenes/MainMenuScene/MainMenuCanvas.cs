using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class MainMenuCanvas : MonoBehaviour
{
  [Header("Main Screen")]
  [SerializeField] private GameObject mainScreen;
  [SerializeField] private TMP_Text persistantText;
  [SerializeField] private TMP_Text title;

  private void Start()
  {
    
  }

  private void OnDestroy()
  {
    // NOTE(WSWhitehouse): Make sure everything is cleaned up before moving to next scene as some things can persist throughout scenes
    HideAll();
  }
  
  private void ShowMainScreen()
  {
    mainScreen.SetActive(true);
  }

  private void HideMainScreen()
  {
    mainScreen.SetActive(false);
  }

  private void HideAll()
  {
    HideMainScreen();
  }

  public void DisableMainMenu() {
   
  }
}

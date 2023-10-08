using System;
using System.Collections;
using UnityEngine;


public class MainMenuManager : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private MainMenuCanvas canvas;
  //[SerializeField] private AudioClip titleSFX; //Refrence to main menu

  private Coroutine transitionCo;
  private bool waitTimerFinished = false;
  private bool subbed = false;
  
  private void Awake()
  {
    SFX.DEBUG_CreateSFXInstance();
    
    PlayerManager.onPlayerJoined += OnPlayerJoined;
    subbed = true;
    // Force all players to leave when main menu starts
    foreach (PlayerData playerData in PlayerManager.PlayerData)
    {
      if (playerData == null) continue;
      PlayerManager.Leave(playerData);
    }
  }

  private void Start() => StartCoroutine(WaitTimerCountdown());

  private void OnDestroy()
  {
    if (!subbed) return;
    PlayerManager.onPlayerJoined -= OnPlayerJoined;
  }

  private void OnPlayerJoined(int playerID)
  {
    // if we have unsubbed from event or are already waiting to transition out of the scene, we return
    if (!subbed || transitionCo != null) return;
    CoroutineUtil.StartSafelyWithRef(this, ref transitionCo, WaitBeforeTransition());
  }


  // HACK(Zack): This is to artificially slowdown the speed in which players are able to transition out of this scene, to stop a potential soft lock
  private IEnumerator WaitTimerCountdown() {
    float elapsed = 0f;
    while (elapsed < 4.5f) {
      elapsed += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }

    waitTimerFinished = true;
    yield break;
  }

  // HACK(Zack): This is to artificially slowdown the speed in which players are able to transition out of this scene, to stop a potential soft lock
  private IEnumerator WaitBeforeTransition() {
    while (!waitTimerFinished) yield return CoroutineUtil.WaitForUpdate;
    PlayerManager.onPlayerJoined -= OnPlayerJoined;
    subbed = false;

        //VoiceOver.PlayGlobal(titleSFX);
    AudioEventSystem.TriggerEvent("StartCyberusVO", null);
    canvas.DisableMainMenu();
    SceneLoad.LoadNextScene();
    yield break;
  }
}

using System.Collections;
using UnityEngine;

public class StreamDeckDisconnectedUI : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private PlayerID player;
  [SerializeField] private Canvas disconnectedCanvas;
  [SerializeField] private GameObject objectParent; // NOTE(Zack): doing this because im lazy in the ui scenes
  
  private StreamDeck streamDeck => StreamDeckManager.StreamDecks[player.ID];

  private void Awake()
  {
    if (disconnectedCanvas != null) disconnectedCanvas.enabled = false;
    if (objectParent != null)       objectParent.SetActive(false);
  }

  private IEnumerator Start()
  {
    yield return StreamDeckManager.WaitForValidStreamDeck(player.ID);
    
    streamDeck.OnConnect    += OnConnect;
    streamDeck.OnDisconnect += OnDisconnect;
  }

  private void OnDestroy()
  {
    if (!player.IsValid)    return;
    if (streamDeck == null) return;
    
    streamDeck.OnConnect    -= OnConnect;
    streamDeck.OnDisconnect -= OnDisconnect;
  }

  private void OnConnect()
  {
    if (disconnectedCanvas != null) disconnectedCanvas.enabled = false;
    if (objectParent != null)       objectParent.SetActive(false);
  }

  private void OnDisconnect()
  {
    if (disconnectedCanvas != null) disconnectedCanvas.enabled = true;
    if (objectParent != null)       objectParent.SetActive(true);

        //SFX.PlayGameScene(PlayerManager.PlayerDisconnectedSFXs[player.ID]);
        AudioEventSystem.TriggerEvent("PlayerDisconnectedVO", null); //For beta make this a 3D event and set a listener for each player 1 stereo left, player 2 center, player 3 stereo right
  }
}

using System.Collections;
using UnityEngine;

public class PlayerDeathUI : MonoBehaviour
{
  [SerializeField] private PlayerID playerID;
  [SerializeField] private Canvas deathUI;

  private void Awake()
  {
    deathUI.enabled = false;
  }

  private IEnumerator Start()
  {
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    
    playerID.PlayerData.OnPlayerDeath += OnPlayerDeath;
  }

  private void OnDisable()
  {
    if (!PlayerManager.IsPlayerValid(playerID.ID)) return;
    playerID.PlayerData.OnPlayerDeath -= OnPlayerDeath;
  }
  
  private void OnPlayerDeath()
  {
    deathUI.enabled = true;
  }
}

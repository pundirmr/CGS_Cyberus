using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
  [SerializeField] private PlayerID playerID;
  [SerializeField] private PlayerHealthIcon[] healthIcons;
  [SerializeField] private TMP_Text healthText;
  
  private const string _healthTextFormat = "x{0}";
  private static readonly string[] _healthTextStrings;
  
  static PlayerHealthUI()
  {
    _healthTextStrings = new string[PlayerData.MaxHealth + 1];
    for (int i = 0; i < PlayerData.MaxHealth + 1; i++)
    {
      _healthTextStrings[i] = string.Format(_healthTextFormat, i);
    }
  }
  
  private void Awake()
  {
    Debug.Assert(playerID != null, "Player ID is null!", this);
    Debug.Assert(healthIcons.Length == PlayerData.MaxHealth, "Health Icons length doesn't match max health!", this);
  }

  private IEnumerator Start()
  {
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    
    playerID.PlayerData.OnPlayerHealthUpdated += OnPlayerHealthUpdated;
    OnPlayerHealthUpdated();
  }

  private void OnDisable()
  {
    if (!PlayerManager.IsPlayerValid(playerID.ID)) return;
    playerID.PlayerData.OnPlayerHealthUpdated -= OnPlayerHealthUpdated;
  }

  private void OnPlayerHealthUpdated()
  {
    int currentHealth = playerID.PlayerData.CurrentHealth;
    
    healthText.text = _healthTextStrings[currentHealth];

    for (int i = 0; i < PlayerData.MaxHealth; i++)
    {
      if (i < currentHealth)
      {
        healthIcons[i].SetFull();
      }
      else
      {
        healthIcons[i].SetEmpty();
      }
    }
  }
}

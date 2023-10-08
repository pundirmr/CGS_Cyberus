using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDifficultyUI : MonoBehaviour
{
  [SerializeField] private PlayerID id;
  [Space]
  [SerializeField] private Image[] difficultyImages;
  [SerializeField] private Sprite difficultyFullImage;
  [SerializeField] private Sprite difficultyEmptyImage;
  
  private IEnumerator Start() 
  {
    yield return PlayerManager.WaitForValidPlayer(id.ID);
    
    UpdateSprites();
    id.PlayerData.OnDifficultyUpdated += UpdateSprites;
  }

  private void OnDestroy()
  {
    if (!id.IsValid) return;
    
    id.PlayerData.OnDifficultyUpdated -= UpdateSprites;
  }

  private void UpdateSprites()
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetSprites(int index) 
    {
      for (int i = 0; i < index; ++i)                       difficultyImages[i].sprite = difficultyFullImage;
      for (int i = index; i < difficultyImages.Length; ++i) difficultyImages[i].sprite = difficultyEmptyImage;
    }

    // we're assuming we never have any more difficulties than this
    switch (id.PlayerData.Difficulty)
    {
      case NoteDifficulty.EASY:   { SetSprites(1); break; } 
      case NoteDifficulty.MEDIUM: { SetSprites(2); break; }              
      case NoteDifficulty.HARD:   { SetSprites(3); break; } 
      default: break;
    }
  }
}
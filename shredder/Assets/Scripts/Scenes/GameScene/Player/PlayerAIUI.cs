using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAIUI : MonoBehaviour
{
  [SerializeField] private PlayerID playerID;
  [SerializeField] private Image aiImage;
  [SerializeField] private TMP_Text playerNumText;
  
  private void Awake()
  {
    // NOTE(WSWhitehouse): Adding 1 to the ID so it starts at 1 not 0
    playerNumText.text = StaticStrings.IDs[playerID.ID];
  }

  private IEnumerator Start()
  {
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);

    var data = playerID.PlayerData;
    aiImage.sprite = StaticData.AvatarUISprites[data.AvatarIndex][data.ColourIndex];
  }
}

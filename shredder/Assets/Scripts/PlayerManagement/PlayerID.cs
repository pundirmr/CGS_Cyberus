using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerID : MonoBehaviour
{
  [field: SerializeField] public int ID { get; private set; } = -1;
  public bool IsValid => PlayerManager.IsPlayerValid(ID);
  public PlayerData PlayerData => PlayerManager.PlayerData[ID];

  // NOTE(WSWhitehouse): Using a set id function to ensure you don't accidentally set the id
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetID(int id) => ID = id;
}

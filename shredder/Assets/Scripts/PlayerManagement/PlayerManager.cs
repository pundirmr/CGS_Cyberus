using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

public static class PlayerManager
{
  public delegate void PlayerIDEvent(int playerID);
  public static PlayerIDEvent onPlayerJoined;
  public static PlayerIDEvent onPlayerLeft; 

  public delegate IEnumerator PlayerValidCoroutine(int playerID);
  public static PlayerValidCoroutine WaitForValidPlayer;  
    
  public const int MaxPlayerCount = 3;
  public static int PlayerCount   = 0;
  
  public static PlayerData[] PlayerData  = new PlayerData[MaxPlayerCount];
  public static List<int> ValidPlayerIDs = new List<int>(MaxPlayerCount);

  private static AudioClip   ConfirmSFX;
 // public  static AudioClip[] PlayerDisconnectedSFXs;
    
  static PlayerManager() {
    WaitForValidPlayer = __WaitForValidPlayer;
  }

  /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void InitSFXs() {
   ConfirmSFX             = Resources.Load<AudioClip>("SFX/Confirm");
    PlayerDisconnectedSFXs = Resources.LoadAll<AudioClip>("SFX/Player/Disconnected");
    
    Debug.Assert(ConfirmSFX                    != null, "Confirm SFX not loaded correctly");
    Debug.Assert(PlayerDisconnectedSFXs.Length == 3,    "Player Disconnected SFX not loaded correctly");
  }
  */

public static void DebugJoin(int playerID)
{
    // NOTE(Zack): this is mainly for stopping issues with the development [SoakTest]
    if (PlayerData[playerID] != null) return;

    PlayerData[playerID] = new PlayerData(playerID);
    PlayerCount++;
    ValidPlayerIDs.Add(playerID);
    ValidPlayerIDs.Sort();

    // HACK(Zack): should really play [PlayerJoined] sfx with voice acting, but it is bug prone and,
    // we're at the end of the project...    
    SFX.PlayUIScene(ConfirmSFX);


    Log.Print($"Player {playerID} joined!");

    onPlayerJoined?.Invoke(playerID);
}
    
  public static void Join(int playerID)
  {
    // NOTE(Zack): this is mainly for stopping issues with the development [SoakTest]
    if (PlayerData[playerID] != null) return;
    
    PlayerData[playerID] = new PlayerData(playerID);
    PlayerCount++;
    ValidPlayerIDs.Add(playerID);
    ValidPlayerIDs.Sort();


    // HACK(Zack): should really play [PlayerJoined] sfx with voice acting, but it is bug prone and,
    // we're at the end of the project...    
    //SFX.PlayUIScene(ConfirmSFX);
    AudioEventSystem.TriggerEvent("PlayerJoinedVO", null);
    
    
    Log.Print($"Player {playerID} joined!");    
    StreamDeckManager.StreamDecks[playerID].ConsumeInput();
    StreamDeckManager.StreamDecks[playerID].ClearDeck();
    onPlayerJoined?.Invoke(playerID);
  }
  
  public static void Leave(PlayerData playerData) => Leave(playerData.PlayerID);

  public static void Leave(int playerID)
  {
    // NOTE(WSWhitehouse): If player doesn't exist at this ID ignore the leave request
    if (PlayerData[playerID] == null) return;
    
    PlayerData[playerID] = null;
    PlayerCount--;
    ValidPlayerIDs.Remove(playerID);
    Log.Print($"Player {playerID} left!");
    
    onPlayerLeft?.Invoke(playerID);
  }
  
  // --- UTIL --- //

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsIDValid(int id)
  {
    if (id < 0)            return false;
    if (id >= PlayerCount) return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPlayerValid(int id) { return PlayerData[id] != null; }
  
  private static IEnumerator __WaitForValidPlayer(int playerID)
  {
    while (PlayerData[playerID] == null) yield return CoroutineUtil.WaitForUpdate;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetPlayerWithHighestDifficultyLevel()
  {
    int playerID = -1;
    NoteDifficulty difficulty = NoteDifficulty.EASY;

    for (int i = 0; i < ValidPlayerIDs.Count; i++)
    {
      NoteDifficulty newDifficulty = PlayerData[ValidPlayerIDs[i]].Difficulty;
      if (difficulty >= newDifficulty) continue;
      
      playerID   = i;
      difficulty = newDifficulty;
      
      // NOTE(WSWhitehouse): If the difficulty is already the hardest then
      // return the player ID as there's no need to continue searching
      if (difficulty == NoteDifficulty.HARD) return playerID;
    }
    
    return playerID;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static NoteDifficulty GetHighestDifficultyLevel()
  {
    NoteDifficulty difficulty = NoteDifficulty.EASY;

    foreach (int playerIndex in ValidPlayerIDs)
    {
      NoteDifficulty newDifficulty = PlayerData[playerIndex].Difficulty;
      if (difficulty >= newDifficulty) continue;
      
      difficulty = newDifficulty;
      
      // NOTE(WSWhitehouse): If the difficulty is already the hardest then
      // return the player ID as there's no need to continue searching
      if (difficulty == NoteDifficulty.HARD) return difficulty;
    }
    
    return difficulty;
  }
}

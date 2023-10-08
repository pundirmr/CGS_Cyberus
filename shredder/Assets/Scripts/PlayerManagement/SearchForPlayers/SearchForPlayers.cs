using UnityEngine;

public static class SearchForPlayers
{
  public static PlayerSearch[] PlayerSearches = new PlayerSearch[PlayerManager.MaxPlayerCount];

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Init()
  {
    Log.Print("Initialising Search For Players!");
    for (int i = 0; i < PlayerManager.MaxPlayerCount; i++)
    {
      PlayerSearches[i] = new PlayerSearch(i);
    }
  }
}

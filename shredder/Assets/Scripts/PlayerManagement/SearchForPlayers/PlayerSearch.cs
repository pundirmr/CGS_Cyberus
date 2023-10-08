using System.Collections;
using UnityEngine;

public class PlayerSearch
{
  public bool SearchingForPlayer;
  public int PlayerID;
  public delegate void OnSearchingEvent();
  public OnSearchingEvent OnStartSearching;
  public OnSearchingEvent OnStopSearching;
  
  private Coroutine _startSearchCoroutine;

  public PlayerSearch(int playerID)
  {
    PlayerID = playerID;
    PlayerManager.onPlayerLeft += OnPlayerLeft;
    
    // NOTE(WSWhitehouse): Dont search for player if one already exists
    if (PlayerManager.PlayerData[PlayerID] != null) return;
    
    CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref _startSearchCoroutine, StartSearch());
  }

  ~PlayerSearch()
  {
    StopSearch();
    PlayerManager.onPlayerLeft -= OnPlayerLeft;
  }

  private IEnumerator StartSearch()
  {
    // NOTE(WSWhitehouse): We dont want to do anything with stream decks while we're in the start scene
    while (SceneHandler.SceneIndex == 0) yield return CoroutineUtil.WaitForUpdate;
    
    yield return StreamDeckManager.WaitForValidStreamDeck(PlayerID);
    StreamDeckManager.StreamDecks[PlayerID].OnDeckInputPerformed += OnDeckInputPerformed;
    
    SearchingForPlayer = true;
    OnStartSearching?.Invoke();
  }

  private void StopSearch()
  {
    if (!SearchingForPlayer) return;
    
    CoroutineUtil.StopSafelyWithRef(StaticCoroutine.Mono, ref _startSearchCoroutine);
    StreamDeckManager.StreamDecks[PlayerID].OnDeckInputPerformed -= OnDeckInputPerformed;
    
    SearchingForPlayer = false;
    OnStopSearching?.Invoke();
  }

  private void OnDeckInputPerformed(int buttonIndex)
  {
    PlayerManager.Join(PlayerID);
    StopSearch();
  }

  private void OnPlayerLeft(int playerID)
  {
    // NOTE(WSWhitehouse): Check that the player that left is the one we care about
    if (PlayerID != playerID) return;
    
    // NOTE(WSWhitehouse): Restart search if player leaves
    CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref _startSearchCoroutine, StartSearch());
  }
}
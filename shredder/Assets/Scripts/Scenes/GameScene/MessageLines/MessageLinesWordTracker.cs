using UnityEngine;

/// <summary>
/// Keeps track of what words players are hitting and fires an event to update the message lines.
/// Talks to the <see cref="MessageLinesCanvas"/> to update the individual lines. This script
/// listens to laser inputs and determines when and how a word should be removed.
/// </summary>
public class MessageLinesWordTracker : MonoBehaviour
{
  public delegate void WordEliminatedDel(int wordIndex, LaserHitTiming hitTiming);
  public WordEliminatedDel OnWordEliminated;
  
  private Laser.LaserHitEvent[] _onLaserInputEvents;

  private void Awake()
  {
    // Create input event array and set everything to null
    _onLaserInputEvents = new Laser.LaserHitEvent[PlayerManager.MaxPlayerCount];
    for (int i = 0; i < _onLaserInputEvents.Length; i++) { _onLaserInputEvents[i] = null; }
  }

  private void Start()
  {
    // Subscribe to already joined players
    foreach (int playerIndex in PlayerManager.ValidPlayerIDs)
    {
      SubscribeToLaserEvent(playerIndex);
    }

    // If new players join sub to their lasers too
    PlayerManager.onPlayerJoined += SubscribeToLaserEvent;

    // sub to the OnWordBlockDestroyed for the [ClearLane] script 
    // ClearLane.OnWordBlockDestroyed += OnLaserInput;
  }

  private void SubscribeToLaserEvent(int playerID)
  {
    // NOTE(WSWhitehouse): Copying playerID into local variable to allow it to be "captured" so it can be used in
    // the lambda properly. Not doing this could result in a bug where all delegates use the same player ID.
    int playerIndex = playerID;
    
    _onLaserInputEvents[playerIndex] = delegate(Laser.LaserHitInfo info) { OnLaserInput(playerIndex, info); };
      
    GameManager.PlayerLasers[playerIndex].OnLaserHit  += _onLaserInputEvents[playerIndex];
    GameManager.PlayerLasers[playerIndex].OnLaserMiss += _onLaserInputEvents[playerIndex];
  }

  private void OnDestroy()
  {
    PlayerManager.onPlayerJoined -= SubscribeToLaserEvent;
    
    for (int i = 0; i < _onLaserInputEvents.Length; i++)
    {
      if (_onLaserInputEvents[i] == null) continue;

      GameManager.PlayerLasers[i].OnLaserHit  -= _onLaserInputEvents[i];
      GameManager.PlayerLasers[i].OnLaserMiss -= _onLaserInputEvents[i];
    }

    // ClearLane.OnWordBlockDestroyed -= OnLaserInput;
  }

  private void OnLaserInput(int playerID, Laser.LaserHitInfo info)
  {
    OnWordEliminated?.Invoke(info.WordData.wordIndex, info.Timing);
  }
}

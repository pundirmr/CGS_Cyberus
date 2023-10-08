using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// The line that is played on each note, should help to keep players in time.
/// </summary>
public class NoteLineSpawner : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private PlayerID player; 
  [Space]
  [SerializeField] private NoteLine noteLinePrefab;
  [SerializeField] private int initialPoolSize = 30;
  [SerializeField] private int poolResizeFactor = 2;
  
  private Queue<NoteLine> _noteLinePool;
  
  private Lane lane   => GameManager.PlayerLanes[player.ID];
  private Laser laser => GameManager.PlayerLasers[player.ID];
  
  // NOTE(WSWhitehouse): Assigning func ptr to delegate up front to avoid delegate allocation every time we have a beat
  private delegate IEnumerator AnimateNoteLineDel(NoteData data, NoteLine line);
  private AnimateNoteLineDel _animateNoteLine;

  private void Awake()
  {
    // Assigning func ptr to delegate
    _animateNoteLine = AnimateNoteLine;
  }

  private IEnumerator Start()
  {
    // Creating beat line pool and instantiating the initial amount
    _noteLinePool = new Queue<NoteLine>(initialPoolSize);
    for (int i = 0; i < initialPoolSize; i++)
    {
      InstantiateNoteLine();
    }
    
    yield return PlayerManager.WaitForValidPlayer(player.ID);
    
    MusicTrackPlayer.OnNote         += ShowNoteLine;
    player.PlayerData.OnPlayerDeath += OnPlayerDeath;
  }

  private void OnDestroy()
  {
    if (!player.IsValid)          return;
    if (player.PlayerData.IsDead) return; 

    MusicTrackPlayer.OnNote         -= ShowNoteLine;
    player.PlayerData.OnPlayerDeath -= OnPlayerDeath;
  }

  private void OnPlayerDeath()
  {
    MusicTrackPlayer.OnNote         -= ShowNoteLine;
    player.PlayerData.OnPlayerDeath -= OnPlayerDeath;
  }

  private void InstantiateNoteLine()
  {
    // Instantiate
    NoteLine line = Instantiate(noteLinePrefab, this.transform);
    
    // Set up line
    line.gameObject.SetActive(false);
    line.transform.position = lane.StartTransform.position;
    line.transform.LookAt(lane.StartTransform.position + lane.LaneNormal);
    
    // Add line to the queue
    _noteLinePool.Enqueue(line);
  }

  private void ShowNoteLine(NoteData data)
  {
    // If there are no beat lines in the pool create a new one
    if (_noteLinePool.Count <= 0)
    {
      // NOTE(WSWhitehouse): This is an error not a warning so it cant be ignored!
      Log.Error("NoteLineSpawner pool has been resized! Consider increasing the initial spawn amount!");
      
      int newSize = _noteLinePool.Count * poolResizeFactor;
      int spawnAmount = maths.Abs(newSize - _noteLinePool.Count);
      
      for (int i = 0; i < spawnAmount; i++)
      {
        InstantiateNoteLine();
      }
    }
    
    // Get a new line object from the pool and start animation
    NoteLine line = _noteLinePool.Dequeue();
    
    // Set size of line
    if (data.isMainBeat) {
        line.transform.localScale = (Vector2)line.mainBeatSize;
        line.activeCol            = line.mainBeatColour;
    } else {
        line.transform.localScale = (Vector2)line.defaultSize;
        line.activeCol            = line.defaultColour;
    }
    line.transform.localScale = (Vector2)(data.isMainBeat ? line.mainBeatSize : line.defaultSize);
    
    // Animate line movement along lane
    StartCoroutine(_animateNoteLine(data, line));
  }

  private IEnumerator AnimateNoteLine(NoteData data, NoteLine line)
  {
    // Set up beat line
    line.gameObject.SetActive(true);
    line.image.color = line.activeCol;

    float3 startValue = lane.StartTransform.position;
    float3 endValue   = laser.transform.position;
    
    double timeOvershoot = data.timeOvershoot;
    
    double startTime     = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
    double timeElapsed   = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - startTime + timeOvershoot;
    
    double laserDuration = GameManager.Track.LaneDuration * GameManager.LaserPosAlongLane;
    double duration      = GameManager.Track.LaneDuration - laserDuration;

    while (timeElapsed < duration)
    {
      double t = timeElapsed / duration;
      line.transform.position = float3Util.Lerp(startValue, endValue, (float)t);
      
      timeElapsed = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - startTime + timeOvershoot;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    // // Get new start and end
    // startValue = endValue;
    // endValue   = lane.EndTransform.position;
    //
    // // Reset time variables
    // startTime   = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - (duration - timeElapsed);
    // timeElapsed = 0.0;
    //
    // while (timeElapsed < laserDuration)
    // {
    //   double t = timeElapsed / laserDuration;
    //   line.transform.position = float3Util.Lerp(startValue, endValue, (float)t);
    //   line.image.color        = Colour.Lerp(line.activeCol, line.inactiveCol, (float)t);
    //   
    //   timeElapsed = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - startTime + timeOvershoot;
    //   yield return CoroutineUtil.WaitForUpdate;
    // }
    

    // Hide beat line and return to queue
    line.gameObject.SetActive(false);
    _noteLinePool.Enqueue(line);
  }
}

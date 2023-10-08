using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct DifficultySettings
{
  public int numberOfHitsToResetMissStreak;
  public int numberOfMissesToResetHitStreak;
  [Space] 
  public int numberOfHitsToIncreaseDifficulty;
  public int numberOfMissesToDecreaseDifficulty;
  [Space] 
  [Tooltip("Only hits with a timing included in this list are counted towards advancing the difficulty")]
  public LaserHitTiming hitTimingsToIncreaseDifficulty;
  [Tooltip("Hits with a timing included in this list are counted towards decreasing the difficulty")]
  public LaserHitTiming hitTimingsToDecreaseDifficulty;
}

public class PlayerDifficulty : MonoBehaviour
{
  [Header("Scene References")] 
  [SerializeField] private PlayerID playerID;
  [SerializeField] private Laser laser;

  [Header("Difficulty Settings")]
  [SerializeField] private DifficultySettings easyDifficultySettings   = new DifficultySettings();
  [SerializeField] private DifficultySettings mediumDifficultySettings = new DifficultySettings();
  [SerializeField] private DifficultySettings hardDifficultySettings   = new DifficultySettings();

  // NOTE(WSWhitehouse): There are different difficulty settings for each difficulty
  // tier, this property returns the correct settings for the tier the player is on.
  private DifficultySettings DifficultySettings =>
    PlayerData.Difficulty switch
    {
      NoteDifficulty.EASY   => easyDifficultySettings,
      NoteDifficulty.MEDIUM => mediumDifficultySettings,
      NoteDifficulty.HARD   => hardDifficultySettings,
      _                     => throw new ArgumentOutOfRangeException()
    };

  private PlayerData PlayerData => playerID.PlayerData;
  
  private DelegateUtil.EmptyCoroutineDel OnPlayerHealthDecreased;

  private readonly int LaserHitTimingCount = Enum.GetNames(typeof(LaserHitTiming)).Length;

  private void Awake()
  {
    OnPlayerHealthDecreased = __OnPlayerHealthDecreased;
    
    Debug.Assert(playerID != null, "PlayerID is null! Please assign one!", this);
    Debug.Assert(laser    != null, "Laser is null! Please assign one!", this);

    laser.OnLaserMiss += OnLaserMiss;
    laser.OnLaserHit  += OnLaserHit;
  }

  private IEnumerator Start()
  {
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    
    PlayerData.OnPlayerHealthUpdated += OnPlayerHealthUpdated;
  }

  private void OnDestroy()
  {
    laser.OnLaserMiss -= OnLaserMiss;
    laser.OnLaserHit  -= OnLaserHit;
    
    if (!playerID.IsValid) return;
    
    PlayerData.OnPlayerHealthUpdated += OnPlayerHealthUpdated;
  }

  private void Update()
  {
    if (!playerID.IsValid) return;
    
    PlayerData.AvgDifficultyCounter[PlayerData.Difficulty] += Time.deltaTime;
  }

  private void OnPlayerHealthUpdated()
  {
    // NOTE(WSWhitehouse): We're assuming that the health will never go back up here so always call the decrease function
    StartCoroutine(OnPlayerHealthDecreased());
  }

  private IEnumerator __OnPlayerHealthDecreased()
  {
    // NOTE(WSWhitehouse): Waiting one frame to ensure the health animation can occur before the difficulty one!
    yield return CoroutineUtil.WaitForUpdate;
    DecreaseDifficulty();
  }

  private void OnLaserHit(Laser.LaserHitInfo info)
  {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    if (DebugOverride.IgnoreDifficulty) return;
    #endif

    PlayerData.HitStreak[info.Timing]++;

    if ((DifficultySettings.hitTimingsToDecreaseDifficulty & info.Timing) != 0)
    {
      OnLaserMiss(info);
      return;
    }

    int totalHitStreak, difficultyHitStreak;
    GetHitStreak(out totalHitStreak, out difficultyHitStreak);

    if (difficultyHitStreak >= DifficultySettings.numberOfHitsToResetMissStreak)
    {
      ResetMissStreak();
    }
    
    if (difficultyHitStreak >= DifficultySettings.numberOfHitsToIncreaseDifficulty)
    {
      IncreaseDifficulty();
    }
  }

  private void OnLaserMiss(Laser.LaserHitInfo info)
  {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    if (DebugOverride.IgnoreDifficulty) return;
    #endif
    
    PlayerData.MissStreak++;

    if (PlayerData.MissStreak >= DifficultySettings.numberOfMissesToResetHitStreak)
    {
      ResetHitStreak();
    }
    
    if (PlayerData.MissStreak >= DifficultySettings.numberOfMissesToDecreaseDifficulty)
    {
      DecreaseDifficulty();
    }
  }

  // NOTE(WSWhitehouse): This function returns the total hit streak and the difficulty hit streak,
  // which is how many hits of the allowed LaserHitTiming for the players current difficulty level.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void GetHitStreak(out int total, out int difficulty)
  {
    total      = 0;
    difficulty = 0;

    for (int i = 0; i < LaserHitTimingCount; i++)
    {
      LaserHitTiming hitTiming = (LaserHitTiming)(1 << i);

      total += PlayerData.HitStreak[hitTiming];

      if ((DifficultySettings.hitTimingsToIncreaseDifficulty & hitTiming) != 0)
      {
        difficulty += PlayerData.HitStreak[hitTiming];
      }
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ResetMissStreak()
  {
    PlayerData.MissStreak = 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ResetHitStreak()
  {
    for (int i = 0; i < LaserHitTimingCount; i++)
    {
      PlayerData.HitStreak[(LaserHitTiming)(1 << i)] = 0;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void IncreaseDifficulty()
  {
    ResetMissStreak();
    ResetHitStreak();
    
    // NOTE(WSWhitehouse): Cant pass hard difficulty so return
    if (PlayerData.Difficulty == NoteDifficulty.HARD) return;
    
    switch (PlayerData.Difficulty)
    {
      case NoteDifficulty.EASY:
      {
        PlayerData.Difficulty = NoteDifficulty.MEDIUM;
        break;
      }

      case NoteDifficulty.MEDIUM:
      {
        PlayerData.Difficulty = NoteDifficulty.HARD;
        break;
      }
      
      default: break;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void DecreaseDifficulty()
  {
    ResetMissStreak();
    ResetHitStreak();
    
    // NOTE(WSWhitehouse): Cant pass easy difficulty so return
    if (PlayerData.Difficulty == NoteDifficulty.EASY) return;

    switch (PlayerData.Difficulty)
    {
      case NoteDifficulty.MEDIUM:
      {
        PlayerData.Difficulty = NoteDifficulty.EASY;
        break;
      }
      
      case NoteDifficulty.HARD:
      {
        PlayerData.Difficulty = NoteDifficulty.MEDIUM; 
        break;
      }

      default: break;
    }
  }
}

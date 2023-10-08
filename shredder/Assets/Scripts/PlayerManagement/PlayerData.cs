using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerData
{
  private static NoteDifficulty[] NoteDifficulties;
  private static LaserHitTiming[] laserHitTimings;
  
  static PlayerData()
  {
    NoteDifficulties = Enum.GetValues(typeof(NoteDifficulty)) as NoteDifficulty[]; 
    laserHitTimings  = Enum.GetValues(typeof(LaserHitTiming)) as LaserHitTiming[]; 
  }
  
  public PlayerData(int playerID)
  {
    PlayerID      = playerID;
    SceneJoinedOn = (Scene)SceneHandler.SceneIndex;

    // randomly set a players data depending on what scene a player joins
    // NOTE(Zack): we deliberately fall through certain case statements
    switch (SceneJoinedOn) {
        // case Scene.REPORT_SCENE: break;
        
        case Scene.TEST_SCENE:
        case Scene.NUMBER_OF_SCENES: // if we are in a scene that is not part of the [BuildIndex] then we default to initializing all of player data
          
        case Scene.START_SCENE:
        case Scene.MAIN_MENU:
        case Scene.AVATAR_SELECT:
        case Scene.TRACK_SELECT:
        case Scene.GAME_SCENE:
        case Scene.REPORT_SCENE:
        default: {
          AvatarIndex = PlayerID;
          Avatar      = StaticData.Avatars[AvatarIndex];

          ColourIndex     = Random.Range(0, StaticData.ColourSchemes.Length);
          ColourScheme    = StaticData.ColourSchemes[ColourIndex];
          HDRColourScheme = StaticData.ColourSchemesHDR[ColourIndex];
        } break;
    }
    
    // Create HitStreak Dictionary
    HitStreak = new Dictionary<LaserHitTiming, int>(laserHitTimings.Length);

    // NOTE(WSWhitehouse): Set up the dictionary with initial values so we dont get a key not found
    for (int i = 0; i < laserHitTimings.Length;  i++)
    {
      LaserHitTiming hitTiming = laserHitTimings[i];
      
      if (HitStreak.ContainsKey(hitTiming))
      {
        HitStreak[hitTiming] = 0;
        continue;
      }
      
      HitStreak.Add(hitTiming, 0);
    }
    
    // Create Avg Difficulty Counter Dictionary
    AvgDifficultyCounter = new Dictionary<NoteDifficulty, float>(NoteDifficulties.Length);

    // NOTE(WSWhitehouse): Set up the dictionary with initial values so we dont get a key not found
    for (int i = 0; i < NoteDifficulties.Length;  i++)
    {
      NoteDifficulty noteDifficulty = NoteDifficulties[i];
      
      if (AvgDifficultyCounter.ContainsKey(noteDifficulty))
      {
        AvgDifficultyCounter[noteDifficulty] = 0.0f;
        continue;
      }
      
      AvgDifficultyCounter.Add(noteDifficulty, 0.0f);
    }
  }
  
  public int PlayerID { get; }
  
  public ColourScheme ColourScheme;
  public ColourScheme HDRColourScheme;
  public Avatar Avatar;
  public int AvatarIndex;
  public int ColourIndex;
  
  // --- HEALTH --- //
  public const int MaxHealth = 3;
  
  private int _currentHealth = MaxHealth;
  public int CurrentHealth
  {
    get => _currentHealth;
    set
    {
      _currentHealth = maths.Clamp(value, 0, MaxHealth);
      OnPlayerHealthUpdated?.Invoke();
      if (IsDead) {
          OnPlayerDeath?.Invoke();

          // HACK(Zack): // HACK(Zack):
          VoiceOver.Play(PlayerID, StaticData.AIEliminatedSFX);
      }
    }
  }
  
  public Action OnPlayerHealthUpdated;
  public Action OnPlayerDeath;
  
  public bool IsDead => CurrentHealth <= 0;
  
  // --- STATISTICS --- //

  // to keep track of the amount of different timings that the player has made
  public int Misses, EarlyLates, Goods, Perfects;

  public int CurrentCombo;
  public int HighestCombo;
  public Action OnComboUpdated; // NOTE(WSWhitehouse): Invoked by Laser.UpdatePlayerDataCombo()

  public int TotalSpam;

  // NOTE(Zack): This is used to query what scene players have joined from, in [ReportCardUI]
  public Scene SceneJoinedOn;
  
  // -- DIFFICULTY VALUES -- //
  // NOTE(WSWhitehouse): We default to medium as we want players to start on the middle
  // difficulty so the game can get easier or harder to accommodate them.
  
  private NoteDifficulty _difficulty = NoteDifficulty.HARD;
  
  public NoteDifficulty Difficulty
  {
    get => _difficulty;
    set
    {
      if (_difficulty == value) return;
      PrevDifficulty = _difficulty;
      _difficulty    = value;
      
      OnDifficultyUpdated?.Invoke();
    }
  }
  
  public NoteDifficulty PrevDifficulty { get; private set; } = NoteDifficulty.NONE;
  
  public Dictionary<NoteDifficulty, float> AvgDifficultyCounter;

  public Action OnDifficultyUpdated;
  
  public int MissStreak;
  public Dictionary<LaserHitTiming, int> HitStreak;
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
  // --- CONST --- //
  public const int NumberOfSublanes = 3;
  
  // --- NON STATIC --- //
  
#if UNITY_EDITOR
  [Header("Debug")]
  [Tooltip("This track will only be chosen if the static one is null")]
  [SerializeField] private MusicTrack musicTrack;
  [SerializeField] private bool randomlyChoose = false;
#endif
  
  // Serialized Fields
  [Header("Components")]
  [SerializeField] private WarningCanvas warningCanvas;
  [SerializeField] private EmailMessageUI emailMessageUI;
  [SerializeField] private EmailClearedUI EmailClearedUI;
  [SerializeField] private MessageLinesCanvas messageLinesCanvas;
  [SerializeField] private GameObject spam2DBox;
  [SerializeField] private CanvasGroup spam2DCanvasGroup;
  
  [Header("Warning State")]
  [SerializeField] private GlitchEffectParams warningStateGlitchEffect;
  
  [Header("Crosshair Search")]
  [SerializeField] private CharacterCrosshair crosshair;
  [SerializeField] private GlitchEffectParams crosshairSearchGlitchEffect;
  [SerializeField] private float crosshairSearchGlitchLerpInDuration;
  
  [Header("Character Pick State")]
  [SerializeField] private CharacterPicker characterPicker;
  [SerializeField] private GlitchEffectParams characterPickGlitchEffect;
  [SerializeField] private float characterPickGlitchLerpInDuration;

  [SerializeField] private GlitchEffectParams characterShowGlitchEffect;
  [SerializeField] private float characterShowGlitchLerpInDuration;
  
  
  [Header("Spam Message Intro State")]
  [SerializeField] private string spamMessageIntroTitle = "Spam Incoming";
  [SerializeField] private GlitchEffectParams spamMessageGlitchEffect;
  [SerializeField] private float spamMessageGlitchLerpInDuration;
  [SerializeField] private float spamMessageAlphaLerpInDuration;
  [SerializeField] private float spamMessageTimePerPage;
  [SerializeField] private float spamMessageWaitBeforeLerpOut;
  [SerializeField] private float spamMessageLerpOutDuration;
  [SerializeField] private float clearedUIWaitBeforeLerpOut = 1f;
  
  [Header("Gameplay Transition")]
  [SerializeField] private float spam2DAlphaTransitionDuration;
  [SerializeField] private float disableGlitchDuration;
  
  [Header("Spam Message Outro State")]
  [SerializeField] private string spamMessageOutroTitle = "Spam Destroyed";

  [Header("Player")]
  [SerializeField] private PlayerID[] players;
  // NOTE(Zack): if this variable name is changed [Laser.cs] line ~145 must also be updated
  [SerializeField] [Range(0.0f, 1.0f)] public float laserPosAlongLane = 0.1f;

  [Header("UI References")]
  [SerializeField] private LaneWalls laneWalls;
  [SerializeField] private ScrollingTextureEffect laneBackground;
  [SerializeField] private PlayerUIWrapper[] playerUIWrappers;

 /* [Header("SFX References")]
  [SerializeField] private AudioClip preGameEmailMessageSFX;
  [Space]
 
  [SerializeField] private AudioSource onTrackEndedSFX;
  [Space]
  
  [SerializeField] private AudioClip postGameEmailShowSFX;
  [SerializeField] private AudioClip postGameEmailKeywordsSFX;
  [SerializeField] private AudioClip postGameEmailKeywordEnumerateSFX;
  [SerializeField] private AudioClip postGameEmailHideSFX;
  [SerializeField] private AudioClip postGameEmailStaticSFX;
  [SerializeField] private AudioClip postGameEmailClearedShowSFX; 
  [SerializeField] private AudioClip postGameEmailClearedHideSFX;
  */
  [Space]
  
  [SerializeField] private AudioSource backgroundStaticSource;
  [SerializeField, Range(0f, 1f)] private float backgroundStaticMaxVol = 0.25f;


  
  // --- STATIC --- //
  
  // Singleton
  private static GameManager Instance;
  
  // Components
  public static Camera Camera => StaticCamera.Main;
  public static MessageLinesCanvas MessageLinesCanvas { get; private set; }
  public static EmailMessageUI EmailMessageUI           { get; private set; }
  public static GameObject Spam2DBox                  { get; private set; }

  // Music Track
  // NOTE(WSWhitehouse): The track is set in the TrackSelect scene by TrackSelect.cs
  public static MusicTrack Track { get; set; }
  public static SpamMessage SpamMessage => Track.SpamMessage;
  
  // Game State
  public enum State
  {
    INVALID_STATE = -1,
    WARNING_INTRO,
    CROSSHAIR_SEARCH,
    CHARACTER_PICK,
    SPAM_MSG_INTRO,
    GAMEPLAY,
    SPAM_MSG_OUTRO
  }
  
  public static State GameState = State.INVALID_STATE;

  // Player
  public static Lane[] PlayerLanes              { get; private set; }
  public static Laser[] PlayerLasers            { get; private set; }
  public static PlayerSpamBox[] PlayerSpamBoxes { get; private set; }
  public static float LaserPosAlongLane         { get; private set; }
  
  // Player Deaths
  public static int NumberOfPlayersDead { get; private set; }
  public static bool AllPlayersDead => NumberOfPlayersDead >= PlayerManager.PlayerCount;
  public static Action OnAllPlayersDead;
  
  public static Action OnGameEnd;
  
  private DelegateUtil.EmptyCoroutineDel ShowSpamMessageOutroCoroutine;
  private delegate IEnumerator UILerpDel(int playerID);
  private static UILerpDel WaitToLerpPlayerUIOnScreen;


  private delegate IEnumerator MusicDel(AudioSource source, float startVolume, float endVolume, float duration, bool stopOnExit);
  private MusicDel FadeMusicSource;

  // helpers
  private float halfSpamMessageTimePerPage => spamMessageTimePerPage * 0.5f;
  
  private void Awake()
  {
    if (Instance != null)
    {
      Log.Error("Another GameManager Instance in the scene! Please ensure there is only one!", this);
      Destroy(this.gameObject);
      return;
    }

    Instance = this;

   /* // setup sfx for this scene in the Unity editor
    SFX.DEBUG_CreateSFXInstance();
    VoiceOver.DEBUG_CreateSFXInstance();

    
    // setup static sfx
    backgroundStaticSource.clip = postGameEmailStaticSFX;
    */
    
    // Set up delegates
    ShowSpamMessageOutroCoroutine = __ShowSpamMessageOutroCoroutine;
    WaitToLerpPlayerUIOnScreen    = __WaitToLerpPlayerUIOnScreen;
    FadeMusicSource               = __FadeMusicSource;

    
    // Asserts
    Debug.Assert(warningCanvas      != null, "WarningFlash has not been assigned!",       this);
    Debug.Assert(messageLinesCanvas != null, "MessageLinesCanvas has not been assigned!", this);
    Debug.Assert(emailMessageUI     != null, "SpamMessageUI has not been assigned!",      this);
    Debug.Assert(spam2DBox          != null, "Spam2DBox has not been assigned!",          this);
    
    Debug.Assert(players != null, "Players == null!", this);
    Debug.Assert(players.Length == PlayerManager.MaxPlayerCount, "The players array length doesn't match max player count!", this);

    
    #if UNITY_EDITOR
    // Check for valid music track
    if (Track == null)
    {
      // NOTE(WSWhitehouse): Select randomly choose if the music track is null
      if (musicTrack == null) randomlyChoose = true;

            Track = StaticData.MusicTracks[6];// randomlyChoose ? StaticData.MusicTracks[6] : musicTrack;
      Log.Print($"Static Track was null! Setting MusicTrack to: {Track.name}");
    }
    #else
    if (Track == null)
    {
      Log.Error("TRACK IS NULL!");
      return;
    }
    #endif

    
    // Set up game state
    GameState = State.INVALID_STATE;

    
    // Set up static variables
    LaserPosAlongLane    = laserPosAlongLane;
    EmailMessageUI       = emailMessageUI;
    MessageLinesCanvas   = messageLinesCanvas;
    Spam2DBox            = spam2DBox;
    NumberOfPlayersDead  = 0;
     
    PlayerLanes     = new Lane[PlayerManager.MaxPlayerCount];
    PlayerLasers    = new Laser[PlayerManager.MaxPlayerCount];
    PlayerSpamBoxes = new PlayerSpamBox[PlayerManager.MaxPlayerCount];

    for (int i = 0; i < PlayerManager.MaxPlayerCount; i++)
    {
      int playerID = players[i].ID;
      
      PlayerLanes[playerID]     = players[i].GetComponentInChildren<Lane>(true);
      PlayerLasers[playerID]    = players[i].GetComponentInChildren<Laser>(true);
      PlayerSpamBoxes[playerID] = players[i].GetComponentInChildren<PlayerSpamBox>(true);
      
      Debug.Assert(PlayerLanes[playerID] != null,     $"Player {playerID} lane is null!", this);
      Debug.Assert(PlayerLasers[playerID] != null,    $"Player {playerID} laser is null!", this);
      Debug.Assert(PlayerSpamBoxes[playerID] != null, $"Player {playerID} spam box is null!", this);
    }

    
    // Disable objects not required for first game state
    EmailMessageUI.gameObject.SetActive(false);
    spam2DCanvasGroup.alpha = 0.0f;
    foreach (PlayerID player in players) { player.gameObject.SetActive(false); }
    
    MusicTrackPlayer.OnTrackFinished += ShowSpamMessageOutro;

    
    // Subscribe to all player death events, and allocate arrays to keep track of word timings
    for (int i = 0; i < PlayerManager.ValidPlayerIDs.Count; i++)
    {
      int playerID = PlayerManager.ValidPlayerIDs[i];
      PlayerManager.PlayerData[playerID].OnPlayerDeath += OnPlayerDeath;
    }

    
    // NOTE(WSWhitehouse): Make sure to subscribe to any new players that join
    PlayerManager.onPlayerJoined += OnPlayerJoined;

    // NOTE(Zack): subscribe to event for the end of the fade transition to start the game intro
    LoadingScreen.OnHidingLoadingScreen += StartSceneIntro;
    
    // NOTE(Felix): Tell the scene colourer to apply the colour scheme
    SceneColourer.UpdateSceneMaterialsColours(Track);
  }

  

  // NOTE(Zack): we initialize the Dictionary for the average word timings
  private void Start() => AverageWordTimings.SetupTimingDictionary(SpamMessage);
  
  private void OnDestroy()
  {
    if (Instance != this) return;
    Instance = null;
    
    MusicTrackPlayer.OnTrackFinished -= ShowSpamMessageOutro;
    
    for (int i = 0; i < PlayerManager.ValidPlayerIDs.Count; i++)
    {
      int playerID = PlayerManager.ValidPlayerIDs[i];
      PlayerManager.PlayerData[playerID].OnPlayerDeath -= OnPlayerDeath;
    }

    backgroundStaticSource.Stop();
  }

  private static void OnPlayerJoined(int playerID)
  {
    ref PlayerData playerData = ref PlayerManager.PlayerData[playerID];
    playerData.OnPlayerDeath += OnPlayerDeath;

    // if a player joins mid way through either the intro or outro we don't care about turning their ui on
    if (GameState is State.INVALID_STATE or State.WARNING_INTRO or State.SPAM_MSG_INTRO or State.SPAM_MSG_OUTRO) return;
    StaticCoroutine.Start(WaitToLerpPlayerUIOnScreen(playerID));
  }

  private static void OnPlayerDeath()
  {
    NumberOfPlayersDead = 0;
    for (int i = 0; i < PlayerManager.ValidPlayerIDs.Count; i++)
    {
      int playerID = PlayerManager.ValidPlayerIDs[i];
      if (PlayerManager.PlayerData[playerID].IsDead)
      {
        NumberOfPlayersDead++;
      }
    }
    
    if (NumberOfPlayersDead >= PlayerManager.PlayerCount)
    {
      Log.Print("All players dead!");
      OnAllPlayersDead?.Invoke();
      ShowSpamMessageOutro();
    }
  }


  private IEnumerator __WaitToLerpPlayerUIOnScreen(int playerID) 
  {
    while (GameState != State.GAMEPLAY) yield return CoroutineUtil.WaitForUpdate;
    playerUIWrappers[playerID].LerpUIOpaque(spamMessageAlphaLerpInDuration);
  }
  
  private void StartSceneIntro() 
  {
      // we unsub from the transition finished here so that we don't trigger the game a second time
      LoadingScreen.OnHidingLoadingScreen -= StartSceneIntro;
      StartCoroutine(StartGame());
  }
  
  private IEnumerator StartGame()
  {
        string destination = Application.persistentDataPath + "/MusicTrackJson.txt";

        string s = JsonUtility.ToJson(musicTrack);
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, s);
        file.Close();

        Debug.Log("Music JSON file saved at : " + destination);

        AudioEventSystem.TriggerEvent("StopMenuMusic", null);

        // play static background sfx
        StartCoroutine(FadeMusicSource(backgroundStaticSource, 0f, backgroundStaticMaxVol, 1f, false));
    
    GameState = State.WARNING_INTRO;
    
    // WARNING_INTRO STATE
    {
      StaticCamera.SetToGlitchRenderer();
      StaticCamera.Glitch.SetParams(warningStateGlitchEffect);
      yield return warningCanvas.StartWarningEffect();
    }

    // wait a beat before moving on to the crosshair search
    yield return CoroutineUtil.Wait(0.5f);
    
    GameState = State.CROSSHAIR_SEARCH;
    StartCoroutine(StaticCamera.Glitch.LerpToGlitchParams(crosshairSearchGlitchEffect, crosshairSearchGlitchLerpInDuration));

    yield return crosshair.RandomSearch();
    // start crosshair search co, which after two secs will allow character pick
    yield return crosshair.LockOn(characterPicker.Select());
    
    GameState = State.CHARACTER_PICK;
    StartCoroutine(StaticCamera.Glitch.LerpToGlitchParams(characterPickGlitchEffect, characterPickGlitchLerpInDuration));
    yield return characterPicker.Pick();
    
    GameState = State.SPAM_MSG_INTRO;

    // SPAM_MSG_INTRO STATE
    {
      StartCoroutine(StaticCamera.Glitch.LerpToGlitchParams(characterShowGlitchEffect, characterShowGlitchLerpInDuration));
      EmailMessageUI.titleText.text = spamMessageIntroTitle;
      EmailMessageUI.gameObject.SetActive(true);
      characterPicker.ShowUiCharacter();
      EmailMessageUI.canvasGroup.alpha = 0.0f;
      StartCoroutine(crosshair.FadeCrosshair());

      // play sfx or showing the email
      //SFX.PlayGameScene(preGameEmailMessageSFX);
      AudioEventSystem.TriggerEvent("PreGameEmailMessageSFX", null);

      // show email
      yield return LerpUtil.LerpCanvasGroupAlpha(EmailMessageUI.canvasGroup, 1.0f, spamMessageAlphaLerpInDuration);
      yield return EmailMessageUI.OpenEmailBodyAnimation();
      StartCoroutine(StaticCamera.Glitch.LerpToGlitchParams(spamMessageGlitchEffect, spamMessageGlitchLerpInDuration));
      
      do
      {
        yield return CoroutineUtil.Wait(halfSpamMessageTimePerPage);
        yield return EmailMessageUI.AnimateFadeImportantWordsCurrentPage();
        yield return CoroutineUtil.Wait(spamMessageTimePerPage);
        yield return emailMessageUI.NextPage();
      } while(!emailMessageUI.OnLastPage);
      
      // Reset to first email page
      EmailMessageUI.currentPageIndex = 0;

      yield return EmailMessageUI.CloseEmailBodyAnimation();
      yield return CoroutineUtil.Wait(spamMessageWaitBeforeLerpOut);
      
      // NOTE(WSWhitehouse): re-enabling words on current page here so we dont need to do it later.
      EmailMessageUI.AnimateEnableAllWordsCurrentPage();
      
      RectTransform spamMessageUIRT = (RectTransform)emailMessageUI.transform;
      Vector3 lerpEndPos = spamMessageUIRT.position + new Vector3(0.0f, spamMessageUIRT.GetScaledHeight(), 0.0f);
      yield return EaseInOutUtil.ToPositionExponential(emailMessageUI.transform, lerpEndPos, spamMessageLerpOutDuration);
      
      GameState = State.GAMEPLAY;
    }
    
    // Transition to GAMEPLAY STATE and start game
    
    StartCoroutine(StaticCamera.Glitch.LerpDisableGlitch(disableGlitchDuration));

    // enable player ui elements    
    foreach (PlayerID player in players) { player.gameObject.SetActive(true); }
    
    // NOTE(Zack): fade in the relevant ui for the lanes
    for (int i = 0; i < playerUIWrappers.Length; ++i) 
    {
        if (!PlayerManager.IsPlayerValid(i)) continue;
        playerUIWrappers[i].LerpUIOpaque(spamMessageAlphaLerpInDuration);
    }
    
    laneBackground.FadeToOnScreen(spamMessageAlphaLerpInDuration);
    laneWalls.FadeToOnScreen(spamMessageAlphaLerpInDuration);    
    yield return LerpUtil.LerpCanvasGroupAlpha(spam2DCanvasGroup, 1.0f, spamMessageAlphaLerpInDuration);
    yield return MessageLinesCanvas.TransitionIn();

    StartCoroutine(FadeMusicSource(backgroundStaticSource, backgroundStaticMaxVol, 0f, 1f, true));

        // MusicTrackPlayer.Play();
        AudioEventSystem.TriggerEvent("StartGameMusic", null);
    }

  private static void ShowSpamMessageOutro() => Instance.StartCoroutine(Instance.ShowSpamMessageOutroCoroutine());
  
  private IEnumerator __ShowSpamMessageOutroCoroutine()
  {
    if (GameState != State.GAMEPLAY) yield break;
    
    GameState = State.SPAM_MSG_OUTRO;
    
    MusicTrackPlayer.Stop();
    yield return CoroutineUtil.Wait((float)Track.LaneDuration);

    // play the outro end track sfx************************** THIS WILL NEED TO BE MADE INTO AN EVENT ON FMOD
    //onTrackEndedSFX.Play();
    
    
    OnGameEnd?.Invoke();
    Spam2DPool.FadeAllActiveSpam();

    // NOTE(Zack): fade out relevant ui elements
    for (int i = 0; i < playerUIWrappers.Length; ++i) {
        if (!PlayerManager.IsPlayerValid(i)) continue;
        playerUIWrappers[i].LerpUITransparent(spamMessageAlphaLerpInDuration);
    }
    
    laneBackground.FadeToOffScreen(spamMessageAlphaLerpInDuration);
    laneWalls.FadeToOffScreen(spamMessageAlphaLerpInDuration);
    yield return LerpUtil.LerpCanvasGroupAlpha(spam2DCanvasGroup, 0.0f, spamMessageAlphaLerpInDuration);

    // disable player ui elements
    foreach (PlayerID player in players) { player.gameObject.SetActive(false); }
    
    yield return MessageLinesCanvas.TransitionOut();


    // play static background sfx
    PostGameBackgroundMusic.FadeMusicIn();
    StartCoroutine(FadeMusicSource(backgroundStaticSource, 0f, backgroundStaticMaxVol, 1f, false));


    // TODO(Zack): @AUDIO play post game message sfx
    //SFX.PlayGameScene(postGameEmailShowSFX);
    AudioEventSystem.TriggerEvent("PostGameEmailShowSFX", null);
      
    EmailMessageUI.titleText.text = spamMessageOutroTitle;
    RectTransform spamMessageUIRT = (RectTransform)EmailMessageUI.transform;
    Vector3 lerpEndPos = spamMessageUIRT.position - new Vector3(0.0f, spamMessageUIRT.GetScaledHeight(), 0.0f);
    yield return EaseInOutUtil.ToPositionExponential(emailMessageUI.transform, lerpEndPos, spamMessageLerpOutDuration);
    
    EmailMessageUI.DisableAllWords();
    EmailMessageUI.currentPageIndex = 0;
    yield return EmailMessageUI.OpenEmailBodyAnimation();

    bool cleared = EmailMessageUI.CalculateEmailBreakdown();
    EmailClearedUI.SetEmailCleared(cleared);


    // play sfx to show the keywords again
    //SFX.PlayGameScene(postGameEmailKeywordsSFX);
    AudioEventSystem.TriggerEvent("PostGameEmailKeywordsSFX", null);


        do
    {
      yield return CoroutineUtil.Wait(halfSpamMessageTimePerPage);

      yield return EmailMessageUI.AnimateFadeImportantWordsCurrentPage();
      yield return CoroutineUtil.Wait(halfSpamMessageTimePerPage);

      //SFX.PlayGameScene(postGameEmailKeywordEnumerateSFX);
      AudioEventSystem.TriggerEvent("PostGameEmailKeywordEnumerateSFX", null);
      yield return EmailMessageUI.ShowImportantWordTimings();
      yield return CoroutineUtil.Wait(spamMessageTimePerPage);
   
      //SFX.PlayGameScene(postGameEmailHideSFX);
      AudioEventSystem.TriggerEvent("PostGameEmailHideSFX", null);

            // we keep the last pages text on screen
            if (EmailMessageUI.OnLastPage) break;
      yield return EmailMessageUI.NextPage();
    } while(!EmailMessageUI.OnLastPage);

    
    // dim the background ui and remove it from the screen
    yield return EmailMessageUI.CloseEmailBodyAnimation();
    yield return EmailMessageUI.DimMessageUI();

    
    // show whether they have cleared the email
    yield return EmailClearedUI.OpenClearedUI();
    yield return CoroutineUtil.Wait(clearedUIWaitBeforeLerpOut);

    // hide the cleared text
    //SFX.PlayGameScene(postGameEmailClearedHideSFX);
    AudioEventSystem.TriggerEvent("PostGameEmailClearedHideSFX", null);

        yield return EmailClearedUI.CloseClearedUI();

    
    StartCoroutine(FadeMusicSource(backgroundStaticSource, backgroundStaticMaxVol, 0f, 1f, true));
    PostGameBackgroundMusic.FadeMusicUpToFull();
                   
    // Start Transition to Report Scene
    SceneLoad.LoadNextScene();
  }

  private IEnumerator __FadeMusicSource(AudioSource source, float startVolume, float endVolume, float duration, bool stopOnExit) {
      float elapsed = 0f;
      source.volume = startVolume;

       
      if (!source.isPlaying) {
          //source.Play();
         // AudioEventSystem.TriggerEvent("StartPostGameEmailStaticLoop", null);
      }
      
      while (elapsed < duration) {
          elapsed += Time.deltaTime;
          float t  = elapsed / duration;

          source.volume = maths.Lerp(startVolume, endVolume, t);

          yield return CoroutineUtil.WaitForUpdate;
      }

      source.volume = endVolume;

      if (stopOnExit) {
            //source.Stop();
            AudioEventSystem.TriggerEvent("PostGameEmailStaticLoop", null);
        }
  }
  
#if UNITY_EDITOR
  private void Update()
  {
    if (Keyboard.current.sKey.wasPressedThisFrame && GameState == State.INVALID_STATE)
    {
      StartSceneIntro();
    }
    
    if (Keyboard.current.dKey.wasPressedThisFrame && GameState == State.GAMEPLAY)
    {
      foreach (int index in PlayerManager.ValidPlayerIDs)
      {
        PlayerManager.PlayerData[index].CurrentHealth = 0;
      }
    }
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : TEditor<GameManager>
{
  public override void OnInspectorGUI()
  {
    EditorGUI.BeginChangeCheck();
    base.OnInspectorGUI();

    if (EditorGUI.EndChangeCheck())
    {
      Laser[] lasers = FindObjectsOfType<Laser>();
      foreach (Laser laser in lasers)
      {
        laser.MoveLaserAlongLane();
      }
    }
  }
}
#endif

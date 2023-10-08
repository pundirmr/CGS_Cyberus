using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class StreamDeckTutorialUI : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private PlayerID playerID;

  [Header("Tutorial UI References")]
  [SerializeField] private Canvas tutorialCanvas;
  [SerializeField] private Image dimBackground;
  [SerializeField] private CanvasGroup streamDeckUI;
  [SerializeField] private Image[] streamDeckButtons = Array.Empty<Image>();
  
  [Header("Tutorial UI Settings")]
  [SerializeField] private float buttonFlashDuration        = 0.10f;
  [SerializeField] private float backgroundLerpDuration     = 0.75f;
  [SerializeField] private float streamDeckUILerpDuration   = 0.30f;
  [SerializeField] private float waitBetweenShowingTutorial = 20f;

  [Header("Show Tutorial Settings")]
  [SerializeField] private int numOfMisses;
  [SerializeField] private int numOfHitsToResetMisses;
  [Space]
  [Tooltip("The duration of what is considered the start of a music track")]
  [SerializeField] private float trackStartTimeDuration = 20f;
  [Tooltip("This is the number of misses needed to show the tutorial when the track is considered at the beginning")]
  [SerializeField] private int numOfMissesAtStartOfTrack = 5;
  
  [Header("Hide Tutorial Settings")]
  [SerializeField] private int numOfHits;
  [SerializeField] private int numOfMissesToResetHits;
  [SerializeField] private int numOfHitsAtStartOfTrack;

  public bool TutorialIsActive => tutorialCanvas.enabled;
  public bool CanShowTutorial { get; private set; } = true;
  
  private bool IsStartOfTrack => MusicTrackPlayer.TrackPlaybackTime <= trackStartTimeDuration;
  
  // Delegates
  private delegate IEnumerator FlashStreamDeckColourUIDel(WordBlock wordBlock);
  private FlashStreamDeckColourUIDel FlashStreamDeckColourUI;
  
  private DelegateUtil.EmptyCoroutineDel ResetCanShowTutorial;
  private Coroutine _resetCanShowTutCoroutine;
  
  private DelegateUtil.EmptyCoroutineDel ShowTutorialAnim;
  private DelegateUtil.EmptyCoroutineDel HideTutorialAnim;
  private Coroutine _showTutorialAnimCoroutine;
  private Coroutine _hideTutorialAnimCoroutine;

  // Laser getters
  private Laser _laser           => GameManager.PlayerLasers[playerID.ID];
  private LaserInput _laserInput => _laser.LaserInput;
  
  // Stream Deck Button Colours
  private Color[] inactiveColours;
  private Color[] activeColours;
  
  private Color backgroundCol;
  private Color inactiveBackgroundCol;
  
  private int _missStreak = 0;
  private int _hitStreak  = 0;

  private void Awake()
  {
    FlashStreamDeckColourUI = __FlashStreamDeckColourUI;
    ResetCanShowTutorial    = __ResetCanShowTutorial;
    ShowTutorialAnim        = __ShowTutorialAnim;
    HideTutorialAnim        = __HideTutorialAnim;

    Debug.Assert(streamDeckButtons.Length == StreamDeck.ButtonTotalCount, "Stream Deck Button Images count doesn't equal stream deck total button count!");
    
    backgroundCol         = dimBackground.color;
    inactiveBackgroundCol = Colour.ChangeAlpha(backgroundCol, 0.0f);
    
    tutorialCanvas.enabled = false;
    dimBackground.color    = inactiveBackgroundCol;
    streamDeckUI.alpha     = 0.0f;
  }

  private IEnumerator Start()
  {
    WordBlock.OnWordBlockReachedLaser += OnWordBlockReachedLaser;
    
    _laser.OnLaserHit  += OnLaserHit;
    _laser.OnLaserMiss += OnLaserMiss;
    
    yield return Setup();
    
    ShowTutorial();
  }

  private void OnDestroy()
  {
    WordBlock.OnWordBlockReachedLaser -= OnWordBlockReachedLaser;
    
    _laser.OnLaserHit  -= OnLaserHit;
    _laser.OnLaserMiss -= OnLaserMiss;

    if (playerID.IsValid)
    {
      playerID.PlayerData.OnPlayerDeath -= OnPlayerDeath;
    }
  }
  
  private IEnumerator Setup()
  {
    // Clear all buttons
    foreach (Image button in streamDeckButtons)
    {
      Color white  = Colour.ChangeAlpha(Color.black, 0.5f);
      button.color = white;
    }
    
    // Wait for player to become valid
    yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    yield return StreamDeckManager.WaitForValidStreamDeck(playerID.ID);
    
    playerID.PlayerData.OnPlayerDeath += OnPlayerDeath;
    
    // Create Colours
    ColourScheme colourScheme = playerID.PlayerData.ColourScheme;
    int colourCount = colourScheme.Colours.Length;
    activeColours   = new Color[colourCount];
    inactiveColours = new Color[colourCount];

    for (int i = 0; i < colourCount; i++)
    {
      Color active   = playerID.PlayerData.ColourScheme.Colours[i];
      Color inactive = Colour.ChangeAlpha(active, 0.5f);
      
      activeColours[i]   = active;
      inactiveColours[i] = inactive;
    }
    
    // Assign colours to button indices
    ButtonIndices[] buttonIndices = _laserInput.colourButtons;
    for (int i = 0; i < buttonIndices.Length; i++)
    {
      for (int j = 0; j < buttonIndices[i].Count; j++)
      {
        int index = buttonIndices[i][j];
        streamDeckButtons[index].color = inactiveColours[i];
      }
    }
  }

  private void OnPlayerDeath()
  {
    if (TutorialIsActive) HideTutorial();
    CoroutineUtil.StopSafelyWithRef(this, ref _resetCanShowTutCoroutine);
    CanShowTutorial = false;
  }

  private void OnWordBlockReachedLaser(WordBlock wordBlock)
  {
    if (wordBlock.PlayerID != playerID.ID) return;
    StartCoroutine(FlashStreamDeckColourUI(wordBlock));
  }

  private void OnLaserHit(Laser.LaserHitInfo info)
  {
    _hitStreak++;
    
    if (_hitStreak >= numOfHitsToResetMisses)
    {
      _missStreak = 0;
    }

    if (TutorialIsActive)
    {
      int numberOfHits = IsStartOfTrack ? numOfHitsAtStartOfTrack : numOfHits;
      if (_hitStreak >= numberOfHits)
      {
        HideTutorial();
      }
    }
  }

  private void OnLaserMiss(Laser.LaserHitInfo info)
  {
    _missStreak++;
    
    if (_missStreak >= numOfMissesToResetHits)
    {
      _hitStreak = 0;
    }
    
    if (!TutorialIsActive)
    {
      int numberOfMisses = IsStartOfTrack ? numOfMissesAtStartOfTrack : numOfMisses;
      if (_missStreak >= numberOfMisses)
      {
        ShowTutorial();
      }
    }
  }

  private void ShowTutorial()
  {
    if (!CanShowTutorial) return;
    
    if (_hideTutorialAnimCoroutine != null)
    {
      StopCoroutine(_hideTutorialAnimCoroutine);
      _hideTutorialAnimCoroutine = null;
    }
    
    ResetSettingValues();
    CanShowTutorial = false;
    _showTutorialAnimCoroutine = StartCoroutine(ShowTutorialAnim());
  }

  private void HideTutorial()
  {
    if (_showTutorialAnimCoroutine != null)
    {
      StopCoroutine(_showTutorialAnimCoroutine);
      _showTutorialAnimCoroutine = null;
    }
    
    _hideTutorialAnimCoroutine = StartCoroutine(HideTutorialAnim());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ResetSettingValues()
  {
    _hitStreak   = 0;
    _missStreak = 0;
  }

  private IEnumerator __ShowTutorialAnim()
  {
    tutorialCanvas.enabled = true;
    
    // Show background
    {
      float elapsed = 0f;
      Color startCol = dimBackground.color;

      while (elapsed < backgroundLerpDuration)
      {
        float t = elapsed / backgroundLerpDuration;
        dimBackground.color = Colour.Lerp(startCol, backgroundCol, t);

        elapsed += Time.deltaTime;
        yield return CoroutineUtil.WaitForUpdate;
      }

      dimBackground.color = backgroundCol;
    }
    
    // Show stream deck UI
    yield return LerpUtil.LerpCanvasGroupAlpha(streamDeckUI, 1.0f, streamDeckUILerpDuration);
    
    _showTutorialAnimCoroutine = null;
  }

  private IEnumerator __HideTutorialAnim()
  {
    // Hide stream deck UI
    yield return LerpUtil.LerpCanvasGroupAlpha(streamDeckUI, 0.0f, streamDeckUILerpDuration);
    
    // Hide Background
    {
      float elapsed = 0f;
      Color startCol = dimBackground.color;

      while (elapsed < backgroundLerpDuration)
      {
        float t = elapsed / backgroundLerpDuration;
        dimBackground.color = Colour.Lerp(startCol, inactiveBackgroundCol, t);

        elapsed += Time.deltaTime;
        yield return CoroutineUtil.WaitForUpdate;
      }

      dimBackground.color = inactiveBackgroundCol;
    }
    
    tutorialCanvas.enabled     = false;
    _hideTutorialAnimCoroutine = null;
    
    CoroutineUtil.StartSafelyWithRef(this, ref _resetCanShowTutCoroutine, ResetCanShowTutorial());
  }

  private IEnumerator __ResetCanShowTutorial()
  {
    yield return CoroutineUtil.Wait(waitBetweenShowingTutorial);
    CanShowTutorial = true;
    _resetCanShowTutCoroutine = null;
  }

  private IEnumerator __FlashStreamDeckColourUI(WordBlock wordBlock)
  {
        yield break;//added
        /*
    int colourIndex             = wordBlock.ColourIndex;
    ButtonIndices buttonIndices = _laserInput.colourButtons[colourIndex];

    for (int i = 0; i < buttonIndices.Count; i++)
    {
      int index = buttonIndices[i];
      streamDeckButtons[index].color = activeColours[colourIndex];
    }

    yield return CoroutineUtil.Wait(buttonFlashDuration);
    
    for (int i = 0; i < buttonIndices.Count; i++)
    {
      int index = buttonIndices[i];
      streamDeckButtons[index].color = inactiveColours[colourIndex];
    }
        */
  }
}
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StreamDeckCountdown : MonoBehaviour
{
  [Header("Stream Deck Buttons")]
  [SerializeField] private ButtonIndices tensButton;
  [SerializeField] private ButtonIndices onesButton;
  
  private int _currentTime = 0;
  List<int> _timeDigits = new List<int>(10);
  
  private DelegateUtil.EmptyCoroutineDel OnPlayerJoinedCoroutine;
  
  private void Awake()
  {
    OnPlayerJoinedCoroutine = __OnPlayerJoinedCoroutine;
    
    SceneCountdown.OnCountdownStarted  += OnCountdownStarted;
    SceneCountdown.OnCountdownProgress += OnCountdownProgress;
    SceneCountdown.OnCountdownFinished += OnCountdownFinished;
    PlayerManager.onPlayerJoined       += OnPlayerJoined;
  }

  private void Start()
  {
    _currentTime = (int)SceneCountdown.TimeLimit;
  }

  private void OnDestroy()
  {
    SceneCountdown.OnCountdownStarted  -= OnCountdownStarted;
    SceneCountdown.OnCountdownProgress -= OnCountdownProgress;
    SceneCountdown.OnCountdownFinished -= OnCountdownFinished;
    PlayerManager.onPlayerJoined       -= OnPlayerJoined;
  }

  private void OnPlayerJoined(int id) => StartCoroutine(OnPlayerJoinedCoroutine());

  private IEnumerator __OnPlayerJoinedCoroutine()
  {
    // NOTE(WSWhitehouse): Waiting for other events to fire first before putting
    // timer on the stream deck...
    yield return CoroutineUtil.WaitForUpdate;

    if (StreamDeckManager.StreamDeckCount > 0) //added
        UpdateStreamDecks();
  }

  private void OnCountdownStarted()
  {
    _currentTime = (int)SceneCountdown.TimerReal;
    if(StreamDeckManager.StreamDeckCount>0) //added
        UpdateStreamDecks();
  }

  private void OnCountdownProgress()
  {
    int progress = (int)SceneCountdown.TimerReal;
    if (progress == _currentTime) return;
    
    _currentTime = progress;
        if (StreamDeckManager.StreamDeckCount > 0) //added
            UpdateStreamDecks();
  }

  private void OnCountdownFinished()
  {
    _currentTime = 0;
        if (StreamDeckManager.StreamDeckCount > 0) //added
            UpdateStreamDecks();
  }

  private void UpdateStreamDecks()
  {
    CalculateTimeDigits();
    
    int tensDigit = _timeDigits.Count < 2 ? 0 : _timeDigits[1];
    int onesDigit = _timeDigits[0];
    
    ref ButtonTexture tens = ref StaticData.StreamDeckNumbers[tensDigit];
    ref ButtonTexture ones = ref StaticData.StreamDeckNumbers[onesDigit];
    
    int playerCount = PlayerManager.ValidPlayerIDs.Count;
    for (int i = 0; i < playerCount; i++)
    {
      int index = PlayerManager.ValidPlayerIDs[i];
      tensButton.SetButtonImage(index, tens);
      onesButton.SetButtonImage(index, ones);
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void CalculateTimeDigits()
  {
    _timeDigits.Clear();
    
    do
    {
      _timeDigits.Add(_currentTime % 10);
      _currentTime /= 10;
    } while(_currentTime > 0);
  }
}
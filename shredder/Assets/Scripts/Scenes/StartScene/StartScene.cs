using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class StartScene : MonoBehaviour
{
  [Header("Canvas Groups")]
  [SerializeField] private CanvasGroup disclaimerCanvas;
  [SerializeField] private CanvasGroup streamDeckIconsCanvas;
  [SerializeField] private CanvasGroup searchForStreamDeckCanvas;
  [SerializeField] private CanvasGroup setupStreamDeckCanvas;
  [Space]
  [SerializeField] private float fadeDuration = 1.5f;
  
  [Header("Disclaimer Panel")]
  [SerializeField] private float disclaimerWaitTimer = 2.5f;
  
  [Header("Stream Deck Icons")]
  [SerializeField] private Sprite streamDeckSprite;
  [SerializeField] private RectTransform streamDeckImageParent;
  [SerializeField] private float streamDeckActiveAlpha   = 1.0f;
  [SerializeField] private float streamDeckInactiveAlpha = 0.2f;
  
  [Header("Search For Stream Deck Panel")]
  [SerializeField] private float streamDeckWaitTimer = 1.0f;
  [HelpBox("The following text will be formatted to include the number of stream decks required to play. " +
           "Place '{0}' in the string and it will be replaced with the correct number.")]
  [SerializeField] private TMP_Text searchForStreamDecksText;
  
  [Header("Setup Stream Deck Panel")]
  [SerializeField] private TMP_Text streamDeckNumberText;
  [SerializeField] private StreamDeckColour[] debugDeckColours;

  [Header("Scene")]
  [SerializeField, UnityScene] private int nextSceneIndex;
  
  private Image[] _streamDeckImages = new Image[PlayerManager.MaxPlayerCount];

  private Color _streamDeckActiveColour;
  private Color _streamDeckInactiveColour;

  private void Awake()
  {
    // Set up canvas alphas
    disclaimerCanvas.alpha          = 1.0f;
    streamDeckIconsCanvas.alpha     = 0.0f;
    searchForStreamDeckCanvas.alpha = 0.0f;
    setupStreamDeckCanvas.alpha     = 0.0f;
    
    _streamDeckActiveColour   = new Color(1.0f, 1.0f, 1.0f, streamDeckActiveAlpha);
    _streamDeckInactiveColour = new Color(1.0f, 1.0f, 1.0f, streamDeckInactiveAlpha);

    // Create Stream Deck Icons for the number of players
    for (int i = 0; i < PlayerManager.MaxPlayerCount; i++)
    {
      // Create new game object and parent it to the Icon Parent
      GameObject obj    = new GameObject($"StreamDeckIcon {i + 1}");
      var rectTransform = obj.AddComponent<RectTransform>();
      rectTransform.SetParent(streamDeckImageParent);
      
      // Create image component and set it up
      Image image          = obj.AddComponent<Image>();
      image.sprite         = streamDeckSprite;
      image.color          = _streamDeckInactiveColour;
      image.preserveAspect = true;
      
      // Add image to array so we can use it later
      _streamDeckImages[i] = image;
    }
    
    // Update search for stream deck text to include the number of players
    searchForStreamDecksText.text = string.Format(searchForStreamDecksText.text, PlayerManager.MaxPlayerCount);
    
    StartCoroutine(DisclaimerPanel());
  }

    //added
    private void Start()
    {
        SceneHandler.LoadScene(nextSceneIndex); 
    }

    private IEnumerator DisclaimerPanel()
  {
    yield return CoroutineUtil.WaitUnscaled(disclaimerWaitTimer);
    yield return LerpUtil.LerpCanvasGroupAlpha(disclaimerCanvas, 0.0f, fadeDuration);
    
    StartCoroutine(DetectStreamDeckPanel());
  }

  private IEnumerator DetectStreamDeckPanel()
  {
    int streamDeckCount = StreamDeckManager.SearchForNewStreamDecks(false);
    
    // Local function to update stream deck UI, sets the images to the active or inactive colour
    void UpdateStreamDeckUI()
    {
      for (int i = 0; i < _streamDeckImages.Length; i++)
      {
        Image image = _streamDeckImages[i];
        image.color = streamDeckCount > i ? _streamDeckActiveColour : _streamDeckInactiveColour;
      }
    }
    
    UpdateStreamDeckUI(); // make sure all UI is set up before we fade in

    // Fade in search and icon canvases together
    float timeElapsed  = 0.0f;
    float searchStartValue = searchForStreamDeckCanvas.alpha;
    float iconsStartValue  = streamDeckIconsCanvas.alpha;

    while (timeElapsed < fadeDuration)
    {
      searchForStreamDeckCanvas.alpha = maths.Lerp(searchStartValue, 1.0f, timeElapsed / fadeDuration);
      streamDeckIconsCanvas.alpha     = maths.Lerp(iconsStartValue,  1.0f, timeElapsed / fadeDuration);
      timeElapsed += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }

    searchForStreamDeckCanvas.alpha = 1.0f;
    streamDeckIconsCanvas.alpha     = 1.0f;

    // Wait for all stream decks to be plugged in
    while (streamDeckCount < StreamDeckManager.MaxStreamDeckCount)
    {
      // Check for any new stream decks
      streamDeckCount = StreamDeckManager.SearchForNewStreamDecks(false);
      
      // #if UNITY_EDITOR
      // NOTE(WSWhitehouse): Skip having all stream decks plugged in if space is pressed (Debug only)
      if (Keyboard.current.spaceKey.wasPressedThisFrame) break;
      // #endif
      
      UpdateStreamDeckUI();
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    yield return CoroutineUtil.WaitUnscaled(streamDeckWaitTimer);
    yield return LerpUtil.LerpCanvasGroupAlpha(searchForStreamDeckCanvas, 0.0f, fadeDuration);
    
    StartCoroutine(CalibrateStreamDeckPanel());
  }
  
  private IEnumerator CalibrateStreamDeckPanel()
  {
    StreamDeckManager.SearchForNewStreamDecks(true); // Allocate stream decks
    yield return LerpUtil.LerpCanvasGroupAlpha(setupStreamDeckCanvas, 1.0f, fadeDuration);
    
    // Reset all stream deck icons
    foreach (Image image in _streamDeckImages)
    {
      image.color = _streamDeckInactiveColour;
    }

    for (int i = 0; i < StreamDeckManager.StreamDeckCount;) // NOTE(WSWhitehouse): Iterating i further in the loop
    {
      streamDeckNumberText.text = StaticStrings.IDs[i]; // Adding one to start at player 1 rather than 0
      int streamDeckID = -1;
      
      while(streamDeckID < 0)
      {
        for (int j = i; j < StreamDeckManager.StreamDeckCount; j++)
        {
          if (!StreamDeckManager.StreamDecks[j].WasButtonPressedThisFrame) continue;

          streamDeckID = j;
          break;
        }

        yield return CoroutineUtil.WaitForUpdate;
      }
      
      // Swapping Stream Decks
      StreamDeck temp = StreamDeckManager.StreamDecks[i];
      StreamDeckManager.StreamDecks[i] = StreamDeckManager.StreamDecks[streamDeckID];
      StreamDeckManager.StreamDecks[streamDeckID] = temp;

      StreamDeckManager.StreamDecks[i].SetDeckColour(debugDeckColours[i]);
      
      i++; // NOTE(WSWhitehouse): Iterating i here so we can do stuff at the end of the loop
      
      for (int j = 0; j < _streamDeckImages.Length; j++)
      {
        Image image = _streamDeckImages[j];
        image.color = j < i ? _streamDeckActiveColour : _streamDeckInactiveColour;
      }
    }
    
    yield return LerpUtil.LerpCanvasGroupAlpha(setupStreamDeckCanvas, 0.0f, fadeDuration);
    SceneHandler.LoadScene(nextSceneIndex);
  }
}

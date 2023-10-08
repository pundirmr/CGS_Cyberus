using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

// Spam Pages Info
public struct MessagePage
{
  public int2 words; // x = word index for start of page, y = word index for end of page
  public List<int2> lines; // x = word index for start of line, y = word index for end of line
  public int numOfLines => lines.Count; // number of lines in this page
  public int numOfWords => words.y - words.x;
}

public class EmailMessageUI : MonoBehaviour
{
  [Header("Scene References")] 
  [SerializeField] public CanvasGroup canvasGroup;
  [SerializeField] public TMP_Text titleText;
  [SerializeField] public RectTransform viewport;
  [SerializeField] private Transform parent;
  [SerializeField] private EmailWordUI wordPrefab;

  [Header("Email Body")] 
  [SerializeField] private RectTransform windowFocus;
  [SerializeField] private CanvasGroup windowFocusCanvasGroup;
  [SerializeField] private TMP_Text toText;
  [SerializeField] private TMP_Text fromText;
  [SerializeField] private TMP_Text subjectText;
  [SerializeField] private float widthDuration = 0.25f;
  [SerializeField] private float heightDuration = 0.1f;

  [Header("Disable Word Settings")]
  [SerializeField] private float waitOffsetIncrementAmount = 0.05f;

  

  private const float WindowFocusMinWidth = 100f;
  private Vector2 _wndFocusOriginalSize;

  public DelegateUtil.EmptyEventDel OnMessageCreated;

  [NonSerialized] public EmailWordUI[] words;
  [NonSerialized] public List<MessagePage> pages = new List<MessagePage>();
  [NonSerialized] public int currentPageIndex = 0;

  public MessagePage currentPage => pages[currentPageIndex];
  public bool OnLastPage         => currentPageIndex >= pages.Count;

  // Internal Spam Message Info
  public SpamMessage spam     => GameManager.SpamMessage;
  private StringArr[] message => spam.splitLines;
  private float space         => spam.spaceWidth;

  // player average info breakdown vars
  private LaserHitTiming[] averagedTimings;
  
  public DelegateUtil.EmptyCoroutineDel NextPage;
  public DelegateUtil.EmptyCoroutineDel OpenEmailBodyAnimation;
  public DelegateUtil.EmptyCoroutineDel CloseEmailBodyAnimation;
  public DelegateUtil.EmptyCoroutineDel AnimateFadeImportantWordsCurrentPage;
  public DelegateUtil.EmptyCoroutineDel DimMessageUI;
  public DelegateUtil.EmptyCoroutineDel ShowImportantWordTimings;
  
  private void Awake()
  {
    NextPage                             = __NextPage;
    OpenEmailBodyAnimation               = __OpenEmailBodyAnimation;
    CloseEmailBodyAnimation              = __CloseEmailBodyAnimation;
    AnimateFadeImportantWordsCurrentPage = __AnimateFadeImportantWordsCurrentPage;
    DimMessageUI                         = __DimMessageUI;
    ShowImportantWordTimings             = __ShowImportantWordTimings;
  }

  private IEnumerator Start()
  {
    while (spam == null) yield return CoroutineUtil.WaitForUpdate;

    averagedTimings = new LaserHitTiming[spam.NumOfWords];
    InitializeMessage();
    OnMessageCreated?.Invoke();
  }

  private void InitializeMessage()
  {
    toText.text      = spam.To;
    fromText.text    = spam.From;
    subjectText.text = spam.Subject;
    
    words = new EmailWordUI[spam.NumOfWords];

    for (int i = 0; i < spam.NumOfWords; ++i)
    {
      words[i] = Instantiate(wordPrefab, parent);
    }

    // set the text to the instantiated EmailWordUI block
    int currentWord = 0;
    for (int i = 0; i < message.Length; ++i)
    {
      ref var line = ref message[i];
      for (int j = 0; j < line.Length; ++j)
      {
        ref EmailWordUI word = ref words[currentWord];
        word.SetWord(line[j], currentWord);
        currentWord += 1;
      }
    }

    // make content fitters adjust their values for the newly allocated words
    Canvas.ForceUpdateCanvases();

    // set start spawn to the top left of the viewport
    float3 startSpawn = viewport.localPosition;
    startSpawn.x -= viewport.GetWidth() * 0.5f;
    startSpawn.y += viewport.GetHeight() * 0.5f;
    startSpawn.y -= words[0].txt.rectTransform.GetHeight() * 0.5f;

    // iteration variable setup
    float3 pos = startSpawn;
    float startx = pos.x;
    float starty = pos.y;
    float offsetY = words[0].txt.rectTransform.GetHeight() + 5f;
    float halfOffsetY = offsetY * 0.5f;
    float halfSpace = space * 0.5f;
    float minY = viewport.rect.yMin;

    // current word
    currentWord = 0;

    // create first page
    MessagePage page = new()
    {
      words = new(),
      lines = new(),
    };

    int2 lineIndex = new()
    {
      x = currentWord,
    };

    page.words.x = currentWord;

    // loop though lines
    for (int i = 0; i < message.Length; ++i)
    {
      ref var line = ref message[i];
      // loop through words in line
      for (int j = 0; j < line.Length; ++j)
      {
        ref var word = ref words[currentWord];

        var halfW = word.txt.rectTransform.GetWidth() * 0.5f;
        if (j == 0)
        {
          pos.x += halfW;
        }
        else
        {
          pos.x += halfW + halfSpace;
        }

        word.txt.rectTransform.localPosition = pos;
        pos.x += (halfW + halfSpace);

        // increment index
        currentWord += 1;
      }

      // setup indices start and end point for line
      lineIndex.y = currentWord;
      page.lines.Add(lineIndex);
      lineIndex.x = currentWord; // done after adding to [List] so we don't override previous value

      // offset in y axis
      if ((pos.y - offsetY) - halfOffsetY < minY)
      {
        // reset position of words to start of 'viewport'
        pos.y = starty;

        // start a 'new page'
        page.words.y = currentWord;
        pages.Add(page);

        page.words.x = currentWord;
        page.lines = new();
      }
      else
      {
        pos.y -= offsetY;
      }

      pos.x = startx;
    }

    // set last page if we have a page left over
    if (page.words.x != currentWord)
    {
      page.words.y = currentWord;
      pages.Add(page);
    }

    DisableAllWords();
    AnimateEnableAllWordsCurrentPage();

    _wndFocusOriginalSize = windowFocus.GetSize();
    windowFocus.SetSize(new Vector2(WindowFocusMinWidth, 0.0f));
    windowFocusCanvasGroup.alpha = 0.0f;
  }

  // NOTE(Zack): we're making the assumption that the amount of word blocks are greater to or equal to the number of words
  public bool CalculateEmailBreakdown() {
    int numberOfWordsCleared = 0;
    // loop through the dictionary of words and timings
    foreach (var(wordIndex, timings) in AverageWordTimings.Words) {
      // loop through each word timing, and calculate the aggregate value of the timings and hit count
      int hitCount      = 0;
      int averageTiming = 0;
      foreach (var(timing, timingCount) in timings.Timings) {
        int tempTimingVal = (int)maths.Round(math.log2((float)timing));

        averageTiming += (tempTimingVal * timingCount);
        hitCount      += timingCount;
      }

      // to ensure we don't divide by zero, and we default to having missed the word
      if (hitCount == 0) { words[wordIndex].SetWordCleared(false); continue; }
      
      LaserHitTiming t = (LaserHitTiming)(1 << (int)(averageTiming / hitCount));

      // if a word on average has been hit better than a late timing, then the word has been cleared
      bool wordCleared = t >= LaserHitTiming.LATE;
      if (wordCleared) {
        words[wordIndex].SetWordCleared(true);
        numberOfWordsCleared += 1;
      } else {
        words[wordIndex].SetWordCleared(false);
      }
    }


    // if all players have not cleared they are out of they failed the email
    int numOut = 0;
    foreach (var player in PlayerManager.ValidPlayerIDs) {
      if (!PlayerManager.PlayerData[player].IsDead) continue;
      // we decrement a floating point value here so that we can reduce the overall score of the players by a percentage later
      numOut += 1;
    }
    
    bool allFailed = numOut == PlayerManager.ValidPlayerIDs.Count;
    if (allFailed) return false;

    
    // if more than half of all the keywords have been hit then the have cleared the email
    bool enoughCleared = numberOfWordsCleared >= (AverageWordTimings.Words.Count / 2);
    return enoughCleared;
  }
 

  private IEnumerator __OpenEmailBodyAnimation()
  {
    float elapsed  = 0f;
    float endHeight = _wndFocusOriginalSize.y;
    while (elapsed < heightDuration)
    {
      elapsed += Time.deltaTime;
      float t = EaseOutUtil.Exponential(elapsed / heightDuration);

      // set height and opacity of the image
      float y = maths.Lerp(0f, endHeight, t);
      windowFocus.SetHeight(y);
      
      windowFocusCanvasGroup.alpha = maths.Lerp(0.0f, 1.0f, t);

      yield return CoroutineUtil.WaitForUpdate;
    }

    // lerp the width to the original size of the text
    elapsed        = 0f;
    float endWidth = _wndFocusOriginalSize.x;
    while (elapsed < widthDuration)
    {
      elapsed += Time.deltaTime;
      float t = EaseOutUtil.Exponential(elapsed / widthDuration);

      // set width of the corners
      float x = maths.Lerp(WindowFocusMinWidth, endWidth, t);
      windowFocus.SetWidth(x);

      yield return CoroutineUtil.WaitForUpdate;
    }

    AnimateEnableAllWordsCurrentPage();
  }

  private IEnumerator __CloseEmailBodyAnimation()
  {
    Vector2 startSize = windowFocus.GetSize();

    float elapsed = 0f;
    while (elapsed < widthDuration)
    {
      elapsed += Time.deltaTime;
      float t  = EaseOutUtil.Exponential(elapsed / widthDuration);

      // set width of the corners
      float x = maths.Lerp(startSize.x, WindowFocusMinWidth, t);
      windowFocus.SetWidth(x);

      yield return CoroutineUtil.WaitForUpdate;
    }

    elapsed = 0f;
    while (elapsed < heightDuration)
    {
      elapsed += Time.deltaTime;
      float t = EaseOutUtil.Exponential(elapsed / heightDuration);

      // set height and opacity of the image
      float y = maths.Lerp(startSize.y, 0.0f, t);
      windowFocus.SetHeight(y);
      
      windowFocusCanvasGroup.alpha = maths.Lerp(1.0f, 0.0f, t);

      yield return CoroutineUtil.WaitForUpdate;
    }

    // we reset every word back to it's orignal state, ready for when the email is opened again
    ResetAllWords();  
  }

  private void ResetAllWords() {
      foreach (var word in words) {
          word.ResetWord();
      }
  }

  private IEnumerator __DimMessageUI()
  {
    yield return LerpUtil.LerpCanvasGroupAlpha(canvasGroup, 0.0f, 0.3f);
  }
  
  public void AnimateDisableAllWords()
  {
    foreach (var word in words)
    {
      word.AnimateDisable();
    }
  }

  public void AnimateEnableAllWordsCurrentPage()
  {
    for (int i = currentPage.words.x; i < currentPage.words.y; ++i)
    {
      words[i].AnimateEnable();
    }
  }

  public void AnimateDisableAllWordsCurrentPage()
  {
    for (int i = currentPage.words.x; i < currentPage.words.y; ++i)
    {
      if (!words[i].IsEnabled) continue;
      words[i].AnimateDisable();
    }
  }

  public void DisableAllExceptCurrentPage()
  {
    for (int i = 0; i < pages.Count; ++i)
    {
      if (i == currentPageIndex) continue;
      for (int j = pages[i].words.x; j < pages[i].words.y; ++j)
      {
        words[j].DisableWord();
      }
    }
  }

  public void DisableAllWords()
  {
    for (int i = 0; i < pages.Count; ++i)
    {
      for (int j = pages[i].words.x; j < pages[i].words.y; ++j)
      {
        words[j].DisableWord();
      }
    }
  }

  private IEnumerator __AnimateFadeImportantWordsCurrentPage()
  {
    for (int i = currentPage.words.x; i < currentPage.words.y; ++i)
    {
      if (!words[i].IsEnabled) continue;
      words[i].AnimateIsKeyword();
    }

    // we wait here to allow the spam word fade animations to finish
    yield return CoroutineUtil.Wait(1f);
  }

  private IEnumerator __NextPage()
  {
    AnimateDisableAllWordsCurrentPage();
    yield return CoroutineUtil.Wait(0.8f);
    currentPageIndex += 1;
    if (currentPageIndex >= pages.Count) yield break; // we return if there are no more pages
    AnimateEnableAllWordsCurrentPage();
  }

  public IEnumerator __ShowImportantWordTimings() {
      float waitOffset = 0f;
      for (int i = currentPage.words.x; i < currentPage.words.y; ++i) {
          if (!words[i].IsKeyWord) continue;
          words[i].AnimateWordCleared(waitOffset);
          waitOffset += waitOffsetIncrementAmount;
      }
      
      yield break;
  }
}

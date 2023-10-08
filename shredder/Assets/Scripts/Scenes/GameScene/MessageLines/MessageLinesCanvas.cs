using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Updates the Message Lines on the screen, removes words from each line and transitions to new
/// lines when needed. Has functions for transitioning in/out the message lines on the screen.
/// </summary>
public class MessageLinesCanvas : MonoBehaviour
{
  [Header("Message Lines Word Tracker")]
  [SerializeField] private MessageLinesWordTracker wordTracker;
  
  [Header("Scene References")]
  [SerializeField] private MessageLine messageLinePrefab;
  [SerializeField] private Transform messageLineParent;
  [SerializeField] private Transform messageLineActiveTransform;
  [SerializeField] private Transform messageLineInactiveTransform;
  
  [Header("Line Positioning")]
  [SerializeField] private float3 messageLinePosOffset  = new float3(0.0f, 100.0f, 60.0f);
  [SerializeField] private float messageLineAlphaOffset = 0.1f;
  [SerializeField] private float wordAlphaOffset        = 0.2f;

  [Header("Transition In")]
  [SerializeField] private int numberOfLinesToTransition;
  [SerializeField] private float waitBeforeEachLineTransition;
  [SerializeField] private float transitionInDuration;
  
  private SpamMessage _spamMessage => GameManager.SpamMessage;
  
  private const int InitialMessageLinesCount = 20;
  private List<MessageLine> _messageLines;
  private int _currentLineIndex = 0;
  private MessageLine _currentLine => _messageLines[_currentLineIndex];
  
  private delegate IEnumerator LerpLineDel(MessageLine line, float3 startPos, float3 endPos, float bgStartAlpha, float bgEndAlpha, float wordStartAlpha, float wordEndAlpha);
  private LerpLineDel LerpLine;
  
  private delegate IEnumerator TransitionLineInactiveDel(MessageLine line, bool reenableWords);
  private TransitionLineInactiveDel TransitionLineInactive;

  public DelegateUtil.EmptyCoroutineDel TransitionIn;
  public DelegateUtil.EmptyCoroutineDel TransitionOut;
  public DelegateUtil.EmptyCoroutineDel NextLineTransition;
  
  private void Awake()
  {
    // Assign Delegates
    TransitionIn           = __TransitionIn;
    TransitionOut          = __TransitionOut;
    TransitionLineInactive = __TransitionLineInactive;
    LerpLine               = __LerpLine;
    NextLineTransition     = __NextLineTransition;
    
    wordTracker.OnWordEliminated += OnWordEliminated;
  }

  private void OnDestroy()
  {
    wordTracker.OnWordEliminated -= OnWordEliminated;
  }

  private IEnumerator Start()
  {
    while (_spamMessage == null) yield return CoroutineUtil.WaitForUpdate;
    
    // Instantiate each line
    _messageLines = new List<MessageLine>(InitialMessageLinesCount);
    int wordCount = _spamMessage.NumOfWords;

    for (int i = 0; i < wordCount; ++i)
    {
      MessageLine line             = Instantiate(messageLinePrefab, messageLineParent);
      line.transform.localPosition = GetLinePosition(_messageLines.Count);
      i += line.SpawnWordBlocks(i);

      _messageLines.Add(line);
    }
  }

  private void OnWordEliminated(int wordIndex, LaserHitTiming timing)
  {
    // Log.Print($"MessageLinesCanvas WordIndex: ({wordIndex}) CurrentStartIndex: ({_currentLine.startWordIndex})");
    // Make sure the index is valid on this line
    if (wordIndex < _currentLine.startWordIndex) return;
    
    // Calculate the index of the word on this particular line
    int localLineWordIndex = wordIndex - _currentLine.startWordIndex;
    
    if (ArrayUtil.IsValidIndex(localLineWordIndex, _currentLine.words) && _currentLine.words[localLineWordIndex].TxtIsEnabled)
    {
      _currentLine.words[localLineWordIndex].AnimateDisableWord();
    }

    // Transition to next line if this word was the final word on that line
    if (wordIndex >= _currentLine.endWordIndex)
    {
      NextLine();
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float3 GetLinePosition(int lineIndex)
  {
    float3 pos    = messageLineActiveTransform.localPosition;
    float3 offset = messageLinePosOffset * lineIndex;
    return pos + offset;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float GetLineBackgroundAlpha(int lineIndex)
  {
    return maths.Clamp01(1 - (messageLineAlphaOffset * lineIndex));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float GetLineWordAlpha(int lineIndex)
  {
    return maths.Clamp01(1 - (wordAlphaOffset * lineIndex));
  }

  
  private void NextLine()
  {
      StartCoroutine(TransitionLineInactive(_messageLines[_currentLineIndex], reenableWords: true));
    _currentLineIndex = ArrayUtil.WrapIndex(_currentLineIndex + 1, _messageLines.Count);
    
    StartCoroutine(NextLineTransition());
  }
  
  private IEnumerator __NextLineTransition()
  {
    yield return CoroutineUtil.Wait(waitBeforeEachLineTransition);
    
    int linesTransitionCount = maths.Min(numberOfLinesToTransition, _messageLines.Count - _currentLineIndex);
    for (int i = 0; i < linesTransitionCount; i++)
    {
      // get positions
      float3 startPos  = GetLinePosition(i + linesTransitionCount);
      float3 endPos    = GetLinePosition(i);

      // get scales
      float bgStartAlpha   = GetLineBackgroundAlpha(i + linesTransitionCount);
      float bgEndAlpha     = GetLineBackgroundAlpha(i);
      float wordStartAlpha = GetLineWordAlpha(i + linesTransitionCount);
      float wordEndAlpha   = GetLineWordAlpha(i);

      MessageLine msgLine = _messageLines[maths.Clamp(_currentLineIndex + i, 0, _messageLines.Count)];
      StartCoroutine(LerpLine(msgLine, startPos, endPos, bgStartAlpha, bgEndAlpha, wordStartAlpha, wordEndAlpha));
      yield return CoroutineUtil.Wait(waitBeforeEachLineTransition);
    }
  }

  private IEnumerator __TransitionIn()
  {
    int linesTransitionCount = maths.Min(numberOfLinesToTransition, _messageLines.Count);
    for (int i = 0; i < linesTransitionCount; i++)
    {
      // get positions
      float3 startPos  = GetLinePosition(i + linesTransitionCount);
      float3 endPos    = GetLinePosition(i);

      // get scales
      float bgStartAlpha   = GetLineBackgroundAlpha(i + linesTransitionCount);
      float bgEndAlpha     = GetLineBackgroundAlpha(i);
      float wordStartAlpha = GetLineWordAlpha(i + linesTransitionCount);
      float wordEndAlpha   = GetLineWordAlpha(i);
      
      StartCoroutine(LerpLine(_messageLines[i], startPos, endPos, bgStartAlpha, bgEndAlpha, wordStartAlpha, wordEndAlpha));
      yield return CoroutineUtil.Wait(waitBeforeEachLineTransition);
    }
  }

  private IEnumerator __TransitionOut()
  {
    int linesTransitionCount = maths.Min(numberOfLinesToTransition, _messageLines.Count - _currentLineIndex);
    for (int i = 0; i < linesTransitionCount; i++)
    {
        StartCoroutine(TransitionLineInactive(_messageLines[_currentLineIndex + i], reenableWords: false));
      yield return CoroutineUtil.Wait(waitBeforeEachLineTransition);
    }
  }
  
  private IEnumerator __TransitionLineInactive(MessageLine line, bool reenableWords)
  {
    float3 currentPos    = line.transform.localPosition;
    float bgCurrentAlpha = line.backgroundCanvasGroup.alpha;
    float wdCurrentAlpha = line.wordsCanvasGroup.alpha;
    float3 inActivePos   = messageLineInactiveTransform.localPosition;
    
    float timeElapsed = 0.0f;
    while (timeElapsed < transitionInDuration)
    {
      float tPos   = EaseOutUtil.Exponential(timeElapsed / transitionInDuration);
      float tAlpha = EaseOutUtil.Exponential(timeElapsed / (transitionInDuration * 0.5f));

      line.transform.localPosition     = float3Util.Lerp(currentPos, inActivePos, tPos);
      line.backgroundCanvasGroup.alpha = maths.Lerp(bgCurrentAlpha, 0.0f, tAlpha);
      line.wordsCanvasGroup.alpha      = maths.Lerp(wdCurrentAlpha, 0.0f, tAlpha);
      
      timeElapsed += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    line.transform.localPosition     = inActivePos;
    line.backgroundCanvasGroup.alpha = 0.0f;
    line.wordsCanvasGroup.alpha      = 0.0f;

    // NOTE(Zack): we re-enable the words on this message line if we're still in the main game loop
    // we're doing this here to stop the glitch effect from continuing when the line is no longer visible
    if (reenableWords) foreach(var word in line.words) word.ResetWord();
  }
  
  private IEnumerator __LerpLine(MessageLine line, float3 startPos, float3 endPos, float bgStartAlpha, float bgEndAlpha, float wordStartAlpha, float wordEndAlpha)
  {
    float timeElapsed = 0.0f;
    
    while (timeElapsed < transitionInDuration)
    {
      float t = EaseOutUtil.Exponential(timeElapsed / transitionInDuration);
      line.transform.localPosition     = float3Util.Lerp(startPos, endPos, t);
      line.backgroundCanvasGroup.alpha = maths.Lerp(bgStartAlpha, bgEndAlpha, t);
      line.wordsCanvasGroup.alpha      = maths.Lerp(wordStartAlpha, wordEndAlpha, t);
      
      timeElapsed += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    line.transform.localPosition     = endPos;
    line.backgroundCanvasGroup.alpha = bgEndAlpha;
    line.wordsCanvasGroup.alpha      = wordEndAlpha;
  }
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// The message line shown on the screen, controlled by the <see cref="MessageLinesCanvas"/>.
/// </summary>
public class MessageLine : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private RectTransform wordParent;
  [SerializeField] private MessageLineWordUI wordUIPrefab;
  [SerializeField] private float3 wordScale;
  [SerializeField] private Transform wordStartTransform;
  [SerializeField] private Transform wordEndTransform;
  [SerializeField] private float spaceWidth;
  [Space]
  [SerializeField] public CanvasGroup backgroundCanvasGroup;
  [SerializeField] public CanvasGroup wordsCanvasGroup;
  
  [NonSerialized] public int startWordIndex;
  [NonSerialized] public int endWordIndex;
  [NonSerialized] public MessageLineWordUI[] words;
  
  private SpamMessage spam => GameManager.SpamMessage;

  // NOTE(WSWhitehouse): Returns amount of words spawned
  public int SpawnWordBlocks(int wordIndex)
  {
    startWordIndex              = wordIndex;
    backgroundCanvasGroup.alpha = 0.0f;
    wordsCanvasGroup.alpha      = 0.0f;
    
    // NOTE(WSWhitehouse): Just init capacity to 15 here as that should be more than enough
    List<MessageLineWordUI> wordList = new List<MessageLineWordUI>(15);
    float endPos = wordEndTransform.localPosition.x;
    float xPos   = wordStartTransform.localPosition.x;
    float yPos   = wordStartTransform.localPosition.y;

    int currentWordIndex = startWordIndex;
    
    // REVIEW(WSWhitehouse): Should probably find a better way of doing this so we dont destroy 
    // the final overflowing word as its a complete waste of resources! But it works...
    while(true)
    {
      MessageLineWordUI word = Instantiate(wordUIPrefab, wordParent);
      word.transform.localScale = wordScale;
      word.SetWord(spam.wordsWithoutPunctuation[currentWordIndex], currentWordIndex);
      
      Canvas.ForceUpdateCanvases();

      float width = word.txt.rectTransform.GetWidth() * wordScale.x;
      float3 pos  = new float3(xPos + (width * 0.5f), yPos, 0.0f);
      
      if (xPos + width >= endPos)
      {
        Destroy(word.gameObject);
        break;
      }
      
      word.txt.rectTransform.localPosition = pos;
      wordList.Add(word);
      
      currentWordIndex++;
      xPos += width + spaceWidth;

      if (currentWordIndex >= spam.NumOfWords) break;
    }
    
    words = wordList.ToArray();

    for (int i = 0; i < words.Length; ++i) {
        words[i].transform.SetParent(wordsCanvasGroup.transform);
    }
    
    int wordCount = words.Length - 1;
    endWordIndex  = startWordIndex + wordCount;
    
    return wordCount;
  }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering; // SerializedDictionary
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

[Serializable]
public struct StringArr {
    public string[] line;

    public int Length => line.Length;
    
    public string this[int i] {
        get => line[i];
        set => line[i] = value;
    }
}

[Serializable]
public struct UniqueWord
{
  public UniqueWord(string word, IEnumerable<int> indices = null)
  {
    this.word    = word;
    this.indices = indices == null ? new List<int>() : new List<int>(indices);
  }
  
  public string word;
  public List<int> indices;
}

[CreateAssetMenu(fileName = "SpamMessage", menuName = "Spam Message", order = 0)]
public class SpamMessage : ScriptableObject
{
  [SerializeField] private string to;
  public string To => to;
  
  [SerializeField] private string from;
  public string From => from;
  
  [SerializeField] private string subject;
  public string Subject => subject;
  
  [SerializeField, TextArea(20, int.MaxValue)] private string message;
  public string Message => message;
  
  [DisableInInspector] public string[] words;
  [DisableInInspector] public string[] wordsWithoutPunctuation;
  public List<UniqueWord> uniqueWords;
  
  public bool[] keyWords;

  public int NumOfWords => wordsWithoutPunctuation.Length;

  [Header("Message Formatting")]
  public EmailWordUI wordui   = default;
  public float lineLength     = 1800;
  
  [DisableInInspector] public float spaceWidth;
  
  [Header("Lines as one string")]
  [DisableInInspector] public string[] lines;
  [DisableInInspector] public string[] linesWithoutPunctuation;  
  
  // NOTE(WSWhitehouse): Not using the disable in inspector attribute here as it 
  // prevents you from opening the internal array and checking the strings.
  [Header("Lines with words split into individual strings")]
  public StringArr[] splitLines;
  public StringArr[] splitLinesWithoutPunctuation;
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpamMessage))]
public class SpamMessageEditor : Editor
{
  private SpamMessage Target => target as SpamMessage;
  
  private SerializedProperty _messageProperty;
  private static readonly Color32 SelectedColor = new Color32(138, 201, 38, 255);
  
  // NOTE(WSWhitehouse): We store words locally so we can get special characters that
  // are removed when calculating the actual words. Such as "\n"
  private string[] words;

  private void OnEnable()
  {
    _messageProperty = serializedObject.FindProperty("message");
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    
    EditorGUI.BeginChangeCheck();
    
    base.OnInspectorGUI();
    
    bool updated = DrawKeyWordSelector();

    if (EditorGUI.EndChangeCheck() || updated)
    {
      words = StringUtil.SentenceToWords(_messageProperty.stringValue, true).ToArray();
      
      Target.words  = words.Where(x => x != "\\n").ToArray();
      int wordCount = Target.words.Length;
      Target.wordsWithoutPunctuation = new string[wordCount];
      for (int i = 0; i < wordCount; i++)
      {
        Target.wordsWithoutPunctuation[i] = StringUtil.RemovePunctuation(Target.words[i]);
      }
      
      CalculateUniqueWords();

      if (Target.wordui == null)
      {
        Log.Error("SpamMessageUI is null on Spam Message", Target);
        Log.Error("WordUI is null on Spam Message", Target);
      }
      else
      {
        CalculateLines();
      }
      
      serializedObject.ApplyModifiedProperties();
      
      // Reload asset database
      string assetPath = AssetDatabase.GetAssetPath(Target);
      AssetDatabase.ImportAsset(assetPath);
      AssetDatabase.SaveAssets(); 
      AssetDatabase.Refresh();
    }
    
    serializedObject.ApplyModifiedProperties();
  }

  private bool DrawKeyWordSelector()
  {
    EditorGUILayout.LabelField("Key Words", EditorUtil.LabelStyleBold);
    
    if (Target.keyWords == null)
    {
      Target.keyWords = new bool[Target.NumOfWords];
    }
    
    if (Target.keyWords.Length != Target.NumOfWords)
    {
      List<bool> temp = new List<bool>(Target.keyWords);
      
      Target.keyWords = new bool[Target.NumOfWords];
      for (int i = 0; i < temp.Count; i++)
      {
        if (i >= Target.NumOfWords) break;
        Target.keyWords[i] = temp[i];
      }
    }
    
    const float WordPadding = 10f;
    Color standardCol = GUI.color;
    Rect rect         = EditorUtil.GetRectArea(EditorUtil.SingleLineHeight);
    float lineWidth   = rect.width;
    bool changed      = false;

    for (int i = 0; i < Target.NumOfWords; i++)
    {
      GUIContent label = new GUIContent(Target.wordsWithoutPunctuation[i]);
      float width      = EditorUtil.CalculateLabelWidth(label) + WordPadding;
      
      // Get a new line if it will overflow
      if (rect.x + width > lineWidth)
      {
        rect = EditorUtil.GetRectArea(EditorUtil.SingleLineHeight);
      }
      
      rect.width = width;
      
      if (Target.keyWords[i]) GUI.color = SelectedColor;
        
      if (GUI.Button(rect, label))
      {
        Target.keyWords[i] = !Target.keyWords[i];
        changed = true;
      }
      
      GUI.color = standardCol;
      
      rect.x += width;
    }
    
    return changed;
  }
  
  private void CalculateUniqueWords()
  {
    Target.uniqueWords = new List<UniqueWord>(Target.wordsWithoutPunctuation.Length);

    for (int i = 0; i < Target.NumOfWords; i++)
    {
      // NOTE(WSWhitehouse): Using ToLower here to ensure identical words with different capitalisation are handled as the same
      string word = Target.wordsWithoutPunctuation[i].ToLower();

      // Check if the word has been previously found
      bool found = false;
      for (int j = 0; j < Target.uniqueWords.Count; j++)
      {
        if (Target.uniqueWords[j].word != word) continue;
        Target.uniqueWords[j].indices.Add(i);
        found = true;
      }
      
      if (found) continue;
      
      // Create new unique word and add it to the list
      UniqueWord newUniqueWord = new UniqueWord(word);
      newUniqueWord.indices.Add(i);
      
      Target.uniqueWords.Add(newUniqueWord);
    }
  }

  private void CalculateLines()
  {
    // NOTE(WSWhitehouse): If we decide to add padding between glyphs or words change these constant
    const float GlyphPadding = 0f;
    const float WordPadding  = 0.5f;
     
    ref var wordUI = ref Target.wordui;
    float fontSize    = wordUI.txt.fontSize;
    Dictionary<char, float> notKeyWordGlyphWidths = StringUtil.CreateGlyphWidthApproximationDictionary(wordUI.notKeyWordFont, fontSize, wordUI.notKeyWordStyle);
    Dictionary<char, float> keyWordGlyphWidths    = StringUtil.CreateGlyphWidthApproximationDictionary(wordUI.keyWordFont, fontSize, wordUI.keyWordStyle);
  
    
    float maxLength    = maths.Abs(Target.lineLength);
    float space        = maths.Max(notKeyWordGlyphWidths[' '], keyWordGlyphWidths[' ']);
    Target.spaceWidth  = space;
    float currentWidth = 0f;

    // arrays to cache values
    List<string> currentWords = new List<string>(15);
    List<string> lines        = new List<string>(30);

    // Loop through the array of words
    // NOTE(WSWhitehouse): Keeping track of current word index as we skip some words
    int currentWordIndex = 0;
    for (int i = 0; i < words.Length; ++i, ++currentWordIndex)
    {
      ref string currentWord = ref words[i];
      
      // NOTE(WSWhitehouse): Check for a new line character, if it exists move onto a new line,
      // Unity Inspector strings ignore newline characters so it must be checked as a string here.
      // It must also be manually skipped.
      if (currentWord.Contains("\\n"))
      {
        currentWidth = 0f;
        lines.Add(StringUtil.StringsToString(currentWords.ToArray()));
        currentWords.Clear();
        --currentWordIndex;
        continue;
      }
      
      bool isKeyWord = Target.keyWords[currentWordIndex];
      
      // Loop through current word characters to calculate width
      for (int j = 0; j < currentWord.Length; ++j)
      {
        // Get character and its width
        char character  = currentWord[j];
        float charWidth = (isKeyWord ? keyWordGlyphWidths[character] : notKeyWordGlyphWidths[character]) + GlyphPadding;

        // Update the width
        currentWidth += charWidth;

        // If we are still below the max length move onto next char, else
        // start a new line and restart calculating the line width
        if (currentWidth < maxLength) continue;

        // Reset width and restart calculating width on this word
        currentWidth = 0f;
        j            = 0;

        // Add current words to available lines
        lines.Add(StringUtil.StringsToString(currentWords.ToArray()));
        currentWords.Clear();
      }

      // Add a space after the word and add the word to current words
      currentWidth += space + GlyphPadding + WordPadding;
      currentWords.Add(words[i]);
    }

    // Add any remaining words to a line
    if (currentWords.Count > 0)
    {
      lines.Add(StringUtil.StringsToString(currentWords.ToArray()));
    }
    
    int lineCount = lines.Count;
    
    // Setup target arrays
    Target.lines                        = lines.ToArray();
    Target.linesWithoutPunctuation      = new string[lineCount];
    Target.splitLines                   = new StringArr[lineCount];
    Target.splitLinesWithoutPunctuation = new StringArr[lineCount];

    for (int i = 0; i < lineCount; i++)
    {
      Target.linesWithoutPunctuation[i] = StringUtil.RemovePunctuation(lines[i]);
      
      Target.splitLines[i].line                   = StringUtil.SentenceToWords(lines[i], true).ToArray();
      Target.splitLinesWithoutPunctuation[i].line = StringUtil.SentenceToWords(lines[i], false).ToArray();
    }
  }
}
#endif

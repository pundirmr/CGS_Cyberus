using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class EmailWordUI : MonoBehaviour {
    [Header("Prefab References")]
    public TMP_Text txt;

    // NOTE(Zack): these are public as they are accessed from [SpamMessage.cs]
    [Header("Font Settings")] 
    public TMP_FontAsset keyWordFont;
    public TMP_FontAsset notKeyWordFont;
    public FontStyles keyWordStyle;
    public FontStyles notKeyWordStyle;

    [Header("Disable Word Animation Settings")]
    [SerializeField] private RangedFloat disableTimes    = new (0.2f, 0.75f);
    [SerializeField] private RangedFloat randomWaitTimes = new (0.1f, 0.3f);
    
    [Header("Scaling Lerp Settings")] // these settings control the speed at which 
    [SerializeField] private float keywordDuration    = 0.6f;
    [SerializeField] private float nonKeywordDuration = 0.4f;
    [SerializeField] private float keywordScale       = 1.05f;
    [SerializeField] private float nonKeywordScale    = 0.8f;    
    
    [Header("Colour Lerp Settings")]
    [SerializeField] private float wordClearedOrFailedDuration = 0.25f;
    
    [Header("Colour Settings")]
    [SerializeField] private Color originalColour    = Colour.WhiteOpaque;
    [SerializeField] private Color keywordColour     = new (255, 185, 46);
    [SerializeField] private Color nonKeywordColour  = Colour.GreyOpaque;
    [SerializeField] private Color wordClearedColour = new (0, 255, 0);
    [SerializeField] private Color wordFailedColour  = new (255, 0, 0);
    

    public bool IsEnabled => txt.enabled;
    [NonSerialized] public bool IsKeyWord;
    [NonSerialized] public int WordIndex;

    private Color finalColour; // used for if a word has been cleared or failed
    private Color fadeColour;  // used to fade a word to be a keyword or not
    private float fadeScale;
    private float fadeDuration;


    // delegates
    private delegate IEnumerator WordClearedDel(float waitTime);
    private WordClearedDel WordClearedLerp;

    private delegate IEnumerator EnableDisableDel(bool enable);
    private EnableDisableDel WaitEnableOrDisableWord;
    
    private DelegateUtil.EmptyCoroutineDel ScaleIsKeyword;

    private void Awake() {
        WordClearedLerp         = __WordClearedLerp;
        WaitEnableOrDisableWord = __WaitEnableOrDisableWord;
        ScaleIsKeyword          = __ScaleIsKeyword;
    }
    
    //////////////////////////////////////////////////////////////////////////////////
    /////////// Setup Functions
    public void SetWord(string word, int wordIndex) {
        txt.text  = word;
        WordIndex = wordIndex;
        IsKeyWord = GameManager.SpamMessage.keyWords[wordIndex];

        if (IsKeyWord) {
            fadeColour   = keywordColour;
            fadeScale    = keywordScale;
            fadeDuration = keywordDuration;
            
            txt.font      = keyWordFont;
            txt.fontStyle = keyWordStyle;
        } else {
            fadeColour   = nonKeywordColour;
            fadeScale    = nonKeywordScale;
            fadeDuration = nonKeywordDuration;
            
            txt.font      = notKeyWordFont;
            txt.fontStyle = notKeyWordStyle;
        }
    }

    public void DisableWord() {
        txt.enabled = false;
    }

    public void ResetWord() {
        txt.enabled              = true;
        txt.color                = originalColour;
        txt.transform.localScale = float3Util.one;
    }

    public void SetWordCleared(bool cleared) {
        if (cleared) {
            finalColour = wordClearedColour;
        } else {
            finalColour = wordFailedColour;
        }
    }



    //////////////////////////////////////////////////////////////////////////////////
    /////////// Animation Functions
    public void AnimateIsKeyword() {
        StartCoroutine(ScaleIsKeyword());
    }

    private IEnumerator __ScaleIsKeyword() {
        float elapsed     = 0f;
        float3 finalScale = new (fadeScale, fadeScale, fadeScale);
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / fadeDuration;

            float3 scale             = float3Util.Lerp(float3Util.one, finalScale, t);
            txt.transform.localScale = scale;
            
            txt.color = Colour.Lerp(originalColour, fadeColour, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        txt.transform.localScale = finalScale;
        txt.color = fadeColour;
        yield break;
    }


    public void AnimateEnable()  => StartCoroutine(WaitEnableOrDisableWord(true));
    public void AnimateDisable() => StartCoroutine(WaitEnableOrDisableWord(false));

    private IEnumerator __WaitEnableOrDisableWord(bool enable) {
        yield return CoroutineUtil.Wait(disableTimes.Random());
        txt.enabled = enable;
        yield break;
    }


    
    public void AnimateWordCleared(float waitTime) {
        if (!IsKeyWord) return;
        StartCoroutine(WordClearedLerp(waitTime));
    }

    private IEnumerator __WordClearedLerp(float waitTime) {
        yield return CoroutineUtil.Wait(waitTime);

        Color startColour = txt.color;
        float elapsed = 0f;
        while (elapsed < wordClearedOrFailedDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / wordClearedOrFailedDuration;

            txt.color = Colour.Lerp(startColour, finalColour, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        txt.color = finalColour;
        yield break;
    }
}

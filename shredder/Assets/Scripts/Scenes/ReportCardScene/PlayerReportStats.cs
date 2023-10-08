using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


public class PlayerReportStats : MonoBehaviour {
    [Header("General Prefab References")]
    [SerializeField] private CanvasGroup parent;
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private RectTransform[] seperationLines;

    [Header("Avatar/Player References")]
    [SerializeField] private RectTransform avatarParent;
    [SerializeField] private Image avatar;
    [SerializeField] private TMP_Text avatarName;
    [SerializeField] private TMP_Text playerNumber;

    [Header("Timing References")]
    [SerializeField] private TMP_Text[]  timingTexts;
    [SerializeField] private TMP_Text[]  timingNumbers;
    [SerializeField] private TMP_Text[]  timingPercents;
    [SerializeField] private Image[]     timingBars;
    [SerializeField] private CanvasGroup timingBarsParent;

    [Header("Combo/Spam/Difficulty References")]
    [SerializeField] private CanvasGroup comboAndSpamParent;
    [SerializeField] private TMP_Text comboNumber;
    [SerializeField] private TMP_Text spamNumber;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;

    [Header("Animation Settings")]
    [SerializeField] private float serperationLinesAnimLength = 1f;
    [SerializeField] private float barsAnimLength             = 1f;

    /*[Header("SFX References")]
    [SerializeField] private AudioClip barsSFX;
    */

    

    private PlayerID id;
    private PlayerData data => id.PlayerData;

    // position variables
    private float3[] serperationBarOrigins;
    private float3 avatarParentOrigin;


    // timing stats variables
    private const int timingsCount        = 4;
    private int[] timingPercentInts       = new int[timingsCount];
    private float[] timingBarsFinalWidths = new float[timingsCount];

    
    private static bool sfxPlayed = false;
    private void OnDestroy() => sfxPlayed = false;
    
    
    public void Initialize(PlayerID playerID) {
        id = playerID;

        // set certain elements to invisible
        parent.alpha = 0f;
        comboAndSpamParent.alpha = 0f;
        timingBarsParent.alpha = 0f;
    }


    public void SetupUI() {
        // set up origin positions to lerp to later in animations
        avatarParentOrigin = avatarParent.localPosition;

        float cardWidth = cardRect.GetWidth();
        serperationBarOrigins = new float3[seperationLines.Length];
        for (int i = 0; i < seperationLines.Length; ++i) {
            ref var line = ref seperationLines[i];

            // save original position to be able to lerp to later for animation
            serperationBarOrigins[i] = line.localPosition;


            // set the position of the line off screen
            var p = line.localPosition;
            p.x  -= cardWidth;
            line.localPosition = p;
        }

        

        // set the avatar ui
        avatar.sprite = StaticData.AvatarUISprites[data.AvatarIndex][data.ColourIndex];
        avatarName.text = data.Avatar.Name;
        playerNumber.text = StaticStrings.IDs[id.ID];

        // set avatar name position to be outside of viewport mask
        float3 pos = avatarName.rectTransform.localPosition;
        pos.x -= avatarName.rectTransform.GetWidth();
        avatarName.rectTransform.localPosition = pos;

        // set player number to be outside of viewport mask
        pos = playerNumber.rectTransform.localPosition;
        pos.y -= playerNumber.rectTransform.GetHeight();
        playerNumber.rectTransform.localPosition = pos;


        // set avatar ui offscreen
        float cardHeight = cardRect.GetHeight();
        float cardBottom = cardRect.localPosition.y - (cardHeight * 0.5f);

        pos   = avatarParent.localPosition;
        pos.y = (cardBottom - avatarParent.GetHeight());
        avatarParent.localPosition = pos;


        
        // set combo and spam numbers
        comboNumber.text = StaticStrings.Nums[data.HighestCombo];
        spamNumber.text  = StaticStrings.Nums[data.TotalSpam];
        
        // set difficulty stars
        
        NoteDifficulty avgDifficulty = NoteDifficulty.NONE;
        float avgTime                = float.MinValue;
        
        NoteDifficulty[] difficulties = Enum.GetValues(typeof(NoteDifficulty)) as NoteDifficulty[]; 
        for (int i = 0; i < difficulties.Length; i++)
        {
            NoteDifficulty difficulty = difficulties[i];
            float difficultyTime      = data.AvgDifficultyCounter[difficulty];

            if (difficultyTime >= avgTime)
            {
                avgDifficulty = difficulty;
                avgTime       = difficultyTime;
            }
        }
        
        switch (avgDifficulty)
        {
            case NoteDifficulty.NONE:
            case NoteDifficulty.EASY:
                starImages[0].sprite = starOnSprite;
                starImages[1].sprite = starOffSprite;
                starImages[2].sprite = starOffSprite;
                break;
            case NoteDifficulty.MEDIUM:
                starImages[0].sprite = starOnSprite;
                starImages[1].sprite = starOnSprite;
                starImages[2].sprite = starOffSprite;
                break;
            case NoteDifficulty.HARD:
                starImages[0].sprite = starOnSprite;
                starImages[1].sprite = starOnSprite;
                starImages[2].sprite = starOnSprite;
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        // set the timings ui to the starting values
        for (int i = 0; i < timingsCount; ++i) {
            timingTexts[i].alpha     = 0f;
            timingNumbers[i].alpha   = 0f;
            timingPercents[i].alpha  = 0f;
            timingBars[i].fillAmount = 0f;
        }
    }


    public void CalculateStats() {
        DEBUG_CalcStats();
        CalcStats();

        // set the timing numbers ui here after we have calulated the percentages for the players timings
        for (int i = 0; i < timingsCount; ++i) {
            timingNumbers[i].text = StaticStrings.Nums[timingPercentInts[i]];
        }
    }

    // NOTE(Zack): this is to allow us to start in the [Report Scene] for debug/development purposes
    [Conditional("UNITY_EDITOR")]
    private void DEBUG_CalcStats() {
        if (SceneHandler.PreviousSceneIndex == (int)Scene.GAME_SCENE) return;

        // NOTE(Zack): if we're starting in the ReportScene in the editor we will use the below values for the percents
        int totalHits = 200;
        // perfects
        int perfects = 55;
        timingPercentInts[0] = (int)maths.Round(((float)perfects / (float)totalHits) * 100f);
        
        // goods
        int goods = 25;
        timingPercentInts[1] = (int)maths.Round(((float)goods / (float)totalHits) * 100f);

        // early/lates
        int early = 15;
        timingPercentInts[2] = (int)maths.Round(((float)early / (float)totalHits) * 100f);

        // misses
        int misses = 105;
        timingPercentInts[3] = (int)maths.Round(((float)misses / (float)totalHits) * 100f);



        // NOTE(Zack): we're doing a reverse loop so that we calculate the overlap of the bars correctly
        // calculate the final widths of the timing bars
        float runningWidth = 0f;
        for (uint i = (uint)timingsCount - 1; i < timingsCount; --i) {
            ref var finalWidth = ref timingBarsFinalWidths[i];
            finalWidth  = ((float)timingPercentInts[i] / 100f);
            
            finalWidth   += runningWidth;
            runningWidth  = finalWidth;
        }

        // set the perfect bar to always fill the entire amount
        timingBarsFinalWidths[0] = 1f;
    }

    private void CalcStats() {
#if UNITY_EDITOR
        if (SceneHandler.PreviousSceneIndex != (int)Scene.GAME_SCENE) return;
#endif

        // total hits a player had
        int totalHits = data.Perfects + data.Goods + data.EarlyLates + data.Misses;

        // perfects
        int perfects = data.Perfects;
        timingPercentInts[0] = (int)maths.Round(((float)perfects / (float)totalHits) * 100f);

        // goods
        int goods = data.Goods;
        timingPercentInts[1] = (int)maths.Round(((float)goods / (float)totalHits) * 100f);

        // early/lates
        int early = data.EarlyLates;
        timingPercentInts[2] = (int)maths.Round(((float)early / (float)totalHits) * 100f);

        // misses
        int misses = data.Misses;
        timingPercentInts[3] = (int)maths.Round(((float)misses / (float)totalHits) * 100f);

        

        // NOTE(Zack): we do a sanity check to ensure that all of the percents add up to 100%
        int runningTotal = 0;
        int highestIndex = 0;
        int highestScore = 0;
        for (int i = 0; i < timingsCount; ++i) {
            runningTotal += timingPercentInts[i];

            if (timingPercentInts[i] < highestScore) continue;
            highestScore = timingPercentInts[i];
            highestIndex = i;
        }

        // NOTE(Zack): if the totals do not add up to 100% we will add the [+/-] remainder to the highest percent to balance it out
        int remainder = 100 - runningTotal;
        Log.Print($"Player: {id.ID + 1}; Remainder from 100%: {remainder}");
        if (remainder != 0) {
            timingPercentInts[highestIndex] += remainder;
        }



        
        // NOTE(Zack): we're doing a reverse loop so that we calculate the overlap of the bars correctly
        // calculate the final widths of the timing bars
        float runningWidth = 0f;
        for (uint i = (uint)timingsCount - 1; i < timingsCount; --i) {
            ref var finalWidth = ref timingBarsFinalWidths[i];
            finalWidth  = ((float)timingPercentInts[i] / 100f);
            
            finalWidth   += runningWidth;
            runningWidth  = finalWidth;
        }

        // set the perfect bar to always fill the entire amount
        timingBarsFinalWidths[0] = 1f;
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Animation Functions
    public void SetStatsVisible() => parent.alpha = 1f;

    public IEnumerator ShowSeperationLinesAndAvatar() {    
        // animate the lines onto the screen
        for (int i = 0; i < seperationLines.Length; ++i) {
            yield return CoroutineUtil.Wait(0.1f);
            StartCoroutine(EaseOutUtil.ToLocalPositionExponential(seperationLines[i], serperationBarOrigins[i], serperationLinesAnimLength));
        }

        
        // animate the small avatar ui onto the screen
        yield return EaseOutUtil.ToLocalPositionExponential(avatarParent, avatarParentOrigin, serperationLinesAnimLength);


        // animate the avatar name and player number on screen
        float3 nameStartPos = avatarName.rectTransform.localPosition;
        float3 numStartPos  = playerNumber.rectTransform.localPosition;
        float3 zero         = float3Util.zero;
        
        float elapsed  = 0f;
        float duration = serperationLinesAnimLength;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = EaseOutUtil.Quadratic(elapsed / duration);

            avatarName.rectTransform.localPosition   = float3Util.Lerp(nameStartPos, zero, t);
            playerNumber.rectTransform.localPosition = float3Util.Lerp(numStartPos, zero, t);

            yield return CoroutineUtil.WaitForUpdate;
        }

        avatarName.rectTransform.localPosition   = zero;
        playerNumber.rectTransform.localPosition = zero;
        
        yield break;
    }

    
    public IEnumerator EnumerateTimingStatsAndBars() {
        // show the timing bars outline
        yield return LerpUtil.LerpCanvasGroupAlpha(timingBarsParent, 1f, barsAnimLength);

        // NOTE(Zack): this sfx should also encompass the showing of the Spam and Combo numbers on screen
        // NOTE(Zack): this is a super dirty way of implementing this, but we're at the end of the project so....
        if (!sfxPlayed) {
            sfxPlayed = true;
            //SFX.PlayUIScene(barsSFX);
            AudioEventSystem.TriggerEvent("BarsSFX", null);
        }

        
        // animate the timing bar filling up and the respective text for the stat becoming opaque
        for (int i = 0; i < timingBars.Length; ++i) {
            float elapsed = 0f;
            // float duration = 0.75f;
            float duration = 0.75f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t  = elapsed / duration;

                timingTexts[i].alpha     = t;
                timingNumbers[i].alpha   = t;
                timingPercents[i].alpha  = t;
                timingBars[i].fillAmount = maths.Lerp(0f, timingBarsFinalWidths[i], t);
                                
                yield return CoroutineUtil.WaitForUpdate;                
            }

            timingTexts[i].alpha     = 1f;
            timingNumbers[i].alpha   = 1f;
            timingPercents[i].alpha  = 1f;
            timingBars[i].fillAmount = timingBarsFinalWidths[i];
            
            yield return CoroutineUtil.Wait(0.5f);
        }

        yield break;
    }

    public IEnumerator ShowSpamAndCombo() {
        yield return LerpUtil.LerpCanvasGroupAlpha(comboAndSpamParent, 1f, barsAnimLength * 0.25f);
    }
}

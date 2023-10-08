using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;



public class PlayerReportGrade : MonoBehaviour {
    [Header("Prefab References")]
    [SerializeField] private CanvasGroup parent;
    [SerializeField] private Material[] materials;
    [SerializeField] private AberrationEffect[] aberrationEffects;
    
    [Header("Avatar References")]
    [SerializeField] private Image[] avatarLayers;

    [Header("Laser References")]
    [SerializeField] private Image laser;
    [SerializeField] private RangedFloat laserEmissionOverTime;
    [SerializeField] private Image gradient;
    [SerializeField] private Image gradientMask;
    [SerializeField] private Color laserPSColour;

    [Header("Rank References")]
    [SerializeField] private TMP_Text rankNumber;

    [Header("Eliminated Settings and References")]
    [SerializeField] private TMP_Text eliminatedText;
    [SerializeField] private float3 elimStartScale = new (1.05f, 1.05f, 1.05f);
    [SerializeField] private Color gradientEliminatedColour = new (0.6698113f, 0.03311137f, 0f, 0.509804f);

    [Header("Animation Settings")]
    [SerializeField] private float gradeAnimLength    = 8f;
    [SerializeField] private float hideGradeLength    = 0.75f;
    [SerializeField] private float showElimTextLength = 0.5f;

   /*[Header("SFX References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enumerateSFX;
    [SerializeField] private AudioClip moveLaserOffScreenSFX;
    [SerializeField] private AudioClip staticStopSFX;
   */
    
    private static readonly char[] grades = new char[] { 'F', 'E', 'D', 'C', 'B', 'A', 'S' };
    private static float[] gradeBoundaryPositions;
    private static int playersFinishedAnimCount = 0;
    private static int validPlayerCount = 0;
    private static bool allPlayersFinishedAnimation = false;
    private static AudioSource source;

    private PlayerID id;
    private float3 laserEndPos;
    private float percentOfMaxScore;
    private int currentGrade = 0;

    static PlayerReportGrade() {
        gradeBoundaryPositions = new float[grades.Length];
    }

    private void OnDestroy() {
        validPlayerCount = 0;
        source = null;
    }
    
    
    // Called in [Awake()] of PlayerReportCardUI   
    public void Initialize(PlayerID playerID) {
        id = playerID;

        gradientMask.fillAmount = 0f;
        laser.fillAmount        = 1f;
        rankNumber.text         = StringUtil.CharToString[grades[currentGrade]]; // set grade to [F]

        eliminatedText.alpha = 0f;
        eliminatedText.rectTransform.localScale = elimStartScale;
        
        parent.alpha = 1f;
    }

    public void SetupUI() {
        // reset static variables
        allPlayersFinishedAnimation = false;
        playersFinishedAnimCount = 0;
        validPlayerCount += 1;
        //if (source == null) source = audioSource;

        // set the player avatar, colour scheme, materials, and aberration effects
        for (int i = 0; i < avatarLayers.Length; ++i) {
            ref var layer = ref avatarLayers[i];

            layer.sprite   = id.PlayerData.Avatar.Sprites[i];
            layer.color    = id.PlayerData.ColourScheme.Colours[i];
            layer.material = materials[id.ID];

            aberrationEffects[i].Setup();
        }


        // calculate the enumaration lasers' start and end points
        float height = gradient.rectTransform.GetHeight();
        float3 local = laser.rectTransform.localPosition;

        
        float3 pos;
        pos.x = local.x;
        pos.z = local.z;
        laserEndPos.x = pos.x;
        laserEndPos.z = pos.z;


        // set the laser's beginning position to the bottom of the gradient of the card ui
        pos.y  = gradient.rectTransform.localPosition.y;
        pos.y -= (height * 0.5f);
        laser.rectTransform.localPosition = pos; // set the position of the laser


        // set the very end point for the laser to the top of the gradient of the card ui
        laserEndPos.y  = gradient.rectTransform.localPosition.y;
        laserEndPos.y += (height * 0.5f);



        // calculate the Y position of the different grade boundaries
        float diff = height / grades.Length;
        for (int i = 0; i < grades.Length; ++i) {
            gradeBoundaryPositions[i] = (pos.y + (i * diff));
        }
    }
    
    
    public void CalculateGrade() {
        DEBUG_CalculateScore();
        CalculateScore();
    }

    // NOTE(Zack): this is to allow us to start in the [Report Scene] for debug/development purposes
    [Conditional("UNITY_EDITOR")]
    private void DEBUG_CalculateScore() {
        if (SceneHandler.PreviousSceneIndex == (int)Scene.GAME_SCENE) return;

        percentOfMaxScore = 100f;// random.Range(0f, 100f);
        // if (!id.PlayerData.IsDead) percentOfMaxScore = maths.Clamp(percentOfMaxScore, 0f, 60f);
    }

    private void CalculateScore() {
#if UNITY_EDITOR
        if (SceneHandler.PreviousSceneIndex != (int)Scene.GAME_SCENE) return;
#endif

        ref var data = ref PlayerManager.PlayerData[id.ID];
        int maxPoss = MusicTrackPlayer.TotalBlocksSent;
        float maxScore = TimingScoreValues.Perfects * maxPoss;

        float p = data.Perfects   * TimingScoreValues.Perfects;
        float g = data.Goods      * TimingScoreValues.GoodOkays;
        float e = data.EarlyLates * TimingScoreValues.EarlyLates;
        float m = data.Misses     * TimingScoreValues.Misses;

        // calcualte the score
        float score = p + g + e + m;
        score = maths.Clamp(score, 0f, maxScore);
        percentOfMaxScore = maths.PercentageOfMax(score, maxScore);

        // we clamp the max score a player got if they are considered "dead"
        if (data.IsDead) percentOfMaxScore = maths.Clamp(percentOfMaxScore, 0f, 60f);        
    }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Animation Functions
    public IEnumerator EnumerateGrade() {
        yield return CoroutineUtil.Wait(2f);

        if (!source.isPlaying) {
            // source.clip = enumerateSFX;
            //source.Play();
            AudioEventSystem.TriggerEvent("PlayerResultsEnumerateSFX", null);
        }

        int prev = currentGrade;
        float finalLimit = maths.GetPercentage(percentOfMaxScore, 1f);
        float3 startPos = laser.rectTransform.localPosition;

        bool reachedFinalPosition = false;
        float elapsed = 0f;
        float duration = gradeAnimLength;
        while (elapsed < duration) {
            yield return CoroutineUtil.WaitForUpdate;
            elapsed += Time.deltaTime;

            if (allPlayersFinishedAnimation) break;
            if (reachedFinalPosition) continue;

            float t = elapsed / duration;

            
            // we move the gradient and laser up on the card (from bottom to top)
            gradientMask.fillAmount = t;
            float3 pos = float3Util.Lerp(startPos, laserEndPos, t);
            laser.rectTransform.localPosition = pos;

            // check if the laser has reached it's final position for the grade the player got
            if (maths.FloatCompare(t, finalLimit) || t > finalLimit) {
                // show the eliminated text and colour if they were dead
                if (id.PlayerData.IsDead) yield return ShowEliminatedText();

                reachedFinalPosition = true;

                // increment static variables to ensure that players know when all players have finished their animations
                playersFinishedAnimCount += 1;
                if (playersFinishedAnimCount >= validPlayerCount) {
                    allPlayersFinishedAnimation = true;
                    
                    // TODO(Zack): lerp the fade the audio source to 0?
                    source.Stop();
                }

                //SFX.PlayUIScene(staticStopSFX);
                AudioEventSystem.TriggerEvent("StaticStopSFX", null);
            }


            CalcGradeBasedOnYPos(pos.y);
            if (prev != currentGrade) {
                prev = currentGrade;

                rankNumber.text = StringUtil.CharToString[grades[currentGrade]];
            }
            
        }

        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CalcGradeBasedOnYPos(float y) {
        // if we're at the top grade we just return
        if ((currentGrade + 1) == gradeBoundaryPositions.Length) return;

        // if we're still within the boundary of the grade we return
        if (y < gradeBoundaryPositions[currentGrade + 1]) return;

        // we increment the grade boundary as we'll have crossed the boundary
        currentGrade += 1;
    }
    
    private IEnumerator ShowEliminatedText() {
        yield return CoroutineUtil.Wait(0.25f);

        float3 finalScale = float3Util.one;
        Color startColour = gradient.color;

        float elapsed = 0f;
        float duration = showElimTextLength;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            float3 s = float3Util.Lerp(elimStartScale, finalScale, t);
            eliminatedText.rectTransform.localScale = s;

            eliminatedText.alpha = t;

            Color c = Colour.Lerp(startColour, gradientEliminatedColour, t);
            gradient.color = c;

            yield return CoroutineUtil.WaitForUpdate;
        }

        eliminatedText.rectTransform.localScale = finalScale;
        eliminatedText.alpha = 1f;
        gradient.color = gradientEliminatedColour;
        yield break;
    }
        

    
    public IEnumerator MoveLaserOffTop() {
        // wait before moving the laser off of the screen
        yield return CoroutineUtil.Wait(1f);

        if (AudioEngine.audioEngineInstance.eventPbState.PlaybackState(AudioEngine.audioEngineInstance.fmodEventReferences.moveLaserOffScreenSFXInstance) != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
            AudioEventSystem.TriggerEvent("MoveLaserOffScreenSFX", null);
            //source.loop = false;
           // source.Play();
        }
        
        float3 startPos = laser.rectTransform.localPosition;
        float startFill = gradientMask.fillAmount;
        
        float elapsed  = 0f;
        float duration = 1f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = EaseInUtil.Cubic(elapsed / duration);

            // set the fill amount of mask
            gradientMask.fillAmount = maths.Lerp(startFill, 1f, t);
            
            // set the position
            laser.rectTransform.localPosition = float3Util.Lerp(startPos, laserEndPos, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }


        laser.rectTransform.localPosition = laserEndPos;
        gradientMask.fillAmount = 1f;
        laser.gameObject.SetActive(false);
        yield break;
    }

    

    public IEnumerator HideGrade() {
        yield return LerpUtil.LerpCanvasGroupAlpha(parent, 0f, hideGradeLength);
        parent.gameObject.SetActive(false);
    }
}

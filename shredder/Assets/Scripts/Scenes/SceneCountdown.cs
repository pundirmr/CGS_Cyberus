using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SceneCountdown : MonoBehaviour {
    [Header("Prefab References")]
    [SerializeField] private Image fillMask;
    [SerializeField] private TMP_Text timerText;

    [Header("SFX Refs/Settings")]
   // [SerializeField] private AudioClip titleTransitionSFX;
   // [SerializeField] private AudioClip countdownSFX;
    [SerializeField] private float countdownSFXFrequencyInSeconds = 1f;
    
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float timerModIncrease = 2;

    [Header("Transition References")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private RectTransform barOffScreenPos;
    [SerializeField] private RectTransform titleOffScreenPos;
    [SerializeField] private float barLerpDuration   = 1.25f;
    [SerializeField] private float titleLerpDuration = 0.5f;
    [SerializeField] private float waitBeforeStartingCountdown = 1f;
    
#if UNITY_EDITOR
    [Header("Debug Settings")]
    [SerializeField] private bool pauseCountdown = false;
#endif

    private float3 barOnScreenPos;
    private float3 titleOnScreenPos;

    // timer progress events
    public static DelegateUtil.EmptyEventDel OnCountdownStarted;
    public static DelegateUtil.EmptyEventDel OnCountdownStopped;
    public static DelegateUtil.EmptyEventDel OnCountdownFinished;

    // used for the timer animation triggers
    public DelegateUtil.EmptyEventDel OnTimerOnScreen;
    public DelegateUtil.EmptyEventDel OnTimerOffScreen;
    
    public static float ElapsedTime { get; private set; }
    public static float TimeLimit   { get; private set; }
    public static float Timer       { get; private set; }
    
    public static float TimerReal => Timer + 1;

    public static Action OnCountdownProgress;

    private DelegateUtil.LerpTransFloat3Coroutine MoveOnScreen;
    private DelegateUtil.LerpTransFloat3Coroutine MoveOffScreen;

    private DelegateUtil.EmptyCoroutineDel Countdown;
    private Coroutine countdownCo;

    private float timerModifier = 1f;
    
    private const float fillAmountMin = 0f;

    public static bool HasStarted { get; private set; } = false;
    public static bool IsFinished { get; private set; } = false;
    
    private void Awake() {
        // delegate setup
        MoveOnScreen  = EaseOutUtil.ToLocalPositionQuadratic;
        MoveOffScreen = EaseInUtil.ToLocalPositionQuadratic;
        Countdown     = __Countdown;
        
        TimeLimit = timeLimit;

        timerText.text = StaticStrings.Nums[(int)timeLimit];
        fillMask.fillAmount = 1f;

        HasStarted = false;
        IsFinished = false;

        ConfirmSelection.OnChoicesLockedIn += IncreaseTimerSpeed;
    }

    private void OnDestroy() => ConfirmSelection.OnChoicesLockedIn -= IncreaseTimerSpeed;

    private void Start() {
        // setup positions of the timer
        barOnScreenPos        = barRect.localPosition;
        barRect.localPosition = barOffScreenPos.localPosition;

        titleOnScreenPos        = titleRect.localPosition;
        titleRect.localPosition = titleOffScreenPos.localPosition;
        
        //if (!playOnStart) return;
        StartCountdown();
    }


    public void MoveTimerOnScreen() => StartCoroutine(MoveOnCoroutine());
    private IEnumerator MoveOnCoroutine() {
        yield return MoveOnScreen(barRect, barOnScreenPos, barLerpDuration);

        //SFX.PlayUIScene(titleTransitionSFX);
        AudioEventSystem.TriggerEvent("StartTitleTransitionSFX", null);

        yield return MoveOnScreen(titleRect, titleOnScreenPos, titleLerpDuration);
        yield return CoroutineUtil.Wait(0.25f);
        OnTimerOnScreen?.Invoke();
        yield break;
    }

    public void MoveTimerOffScreen() => StartCoroutine(MoveOffCoroutine());
    private IEnumerator MoveOffCoroutine() {
        StartCoroutine(MoveOffScreen(titleRect, barOffScreenPos.localPosition, barLerpDuration));
        yield return MoveOffScreen(barRect, barOffScreenPos.localPosition, barLerpDuration);
        OnTimerOffScreen?.Invoke();
        yield break;
    }
    
    public void StartCountdown() => CoroutineUtil.StartSafelyWithRef(this, ref countdownCo, Countdown());
    
    private IEnumerator __Countdown() {
        yield return CoroutineUtil.Wait(waitBeforeStartingCountdown);
        HasStarted = true;

        float sfxTimer = 2f;
        ElapsedTime = 0f;
        Timer       = timeLimit;
        OnCountdownStarted?.Invoke();

        int lastIntegerValue = (int)Timer; // set timer to one more than the max value, so that the text countdown shows correctly on the screen
        while (ElapsedTime < timeLimit) {
#if UNITY_EDITOR
            if (pauseCountdown) {
                yield return CoroutineUtil.WaitForUpdate;
                continue;
            }
#endif
            ElapsedTime += (timerModifier * Time.deltaTime);
            Timer       -= (timerModifier * Time.deltaTime);
            
            float t = ElapsedTime / timeLimit;
            fillMask.fillAmount = maths.Remap(1f, 0f, fillAmountMin, 1f, t);
            
            OnCountdownProgress?.Invoke();

            // check if it's time to play the countdown sfx
            if (Timer <= 5f) {
                sfxTimer += (timerModifier * Time.deltaTime);
                if (sfxTimer >= countdownSFXFrequencyInSeconds) {
                    sfxTimer = 0f;
                    //  SFX.PlayUIScene(countdownSFX);
                    AudioEventSystem.TriggerEvent("StartCountDownSFX", null);
                }
            }

            
            
            // check if a second has passed
            if (lastIntegerValue != (int)Timer) {
                lastIntegerValue = (int)Timer;

                // we plus [1] so that the string on the screen matches what we would expect for a countdown timer
                if (lastIntegerValue + 1 < 10) {
                    timerText.text = StaticStrings.ZeroNums[lastIntegerValue + 1];
                } else {
                    timerText.text = StaticStrings.Nums[lastIntegerValue + 1];
                }
            }
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        fillMask.fillAmount = fillAmountMin;
        timerText.text = StaticStrings.ZeroNums[0];
        AudioEventSystem.TriggerEvent("StartCountDownSFX", null);
        //SFX.PlayUIScene(countdownSFX);

        OnCountdownFinished?.Invoke();
        yield break;
    }


    private void IncreaseTimerSpeed() {
        Log.Print("Increasing Timer");
        timerModifier = timerModIncrease;
    }
}

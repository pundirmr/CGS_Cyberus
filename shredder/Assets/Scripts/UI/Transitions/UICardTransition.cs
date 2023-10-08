using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class UICardTransition : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private UISpawner spawner;
    [SerializeField] private ColumnLayoutGroup layout;

    [Header("Transform References")]
    [SerializeField] private Transform startOffScreenPos;
    [SerializeField] private Transform endOffScreenPos;
    [SerializeField] private Transform[] onScreenPositions;
    
    [Header("Transition Settings")]
    [SerializeField] private float cascadeDelay     = 0.75f;
    [SerializeField] private float transitionLength = 2f;

   /* [Header("SFX References")]
    [SerializeField] private AudioClip sceneTitleSFX; // NOTE(Zack): set on a per scene basis
    [SerializeField] private AudioClip transitionInSFX;
    [SerializeField] private AudioClip transitionOutSFX;
   */
    

#if UNITY_EDITOR
    [Header("Debug Settings")]
    [SerializeField] private bool transitionOnScreen  = false;
    [SerializeField] private bool transitionOffScreen = false;
#endif
    

    // static accessors for instantiated card ui
    public static bool FinishedTransitioningOnScreen  = false;
    public static bool FinishedTransitioningOffScreen = false;

    // transition event
    public static DelegateUtil.EmptyEventDel OnTransitionOnFinishedStatic;
    public static DelegateUtil.EmptyEventDel OnTransitionOffFinishedStatic;
    public DelegateUtil.EmptyEventDel OnTransitionOnFinished;
    public DelegateUtil.EmptyEventDel OnTransitionOffFinished;
    
    private short transitionsFinished = 0;
    private int UICount => spawner.SpawnedUI.Count;

    private DelegateUtil.LerpTransFloat3Coroutine AnimateCardOn;
    private DelegateUtil.LerpTransFloat3Coroutine AnimateCardOff;
    
    private void Awake() {
        layout.OnUISetupFinished      += SetOffScreen;
        FinishedTransitioningOnScreen  = false;
        FinishedTransitioningOffScreen = false;

        // delegate allocation
        AnimateCardOn  = EaseOutUtil.ToLocalPositionCubic;
        AnimateCardOff = EaseInUtil.ToLocalPositionCubic;
    }
    
#if UNITY_EDITOR
    private void Update() {
        if (transitionOnScreen) {
            transitionOnScreen = false;
            StartTransitionToOnScreen();
        }

        if (transitionOffScreen) {
            transitionOffScreen = false;
            StartTransitionToOffScreen();
        }
    }
#endif

    private void SetOffScreen() {        
        // set y pos on off screen pos
        var os = startOffScreenPos;
        os.position = new (os.position.x, spawner.SpawnedUI[0].transform.position.y, os.position.z);

        // set y pos on off screen pos
        var oe = endOffScreenPos;
        oe.position = new (oe.position.x, spawner.SpawnedUI[0].transform.position.y, oe.position.z);

        // loop through all ui and set to be off screen
        for (int i = 0; i < UICount; ++i) {
            spawner.SpawnedUI[i].transform.position = startOffScreenPos.position;
        }
    }

    private void IncrementCounterOnScreen() {
        transitionsFinished += 1;
        if (transitionsFinished != UICount) return;
        
        transitionsFinished = 0;
        OnTransitionOnFinishedStatic?.Invoke();
        OnTransitionOnFinished?.Invoke();
        FinishedTransitioningOnScreen = true;

        // play the scene title if we have set one in the inspector
        /*if (sceneTitleSFX != null) {
            VoiceOver.PlayGlobal(sceneTitleSFX);
        }
        */
    }
    
    private void IncrementCounterOffScreen() {
        transitionsFinished += 1;
        if (transitionsFinished != UICount) return;
        
        transitionsFinished = 0;
        OnTransitionOffFinishedStatic?.Invoke();
        OnTransitionOffFinished?.Invoke();
        FinishedTransitioningOffScreen = true;
    }
    
    public void StartTransitionToOnScreen() {
        StartCoroutine(TransitionToOnScreen());
    }

    private IEnumerator TransitionToOnScreen() {
        // play card movement sfx
      //  SFX.PlayUIScene(transitionInSFX);
        AudioEventSystem.TriggerEvent("StartTransitionInSFX", null);

        // transitions cards on screen
        for (int i = 0; i < UICount; ++i) {
            StartCoroutine(AnimateCardOn(spawner.SpawnedUI[i].transform, onScreenPositions[i].localPosition, transitionLength, IncrementCounterOnScreen));
            yield return CoroutineUtil.Wait(cascadeDelay);
        }

        yield break;
    }
    
    public void StartTransitionToOffScreen() {
        StartCoroutine(TransitionToOffScreen());
    }

    private IEnumerator TransitionToOffScreen() {
        // play card movement sfx
        //SFX.PlayUIScene(transitionOutSFX);
        AudioEventSystem.TriggerEvent("StartTransitionOutSFX", null);

        // transitions cards off screen
        for (int i = UICount - 1; i > -1; --i) {
            StartCoroutine(AnimateCardOff(spawner.SpawnedUI[i].transform, endOffScreenPos.localPosition, transitionLength, IncrementCounterOffScreen));
            yield return CoroutineUtil.Wait(cascadeDelay);
        }

        yield break;
    }
}

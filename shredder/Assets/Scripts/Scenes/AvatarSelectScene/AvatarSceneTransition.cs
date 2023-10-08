using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AvatarSceneTransition : MonoBehaviour {
    [Header("Scene Transition References")]
    [SerializeField] private SceneCountdown countdown;
    [SerializeField] private UICardTransition cardTransition;

    private void Awake() {
        // transitioning into the scene, show the player cards and countdown timer
        SceneLoad.OnLoadingComplete += countdown.MoveTimerOnScreen;
        countdown.OnTimerOnScreen   += cardTransition.StartTransitionToOnScreen;

        // start the timer countdown
        cardTransition.OnTransitionOnFinished += countdown.StartCountdown;

        // confirm player choices and start the transition out of the scene
        ConfirmSelection.OnChoicesConfirmed    += TransitionOutOfScene;
        cardTransition.OnTransitionOffFinished += SceneLoad.LoadNextScene;
    }

    private void OnDestroy() {
        // transitioning into the scene, show the player cards and countdown timer
        SceneLoad.OnLoadingComplete -= countdown.MoveTimerOnScreen;        
        countdown.OnTimerOnScreen   -= cardTransition.StartTransitionToOnScreen;

        // start the timer countdown
        cardTransition.OnTransitionOnFinished -= countdown.StartCountdown;
        
        // confirm player choices and start the transition out of the scene
        ConfirmSelection.OnChoicesConfirmed    -= TransitionOutOfScene;
        cardTransition.OnTransitionOffFinished -= SceneLoad.LoadNextScene;
    }

    private void TransitionOutOfScene() {
        StartCoroutine(OutOfSceneCoroutine());
    }

    private IEnumerator OutOfSceneCoroutine() {
        yield return CoroutineUtil.Wait(1f);
        cardTransition.StartTransitionToOffScreen();
        countdown.MoveTimerOffScreen();
        yield break;
    }

#if UNITY_EDITOR
    private void Update() {
        if (Keyboard.current.sKey.wasPressedThisFrame) {
            countdown.MoveTimerOnScreen();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame) {
            TransitionOutOfScene();
        }
    }
#endif 
}

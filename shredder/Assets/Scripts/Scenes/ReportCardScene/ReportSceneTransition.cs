using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReportSceneTransition : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private SceneCountdown countdown;
    [SerializeField] private UICardTransition cardTransition;

    private void Awake() {
        // transitioning into the scene, show the player cards and countdown timer
        SceneLoad.OnLoadingComplete += countdown.MoveTimerOnScreen;
        countdown.OnTimerOnScreen   += cardTransition.StartTransitionToOnScreen;

        // start the timer countdown, when cards are on screen
        cardTransition.OnTransitionOnFinished += countdown.StartCountdown;

        // when countdown has finished we transition out of the scene
        SceneCountdown.OnCountdownFinished     += TransitionOutOfScene;
        cardTransition.OnTransitionOffFinished += SceneLoad.LoadNextScene;
    }

    private void OnDestroy() {
        // transitioning into the scene, show the player cards and countdown timer
        SceneLoad.OnLoadingComplete -= countdown.MoveTimerOnScreen;
        countdown.OnTimerOnScreen   -= cardTransition.StartTransitionToOnScreen;

        // start the timer countdown, when cards are on screen
        cardTransition.OnTransitionOnFinished -= countdown.StartCountdown;

        // when countdown has finished we transition out of the scene
        SceneCountdown.OnCountdownFinished     -= TransitionOutOfScene;
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

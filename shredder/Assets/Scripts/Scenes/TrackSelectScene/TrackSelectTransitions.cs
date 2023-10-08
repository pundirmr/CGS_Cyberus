using UnityEngine;
using UnityEngine.InputSystem;

public class TrackSelectTransitions : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private TrackSelect trackSelect;
    [SerializeField] private SceneCountdown countdown;

    private void Awake() {
        SceneLoad.OnLoadingComplete      += IntoSceneTransitions;
        countdown.OnTimerOnScreen        += trackSelect.MoveTracksOnScreen;
        TrackSelect.OnTransitionFinished += countdown.StartCountdown;

        SceneCountdown.OnCountdownFinished += TrackSelect.SetPlayerTrackChoices;
        
        TrackSelect.OnTrackChosen += OutOfSceneTransitions;
    }

    private void OnDestroy() {
        SceneLoad.OnLoadingComplete -= IntoSceneTransitions;
        countdown.OnTimerOnScreen   -= trackSelect.MoveTracksOnScreen;

        SceneCountdown.OnCountdownFinished -= TrackSelect.SetPlayerTrackChoices;
        
        TrackSelect.OnTrackChosen -= OutOfSceneTransitions;

        TrackSelect.OnTransitionFinished -= SceneLoad.LoadNextScene;
    }
    
    private void IntoSceneTransitions() {
        countdown.MoveTimerOnScreen();
    }

    private void OutOfSceneTransitions() {
        countdown.MoveTimerOffScreen();
        trackSelect.MoveTracksOffScreen();
        
        TrackSelect.OnTransitionFinished -= countdown.StartCountdown; // NOTE(Zack): we're unsubbing here to stop invocation of the countdown timer again
        TrackSelect.OnTransitionFinished += SceneLoad.LoadNextScene;
    }

#if UNITY_EDITOR
    private void Update() {
        if (Keyboard.current.sKey.wasPressedThisFrame) {
            IntoSceneTransitions();
        }


        if (Keyboard.current.eKey.wasPressedThisFrame) {
            OutOfSceneTransitions();
        }
    }
#endif
}

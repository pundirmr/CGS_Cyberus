using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum Scene {
    TEST_SCENE    = -1,
    START_SCENE   = 0,
    MAIN_MENU     = 1,
    AVATAR_SELECT = 2,
    TRACK_SELECT  = 3,
    GAME_SCENE    = 4,
    REPORT_SCENE  = 5,

    NUMBER_OF_SCENES,
}

public class SceneLoad : MonoBehaviour {    
    [Header("Scene Settings")]
    [SerializeField] [UnityScene] private int nextScene = (int)Scene.TEST_SCENE;
    [SerializeField] private float loadingTime = 10f;

    private static SceneLoad instance = null;
    private static int scene;
    private static float loadTime = 10f;

    // loading events
    public static DelegateUtil.EmptyEventDel OnLoadingStarted;
    public static DelegateUtil.EmptyEventDel OnLoadingComplete;
    public static DelegateUtil.EmptyEventDel OnLoadingHalfComplete;

    public delegate void ProgressLoadingEvent(float progress);
    public static ProgressLoadingEvent OnLoadingProgress;

    // coroutine delegates
    private static DelegateUtil.EmptyCoroutineDel Load;

    // we enforce only one instance of this script/gameObject to exist in the game
    private void Awake() {
        if (instance != null) {
            SceneLoad.loadTime = this.loadingTime;
            SceneLoad.scene    = this.nextScene;

            #if UNITY_EDITOR
            Log.Print($"Current Scene:({SceneHandler.SceneIndex}, {(Scene)SceneHandler.SceneIndex}), Next Scene:({SceneLoad.scene}, {(Scene)SceneLoad.scene})");
            #endif
            
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        loadTime = loadingTime;
        scene    = nextScene;
        Load     = __Load;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Load a scene at a specific index. DONT USE THIS! Use <see cref="LoadNextScene"/>
    /// instead! This is for debugging and the PCMenu only.
    /// </summary>
    public static void LoadScene(int sceneIndex) {
        scene = sceneIndex;
        LoadNextScene();
    }
    
    public static void LoadNextScene() {
        if (SceneHandler.IsLoading) {
            Log.Error("SceneLoad: Already loading a scene but LoadNextScene() has been called!");
            return;
        }
        
        StaticCoroutine.Start(Load());
    }

    private static IEnumerator __Load() {
        SceneHandler.LoadScene(scene, activateSceneWhenReady: false);
        OnLoadingStarted?.Invoke();

        float elapsed       = 0.0f;
        float duration      = loadTime;
        float halfDuration  = duration * 0.5f;
        bool sceneActivated = false;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            // check to see if we're half way through the loading time, we activate the next scene
            if (elapsed >= halfDuration && !sceneActivated) {
                SceneHandler.ActivateScene();
                OnLoadingHalfComplete?.Invoke();

                // NOTE(WSWhitehouse): Clear the stream deck when loading next scene
                for (int i = 0; i < StreamDeckManager.StreamDecks.Length; i++)
                {
                    // NOTE(WSWhitehouse): We only clear the stream deck for players 
                    // that have already joined - we want the 'tap to join' screen to 
                    // persist throughout all the scenes until a player joins.
                    if (!PlayerManager.IsPlayerValid(i)) continue;
                    StreamDeckManager.StreamDecks[i]?.ClearDeck();
                }

                sceneActivated = true;
            }

            OnLoadingProgress?.Invoke(elapsed);
            yield return CoroutineUtil.WaitForUpdate;
        }

        OnLoadingComplete?.Invoke();
    }
}

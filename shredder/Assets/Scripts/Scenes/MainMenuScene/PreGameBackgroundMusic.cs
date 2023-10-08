using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PreGameBackgroundMusic : MonoBehaviour {
    /*[Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration    = 2f;
    [SerializeField] private float fadeVolumeModifier = 0.35f;
    */

    private static PreGameBackgroundMusic inst;
    private delegate IEnumerator FadeDel(float vol, bool stop, float duration);
    private static FadeDel Fade;
    
    //private static float fadeDuration;
  //  private static float fadeVolumeMod;
    //private static AudioSource music; Replace with fmod event instance
    private static Coroutine fadeCo;
    
    private void Awake() {
        if (inst != null) {
            Destroy(this.gameObject);
            return;
        }

        // delegate allocation
        //Fade = __Fade;

        // components
       // music = GetComponent<AudioSource>();

        // static variable setup
        //fadeDuration  = fadeOutDuration;
      //  fadeVolumeMod = fadeVolumeModifier;

        
        // event subscriptions
        SceneLoad.OnLoadingComplete     += OnLoadingComplete;
        MusicTrackPlayer.OnTrackStarted += OnTrackStarted;

        // we start the music for the [MainMenu]
        //music.Play();
        AudioEventSystem.TriggerEvent("StartMenuMusic", null); 

        // set this instance to be don't destroy on load
        inst = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // HACK(Zack): should rely do this a better way but we're at the end of the project, with no time left....
    private static void OnLoadingComplete() {
        if ((Scene)SceneHandler.SceneIndex == Scene.GAME_SCENE) {
         //   FadeMusicToVolume(1f * fadeVolumeMod, false, fadeDuration * fadeVolumeMod);
        } else if ((Scene)SceneHandler.SceneIndex != Scene.GAME_SCENE && (Scene)SceneHandler.SceneIndex != Scene.REPORT_SCENE) {

            // HACK(Zack): This check is due to the players being able to sometimes transition from the main menu before music has a chance to player
             if (AudioEngine.audioEngineInstance.eventPbState.PlaybackState(AudioEngine.audioEngineInstance.fmodEventReferences.menuMusicInstance) != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                AudioEventSystem.TriggerEvent("StartMenuMusic", null);
            }
        }
    }

    private static void OnTrackStarted() {
        //FadeMusicToVolume(0f, true, fadeDuration);
    }
    
    public static void FadeMusicToVolume(float vol, bool stopAfter, float duration) {
        CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref fadeCo, Fade(vol, stopAfter, duration));
    }

   /* private static IEnumerator __Fade(float finalVol, bool stopAfter, float duration) {
        float elapsed = 0f;
        float startVol = music.volume;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            float v = maths.Lerp(startVol, finalVol, t);
            music.volume = v;
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        music.volume = finalVol;
        if (stopAfter) music.Stop();
        
        yield break;
    }
    */
#if UNITY_EDITOR
    private void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame) {
        //    music.Play();
        }
    }
    
    
    private void OnValidate() {
       // fadeDuration = fadeOutDuration;        
    }
#endif
}

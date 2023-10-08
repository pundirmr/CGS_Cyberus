using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PostGameBackgroundMusic : MonoBehaviour {
    [Header("MusicVolumeSettings")]
    [SerializeField] private float fadeInOutDuration = 2f;
    [SerializeField] private float fadeVolumeModifier = 0.35f;


    private delegate IEnumerator FadeDel(float duration, float startVol, float endVol, bool stopAfter);
    private static FadeDel Fade;
    private static Coroutine fadeCo;
    

    private static PostGameBackgroundMusic inst;    
    //private static AudioSource music;
    private static float fadeDuration;
    private static float fadeVolumeMod;
    private static float cachedFullVolume;
    
    
    private void Awake() {
        if (inst != null) {
            Destroy(this.gameObject);
            return;            
        }

        // delegate allocation
        Fade = __Fade;

        // components
       // music = GetComponent<AudioSource>();

        // static variable setup
      //  cachedFullVolume = music.volume;
        fadeDuration     = fadeInOutDuration;
        fadeVolumeMod    = fadeVolumeModifier;
        

        // event subscriptions
        SceneLoad.OnLoadingStarted += FadeMusicOut;

        
        // we stop the music from auto playing on startup, and set it to loop
       /* music.volume = 0f;
        music.loop = true;
        music.Stop();
       */

        
        // set to be don't destroy on load
        inst = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // called by [GameManager] ~ around line 455
    public static void FadeMusicIn() {
        //music.Play();
        //music.volume = 0f;
        AudioEventSystem.TriggerEvent("StartPostGameBackgroundMusicLoop", null);

        CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref fadeCo, Fade(fadeDuration, 0f, cachedFullVolume * 0.5f, false));
    }

    public static void FadeMusicUpToFull() {
        CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref fadeCo, Fade(fadeDuration, cachedFullVolume * 0.5f, cachedFullVolume, false));
    }
    
    private static void FadeMusicOut() {
        if (SceneHandler.SceneIndex != (int)Scene.MAIN_MENU) return;
        CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref fadeCo, Fade(fadeDuration, cachedFullVolume, 0f, true));
    }
    
    private static IEnumerator __Fade(float duration, float startVol, float endVol, bool stopAfter) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

           // music.volume = maths.Lerp(startVol, endVol, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        //music.volume = endVol;

        if (stopAfter) AudioEventSystem.TriggerEvent("StopPostGameBackgroundMusicLoop", null);
        
        yield break;
    }
}

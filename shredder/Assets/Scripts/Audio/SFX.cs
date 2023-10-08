using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;


// HACK(Zack): don't setup audio in this way, in future projects
[RequireComponent(typeof(AudioSource))]
public class SFX : MonoBehaviour
{
  private static AudioSource _uiSceneSource;
  private static AudioSource _gameSceneSource;
  private static SFX _instance;

  private void Awake()
  {
    if (_instance != null)
    {
      Destroy(this.gameObject);
      return;
    }
    
    _instance = this;
    DontDestroyOnLoad(this.gameObject);
    
    _uiSceneSource = GetComponent<AudioSource>();
    
    // Set up ui audio source
    _uiSceneSource.playOnAwake           = false;
    _uiSceneSource.loop                  = false;
    _uiSceneSource.outputAudioMixerGroup = AudioManager.Mixer.FindMatchingGroups("SFX")[2];

    // HACK(Zack): fake 3D surround a little
    _uiSceneSource.spatialBlend = 0.25f;

    // setup gamescene audio source
    _gameSceneSource = gameObject.AddComponent<AudioSource>();
    _gameSceneSource.playOnAwake           = false;
    _gameSceneSource.loop                  = false;
    _gameSceneSource.outputAudioMixerGroup = AudioManager.Mixer.FindMatchingGroups("SFX")[1];

    // HACK(Zack): fake 3D surround a little
    _gameSceneSource.spatialBlend = 0.25f;


    // HACK(Zack): set source to play from origin
    gameObject.transform.position = new (0f, 0f, 0f);
  }

  private void OnDestroy()
  {
    if (_instance != this) return;
    _instance = null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PlayUIScene(AudioClip clip, float volumeScale = 1.0f)
  {
    // NOTE(WSWhitehouse): Only checking for a null instance in editor as the editor can be started in any scene,
    // during a build the SFX instance should be set up in the main menu and won't need to be created at runtime.
    DEBUG_CreateSFXInstance();
    
    _uiSceneSource.PlayOneShot(clip, volumeScale);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PlayGameScene(AudioClip clip, float volumeScale = 1.0f)
  {
    // NOTE(WSWhitehouse): Only checking for a null instance in editor as the editor can be started in any scene,
    // during a build the SFX instance should be set up in the main menu and won't need to be created at runtime.
    DEBUG_CreateSFXInstance();
    
    _gameSceneSource.PlayOneShot(clip, volumeScale);
  }

  [Conditional("UNITY_EDITOR"), MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void DEBUG_CreateSFXInstance()
  {
    if (_instance != null) return;
    GameObject obj = new GameObject();
    obj.AddComponent<AudioSource>();
    obj.AddComponent<SFX>();

    // HACK(Zack): set source to play from origin
    obj.transform.position = new (0f, 0f, 0f);
    Log.Warning($"SFX: Created an SFX instance!");
  }
}

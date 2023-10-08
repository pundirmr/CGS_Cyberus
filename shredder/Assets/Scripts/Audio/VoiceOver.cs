using UnityEngine;
using System.Diagnostics;
using System.Runtime.CompilerServices;


// HACK(Zack): don't setup audio in this way, in future projects
public class VoiceOver : MonoBehaviour
{
  private static AudioSource[] _playerAudioSource;
  private static AudioSource _globalAudioSource;
  
  private static VoiceOver _instance;

  private void Awake()
  {
    if (_instance != null)
    {
      Destroy(this.gameObject);
      return;
    }
    
    _instance = this;
    DontDestroyOnLoad(this.gameObject);
    
    _globalAudioSource = gameObject.AddComponent<AudioSource>();
    SetUpAudioSource(_globalAudioSource);

    _playerAudioSource = new AudioSource[PlayerManager.MaxPlayerCount];
    for (int i = 0; i < PlayerManager.MaxPlayerCount; i++)
    {
      _playerAudioSource[i] = gameObject.AddComponent<AudioSource>();
      SetUpAudioSource(_playerAudioSource[i]);
    }

    // HACK(Zack): set the source to play from origin
    gameObject.transform.position = new (0f, 0f, 0f);
  }

  private void OnDestroy()
  {
    if (_instance != this) return;
    _instance = null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void SetUpAudioSource(AudioSource source)
  {
    source.playOnAwake           = false;
    source.loop                  = false;
    source.volume                = 1.0f;
    source.outputAudioMixerGroup = AudioManager.Mixer.FindMatchingGroups("VoiceOverMaster")[1];
    
    // HACK(Zack): fake 3D surround a little
    source.spatialBlend = 0.25f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PlayGlobal(AudioClip clip, float volumeScale = 1.0f)
  {
    // NOTE(WSWhitehouse): Only checking for a null instance in editor as the editor can be started in any scene,
    // during a build the VoiceOver instance should be set up in the main menu and won't need to be created at runtime.
    DEBUG_CreateSFXInstance();
    
    _globalAudioSource.PlayOneShot(clip, volumeScale);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void PlayP0(AudioClip clip) => Play(0, clip);
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void PlayP1(AudioClip clip) => Play(1, clip);
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void PlayP2(AudioClip clip) => Play(2, clip);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Play(int playerID, AudioClip clip)
  {
    AudioSource source = _playerAudioSource[playerID];
    source.Stop();
    source.clip = clip;
    source.Play();
  }

  [Conditional("UNITY_EDITOR"), MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void DEBUG_CreateSFXInstance()
  {
    if (_instance != null) return;
    GameObject obj = new GameObject();
    obj.AddComponent<VoiceOver>();

    // HACK(Zack): set source to play from origin
    obj.transform.position = new (0f, 0f, 0f);
    Log.Warning($"VoiceOver: Created an VoiceOver instance!");
  }
}

using UnityEngine;

public class MetronomePlayer : MonoBehaviour
{
  [SerializeField] private AudioSource beatAudioSource;
  [SerializeField] private AudioSource noteAudioSource;
  [SerializeField] private AudioClip audioClip;

  public static bool BeatMetronomeEnabled = false;
  public static bool NoteMetronomeEnabled = false;
  
  private const string BeatMetronomePlayerPrefsKey = "MUSIC_TRACK_EDITOR_ENABLE_BEAT_METRONOME";
  private const string NoteMetronomePlayerPrefsKey = "MUSIC_TRACK_EDITOR_ENABLE_NOTE_METRONOME";

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    BeatMetronomeEnabled = PlayerPrefs.GetInt(BeatMetronomePlayerPrefsKey, 0) == 1;
    NoteMetronomeEnabled = PlayerPrefs.GetInt(NoteMetronomePlayerPrefsKey, 0) == 1;
  }
  
  private void Awake()
  {
    MusicTrackPlayerEditor.OnNote += OnNote;
  }

  private void OnDestroy()
  {
    PlayerPrefs.SetInt(BeatMetronomePlayerPrefsKey, BeatMetronomeEnabled ? 1 : 0);
    PlayerPrefs.SetInt(NoteMetronomePlayerPrefsKey, NoteMetronomeEnabled ? 1 : 0);
    
    MusicTrackPlayerEditor.OnNote -= OnNote;
  }
  
  private void OnNote(NoteData noteData)
  {
    if (noteData.isMainBeat)
    {
      if (!BeatMetronomeEnabled) return;
      beatAudioSource.PlayOneShot(audioClip);
    }
    else
    {
      if (!NoteMetronomeEnabled) return;
      noteAudioSource.PlayOneShot(audioClip);
    }
  }
}
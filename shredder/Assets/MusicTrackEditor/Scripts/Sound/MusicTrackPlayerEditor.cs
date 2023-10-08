using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicTrackPlayerEditor : MonoBehaviour
{
  // -- SERIALIZED FIELDS -- //
  [SerializeField] private AudioMixer audioMixer;
  
  // -- STATIC FIELDS -- //
  public static Action OnTrackStarted;
  public static Action<NoteData> OnNote;
  public static Action<WordData> OnSendWord;
  public static Action OnTrackFinished;
  
  private static MusicTrack Track => TrackEditor.MusicTrack;
  
  // Song Position Data
  private static double _trackPlaybackTime = 0.0;
  public static double TrackPlaybackTime 
  {
    get => _trackPlaybackTime;
    set => _trackPlaybackTime = value * _playbackSpeed;
  }

  public static double SongStartTime     { get; private set; }
  public static double SongStartOffset   { get; private set; }
  public static int CurrentNoteIndex     { get; private set; }
  public static int CurrentWordIndex     { get; private set; }

  public const float PlaybackSpeedMin     = 0.1f;
  public const float PlaybackSpeedMax     = 2.0f;
  public const float PlaybackSpeedDefault = 1.0f;
  
  private static float _playbackSpeed = PlaybackSpeedDefault;
  public static float PlaybackSpeed
  {
    get => _playbackSpeed;
    set
    {
      _playbackSpeed = maths.Clamp(value, PlaybackSpeedMin, PlaybackSpeedMax);
      
      AudioSource.pitch = _playbackSpeed;
      AudioMixer.SetFloat("PitchBlend", 1.0f / _playbackSpeed);
    }
  }

  private static AudioSource AudioSource;
  private static AudioMixer AudioMixer;
  
  private static DelegateUtil.EmptyCoroutineDel PlayTrack;
  private static Coroutine PlayCoroutine;
  
  static MusicTrackPlayerEditor()
  {
    PlayTrack = __PlayTrack;
  }

  private void Awake()
  {
    AudioSource = GetComponent<AudioSource>();
    AudioMixer  = audioMixer;
    TrackEditor.OnMusicTrackChanged += OnMusicTrackChanged;
  }

  private void OnDestroy()
  {
    TrackEditor.OnMusicTrackChanged -= OnMusicTrackChanged;
  }

  private static void OnMusicTrackChanged()
  {
    if (Track == null) return;
    
    AudioSource.clip = Track.AudioClip;
    AudioSource.Play();
    AudioSource.Pause();
  }

  public static void Play()
  {
    Debug.Assert(Track           != null, "Trying to play a null track!");
    Debug.Assert(Track.AudioClip != null, "Trying to play a null audio clip!");
    Debug.Assert(AudioSource     != null, "Trying to play with no AudioSource!");

    PlayCoroutine = StaticCoroutine.Start(PlayTrack());
  }

  public static void Stop()
  {
    if (PlayCoroutine == null) return;
    
    StaticCoroutine.Stop(PlayCoroutine);
    AudioSource.Pause();
  }
  
  private static IEnumerator __PlayTrack()
  {
    const int INVALID_INDEX = -1;
    
    OnTrackStarted?.Invoke();
    Log.Print("Track Started");

    // Set up track position data
    // NOTE(WSWhitehouse): offsetting start time by the lane duration minus how far up the laser is
    // along the lane. So the start of the music lines up with the laser position.
    SongStartOffset     = 0.0;
    SongStartTime       = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - TrackPlaybackTime;
    CurrentWordIndex    = INVALID_INDEX;
    
    CurrentNoteIndex    = 0;
    double nextNoteTime = TrackPlaybackTime;

    if (TrackPlaybackTime > 0.0001)
    {
      // NOTE(WSWhitehouse): Adding 1 to then current note index as we want the next note that will be played
      CurrentNoteIndex = (int)maths.Floor(TrackPlaybackTime / Track.SecondsPerNote) + 1;
      nextNoteTime           = CurrentNoteIndex * Track.SecondsPerNote;
    }

    string word = string.Empty;
    
    AudioSource.time = (float)TrackPlaybackTime;
    AudioSource.UnPause();

    // NOTE(WSWhitehouse): We are using Try Catches when invoking events here as an exception will cause 
    // the music play back to end and break a lot of other things in the scene. If any exceptions are 
    // caught they are logged to the console and the track *should* continue to play without any issues.

    while (TrackPlaybackTime <= Track.TrackDuration)
    {
      TrackPlaybackTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - SongStartTime;

      if (TrackPlaybackTime >= nextNoteTime)
      {
        // NOTE(WSWhitehouse): We need to ensure we're on the correct timing so remove how much we overshot by
        double overshoot = TrackPlaybackTime - nextNoteTime;
        int notesPlayed  = 0;

        do
        {
          // NOTE(WSWhitehouse): Taking a copy of the current note index so we can increment it safely and use what 
          // was the current not index inside this loop. Otherwise, we are a note in-front when sending notes/words.
          int currentNoteIndex = CurrentNoteIndex;
          
          CurrentNoteIndex++;
          notesPlayed++;
          
          NoteData noteData = new NoteData()
          {
            noteIndex  = currentNoteIndex,
            noteTime   = nextNoteTime + (Track.SecondsPerNote * notesPlayed),
            isMainBeat = currentNoteIndex % TrackEditor.MusicTrack.NotesPerBeat == 0,
          };
          
          OnNote?.TryCatchInvoke(noteData);

          // Create word data
          WordData data = new WordData
          {
            noteData      = noteData,
            wordIndex     = CurrentWordIndex,
            word          = word,
          };
          
          // Send word
          bool updatedWord = false;
          for (int i = 0; i < Track.BeatMap.Length; ++i)
          {
            // NOTE(WSWhitehouse): Using a try/catch here as if we encounter an
            // exception in this coroutine the track playback breaks. Its better
            // to catch the error and log it.
            try
            {
              if (!Track.BeatMap[i][currentNoteIndex].IsActive) continue;
              
              // Update word data
              data.subLaneIndex = i;
              data.difficulty   = Track.BeatMap[i][currentNoteIndex].Difficulty;
              
              // Get new word if its an easy difficulty word and we haven't already updated the word on this note
              if (!updatedWord && data.difficulty == NoteDifficulty.EASY || CurrentWordIndex == INVALID_INDEX)
              {
                updatedWord = true; 
                CurrentWordIndex = ArrayUtil.WrapIndex(CurrentWordIndex + 1, Track.SpamMessage.NumOfWords);
                word             = Track.SpamMessage.wordsWithoutPunctuation[CurrentWordIndex];
                data.wordIndex   = CurrentWordIndex;
                data.word        = word;
              }
              
              // NOTE(WSWhitehouse): Not using TryCatchInvoke here as its surrounded by a try/catch already
              OnSendWord?.Invoke(data);
            }
            catch(Exception e)
            {
              Log.Error($"{e.Message}\n{e.StackTrace}");
              break;
            }
          }
          
          overshoot -= Track.SecondsPerNote;

        } while (overshoot > 0);
        
        nextNoteTime += (Track.SecondsPerNote * notesPlayed);
      }

      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    Log.Print("Track Finished");
    PlayCoroutine = null;
    OnTrackFinished?.Invoke();
  }
}
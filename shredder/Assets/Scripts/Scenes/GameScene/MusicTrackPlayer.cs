using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NoteData
{
  public int noteIndex;
  public double noteTime;
  public bool isMainBeat;
  public double timeOvershoot => MusicTrackPlayer.TrackPlaybackTime - noteTime;
}

public struct WordData
{
  public NoteData noteData;
  public int subLaneIndex;
  public int wordIndex;
  public NoteDifficulty difficulty;
  public string word;
}

[RequireComponent(typeof(AudioSource))]
public class MusicTrackPlayer : MonoBehaviour
{
  public static Action OnTrackStarted;
  public static Action<NoteData> OnNote;
  public static Action<WordData> OnSendWord;
  public static Action OnTrackFinished;
  
  private static MusicTrack Track => GameManager.Track;

  // Song Position Data
  public static double TrackPlaybackTime { get; private set; }
  public static double SongStartTime     { get; private set; }
  public static double SongStartOffset   { get; private set; }
  public static double NextNoteTime      { get; private set; }
  
  public const int INVALID_INDEX = -1;
  public static int CurrentNoteIndex { get; private set; } = INVALID_INDEX;
  public static int CurrentWordIndex { get; private set; } = INVALID_INDEX;
  
  public static string CurrentWord { get; private set; } = string.Empty;

  public static int TotalBlocksSent { get; private set; } = 0;
  
  private static DelegateUtil.EmptyCoroutineDel PlayTrack;
  
  private delegate IEnumerator StopTrackDel(float volFadeDuration);
  private static StopTrackDel StopTrack;
  
  private delegate IEnumerator VolumeFadeDel(float start, float end, float duration);
  private static VolumeFadeDel VolumeFade;

  private static AudioSource AudioSource;
  private static Coroutine PlayCoroutine;
  
  static MusicTrackPlayer()
  {
    PlayTrack  = __PlayTrack;
    StopTrack  = __StopTrack;
    VolumeFade = __VolumeFade;
  }

  private void Awake()
  {
    AudioSource     = GetComponent<AudioSource>();
    TotalBlocksSent = 0;
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
    
    Log.Print("MusicTrackPlayer: Stopping Track!");
    
    StaticCoroutine.Stop(PlayCoroutine);
    PlayCoroutine = null;
    
    double timeRemaining  = Track.TrackDuration - TrackPlaybackTime;
    float volFadeDuration = (float)maths.Min(Track.LaneDuration, timeRemaining);
    StaticCoroutine.Start(StopTrack(volFadeDuration));
  }
  
  private static IEnumerator __PlayTrack()
  {
    OnTrackStarted?.Invoke();
    Log.Print($"MusicTrackPlayer: Track Started ({Track.TrackName})");
    
    float volume       = AudioSource.volume;
    AudioSource.volume = 0.0f;
    
    // NOTE(WSWhitehouse): Play and pausing here to get more accurate timings later on...
    AudioSource.clip = Track.AudioClip;
    AudioSource.Play();
    AudioSource.Pause();
    AudioSource.time = 0f;

    // Set up track position data
    // NOTE(WSWhitehouse): offsetting start time by the lane duration minus how far up the laser is
    // along the lane. So the start of the music lines up with the laser position.
    SongStartOffset  = Track.LaneDuration - ((Track.LaneDuration * GameManager.LaserPosAlongLane) );
    NextNoteTime     = Track.FirstBeatOffset;
    CurrentWordIndex = 0;
    CurrentNoteIndex = 0;
    CurrentWord      = Track.SpamMessage.wordsWithoutPunctuation[CurrentWordIndex];


        //AudioEngine.s_audioEngineInstance.
        SongStartTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
        Debug.Log("TrackPlaybackTime init" + TrackPlaybackTime);
        //SongStartTime     = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
        TrackPlaybackTime = 0.0;
    while (TrackPlaybackTime < SongStartOffset)
    {
      TrackPlaybackTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - SongStartTime;
            Debug.Log("TrackPlaybackTime" + TrackPlaybackTime);
       if (CheckForNote(TrackPlaybackTime))
      {
        Stop();
        PlayCoroutine = null;
        OnTrackFinished?.Invoke();
        yield break;
      }
      yield return CoroutineUtil.WaitForFixedUpdate;
    }
    
    SongStartTime     = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - (TrackPlaybackTime - SongStartOffset);
    TrackPlaybackTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - SongStartTime;
    NextNoteTime     -= SongStartOffset;
    
    // StaticCoroutine.Start(VolumeFade(0.0f, 0f, 0.5f));
    StaticCoroutine.Start(VolumeFade(0.0f, volume, 0.5f));
    
    AudioSource.time = (float)TrackPlaybackTime;
    AudioSource.UnPause();

    while (TrackPlaybackTime <= Track.TrackDuration)
    {
      TrackPlaybackTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - SongStartTime;
      
      if (CheckForNote(TrackPlaybackTime))
      {
        Stop();
        StaticCoroutine.Start(StopTrack((float)Track.LaneDuration));
        PlayCoroutine = null;
        OnTrackFinished?.Invoke();
        yield break;
      }

      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    Log.Print("MusicTrackPlayer: Track Finished");
    PlayCoroutine = null;
    OnTrackFinished?.Invoke();
  }

  private static IEnumerator __OnEndOfTrack()
  {
    Log.Print("MusicTrackPlayer: End of Track");
    
    double timeRemaining = Track.TrackDuration - TrackPlaybackTime;
    double waitDuration  = maths.Min(Track.LaneDuration, timeRemaining);
        
    double startTime   = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
    double currentTime = 0.0;

    while (currentTime <= waitDuration)
    {
      currentTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - startTime;
      yield return CoroutineUtil.WaitForFixedUpdate;
    }
    
    Stop(); 
    PlayCoroutine = null;
    OnTrackFinished?.Invoke();
  }

  /// <summary>
  /// Check for notes at the playback time given and fire send word event. Returns
  /// true if note indicates end of track.
  /// </summary>
  /// <returns>Returns true if note is end of trck</returns>
  private static bool CheckForNote(double playbackTime)
  {
    if (playbackTime < NextNoteTime) return false;
    
    // NOTE(WSWhitehouse): We are using Try Catches when invoking events here as an exception will cause 
    // the music play back to end and break a lot of other things in the scene. If any exceptions are 
    // caught they are logged to the console and the track *should* continue to play without any issues.
    
    // NOTE(WSWhitehouse): We need to ensure we're on the correct timing so remove how much we overshot by
    double overshoot = playbackTime - NextNoteTime;
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
        noteTime   = NextNoteTime + (Track.SecondsPerNote * notesPlayed),
        isMainBeat = currentNoteIndex % Track.NotesPerBeat == 0,
      };

      OnNote?.TryCatchInvoke(noteData);

      // Create word data
      WordData data = new WordData
      {
        noteData  = noteData,
        wordIndex = CurrentWordIndex,
        word      = CurrentWord,
      };
          
      // Send word
      bool sentWord = false;
      for (int i = 0; i < Track.BeatMap.Length; ++i)
      {
        // NOTE(WSWhitehouse): Using a try/catch here as if we encounter an
        // exception in this coroutine the track playback breaks. Its better
        // to catch the error and log it.
        try
        {
          if (currentNoteIndex >= Track.BeatMap[i].Length)     return true;
          if (Track.BeatMap[i][currentNoteIndex].IsEndOfTrack) return true;
          
          if (!Track.BeatMap[i][currentNoteIndex].IsActive) continue;
              
          // Update word data
          data.subLaneIndex = i;
          data.difficulty   = Track.BeatMap[i][currentNoteIndex].Difficulty;

          bool playerHasDifficulty = false;
          foreach(int player in PlayerManager.ValidPlayerIDs) 
          {
            if ((PlayerManager.PlayerData[player].Difficulty & data.difficulty) == 0) continue;
            playerHasDifficulty = true;
            break;
          }

          // if they don't then we don't send the word
          if (!playerHasDifficulty) continue;
              
          // NOTE(WSWhitehouse): Not using TryCatchInvoke here as its surrounded by a try/catch already
          OnSendWord?.Invoke(data);
          sentWord = true;

          TotalBlocksSent += 1;
        }
        catch(Exception e)
        {
          Log.Error($"{e.Message}\n{e.StackTrace}");
          break;
        }
      }

      if (sentWord)
      {
        CurrentWordIndex = ArrayUtil.WrapIndex(CurrentWordIndex + 1, Track.SpamMessage.NumOfWords);
        CurrentWord      = Track.SpamMessage.wordsWithoutPunctuation[CurrentWordIndex];
      }
          
      overshoot -= Track.SecondsPerNote;

    } while (overshoot > 0);
        
    NextNoteTime += (Track.SecondsPerNote * notesPlayed);
    return false;
  }

  private static IEnumerator __StopTrack(float volFadeDuration)
  {
    if (!AudioSource.isPlaying) yield break;
    
    float startValue = AudioSource.volume;
    
    yield return VolumeFade(startValue, 0.0f, (float)Track.LaneDuration);
    
    AudioSource.Stop();
    AudioSource.volume = startValue;
  }

  private static IEnumerator __VolumeFade(float start, float end, float duration)
  {
    double timeStart   = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
    double timeElapsed = 0.0;
    while (timeElapsed < duration)
    {
      AudioSource.volume = maths.Lerp(start, end, (float)(timeElapsed / duration));
      timeElapsed        = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - timeStart;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    AudioSource.volume = end;
  }
}

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioManager
{
  private const string MixerMasterVolTag          = "MasterVol";
  private const string MixerMusicVolTag           = "MusicVol";
  private const string MixerSFXVolTag             = "SFXVol";
  private const string MixerVoiceOverVolTag       = "VoiceOverVol";

  private static float _MasterVol = 1f;
  private static float _MusicVol = 1f;
  private static float _SFXVol = 1f;
  private static float _VoiceOverVol = 1f;

  public static float MasterVol
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _MasterVol;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set
    { 
      _MasterVol = maths.Clamp(value, 0.0001f, 1.0f);
      PlayerPrefs.SetFloat(nameof(MasterVol), MasterVol);
      //Mixer.SetFloat(MixerMasterVolTag, VolToDb(MasterVol));
      AudioEngine.audioEngineInstance.fmodRouting.masterBus.setVolume(MasterVol); //Set bus bus volume in fmodRouting through audio engine 
    }
  }
  
  public static float MusicVol
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _MusicVol;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set 
    {
      _MusicVol = maths.Clamp(value, 0.0001f, 1.0f);
      PlayerPrefs.SetFloat(nameof(MusicVol), MusicVol);
      //Mixer.SetFloat(MixerMusicVolTag, VolToDb(MusicVol));
      AudioEngine.audioEngineInstance.fmodRouting.musicBus.setVolume(MusicVol); //Set music bus volume in fmodRouting through audio engine 
    }
  }
  
  public static float SFXVol
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _SFXVol;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set 
    {
      _SFXVol = maths.Clamp(value, 0.0001f, 1.0f);
      PlayerPrefs.SetFloat(nameof(SFXVol), SFXVol);
      // Mixer.SetFloat(MixerSFXVolTag, VolToDb(SFXVol));
      AudioEngine.audioEngineInstance.fmodRouting.sfxBus.setVolume(SFXVol); //Set sfx bus volume in fmodRouting through audio engine 
    }
  }
  
  public static float VoiceOverVol
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _VoiceOverVol;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set 
    {
      _VoiceOverVol = maths.Clamp(value, 0.0001f, 1.0f);
      PlayerPrefs.SetFloat(nameof(VoiceOverVol), VoiceOverVol);
      //Mixer.SetFloat(MixerVoiceOverVolTag, VolToDb(VoiceOverVol));
      AudioEngine.audioEngineInstance.fmodRouting.voBus.setVolume(VoiceOverVol); //Set voice over bus volume in fmodRouting through audio engine 
    }
  }
  
  public static AudioMixer Mixer                     { get; private set; }
  public static AudioMixerGroup MasterMixer          { get; private set; }
  public static AudioMixerGroup MusicMixer           { get; private set; }
  public static AudioMixerGroup SFXMixer             { get; private set; }
  public static AudioMixerGroup VoiceOverMixer       { get; private set; }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    Log.Print("Initialising Audio Manager!");
    
    Mixer = Resources.Load<AudioMixer>("GameMixer");
    
    Debug.Assert(Mixer != null, "AudioManager: GameMixer cannot be loaded from resources! Please ensure its named correctly.");
    
    MasterMixer          = Mixer.FindMatchingGroups("Master")[0];
    MusicMixer           = Mixer.FindMatchingGroups("Music")[0];
    SFXMixer             = Mixer.FindMatchingGroups("SFX")[0];
    VoiceOverMixer       = Mixer.FindMatchingGroups("VoiceOverMaster")[0];
    
    Debug.Assert(MasterMixer          != null, "AudioManager: MasterMixer cannot be loaded from GameMixer! Please ensure its named correctly.");
    Debug.Assert(MusicMixer           != null, "AudioManager: MusicMixer cannot be loaded from GameMixer! Please ensure its named correctly.");
    Debug.Assert(SFXMixer             != null, "AudioManager: SFXMixer cannot be loaded from GameMixer! Please ensure its named correctly.");
    Debug.Assert(VoiceOverMixer       != null, "AudioManager: VoiceOverMixer cannot be loaded from GameMixer! Please ensure its named correctly.");
    
    LoadValues();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void LoadValues()
  {
    MasterVol          = PlayerPrefs.GetFloat(nameof(MasterVol), 1.0f);
    MusicVol           = PlayerPrefs.GetFloat(nameof(MusicVol), 1.0f);
    SFXVol             = PlayerPrefs.GetFloat(nameof(SFXVol), 0.75f);
    VoiceOverVol       = PlayerPrefs.GetFloat(nameof(VoiceOverVol), 1.0f);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float VolToDb(float vol)
  {
    // https://stackoverflow.com/a/50940850/13195883
    return Mathf.Log10(vol) * 20;
  }
  
  
}

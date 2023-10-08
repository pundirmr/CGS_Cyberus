using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using HidSharp;
using HidSharp.Platform;
using Unity.Mathematics;

public static class StreamDeckManager
{
  public static StreamDeck[] StreamDecks = new StreamDeck[PlayerManager.MaxPlayerCount];
  public static int MaxStreamDeckCount   => StreamDecks.Length;
  public static int StreamDeckCount      => StreamDecks.Count(deck => deck != null);
  
  public delegate IEnumerator WaitForValidStreamDeckDel(int streamDeckID);
  public static WaitForValidStreamDeckDel WaitForValidStreamDeck;

  private static Coroutine _updateCoroutine = null;
  
  // NOTE(WSWhitehouse): The local device list changed event is done on a different thread,
  // so update this boolean and check it in the update loop. This is so we can perform the 
  // updates on the main thread.
  private static volatile bool _localDeviceListChanged = false;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    #if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
      Log.Warning("You are not running on Windows! Not initialsing Stream Decks.");
      return;
    #endif
    
    Log.Print("Initialising Stream Deck Manager!");
    
    WaitForValidStreamDeck = __WaitForValidStreamDeck;
    
    HidSelector.Setup();
    DeviceList.Local.Changed += OnLocalDeviceListChanged;
    
    // NOTE(WSWhitehouse): Only allocate new stream decks if we're not in the start scene, this is 
    // wrapped in Unity editor #if blocks as the game will always load scene 0 on start-up.
    #if UNITY_EDITOR
    if (SceneHandler.SceneIndex != 0)
    {
      int count = SearchForNewStreamDecks(true);
      Log.Print($"Stream Decks Found: {count.ToString()}");
    }
    #endif

    _updateCoroutine = StaticCoroutine.Start(UpdateCoroutine());
    Application.quitting += Dispose;
  }

  private static void OnLocalDeviceListChanged(object sender, DeviceListChangedEventArgs e)
  {
    _localDeviceListChanged = true;
  }

  private static void Dispose()
  {
    StaticCoroutine.Stop(_updateCoroutine);
    _updateCoroutine = null;

    // NOTE(WSWhitehouse): Using a try/catch here to ensure the HidSelector gets destroyed correctly
    try
    {
      for (int i = 0; i < StreamDecks.Length; i++)
      {
        StreamDecks[i]?.Dispose();
        StreamDecks[i] = null;
      }
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
    }
    
    HidSelector.Destroy();
  }
  
  private static IEnumerator UpdateCoroutine()
  {
    while (true)
    {
      if (_localDeviceListChanged)
      {
        _localDeviceListChanged = false;
        foreach (StreamDeck deck in StreamDecks)
        {
          deck?.RefreshConnection();
        }
      }

      // Update stream decks
      foreach (StreamDeck deck in StreamDecks)
      {
        deck?.UpdateInput();
      }
      
      yield return CoroutineUtil.WaitForUpdate;
    }
  }

  public static int SearchForNewStreamDecks(bool allocateStreamDecks)
  {
    HidDevice[] streamDecks = StreamDeckInternal.GetStreamDeckHidDevices().ToArray();

    if (!allocateStreamDecks) return streamDecks.Length;

    for (int i = 0; i < streamDecks.Length; i++)
    {
      // Dont allow new stream decks to be allocated if its above the max count
      if (i >= MaxStreamDeckCount) break;

      if (StreamDecks[i] != null)
      {
        // Clean up existing stream deck
        StreamDecks[i].Dispose();
        StreamDecks[i] = null;
      }

      StreamDecks[i] = new StreamDeck(streamDecks[i]);
    }

    return streamDecks.Length;
  }

  // --- UTIL --- //
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsStreamDeckValid(int streamDeckID) => StreamDecks[streamDeckID] != null;

  private static IEnumerator __WaitForValidStreamDeck(int streamDeckID)
  {
    while (StreamDecks[streamDeckID] == null) yield return CoroutineUtil.WaitForUpdate;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Index2DTo1D(int2 i) => ArrayUtil.Index2DTo1D(i, StreamDeck.ButtonXCount);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Index2DTo1D(int x, int y) => ArrayUtil.Index2DTo1D(x, y, StreamDeck.ButtonXCount);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int2 Index1DTo2D(int i) => ArrayUtil.Index1DTo2D(i, StreamDeck.ButtonXCount);
}

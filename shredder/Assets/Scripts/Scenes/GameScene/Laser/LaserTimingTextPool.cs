using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class LaserTimingTextPool : MonoBehaviour
{
  [SerializeField] private Canvas prefab;

  [Header("Spawning Data")] 
  [Tooltip("The amount of char blocks to initially spawn on awake")] 
  [SerializeField] private int initialSpawnAmount = 15;
  [Tooltip("The amount of char blocks to spawn when we hit the limit")] 
  [SerializeField] private int resizeFactor = 2;

  private static readonly string[] hitTimingText = { "Invalid", "Miss", "Wrong Colour", "Early", "Late", "Okay", "Good", "Perfect" };
  
  public class TextInstance
  {
    public TextInstance(Canvas canvas, LaserHitTiming hitTiming)
    {
      Canvas     = canvas;
      HitTiming  = hitTiming;

      Text      = GameObject.GetComponentInChildren<TMP_Text>(true);
      Text.text = _laserHitTimingText[HitTiming];
    }
    
    public Canvas Canvas;
    public TMP_Text Text;
    public LaserHitTiming HitTiming;
    public GameObject GameObject => Canvas.gameObject;
    public Transform Transform   => Canvas.transform;
  }

  private Dictionary<LaserHitTiming, Queue<TextInstance>> _pool = new Dictionary<LaserHitTiming, Queue<TextInstance>>();

  // Singleton
  private static LaserTimingTextPool SingletonInstance;
  
  // NOTE(WSWhitehouse): ToString() calls are expensive so caching them in static ctor
  private static readonly Dictionary<LaserHitTiming, string> _laserHitTimingText;
  private static readonly int LaserHitTimingCount = Enum.GetNames(typeof(LaserHitTiming)).Length;
  
  static LaserTimingTextPool()
  {
    // Cache laser hit timing text
    _laserHitTimingText = new Dictionary<LaserHitTiming, string>();
    for (int i = 0; i < LaserHitTimingCount; i++)
    {
      // NOTE(WSWhitehouse): Can't use Enum.GetNames for the names as they need to be
      // added to a dictionary with the LaserHitTiming as its key.
      LaserHitTiming hitTiming = (LaserHitTiming)(1 << i);
      _laserHitTimingText.Add(hitTiming, hitTimingText[i]);
    }
  }

  private void Awake()
  {
    // If a char block pool already exists then destroy this instance
    if (SingletonInstance != null)
    {
      Log.Warning("Another instance of the Char Block Pool has been created. Destroying this instance.", this);
      Destroy(this.gameObject);
      return;
    }

    // Set up singleton
    SingletonInstance = this;

    // Spawn initial blocks

    
    for (int i = 0; i < LaserHitTimingCount; i++)
    {
      LaserHitTiming hitTiming = (LaserHitTiming)(1 << i);
      _pool.Add(hitTiming, new Queue<TextInstance>(initialSpawnAmount));
      
      for (int j = 0; j < initialSpawnAmount; j++)
      {
        SpawnText(hitTiming);
      }
    }
  }

  private void OnDestroy()
  {
    // Dont do anything if this instance isn't the one stored in the singleton
    if (SingletonInstance != this) return;

    SingletonInstance = null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SpawnText(LaserHitTiming timing)
  {
    Canvas canvas      = Instantiate(prefab);
    TextInstance text  = new TextInstance(canvas, timing);
    ReturnToPool_Impl(text);
  }

  private TextInstance GetText_Impl(LaserHitTiming timing)
  {
    // If there are no timing texts in the pool then spawn some more
    if (_pool[timing].Count <= 0)
    {
      Log.Error("Laser Timing Text Pool has resized! Consider increasing initial spawn amount.", this);
      int newCount = _pool.Count * resizeFactor;
      for (int i = 0; i < LaserHitTimingCount; i++)
      {
        LaserHitTiming hitTiming = (LaserHitTiming)(1 << i);
        for (int j = 0; j < newCount; j++)
        {
          SpawnText(hitTiming);
        }
      }
    }

    TextInstance block = _pool[timing].Dequeue();
    return block;
  }

  private void ReturnToPool_Impl(TextInstance text)
  {
    text.Transform.SetParent(this.transform);
    text.Canvas.enabled = false;
    _pool[text.HitTiming].Enqueue(text);
  }

  // --- STATIC FUNCTIONS --- //
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TextInstance GetText(LaserHitTiming timing)
  {
    Debug.Assert(SingletonInstance != null, "There is no instance of Laser Timing Pool! Please add one.");
    return SingletonInstance.GetText_Impl(timing);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReturnToPool(TextInstance text)
  {
    Debug.Assert(SingletonInstance != null, "There is no instance of Laser Timing Pool! Please add one.");
    SingletonInstance.ReturnToPool_Impl(text);
  }
}

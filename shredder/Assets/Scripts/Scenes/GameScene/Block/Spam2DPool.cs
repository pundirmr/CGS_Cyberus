using System;
using System.Collections.Generic;
using UnityEngine;

public class Spam2DPool : MonoBehaviour
{
  [SerializeField] private Spam2D spamPrefab;
  [SerializeField] private Vector3 spamSize;
  [SerializeField] private int initialSpawnAmount = 200;
  [SerializeField] private int poolResizeFactor = 2;
  
  // NOTE(WSWhitehouse): Keep track of if the pool has been initialised, throw
  // an error if a function is called without being initialised
  private static bool StaticPoolInitialised = false;
  
  private static Spam2D SpamPrefab;
  private static Vector3 SpamSize;
  private static int PoolResizeFactor;
  private static Transform PoolParent;

  private static StackArray<Spam2D> SpamPool;
  private static List<Spam2D> ActiveSpam;

  private void Awake()
  {
    // Set static vars
    SpamPrefab       = spamPrefab;
    SpamSize         = spamSize;
    PoolResizeFactor = poolResizeFactor;
    PoolParent       = this.transform;
    
    // Pool has been initialised
    StaticPoolInitialised = true;
    
    // Spawn initial pool
    SpamPool   = new StackArray<Spam2D>(initialSpawnAmount);
    ActiveSpam = new List<Spam2D>(initialSpawnAmount);
    for (int i = 0; i < initialSpawnAmount; i++)
    {
      ReturnSpamToPool(InstantiateSpam2D());
    }
  }

  // NOTE(WSWhitehouse): If this object is destroyed set the init flag to false
  private void OnDestroy() => StaticPoolInitialised = false;
  
  private static Spam2D InstantiateSpam2D()
  {
    Spam2D spam = Instantiate(SpamPrefab, PoolParent);
    spam.transform.localScale = SpamSize;
    return spam;
  }
  
  private static void ResizeSpamPool()
  {
    // NOTE(WSWhitehouse): Raising an error here as ideally we don't want the pool to ever resize. 
    // We could raise a warning instead, but it's more likely to be ignored.
    Log.Error("Resizing Spam2D Object Pool! Consider increasing initial spawn amount.");
    
    int newCapacity     = SpamPool.Capacity * PoolResizeFactor;
    int newSpamToSpawn  = maths.Abs(newCapacity - SpamPool.Capacity);
    SpamPool.Capacity   = newCapacity;
    ActiveSpam.Capacity = newCapacity;

    for (int i = 0; i < newSpamToSpawn; i++)
    {
      ReturnSpamToPool(InstantiateSpam2D());
    }
  }
  
  public static Spam2D GetSpam( Vector3 position, Color colour)
  {
    if (!StaticPoolInitialised)
    {
      Log.Error("Spam 2D Object Pool not initialised! Please ensure a Spam2DPool exists in the scene.");
      return null;
    }
    
    position.z = 0; // NOTE(WSWhitehouse): As spam is 2D make sure they are all on the same z plane
        
    if (SpamPool.Count <= 0) ResizeSpamPool();

    Spam2D spam = SpamPool.Pop();
    spam.transform.position = position;
    spam.CreationTime       = Time.time;
    
    spam.SetColour(colour);
    spam.EnableSpam();

    ActiveSpam.Add(spam);
    
    return spam;
  }
  
  public static void ReturnSpamToPool(Spam2D spam)
  {
    if (!StaticPoolInitialised)
    {
      Log.Error("Spam 2D Object Pool not initialised! Please ensure a Spam2DPool exists in the scene.");
      return;
    }
    
    spam.DisableSpam();

    if (ActiveSpam.Contains(spam)) ActiveSpam.Remove(spam);
    
    SpamPool.Push(spam);
  }

  public static void FadeAllActiveSpam(float fadeDuration = 1.5f) {
      // NOTE(Zack): the [FadeSpamOut] function automatically adds spam back to the pool after its finished 
      foreach (var spam in ActiveSpam) spam.FadeSpamOut(fadeDuration);
  }
}

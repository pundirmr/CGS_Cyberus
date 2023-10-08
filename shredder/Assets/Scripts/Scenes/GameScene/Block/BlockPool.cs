using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This is a singleton class - there should only be one instance. This takes
/// a word or char block as a template parameter. And initially spawns an amount
/// and more are added if the limit is hit.
/// </summary>
public class BlockPool<T> : MonoBehaviour where T : MonoBehaviour
{
  [SerializeField] private T prefab;

  [Header("Spawning Data")] 
  [Tooltip("The amount of char blocks to initially spawn on awake")] 
  [SerializeField] private int initialSpawnAmount = 100;
  [Tooltip("The amount of char blocks to spawn when we hit the limit")] 
  [SerializeField] private int resizeFactor = 2;

  private Queue<T> _pool = new Queue<T>();

  // Singleton
  private static BlockPool<T> SingletonInstance;

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
    for (int i = 0; i < initialSpawnAmount; i++)
    {
      SpawnBlock();
    }
  }

  private void OnDestroy()
  {
    // Dont do anything if this instance isn't the one stored in the singleton
    if (SingletonInstance != this) return;

    SingletonInstance = null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SpawnBlock()
  {
    T block = Instantiate(prefab);
    ReturnBlock_Impl(block);
  }

  private T GetBlock_Impl()
  {
    // If there are no blocks in the pool then spawn some more
    if (_pool.Count <= 0)
    {
      Log.Error("Block Pool has resized! Consider increasing initial spawn amount.", this);
      int newCount = _pool.Count * resizeFactor;
      for (int i = 0; i < newCount; i++)
      {
        SpawnBlock();
      }
    }

    T block = _pool.Dequeue();
    block.gameObject.SetActive(true);
    return block;
  }

  private void ReturnBlock_Impl(T block)
  {
    block.transform.SetParent(this.transform);
    block.gameObject.SetActive(false);
    _pool.Enqueue(block);
  }

  // --- STATIC FUNCTIONS --- //
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T GetBlock()
  {
    Debug.Assert(SingletonInstance != null, "There is no instance of Block Pool! Please add one.");
    return SingletonInstance.GetBlock_Impl();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReturnBlock(T block)
  {
    Debug.Assert(SingletonInstance != null, "There is no instance of Block Pool! Please add one.");
    SingletonInstance.ReturnBlock_Impl(block);
  }
}
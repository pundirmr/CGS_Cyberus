using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BackgroundObjectRandomiser : MonoBehaviour
{
  [SerializeField] private GameObject[] assets = Array.Empty<GameObject>();
  
  [Tooltip("Is there are chance to not choose an asset, and have nothing")]
  [SerializeField] private bool canChooseNothing = false;
  
  [Tooltip("How many times will \"Show up\" in the randomiser.")]
  [SerializeField] private int nothingTimes = 1;
  private const int NOTHING_INDEX = -1;
  
  public delegate void AssetSelected(int assetIndex);
  public AssetSelected OnAssetSelected;
  
  // NOTE(WSWhitehouse): A array of all the possible indices for the buildings and streetlamps, it is
  // shuffled randomly so we generate random order of assets. This is more efficient than randomly 
  // generating an index each time. Once we reach the end of the array they are reshuffled.
  private int[] _assetsIndices;
  private Unity.Mathematics.Random _shuffleRandomizer;

  // NOTE(WSWhitehouse): The current index into the indices arrays above. NOT the index into the main
  // asset arrays that are serialized!
  private int _assetsIndex;

  private void Awake()
  {
    int assetsLength = assets.Length;
    
    // Set up indices arrays
    if (canChooseNothing)
    {
      int nothingTimesClamped = maths.Clamp(nothingTimes, 1, int.MaxValue);
      _assetsIndices          = new int[assetsLength + nothingTimes];

      for (int i = 0; i < nothingTimesClamped; i++)
      {
        _assetsIndices[^(i + 1)] = NOTHING_INDEX;
      }
    }
    else
    {
      _assetsIndices = new int[assetsLength];
    }
    
    for (int i = 0; i < assetsLength; i++)
    {
      _assetsIndices[i] = i;
    }

    TimeSpan epoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
    _shuffleRandomizer = new Unity.Mathematics.Random((uint)epoch.TotalSeconds);
    ArrayUtil.Shuffle(_assetsIndices, _assetsIndices.Length, _shuffleRandomizer);

    // NOTE(WSWhitehouse): Randomly generating index into indices array here so each sections starts at
    // a different point throughout the array. Reduces lag spikes as they all wont want to reshuffle at
    // a similar time.
    _assetsIndex = UnityEngine.Random.Range(0, assets.Length);
  }

  public void RandomiseObject()
  {
    _assetsIndex++;
    
    // Check if asset index has runover, if so reshuffle
    if (_assetsIndex >= _assetsIndices.Length)
    {
      _assetsIndex = 0;
      ArrayUtil.Shuffle(_assetsIndices, _assetsIndices.Length, _shuffleRandomizer);
    }

    DeactivateAllAssets();
    
    int index = _assetsIndices[_assetsIndex];
    OnAssetSelected?.Invoke(index);
    if (index == NOTHING_INDEX) return;

    assets[index].SetActive(true);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void DeactivateAllAssets()
  {
    foreach (GameObject asset in assets) { asset.SetActive(false);   }
  }
}
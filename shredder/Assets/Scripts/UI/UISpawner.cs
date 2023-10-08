using System;
using System.Collections.Generic;
using UnityEngine;

public class UISpawner : MonoBehaviour
{
  [HelpBox("The prefab must contain a `UIPlayerID` script in the root of the prefab. This is to ensure each instantiated UI knows which player they are assigned too.")]
  [SerializeField] private PlayerID UIPrefab;
  [Space]
  [SerializeField] private ColumnLayoutGroup spawnParent;
  [SerializeField] private int numberToSpawn = 3;
  
  [HideInInspector] public List<PlayerID> SpawnedUI;
  public Action<int> OnUISpawned; // NOTE(WSWhitehouse): int = index into SpawnedUI list

  private void Start()
  {
    if (UIPrefab == null) return;
    
    SpawnedUI = new List<PlayerID>(numberToSpawn);

    for (int i = 0; i < numberToSpawn; i++)
    {
      PlayerID ui = Instantiate(UIPrefab, spawnParent.transform);
      ui.SetID(i);
      SpawnedUI.Add(ui);
      OnUISpawned?.Invoke(i);
    }

    spawnParent.CalculateUILayout();
  }
}

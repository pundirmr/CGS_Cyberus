using System.Collections.Generic;
using UnityEngine;

// NOTE(Zack): this script is currently used for the TrackSelectScene only
public class PlayerSpawner : MonoBehaviour {
    [Header("Spawn Settings")]
    [SerializeField] private PlayerID prefab;

    public delegate void SpawnEvent(int playerID);
    public SpawnEvent OnPlayerSpawnedInScene;

    private const int count = PlayerManager.MaxPlayerCount;
    [HideInInspector] public List<PlayerID> players = new (count);
    
    private void Start() {
        for (int i = 0; i < count; ++i) {
            PlayerID id = Instantiate(prefab);
            id.SetID(i);
            players.Add(id);
            OnPlayerSpawnedInScene?.Invoke(i);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerSpamBox : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlayerID _playerID;
    [SerializeField] private ClearLane clearLane;

    [Header("Spawn Spam Settings")]
    [SerializeField] private float spamZPos = 0.25f;
    [SerializeField] private RangedFloat xForceRange;
    [SerializeField] private RangedFloat yForceRange;
    [SerializeField] private Transform minSpamXTransform;
    [SerializeField] private Transform maxSpamXTransform;
    
    private float minSpamXPos => minSpamXTransform.position.x;
    private float maxSpamXPos => maxSpamXTransform.position.x;
    
    [Header("Spam Settings")]
    [SerializeField] private float ageOfSpamToTriggerBufferOverflowInSecs = 2f;

    [NonSerialized] public List<Spam2D> Spam = new List<Spam2D>(500);
    public int PlayerID => _playerID.ID;
        
    private List<Vector2> _spamForces = new List<Vector2>(10);
    private Vector2 _playerPresenceOffset;
    private float _flow;
    private float _playerSeparation;
    private bool _inited;
    private int _spamForceIndex = 0;

    private int SpamForceIndex {
        get {
            _spamForceIndex++;
            if (_spamForceIndex >= _spamForces.Count) {
                ArrayUtil.Shuffle(_spamForces, _spamForces.Count);
                _spamForceIndex = 0;
            }

            return _spamForceIndex;
        }
        set => _spamForceIndex = value;
    }

    
    private void Awake() {
        for (int i = 0; i < _spamForces.Capacity; i++) {
            float x = xForceRange.Random();
            float y = yForceRange.Random();
            _spamForces.Add(new Vector2(x, y));
        }
    }

    private IEnumerator Start() {
        yield return PlayerManager.WaitForValidPlayer(PlayerID);
        _inited = true;
        
        clearLane.Init(PlayerID);
        _playerID.PlayerData.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDestroy() {
        if (!_inited) return;
        
        _playerID.PlayerData.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!_inited)                    return;
        if (_playerID.PlayerData.IsDead) return;

        // NOTE(WSWhitehouse): If we're already clearing the lane dont do any checks
        if (clearLane.ClearingLane) return;
        
        Spam2D spam = null;
        if (!other.TryGetComponent(out spam)) return;

        // we check how long a piece of spam has been in the box
        float spamAgeDiff = Time.time - spam.CreationTime;
        if (spamAgeDiff < ageOfSpamToTriggerBufferOverflowInSecs) return;
        
        _playerID.PlayerData.CurrentHealth--;

        // NOTE(WSWhitehouse): We dont want to run the logic bomb ability if this was their final life
        if (_playerID.PlayerData.IsDead) {
            foreach (Spam2D spam2D in Spam) { spam2D.FreezeSpam(); }
            return;
        }

        // we clear the lane of spam
        clearLane.StartClearLane(this);
    }

    public void SpawnSpam(Vector3 position, Color colour) {
        // NOTE(WSWhitehouse): Dont spawn spam if we're currently clearing the lanes
        if (clearLane.ClearingLane) return;
        
        Spam2D newSpam = Spam2DPool.GetSpam(position, colour);
        
        // NOTE(WSWhitehouse): Clamping the X position to ensure it's inside this spam box,
        // there were issues where spam2D were spawning in other peoples spam boxes and causing
        // them to lose a life when this player died.
        float posX = maths.Clamp(position.x, minSpamXPos, maxSpamXPos);
        newSpam.transform.position = new Vector3(posX, position.y, spamZPos);
        
        newSpam.Body.AddForce(_spamForces[SpamForceIndex]);
        newSpam.FreezeSpam();
        Spam.Add(newSpam);
        
        // Add to players total spam counter
        _playerID.PlayerData.TotalSpam += 1;
        
        if (!_playerID.PlayerData.IsDead) return;
        // if a player has been eliminated we dim incoming spam
        newSpam.DimSpam(1f);
    }

    private void OnPlayerDeath() {
        foreach (Spam2D spam2D in Spam) {
            spam2D.DimSpam(1f);
        }
    }
}

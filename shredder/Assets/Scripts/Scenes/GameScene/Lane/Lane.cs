using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The players lane, keeps track of colour lanes and responds to new words being sent.
/// </summary>
public class Lane : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlayerID player;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform endTransform;
    [SerializeField] private ColourLane[] colourLanes = Array.Empty<ColourLane>();

    public Transform StartTransform => startTransform;
    public Transform EndTransform => endTransform;
    public Vector3 LaneNormal { get; private set; }
    public float Distance => float3Util.Distance(startTransform.position, endTransform.position);

    public delegate void BlockReachesEndCallback(WordBlock wordBlock);

    // Assigning callback in awake to reduce delegate allocations
    private BlockReachesEndCallback _callback;

    private void Awake()
    {
        _callback = OnBlockReachesEndCallback;

        CalculateLaneNormal();
    }

    private IEnumerator Start()
    {
        yield return PlayerManager.WaitForValidPlayer(player.ID);

        MusicTrackPlayer.OnSendWord += OnSendWord;
        player.PlayerData.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDestroy()
    {
        if (!player.IsValid) return;
        if (player.PlayerData.IsDead) return;

        MusicTrackPlayer.OnSendWord -= OnSendWord;
        player.PlayerData.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        MusicTrackPlayer.OnSendWord -= OnSendWord;
        player.PlayerData.OnPlayerDeath -= OnPlayerDeath;
    }

    private void CalculateLaneNormal()
    {
        Vector3 side1 = endTransform.transform.position - startTransform.transform.position;
        Vector3 side2 = new Vector3(-1.0f, 0.0f, 0.0f);
        LaneNormal = float3Util.Normalise(float3Util.Cross(side1, side2));
    }

    private void OnSendWord(WordData data)
    {
        // Log.Print($"Lane Word Data: ({data.word} : {data.wordIndex})");
        if (!player.IsValid) return;
        if ((data.difficulty & player.PlayerData.Difficulty) == 0) return;

        
        WordBlock wordBlock = WordBlockPool.GetBlock();
        wordBlock.Generate(data, player.PlayerData);

        Vector3 startPos = colourLanes[data.subLaneIndex].StartPos;
        Vector3 endPos = colourLanes[data.subLaneIndex].EndPos;

        wordBlock.MoveWordBlock(startPos, endPos, data.noteData, _callback);
    }

    private void OnBlockReachesEndCallback(WordBlock wordBlock)
    {
        WordBlockPool.ReturnBlock(wordBlock);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 pointOnLine = float3Util.PointAlongLine(StartTransform.position, EndTransform.position, 0.5f);
        CalculateLaneNormal();

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pointOnLine, pointOnLine + LaneNormal);
    }
#endif
}

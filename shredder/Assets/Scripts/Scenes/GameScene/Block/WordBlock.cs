using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A block that represents a word. Is made up of smaller <see cref="DepreciatedCharBlock"/>.
/// </summary>
public class WordBlock : MonoBehaviour
{
    public delegate void WordBlockReachedLaserDel(WordBlock wordBlock);
    public static WordBlockReachedLaserDel OnWordBlockReachedLaser;

    [Header("Components")]
    [SerializeField] private GameObject wireframeObj;
    [SerializeField] private GameObject dissolveObj;
    [SerializeField] private TMP_Text text;

    [field: SerializeField] public DissolveEffect Dissolve { get; private set; }
    [field: SerializeField] public WireframeEffect Wireframe { get; private set; }
    [field: SerializeField] public KeywordEffect KeywordEffect { get; private set; }

    [Header("Block Settings")]
    [SerializeField] private float maxBlockSize = 6f;

    [Header("Block Destroy Percentages")]
    [SerializeField] private int blockLength = 6;
    [HelpBox("In each of these cases, the value is the percentage of characters that will be destroyed on a hit. " +
             "Therefore, a higher value means more characters are destroyed by the laser and a low value means the " +
             "characters are more likely to fall into the spam box. Percentage value is between 0.0f and 1.0f.")]
    [SerializeField][Range(0.0f, 1.0f)] private float missPercentage;
    [SerializeField][Range(0.0f, 1.0f)] private float wrongColPercentage;
    [SerializeField][Range(0.0f, 1.0f)] private float earlyLatePercentage;
    [SerializeField][Range(0.0f, 1.0f)] private float okayPercentage;
    [SerializeField][Range(0.0f, 1.0f)] private float goodPercentage;
    [SerializeField][Range(0.0f, 1.0f)] private float perfectPercentage;

    [NonSerialized] public WordData WordData;
    [NonSerialized] public Color Colour;
    [NonSerialized] public Color HDRColour;
    [NonSerialized] public int ColourIndex;

    public int PlayerID { get; set; }
    public double TimeOnLane { get; private set; }

    private delegate IEnumerator MoveWordBlockDelegate(Vector3 startPos, Vector3 endPos, NoteData noteData, Lane.BlockReachesEndCallback callback);
    private MoveWordBlockDelegate _moveWordBlockFunc;
    private Coroutine _moveBlockCoroutine;

    private delegate IEnumerator StartDissolve();
    private StartDissolve DissolveEffect;

    // NOTE(WSWhitehouse): This array is shuffled at random, do NOT rely on it for indices
    private int[] _indices;

    private MeshRenderer _wireframeObjRenderer;
    private MeshRenderer _dissolveObjRenderer;

    private void Awake()
    {
        _indices = Enumerable.Range(0, (int)maxBlockSize).ToArray();
        _moveWordBlockFunc = MoveWordBlockCoroutine;
        DissolveEffect = __DissolveEffect;
    }

    private void Start()
    {
        _wireframeObjRenderer = wireframeObj.GetComponent<MeshRenderer>();
        _dissolveObjRenderer = dissolveObj.GetComponent<MeshRenderer>();
        _wireframeObjRenderer.material = Wireframe.MaterialInstance;
        _dissolveObjRenderer.material = Dissolve.MaterialInstance;
    }

    public void MoveWordBlock(Vector3 startPos, Vector3 endPos, NoteData noteData, Lane.BlockReachesEndCallback callback)
    {
        if (_moveBlockCoroutine != null) StopCoroutine(_moveBlockCoroutine);
        _moveBlockCoroutine = StartCoroutine(_moveWordBlockFunc(startPos, endPos, noteData, callback));
    }

    public void StopWordBlockMovement()
    {
        if (_moveBlockCoroutine == null) return;
        StopCoroutine(_moveBlockCoroutine);
        _moveBlockCoroutine = null;
    }

    private IEnumerator MoveWordBlockCoroutine(Vector3 startPos, Vector3 endPos, NoteData noteData, Lane.BlockReachesEndCallback callback)
    {
        transform.position = startPos;
        bool reachedLaser = false;
        double laserPosT = 1.0 - GameManager.LaserPosAlongLane;
        double startTime = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime();
        double overshoot = noteData.timeOvershoot;
        double timeElapsed = overshoot;

        while (timeElapsed < GameManager.Track.LaneDuration)
        {
            // The word block has been destroyed another way (e.g. laser) so dont lerp it anymore and dont call the callback
            if (!this.isActiveAndEnabled)
            {
                yield break;
            }

            double t = timeElapsed / GameManager.Track.LaneDuration;
            transform.position = float3Util.Lerp(startPos, endPos, (float)t);


            if (!reachedLaser && t >= laserPosT)
            {
                reachedLaser = true;
                OnWordBlockReachedLaser?.Invoke(this);
            }

            timeElapsed = AudioEngine.audioEngineInstance.GetCurrentPlaybackTime() - startTime + overshoot;
            TimeOnLane = timeElapsed;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        _moveBlockCoroutine = null;
        callback?.Invoke(this);
    }

    public void Generate(WordData data, PlayerData playerData)
    {
        ref ColourScheme colours = ref playerData.ColourScheme;
        ref ColourScheme hdrColours = ref playerData.HDRColourScheme;

        WordData = data;
        PlayerID = playerData.PlayerID;
        ColourIndex = data.subLaneIndex;
        Colour = colours.Colours[ColourIndex];
        HDRColour = hdrColours.Colours[ColourIndex];

        string word = WordData.word;
        text.text = word;

        //Set up dissolve effect
        Dissolve.GradientColourOne = colours.Colours[0];
        Dissolve.GradientColourTwo = colours.Colours[1];
        Dissolve.GradientColourThree = colours.Colours[2];
        Dissolve.SetDissolveAmount(0, false);

        //Set up wireframe effect

        if (GameManager.SpamMessage.keyWords[data.wordIndex])
        {
            KeywordEffect.Activate();
        }

        Wireframe.SetColour(HDRColour);
        Wireframe.SetBaseColour(Colour);


    #if UNITY_EDITOR
        gameObject.name = word;
    #endif
    }

    public void BreakUpWordBlock(int playerID, LaserHitTiming hitTiming)
    {
        StopWordBlockMovement();

        // NOTE(WSWhitehouse): destroy percentage is a value between 0.0f and 1.0f
        float destroyPercentage = hitTiming switch
        {
            // NOTE(WSWhitehouse): Miss and wrong colour is always 100% break percentage
            LaserHitTiming.MISS         => missPercentage,
            LaserHitTiming.WRONG_COLOUR => wrongColPercentage,

            LaserHitTiming.EARLY   => earlyLatePercentage,
            LaserHitTiming.LATE    => earlyLatePercentage,
            LaserHitTiming.OKAY    => okayPercentage,
            LaserHitTiming.GOOD    => goodPercentage,
            LaserHitTiming.PERFECT => perfectPercentage,

            LaserHitTiming.INVALID => throw new ArgumentOutOfRangeException(nameof(hitTiming), hitTiming, null),
            _                      => throw new ArgumentOutOfRangeException(nameof(hitTiming), hitTiming, null)
        };

        // int wordLength = WordData.word.Length;
        int destroyCount = (int)(blockLength * destroyPercentage);

        // NOTE(WSWhitehouse): Shuffle the indices array instead of randomly generating a load of 
        // indices that are unique to each other. Using this method is faster and doesn't block
        // the thread while waiting for more indices.
        ArrayUtil.Shuffle(_indices, _indices.Length);

        PlayerSpamBox spamBox = GameManager.PlayerSpamBoxes[playerID];
        for (int i = destroyCount; i < blockLength; i++)
        {
            int index = _indices[ArrayUtil.WrapIndex(i, _indices.Length)];

            Vector3 blockPos = transform.position;
            Vector3 blockSize = _wireframeObjRenderer.bounds.size;
            Vector3 halfBlockSize = _wireframeObjRenderer.bounds.extents;

            float charStep = blockSize.x / blockLength;

            float xPos = (blockPos.x - halfBlockSize.x) + (charStep * index);
            float yPos = blockPos.y;
            float zPos = blockPos.z;
            Vector3 pos = new Vector3(xPos, yPos, zPos);

            spamBox.SpawnSpam(pos, HDRColour);
        }

        // NOTE(WSWhitehouse): Sort the array so next time it is in order, we used to use
        // the built in Array.Sort() function. But found it generated garbage so resorted 
        // to this algorithm instead.
        // https://twitter.com/nothings/status/1523058841109164033?s=20&t=SDzN8RtDyIrN5btpPMDncw
        for (int i = 0; i < _indices.Length; i++)
        {
            for (int j = i + 1; j < _indices.Length; j++)
            {
                if (_indices[i] > _indices[j])
                {
                    ArrayUtil.Swap(_indices, i, j);
                }
            }
        }

        KeywordEffect.Deactivate();

        // NOTE(WSWhitehouse): Only start the dissolve effect if the object is active
        // and enabled - otherwise we get a coroutine cannot be started error.
        if (isActiveAndEnabled) StartCoroutine(DissolveEffect());
    }

    private IEnumerator __DissolveEffect()
    {
        yield return Dissolve.DoDissolveEffect(true);
        WordBlockPool.ReturnBlock(this);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
// Reflection used in move laser along lane
using System.Reflection;
#endif

[ExecuteAlways]
[RequireComponent(typeof(LaserInput))]
public class Laser : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Lane lane;
    [SerializeField] private PlayerID playerID;

    [Header("Laser Values")]
    [SerializeField] private float laserDuration = 0.2f;
    [SerializeField] private int wordBlockInitialCapacity = 10;
    [SerializeField] private LayerMask layerMask;

    [Header("Timings")]
    [SerializeField] private float okayDistance;
    [SerializeField] private float goodDistance;
    [SerializeField] private float perfectDistance;

#if UNITY_EDITOR
    [Header("Debug Settings")]
    [SerializeField] private Color okayGizmoColour    = Color.red;
    [SerializeField] private Color goodGizmoColour    = Color.yellow;
    [SerializeField] private Color perfectGizmoColour = Color.magenta;
#endif

    public PlayerID PlayerID => playerID;
    
    public struct LaserInfo
    {
        public int colourIndex;
        public Color colour;
    }

    public delegate void LaserFireEvent(LaserInfo info);
    public LaserFireEvent OnLaserFireStarted;
    public LaserFireEvent OnLaserFireStopped;

    public struct LaserHitInfo
    {
        public WordBlock WordBlock;
        public WordData WordData => WordBlock.WordData;
        public LaserHitTiming Timing;
    }

    public delegate void LaserHitEvent(LaserHitInfo info);
    public LaserHitEvent OnLaserHit;
    public LaserHitEvent OnLaserMiss;

    private delegate IEnumerator LaserCoroutine(int colourIndex);
    private LaserCoroutine LaserFireFunc;
    
    private DelegateUtil.EmptyCoroutineDel CheckForBlocks;
    private Coroutine _checkForBlocksCoroutine = null;
    private bool[] _inputCaptured;
    private LaserInfo[] _laserInfos;

    public LaserInput LaserInput { get; private set; } 
    
    private StreamDeck _streamDeck => StreamDeckManager.StreamDecks[playerID.ID];

    private List<WordBlock> _wordBlock;

    private void Awake()
    {
        CheckForBlocks = __CheckForBlocks;
        
        LaserInput     = GetComponent<LaserInput>();
        _wordBlock     = new List<WordBlock>(wordBlockInitialCapacity);
        _inputCaptured = new bool[LaserInput.colourButtons.Length];
        
        _laserInfos = new LaserInfo[LaserInput.colourButtons.Length];

        LaserFireFunc = FireLaser;
    }

    private IEnumerator Start()
    {
        MoveLaserAlongLane();
      
        yield return PlayerManager.WaitForValidPlayer(playerID.ID);
       // yield return StreamDeckManager.WaitForValidStreamDeck(playerID.ID);
        
        // Fill out laser infos with colour schemes and indices
        for (int i = 0; i < _laserInfos.Length; i++)
        {
            _laserInfos[i] = new LaserInfo()
            {
                colour      = playerID.PlayerData.ColourScheme.Colours[i],
                colourIndex = i
            };
        }

        // Subscribe to input
        _streamDeck.OnDeckInputPerformed.Add(OnDeckPerformed);
        // _input.OnColourButtonPerformed += OnColourButtonPerformed;
        // _input.OnColourButtonCancelled += OnColourButtonCancelled;
    }

    private void OnDestroy()
    {
        if (!playerID.IsValid)   return;
        if (_streamDeck == null) return;
        
        _streamDeck.OnDeckInputPerformed.Remove(OnDeckPerformed);
        // _input.OnColourButtonPerformed -= OnColourButtonPerformed;
        // _input.OnColourButtonCancelled -= OnColourButtonCancelled;
    }

    private void OnTriggerEnter(Collider other)
    {
        // NOTE(WSWhitehouse): If colliders layer isn't included in layer mask then ignore it 
        if (layerMask != (layerMask | (1 << other.gameObject.layer))) return;

        WordBlock component = other.gameObject.GetComponent<WordBlock>();
        if (component == null) return;
        if (_wordBlock.Contains(component)) return;

        // Add word block to the beginning of the list
        _wordBlock.Insert(0, component);
    }

    private void OnTriggerExit(Collider other)
    {
        // NOTE(WSWhitehouse): If colliders layer isn't included in layer mask then ignore it 
        if (layerMask != (layerMask | (1 << other.gameObject.layer))) return;

        WordBlock component = other.gameObject.GetComponent<WordBlock>();
        if (component == null) return;
        if (!_wordBlock.Contains(component)) return;

        LaserHitTiming timing = LaserHitTiming.MISS;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DebugOverride.LaserTimingOverride) {
            timing = DebugOverride.UseRandomHits ? DebugOverride.RandomHitTiming : DebugOverride.LaserHitTiming;
        }
#endif
        
        UpdatePlayerDataTimings(timing);
        UpdatePlayerDataCombo(timing);
        UpdatePlayerDataWordTimingAverage(component.WordData, timing);
        
        LaserHitInfo info = new LaserHitInfo()
        {
          WordBlock = component,
          Timing    = timing
        };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DebugOverride.LaserTimingOverride) {
            OnLaserHit?.Invoke(info);
            RemoveBlockFromList(component);
            component.BreakUpWordBlock(playerID.ID, timing);
            return;
        }
#endif
        
        OnLaserMiss?.Invoke(info);        
        RemoveBlockFromList(component);
        component.BreakUpWordBlock(playerID.ID, timing);
    }

    public void MoveLaserAlongLane()
    {
        // NOTE(WSWhitehouse): Purposefully switching start and end transforms here as we want the 
        // laser to start from the bottom (the end transform) and move up to the beginning.
        Vector3 startPos = lane.EndTransform.position;
        Vector3 endPos   = lane.StartTransform.position;

#if UNITY_EDITOR
        // NOTE(WSWhitehouse): If we're in the editor and not playing the static game manager vars will not be
        // initialised. Therefore, we need to use reflection to get the private laser pos variable and use that
        // instead. This should only be done in the editor!
        if (!Application.isPlaying)
        {
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager == null)
            {
                // Log.Error("No GameManger in scene! Cannot move laser along lane.", this);
                return;
            }

            // Use reflection to get the field info of the private variable. The string MUST match the name of the
            // variable in the GameManager script. If it is renamed it must be changed here too!
            FieldInfo laserPosAlongLaneField = manager.GetType().GetField("laserPosAlongLane");
            if (laserPosAlongLaneField == null)
            {
                Log.Error("Cannot find laser pos along lane field using Reflection! Please ensure the variable name is identical!", this);
                return;
            }

            float laserPos     = (float)laserPosAlongLaneField.GetValue(manager);
            transform.position = float3Util.PointAlongLine(startPos, endPos, laserPos);
            return;
        }
#endif

        transform.position = float3Util.PointAlongLine(startPos, endPos, GameManager.LaserPosAlongLane);
    }

    private void OnDeckPerformed(int buttonIndex)
    {
        if (PlayerID.PlayerData.IsDead) return;
        CoroutineUtil.StartSafelyWithRef(this, ref _checkForBlocksCoroutine, CheckForBlocks());
    }

    private void OnColourButtonPerformed(int colourIndex)
    {
        StartCoroutine(LaserFireFunc(colourIndex));
    }

    private void OnColourButtonCancelled(int colourIndex) { }

    public void ClearWordBlockList() => _wordBlock.Clear();
    
    // NOTE(WSWhitehouse): Doesn't perform any contains check - this must be done before calling this function
    private void RemoveBlockFromList(WordBlock block)
    {
        // block.OnReturnedToPool -= _onWordBlockReturnedToPoolFunc;
        _wordBlock.Remove(block);
    }

    private IEnumerator __CheckForBlocks()
    {
        WordBlock oldestBlock = GetOldestBlockAndTiming(out double oldestTime);

        float timer = 0.0f;
        while (timer < laserDuration)
        {
            for (int i = 0; i < LaserInput.colourButtons.Length; i++)
            {
                if (_inputCaptured[i]) continue;
                
                if (!_streamDeck.GetButtonState(LaserInput.colourButtons[i])) continue;
                
                _inputCaptured[i] = true;
                OnLaserFireStarted?.Invoke(_laserInfos[i]);

                bool blockFound = false;
                foreach (WordBlock block in _wordBlock)
                {
                    if (block.ColourIndex != i) continue;
                    if (!maths.DoubleCompare(block.TimeOnLane, oldestTime)) continue;
                    
                    blockFound = true;
                    
                    LaserHitTiming timing = GetLaserHitTimingFromDistance(block);
                    HandleWordBreakUp(block, timing);
                    break;
                }
                
                if (!blockFound && _wordBlock.Count > 0)
                {
                    foreach (WordBlock block in _wordBlock)
                    {
                        if (!maths.DoubleCompare(block.TimeOnLane, oldestTime)) continue;

                        // NOTE(WSWhitehouse): If block entered the collider after the input was captured
                        // we set the timing to early as technically this block was included in this 
                        if (_inputCaptured[block.ColourIndex])
                        {
                            LaserHitTiming timing = GetLaserHitTimingFromDistance(block);
                            HandleWordBreakUp(block, timing);
                        }
                        else
                        {
                            HandleWordBreakUp(block, LaserHitTiming.WRONG_COLOUR);
                        }
                        
                        break;
                    }
                }
            }
            
            timer += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        // if (wrongColour)
        // {
        //     oldestTime = GetOldestBlockTime(out oldestWordBlock);
        //     
        //     Log.Print("wrong col");
        //     foreach (WordBlock block in _wordBlock)
        //     {
        //         if (!maths.FloatCompare(block.GetTimeOnLane(), oldestTime)) continue;
        //         HandleWordBreakUp(block, LaserHitTiming.WRONG_COLOUR);
        //         Log.Print("word breakup");
        //     }
        // }

        // Reset input captured for next time
        for (int i = 0; i < _inputCaptured.Length; i++)
        {
            // Stop laser fire if this input has been invoked 
            if (_inputCaptured[i]) OnLaserFireStopped?.Invoke(_laserInfos[i]);
            
            _inputCaptured[i] = false;
        }
        
        _checkForBlocksCoroutine = null;
    }

    //added
    public void DebugFireLaser(int colourIndex)
    {
        StartCoroutine(FireLaser(colourIndex));
    }
    private IEnumerator FireLaser(int colourIndex)
    {
        LaserInfo info;
        info.colourIndex = colourIndex;
        
        //for(int i=0;i< playerID.PlayerData.ColourScheme.Colours.Length; i++)
        //{
        //    Debug.Log("colors val :" + playerID.PlayerData.ColourScheme.Colours[i]);
        //}

        info.colour = playerID.PlayerData.ColourScheme.Colours[colourIndex];

        OnLaserFireStarted?.Invoke(info);
        
        WordBlock wordBlock = GetOldestBlockAndTiming(out double oldestTime);
        
        bool found = false;
        foreach (WordBlock block in _wordBlock) //Search for block with correct colour first
        {
            if (block.ColourIndex == colourIndex && maths.DoubleCompare(block.TimeOnLane, oldestTime))
            {
                found = true;
                LaserTimingCheck(block, colourIndex);
                break;
            }
        }

        if (!found && _wordBlock.Count > 0) //If not take oldest block
        {
            LaserTimingCheck(wordBlock, colourIndex);
        }
      

        float timer = 0.0f;
        while (timer < laserDuration)
        {
            timer += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        OnLaserFireStopped?.Invoke(info);
        yield break;
    }

    // NOTE(WSWhitehouse): Can return null when there are no word blocks
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WordBlock GetOldestBlockAndTiming(out double oldestTimeOnLane)
    {
        oldestTimeOnLane    = 0.0;
        WordBlock wordBlock = null;
        
        foreach (WordBlock block in _wordBlock)
        {
            if (block.TimeOnLane > oldestTimeOnLane)
            {
                oldestTimeOnLane = block.TimeOnLane;
                wordBlock     = block;
            }
        }
        
        return wordBlock;
    }

    private void LaserTimingCheck(WordBlock wordBlock, int colourIndex)
    {
        LaserHitTiming timing = wordBlock.ColourIndex == colourIndex
            ? GetLaserHitTimingFromDistance(wordBlock)
            : LaserHitTiming.WRONG_COLOUR;
        
        HandleWordBreakUp(wordBlock, timing);
    }
    
    private void HandleWordBreakUp(WordBlock wordBlock, LaserHitTiming hitTiming)
    {
        LaserHitInfo laserHitInfo;
        laserHitInfo.WordBlock = wordBlock;
        laserHitInfo.Timing    = hitTiming;
        
        UpdatePlayerDataTimings(laserHitInfo.Timing);
        UpdatePlayerDataCombo(laserHitInfo.Timing);
        UpdatePlayerDataWordTimingAverage(wordBlock.WordData, laserHitInfo.Timing);

        OnLaserHit?.Invoke(laserHitInfo);
        RemoveBlockFromList(wordBlock);
        wordBlock.BreakUpWordBlock(playerID.ID, laserHitInfo.Timing);
    }

    private LaserHitTiming GetLaserHitTimingFromDistance(WordBlock block)
    {
        // Calculate block distance from laser
        // NOTE(WSWhitehouse): Ignore the X axis as we only care about the distance along the lane 
        Vector2 distVec = new Vector2(transform.position.y, transform.position.z) -
                          new Vector2(block.transform.position.y, block.transform.position.z);
        float distance = maths.FastSqrt((distVec.x * distVec.x) + (distVec.y * distVec.y));

        // Check for each distance
        if (distance <= perfectDistance) return LaserHitTiming.PERFECT;
        if (distance <= goodDistance)    return LaserHitTiming.GOOD;
        if (distance <= okayDistance)    return LaserHitTiming.OKAY;

        // NOTE(WSWhitehouse): If we didnt get another distance the block is either early or late,
        // there is no need to check for distances here as we checked all the other timings above.
        // Instead we check the height of the block compared to the laser, if the block is below
        // the laser then it was hit late, otherwise it was hit early.
        float yPos = block.transform.position.y;
        if (yPos < transform.position.y) return LaserHitTiming.LATE;

        return LaserHitTiming.EARLY;
    }

    private void UpdatePlayerDataTimings(LaserHitTiming timing) 
    {
      switch (timing) 
      { 
        case LaserHitTiming.MISS: 
        case LaserHitTiming.WRONG_COLOUR: 
        {
          PlayerManager.PlayerData[playerID.ID].Misses += 1;
          break;
        }

        case LaserHitTiming.LATE:
        case LaserHitTiming.EARLY:
        {
            PlayerManager.PlayerData[playerID.ID].EarlyLates += 1; 
            break;
        }
        
        case LaserHitTiming.OKAY:
        case LaserHitTiming.GOOD: 
        {
          PlayerManager.PlayerData[playerID.ID].Goods += 1; 
          break;
        } 

        case LaserHitTiming.PERFECT: 
        {
          PlayerManager.PlayerData[playerID.ID].Perfects += 1;
          break;
        }
        
        case LaserHitTiming.INVALID:
        default: break;
      }
    }
    
    private void UpdatePlayerDataCombo(LaserHitTiming timing)
    {
      ref PlayerData playerData = ref PlayerManager.PlayerData[playerID.ID];
      
      playerData.CurrentCombo++;
      
      if (timing is LaserHitTiming.MISS or LaserHitTiming.WRONG_COLOUR)
      {
        playerData.CurrentCombo = 0;
      }
      
      if (playerData.CurrentCombo > playerData.HighestCombo)
      {
        playerData.HighestCombo = playerData.CurrentCombo;
      }
      
      playerData.OnComboUpdated?.Invoke();
    }

    private void UpdatePlayerDataWordTimingAverage(WordData wordData, LaserHitTiming timing)
    {
        // if this word is not in the dictionary (and therefore not a keyword) we return
        if (!AverageWordTimings.Words.ContainsKey(wordData.wordIndex)) return;

        // if we do not have this word timing, we add it
        if (!AverageWordTimings.Words[wordData.wordIndex].Timings.ContainsKey(timing)) {
            AverageWordTimings.Words[wordData.wordIndex].Timings.Add(timing, 0);
        }


        // we increment hit count of this timing by 1
        AverageWordTimings.Words[wordData.wordIndex].Timings[timing] += 1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        MoveLaserAlongLane();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            MoveLaserAlongLane();
        }
    }

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null) return;

        Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);

        // Draw distance Gizmos
        GizmosUtil.DrawWireCapsule(transform.position, rotation, okayDistance, collider.height, okayGizmoColour);
        GizmosUtil.DrawWireCapsule(transform.position, rotation, goodDistance, collider.height, goodGizmoColour);
        GizmosUtil.DrawWireCapsule(transform.position, rotation, perfectDistance, collider.height, perfectGizmoColour);
    }
#endif
}

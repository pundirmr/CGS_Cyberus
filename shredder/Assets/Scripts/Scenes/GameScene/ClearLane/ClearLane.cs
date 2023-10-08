using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClearLane : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private ClearLaneTrigger trigger;
    
    [Header("Charge Settings")]
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private Transform clearLaneBottom;

    [Header("Effect Settings")]
    [SerializeField] private float clearLaneTime     = 1.5f;
    [SerializeField, Range(0, 1)] private float clearLaneDistance = 0.25f;

    [Header("SFX References")]
    [SerializeField] private AudioClip clearLaneSFX;
    
    
    public bool ClearingLane { get; private set; } = false;

    // events
    public delegate void WordDestroyedEvent(int playerID, Laser.LaserHitInfo info);
    public static WordDestroyedEvent OnWordBlockDestroyed;
    public DelegateUtil.EmptyEventDel OnLaneCleared;
    public DelegateUtil.EmptyEventDel OnLaserFinishedAnimation;

    // coroutine delegates
    private delegate IEnumerator SpamMoveCo(PlayerSpamBox box);
    private SpamMoveCo MoveSpamUp;
    private DelegateUtil.EmptyCoroutineDel ShootEffectUpLane;

    // spam settings
    private List<Spam2D> spamToRemove = new (500);
    private int id;
    private bool isActive;
    private bool subbed = false;

    // cached variables
    private Vector3 effectStartPos = new (3.5f, 0f, 0f);
    private Vector3 effectEndPos   = new (-3.5f, 0f, 0f);

    private void Awake() {
        MoveSpamUp        = __MoveSpamUp;
        ShootEffectUpLane = __ShootEffectUpLane;

    }

    private void OnDestroy() {
        if (!subbed) return;
        trigger.OnBlockDetected  -= OnBlockDetected;
        OnLaneCleared            -= GameManager.PlayerLasers[id].ClearWordBlockList;
        OnLaserFinishedAnimation -= GameManager.PlayerLasers[id].ClearWordBlockList;
    }

    public void Init(int _id) {
        id = _id;

        // set the position to this players laser position
        transform.position = GameManager.PlayerLasers[id].transform.position;

        // subscribe to the box colliders trigger
        trigger.OnBlockDetected += OnBlockDetected;

        // we subscribe to both of these events for extra redundancy
        OnLaneCleared            += GameManager.PlayerLasers[id].ClearWordBlockList;
        OnLaserFinishedAnimation += GameManager.PlayerLasers[id].ClearWordBlockList;
        subbed = true;
    }

    public void StartClearLane(PlayerSpamBox box) {
        // enable the trigger for the clear lane
        trigger.SetEnabled(true);
        
        ClearingLane = true;
        StartCoroutine(MoveSpamUp(box));
    }
    
    private IEnumerator __MoveSpamUp(PlayerSpamBox box) {
        // play sfx for the lane clear
        SFX.PlayGameScene(clearLaneSFX);
        
        // get the visual effect for the laser charge ready
        StartChargeEffect(effectStartPos);
        
        foreach (Spam2D spam in box.Spam) {
            // freeze spam in place
            spam.Body.isKinematic    = true;
            spam.Body.velocity       = Vector2.zero;

            // Set clear lane variables on spam
            float spamDist = maths.Abs(spam.transform.position.y - clearLaneBottom.position.y);
            float maxDist  = maths.Abs(transform.position.y - clearLaneBottom.position.y);
            float distance = 1.0f - maths.NormalizeValue(spamDist, 0.0f, maxDist);
            
            spam.ClearLaneStartPos = spam.transform.position;
            spam.ClearLaneEndPos   = new Vector3(spam.ClearLaneStartPos.x, transform.position.y, spam.ClearLaneStartPos.z);
            spam.ClearLaneDuration = chargeTime - (chargeTime * distance);
            spam.ClearLaneComplete = false;
        }
        
        float timeElapsed = 0.0f;
        while (timeElapsed < chargeTime) {
            foreach (Spam2D spam2D in box.Spam) {
                if (spam2D.ClearLaneComplete) continue;
                
                float t = EaseInUtil.Exponential(timeElapsed / spam2D.ClearLaneDuration);
                
                if (t >= 1.0f || spam2D.transform.position.y >= transform.position.y) {
                    spam2D.transform.position = spam2D.ClearLaneEndPos;
                    spam2D.ClearLaneComplete  = true;
                    spam2D.gameObject.SetActive(false);
                    continue;
                }
                
                spam2D.transform.position = float3Util.Lerp(spam2D.ClearLaneStartPos, spam2D.ClearLaneEndPos, t);
            }
            
            timeElapsed += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        // Remove all spam from box
        foreach (Spam2D spam2D in box.Spam) {
            spam2D.gameObject.SetActive(true);
            Spam2DPool.ReturnSpamToPool(spam2D);
        }
        box.Spam.Clear();

        // signal to the PlayerSpamBox that we have finished charging the clear
        OnLaneCleared?.Invoke();

        // shoot laser up the lane
        yield return ShootEffectUpLane();

        // currently used to tell the laser to clear it's [wordBlock] List
        OnLaserFinishedAnimation?.Invoke();
        
        lineRenderer.SetPosition(1, effectEndPos);

        // disable the trigger
        trigger.SetEnabled(false);
        
        ClearingLane = false;
    }
    
    private void OnBlockDetected(WordBlock block) {
        block.BreakUpWordBlock(id, LaserHitTiming.PERFECT);
        
        Laser.LaserHitInfo info = new Laser.LaserHitInfo();
        info.WordBlock          = block;
        OnWordBlockDestroyed?.Invoke(id, info);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////// Effect Functions
    
    private void StartChargeEffect(Vector3 pos) {
        lineRenderer.SetPosition(1, pos);
        ChargeEffect(0.25f);
    }
    
    private void ChargeEffect(float amount) {
        var newColour = Colour.ChangeAlpha(lineRenderer.startColor, amount);
        lineRenderer.material.color = newColour;
    }    
    
    private IEnumerator __ShootEffectUpLane() {
        Vector3 start = GameManager.PlayerLasers[id].transform.position;
        Vector3 end   = GameManager.PlayerLanes[id].StartTransform.position;
        
        float elapsed = 0f;
        while (elapsed < clearLaneTime) {
            elapsed += Time.deltaTime;
            float t  = elapsed / clearLaneTime;

            var pos  = float3Util.Lerp(start, end, t);
            lineRenderer.transform.position = pos;

            // the normalized [t] value is greater than the normalized [clearLaneDistance] we break out of the coroutine
            if (t > clearLaneDistance) break;
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        // reset the line renderers position
        lineRenderer.transform.localPosition = float3Util.zero;
        yield break;
    }
}

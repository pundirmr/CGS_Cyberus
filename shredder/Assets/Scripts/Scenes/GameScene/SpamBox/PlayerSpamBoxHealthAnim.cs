using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpamBoxHealthAnim : MonoBehaviour {
    [Header("Prefab References")]
    [SerializeField] private RectTransform parent;
    [SerializeField] private CanvasRenderer background;
    [SerializeField] private PlayerID id;
    
    [Header("Prefab Health References")]
    [SerializeField] private Image[] healthImgs;
    [SerializeField] private RectTransform healthImgsParent;
    [SerializeField] private Image[] aberrationImgs;
    [SerializeField] private AberrationEffect[] effects;

    [Header("Prefab Text References")]
    [SerializeField] private TMP_Text eliminatedText; 
    
    [Header("Lerp Settings")]
    [SerializeField] private float lerpUpTime   = 0.5f;
    [SerializeField] private float lerpDownTime = 0.5f;

    [Header("Aberration Settings")]
    [SerializeField] private RangedFloat3 aberrationAmountRange = new (new (-0.3f, -0.3f, -0.3f), new (0.3f, 0.3f, 0.3f));
    [SerializeField] private RangedFloat aberrationWaitRange  = new (0.1f, 0.15f);
    [SerializeField] private RangedInt aberrationChangesRange = new (10, 15);

    [Header("Health Settings")]
    [SerializeField] private Color fullCol  = Color.red;
    [SerializeField] private Color emptyCol = default;
    
    /*[Header("Sound Effects")]
    [SerializeField] private AudioClip heartAberrationSFX;
    */


    
    private float3[] aberrationAmounts;
    private float[] aberrationWaits;
    private int aberrationChanges;
    private int currentHeart;

    private float3 onScreenPos     = new (0f,  0f, 0f);
    private float3 bottomScreenPos = new (0f, -5f, 0f);
    private float3 topScreenPos    = new (0f,  5f, 0f);

    // coroutine delegates
    private delegate IEnumerator LerpDel(float3 finalPos);
    private LerpDel LerpOffScreen;
    private LerpDel LerpAIEliminated;
    
    private DelegateUtil.EmptyCoroutineDel LerpOnScreen;
    private DelegateUtil.EmptyCoroutineDel AnimateAberration;
    public  DelegateUtil.EmptyCoroutineDel EntireAnimation;
    
    private void Awake() {
        // delegate allocations
        LerpOnScreen      = __LerpOnScreen;
        LerpOffScreen     = __LerpOffScreen;
        LerpAIEliminated  = __LerpAIEliminated;
        AnimateAberration = __AnimateAberration;
        EntireAnimation   = __EntireAnimation;
        
        aberrationChanges = aberrationChangesRange.Random();
        aberrationAmounts = new float3[aberrationChanges];
        aberrationWaits   = new float[aberrationChanges];

        for (int i = 0; i < aberrationChanges; ++i) {
            aberrationAmounts[i] = aberrationAmountRange.Random();
            aberrationWaits[i]   = aberrationWaitRange.Random();
        }

        foreach (var img in healthImgs)     img.color   = fullCol;
        foreach (var img in aberrationImgs) img.enabled = false;
        currentHeart = healthImgs.Length - 1;

        bottomScreenPos.y -= maths.Max(bottomScreenPos.y, parent.GetHeight());
        topScreenPos.y     = -bottomScreenPos.y;
        
        healthImgsParent.localPosition         = bottomScreenPos;
        eliminatedText.transform.localPosition = bottomScreenPos;
    }

    private IEnumerator Start() {
        // doing this here so that the ui is setup correctly, as it does not seem to apply when done in [Awake]
        background.SetAlpha(0f);
        
        yield return PlayerManager.WaitForValidPlayer(id.ID);
        foreach(var effect in effects) effect.Setup();
    }

    
    private IEnumerator __EntireAnimation() {
        yield return LerpOnScreen();
        yield return CoroutineUtil.Wait(0.25f);

        yield return AnimateAberration();
        currentHeart -= 1;
         
        yield return CoroutineUtil.Wait(0.5f);

        // if the player is dead
        if (currentHeart == -1) {
            yield return LerpAIEliminated(topScreenPos);
        } else {
            yield return LerpOffScreen(bottomScreenPos);
        }

    }
    
    private IEnumerator __LerpOnScreen() {
        float elapsed = 0f;
        float3 startPos = healthImgsParent.localPosition;
        bool sfxTriggered = false;
        while (elapsed < lerpUpTime) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpUpTime;

            // we trigger the sfx only if the player is not on their last life
            if (t <= 0.5f && !sfxTriggered && !id.PlayerData.IsDead)
            {
                //SFX.PlayGameScene(heartAberrationSFX);
                AudioEventSystem.TriggerEvent("HeartAberrationSFX", null); //Will need to minus 1 from fmod health param 
                AudioEngine.audioEngineInstance.fmodParams.SetParamByName<int>(AudioEngine.audioEngineInstance.fmodEventReferences.heartAberrationSFXInstance, "Health", -1); //Taking 1 health away from fmod 'Health' param, so it wont trigger when player is on their last health
                sfxTriggered = true;
            }
            
            background.SetAlpha(t);
            healthImgsParent.localPosition = float3Util.Lerp(startPos, onScreenPos, EaseOutUtil.Exponential(t));
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        

        background.SetAlpha(1f);
        healthImgsParent.localPosition = onScreenPos;

        yield break;
    }

    private IEnumerator __LerpOffScreen(float3 finalPos) {
        float elapsed = 0f;
        while (elapsed < lerpDownTime) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpDownTime;

            background.SetAlpha(1f - t);
            healthImgsParent.localPosition = float3Util.Lerp(onScreenPos, finalPos, EaseInUtil.Exponential(t));
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        background.SetAlpha(0f);
        healthImgsParent.localPosition = finalPos;
        
        yield break;
    }

    private IEnumerator __LerpAIEliminated(float3 heartFinalPos) {
        float elapsed = 0f;
        while (elapsed < lerpDownTime) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpDownTime;

            healthImgsParent.localPosition         = float3Util.Lerp(onScreenPos, heartFinalPos, EaseInUtil.Exponential(t));
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        healthImgsParent.localPosition         = heartFinalPos;

        elapsed = 0f;
        while (elapsed < lerpDownTime) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpDownTime;

            eliminatedText.transform.localPosition = float3Util.Lerp(bottomScreenPos, onScreenPos, EaseOutUtil.Exponential(t));
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        eliminatedText.transform.localPosition = onScreenPos;
        yield break;
    }

    private IEnumerator __AnimateAberration() {
        healthImgs[currentHeart].enabled      = false;
        aberrationImgs[currentHeart].enabled  = true;

        
        for (int i = 0; i < aberrationChanges; ++i) {
            effects[currentHeart].SetAmount(aberrationAmounts[0], aberrationAmounts[1]);
            ArrayUtil.Shuffle(aberrationAmounts, aberrationChanges);
            yield return CoroutineUtil.Wait(aberrationWaits[i]);
        }

        effects[currentHeart].SetAmount(float3Util.zero, float3Util.zero);
        
        healthImgs[currentHeart].color       = emptyCol;
        healthImgs[currentHeart].enabled     = true;
        aberrationImgs[currentHeart].enabled = false;

        ArrayUtil.Shuffle(aberrationWaits, aberrationChanges);
        yield break;
    }
    
}

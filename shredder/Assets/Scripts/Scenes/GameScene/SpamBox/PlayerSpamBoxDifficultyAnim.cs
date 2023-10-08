using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpamBoxDifficultyAnim : MonoBehaviour {
    [Header("Prefab References")]
    [SerializeField] private PlayerID id;
    [SerializeField] private CanvasRenderer background;
    [SerializeField] private RectTransform inViewportPos;
    [SerializeField] private RectTransform offBottomPos;

    [Header("Prefab Animation References")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform animParent;
    [SerializeField] private Image[] animStars;
                        
    [Header("Animation Settings")]
    [SerializeField] private float lerpMovementDuration = 0.5f;
    [SerializeField] private float lerpFillDuration     = 1f;

    /*[Header("SFX References")]
    [SerializeField] private AudioClip difficultySFX; */


        
    private delegate IEnumerator LerpDel(float3 endPos);
    private LerpDel LerpOnScreen;
    private LerpDel LerpOffScreen;
                        
    public  DelegateUtil.EmptyCoroutineDel EntireAnimation;
    
    private delegate IEnumerator AnimateDel(int starIndex);
    private AnimateDel AnimateChangeUp;
    private AnimateDel AnimateChangeDown;
    
    private PlayerData PlayerData => id.PlayerData;

    private void Awake() {
        // delegate allocations
        LerpOnScreen      = __LerpOnScreen;
        LerpOffScreen     = __LerpOffScreen;
        EntireAnimation   = __EntireAnimation;
        AnimateChangeUp   = __AnimateChangeUp;
        AnimateChangeDown = __AnimateChangeDown;
    }

    private IEnumerator Start() {
        Canvas.ForceUpdateCanvases();

        animParent.position = offBottomPos.position;
        
        yield return PlayerManager.WaitForValidPlayer(id.ID);

        // we're assuming we never have any more difficulties than this
        switch (id.PlayerData.Difficulty) {
            case NoteDifficulty.EASY:   FillInactiveStars(1); break;
            case NoteDifficulty.MEDIUM: FillInactiveStars(2); break;
            case NoteDifficulty.HARD:   FillInactiveStars(3); break;
            default: break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void FillInactiveStars(int index) {
        for (int i = 0; i < index; ++i) animStars[i].fillAmount = 1f;
        for (int i = index; i < 3; ++i) animStars[i].fillAmount = 0f;
    }
    
    private IEnumerator __EntireAnimation() {
        if (id.PlayerData.IsDead) yield break;

        // move sprites on into the viewport
        yield return LerpOnScreen(inViewportPos.position);
        yield return CoroutineUtil.Wait(0.25f);
        
        int starIndex = PlayerData.Difficulty switch
        {
            NoteDifficulty.EASY   => 0,
            NoteDifficulty.MEDIUM => 1,
            NoteDifficulty.HARD   => 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        // play difficulty change SFX
        //SFX.PlayGameScene(difficultySFX);
        AudioEventSystem.TriggerEvent("DifficultyChangeSFX", null);
        
        FillInactiveStars(starIndex + 1);
        
        // if we're moving up from the previous difficulty
        if (PlayerData.Difficulty > PlayerData.PrevDifficulty) {
            yield return AnimateChangeUp(starIndex);
        }
        else { // else we're moving down
            // NOTE(WSWhitehouse): If we're moving down we need to remove the 
            // star corresponding to the difficulty above us
            starIndex++;
            yield return AnimateChangeDown(starIndex);
        }
        
        yield return CoroutineUtil.Wait(0.5f);

        // remove sprites from the viewport
        yield return LerpOffScreen(offBottomPos.position);
    }

    private IEnumerator __LerpOnScreen(float3 endPos) {
        // if the star are already on the screen then we just return 
        if (float3Util.CompareXY(endPos, animParent.position)) yield break;
        
        float3 startPos  = animParent.position;
        
        float elapsed = 0f;
        while (elapsed < lerpMovementDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpMovementDuration;

            animParent.position = float3Util.Lerp(startPos, endPos, EaseOutUtil.Exponential(t));
            
            background.SetAlpha(t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        background.SetAlpha(1f);
        animParent.position = endPos;
        
        yield break;
    }

    private IEnumerator __LerpOffScreen(float3 endPos) {
        float3 startPos  = animParent.position;
        
        float elapsed = 0f;
        while (elapsed < lerpMovementDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpMovementDuration;

            animParent.position = float3Util.Lerp(startPos, endPos, EaseInUtil.Exponential(t));
            
            background.SetAlpha(1f - t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        background.SetAlpha(0f);
        animParent.position = endPos;
        
        yield break;
    }

    
    private IEnumerator __AnimateChangeUp(int starIndex) {
        float elapsed = 0f;
        while (elapsed < lerpFillDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpFillDuration;

            animStars[starIndex].fillAmount = t;
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        animStars[starIndex].fillAmount = 1f;
    }

    
    private IEnumerator __AnimateChangeDown(int starIndex) {
        float elapsed = 0f;
        while (elapsed < lerpFillDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / lerpFillDuration;

            animStars[starIndex].fillAmount = 1f - t;
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        animStars[starIndex].fillAmount = 0f;
    }
}

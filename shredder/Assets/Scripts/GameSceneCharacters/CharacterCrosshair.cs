using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterCrosshair : MonoBehaviour
{
    [Header("GameObject References")]
    [SerializeField] private RectTransform crosshairTransform;
    [SerializeField] private Canvas screenCanvas;
    [SerializeField] private CharacterCrosshairText crosshairText;

    [Header("Anim Values")]
    [SerializeField] private Vector2 padding;
    [SerializeField] private float animTime;
    [SerializeField] private int numberOfMoves;
    [SerializeField] private float minimumMoveDistance;

    [Header("Fade Anim")]
    [SerializeField] private CanvasGroup crosshairCanvasGroup;
    [SerializeField] private float fadeAnimTime;

    [Header("SFX References")]
    // [SerializeField] private AudioClip searchingSFX;
    [SerializeField] private AudioClip spamIdentifiedSFX;



    public delegate IEnumerator CrosshairSearchAnim();
    public CrosshairSearchAnim RandomSearch;

    public delegate IEnumerator CrosshairLockOnAnim(GameSceneCharacter character);
    public CrosshairLockOnAnim LockOn;
    private CrosshairLockOnAnim TrackCharacter;
    private Coroutine _trackCo;

    public delegate IEnumerator CrosshairFade();
    public CrosshairFade FadeCrosshair;

    private float moveTime;

    private void Awake()
    {
        RandomSearch = DoRandomSearch;
        LockOn = DoLockOn;
        TrackCharacter = __TrackCharacter;
        FadeCrosshair = DoCrosshairFade;
    }

    private void OnDestroy()
    {
        if (_trackCo != null)
        {
            StopCoroutine(_trackCo);
        }
    }

    private IEnumerator DoCrosshairFade()
    {
        yield return LerpUtil.LerpCanvasGroupAlpha(crosshairCanvasGroup, 0f, fadeAnimTime);
        CoroutineUtil.StopSafelyWithRef(this, ref _trackCo);
        yield break;
    }

    private IEnumerator DoRandomSearch()
    {

        //SFX.PlayGameScene(searchingSFX);
        AudioEventSystem.TriggerEvent("SearchingScanSFX", null);


        Vector2 maxSize = (screenCanvas.pixelRect.size - padding) / 2;
        Vector2 minSize = -maxSize;
        Vector2[] positions = new Vector2[numberOfMoves];
        moveTime = animTime / numberOfMoves;

        // calculcate and set the random points that the crosshair is going to go to before going to the character
        for (int i = 0; i < numberOfMoves; i++)
        {
            Vector2 lastPos = i == 0 ? Vector2.zero : positions[i - 1];
            Vector2 pos = i == 0 ? Vector2.zero : positions[i - 1];
            int searchCount = 0;
            while (maths.Abs((lastPos - pos).magnitude) < minimumMoveDistance && searchCount < 10)
            {
                pos = new Vector2(Random.Range(minSize.x, maxSize.x), Random.Range(minSize.y, maxSize.y));
                searchCount++;
            }
            positions[i] = pos;
        }


        // do the random movement to the points calculated above
        foreach (Vector2 position in positions)
        {
            // REVIEW(Zack): @AUDIO do we want to play from an audio source on this game object to control volume to more granularity?

            float time = 0;
            Vector3 startPos = crosshairTransform.localPosition;

            while (time < moveTime)
            {
                crosshairTransform.localPosition = float3Util.Lerp(startPos, new float3(position.x, position.y, startPos.z), EaseInOutUtil.Exponential(time / moveTime));
                time += Time.deltaTime;
                yield return CoroutineUtil.WaitForUpdate;
            }
        }
    }

    private IEnumerator DoLockOn(GameSceneCharacter character)
    {
        float time = 0;
        Vector2 startPos = crosshairTransform.anchoredPosition;
        moveTime = animTime / numberOfMoves;

        while (time < moveTime)
        {
            Vector2 position = RectTransformUtil.GetPositionFromWorldTransform(screenCanvas.transform as RectTransform, StaticCamera.Main, character.GetActiveCharacterLockOnTarget().position);
            crosshairTransform.anchoredPosition = float2Util.Lerp(startPos, position, EaseInOutUtil.Exponential(time / moveTime));
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }


        //VoiceOver.PlayGlobal(spamIdentifiedSFX);
        AudioEventSystem.TriggerEvent("SpamAttackVO", null);
        crosshairText.LockOn();

        _trackCo = StartCoroutine(TrackCharacter(character));
    }

    private IEnumerator __TrackCharacter(GameSceneCharacter character)
    {
        while (true)
        {
            Vector2 position = RectTransformUtil.GetPositionFromWorldTransform(screenCanvas.transform as RectTransform, StaticCamera.Main, character.GetActiveCharacterLockOnTarget().position);
            crosshairTransform.anchoredPosition = position;
            yield return CoroutineUtil.WaitForUpdate;
        }
    }
}

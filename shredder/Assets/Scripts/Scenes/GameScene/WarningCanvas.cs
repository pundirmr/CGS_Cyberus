using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class WarningCanvas : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Volume effectVol;
    [SerializeField] private CanvasGroup flashingGroup;

    [Header("Effect Settings")]
    [SerializeField] private float lerpDuration = 0.1f;
    [SerializeField] private float waitBetweenFlashes = 0.375f;
    [SerializeField] private int numOfFlashes = 5;

    [Header("SFX References")]
    [SerializeField] private AudioClip warningClaxonSFX;
    [SerializeField] private AudioClip warningVoiceSFX;


    private static readonly float[] _weights = new float[2] { 1.0f, 0.0f };

    // NOTE(WSWhitehouse): Delegate is used here to stop delegate allocation when
    // calling the start warning effect function. It is assigned in Awake().
    public DelegateUtil.EmptyCoroutineDel StartWarningEffect;

    private void Awake()
    {
        canvas.enabled = false;
        effectVol.weight = 0f;
        flashingGroup.alpha = 0f;

        StartWarningEffect = __StartWarningEffect;
    }

    private IEnumerator __StartWarningEffect()
    {
        canvas.enabled = true;
        //SFX.PlayGameScene(warningClaxonSFX);
        AudioEventSystem.TriggerEvent("WarningClaxonSFX", null);

        for (int i = 0; i < numOfFlashes; ++i)
        {
            float weight = _weights[ArrayUtil.WrapIndex(i, _weights.Length)];
            float elapsed = 0f;
            float start = effectVol.weight;
            while (elapsed < lerpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lerpDuration;

                effectVol.weight = maths.Lerp(start, weight, t);
                flashingGroup.alpha = maths.Lerp(start, weight, t);
                yield return CoroutineUtil.WaitForUpdate;
            }

            effectVol.weight = weight;
            flashingGroup.alpha = weight;
            yield return CoroutineUtil.Wait(waitBetweenFlashes);
        }

        canvas.enabled = false;
        effectVol.weight = 0.0f;
        flashingGroup.alpha = 0.0f;

        yield return CoroutineUtil.Wait(0.5f);

        //VoiceOver.PlayGlobal(warningVoiceSFX);
    }
}

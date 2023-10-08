using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

public class UiLaser : MonoBehaviour
{
    public LaserEffectWave[] laserWave = new LaserEffectWave[3];
    
    [SerializeField] private Material laserMaterialTemplate;
    [SerializeField] private LineRenderer line;
    [SerializeField] private float hiddenAmplitude = 10000;
    [SerializeField] private float hiddenSize = 0;

    private delegate IEnumerator RevealAnimation(LaserUiEffectValues values);
    private RevealAnimation _revealAnimationFunc;
    private Coroutine _revealAnimationCo;
    
    private delegate IEnumerator AmplitudeAnimation(float target, float speed, bool returnToStart);
    private AmplitudeAnimation _amplitudeAnimationFunc;
    private Coroutine _amplitudeAnimationCo;
    
    private Material _laserMaterial;
    private float _idleAmplitude;
    private float _effectAmplitude;
    private float _effectTime;
    
    private float[] _startSizes = new float[3];

    private void Awake()
    {
        _laserMaterial = new Material(laserMaterialTemplate);
        line.material = _laserMaterial;
        
        laserWave[0] = new LaserEffectWave(_laserMaterial, "One");
        laserWave[1] = new LaserEffectWave(_laserMaterial, "Two");
        laserWave[2] = new LaserEffectWave(_laserMaterial, "Three");
        _startSizes[0] = laserWave[0].GetSize();
        _startSizes[1] = laserWave[1].GetSize();
        _startSizes[2] = laserWave[2].GetSize();
        laserWave[0].SetSize(hiddenSize);
        laserWave[1].SetSize(hiddenSize);
        laserWave[2].SetSize(hiddenSize);
        _revealAnimationFunc = Reveal;
        _amplitudeAnimationFunc = AnimateAmplitude;
        foreach (LaserEffectWave wave in laserWave)
        {
            wave.SetAmplitude(hiddenAmplitude);
        }
    }
    
    public void StartReveal(LaserUiEffectValues values)
    {
        _idleAmplitude = values.idleAmplitude;
        _effectAmplitude = values.effectAmplitude;
        _effectTime = values.effectTime;    
        
        laserWave[0].SetSize(_startSizes[0]);
        laserWave[1].SetSize(_startSizes[1]);
        laserWave[2].SetSize(_startSizes[2]);
        
        CoroutineUtil.StartSafelyWithRef(this, ref _revealAnimationCo, _revealAnimationFunc(values));
    }

    public void AnimateAmplitude(float multiplier)
    {
        CoroutineUtil.StartSafelyWithRef(this, ref _amplitudeAnimationCo, _amplitudeAnimationFunc(_effectAmplitude * multiplier, _effectTime, true));
    }

    public void SetColours(Color32[] hdrColours, float factor, float alpha)
    {
        Color[] outColours = new Color[3];
        float saturation = hdrColours == StaticData.ColourSchemesHDR[5].Colours ? 0f : 0.82f; // if black and white
        for (int j = 0; j < 3; j++)
        {
            Color temp = hdrColours[j];
            Color.RGBToHSV(temp * factor, out float hue, out float sat, out float vib);
            Color normalisedColour = Color.HSVToRGB(hue, saturation, vib, true);
            normalisedColour.a = alpha;
            outColours[j] = normalisedColour;
        } 
        
        CoroutineUtil.StartSafelyWithRef(this, ref _amplitudeAnimationCo, _amplitudeAnimationFunc(_effectAmplitude, _effectTime, true));
        
        laserWave[0].SetColour(outColours[0]);
        laserWave[1].SetColour(outColours[1]);
        laserWave[2].SetColour(outColours[2]);
    }

    private IEnumerator AnimateAmplitude(float target, float animTime, bool returnToIdle)
    {
        float start = laserWave[0].GetAmplitude();
        float time = 0;
        while (time < animTime)
        {
            for (int i = 0; i < 3; i++)
            {
                laserWave[i].SetAmplitude(maths.Lerp(start, target, EaseOutUtil.Exponential(time / animTime)));
            }
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        if(!returnToIdle) yield break;
        
        time = 0;
        while (time < animTime)
        {
            for (int i = 0; i < 3; i++)
            {
                laserWave[i].SetAmplitude(maths.Lerp(target, _idleAmplitude, EaseInUtil.Exponential(time / animTime)));
            }
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        // set the amplitudes to the original values after the animation
        for (int i = 0; i < 3; ++i)
        {
            laserWave[i].SetAmplitude(_idleAmplitude);
        }
    }

    private IEnumerator Reveal(LaserUiEffectValues values)
    {
        float time = 0;
        while (time < values.revealTime)
        {
            foreach (LaserEffectWave wave in laserWave)
            {
                wave.SetAmplitude(maths.Lerp(hiddenAmplitude, values.idleAmplitude, EaseOutUtil.Exponential(time / values.revealTime)));
            }
            transform.localPosition = float3Util.Lerp(float3.zero, values.position, EaseOutUtil.Exponential(time / values.revealTime));
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        foreach (LaserEffectWave wave in laserWave)
        {
            wave.SetAmplitude(values.idleAmplitude);
        }
        transform.localPosition = values.position;
    }
}

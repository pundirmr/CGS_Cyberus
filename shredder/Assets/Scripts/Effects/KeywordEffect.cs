using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordEffect : MonoBehaviour
{
    [Header("Material References")]
    [SerializeField] private Material particleSystemMaterial;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private Material trailSparkMaterial;
    [Header("Component References")]
    [SerializeField] private ParticleSystem particleSys;
    [SerializeField] private ParticleSystem sparkPartSys;
    [SerializeField] private TrailRenderer trailSystem;
    [SerializeField] private WireframeEffect wireframe;
    [Header("Effect Parameters")]
    [SerializeField] private Color particleColour;
    [SerializeField] [ColorUsage(true, true)]private Color particleHdrColour;
    [Space]
    [SerializeField] private Color trailColour;
    [SerializeField] [ColorUsage(true, true)]private Color trailHdrColour;
    [Space]
    [SerializeField] private Color trailSparkColour;
    [SerializeField] [ColorUsage(true, true)]private Color trailSparkHdrColour;
    [Space]
    [SerializeField] private Color wireframeColour;
    [SerializeField] [ColorUsage(true, true)]private Color wireframeHdrColour;
    [SerializeField]  private RangedFloat wireframeIntensityRange;

    //Shader var IDs
    private int _psMatColourID;
    private int _psMatEmissColourID;
    
    private int _psSparkMatColourID;
    private int _psSparkMatEmissColourID;
    
    private int _trailMatColourID;
    private int _trialMatEmissColourID;

    private delegate IEnumerator WireframePulse();
    private WireframePulse _wireframePulseFunc;
    private Coroutine _wireframePulseCo;

    private static bool _colourHasBeenSet;
    
    private void Awake()
    {
        particleSys.GetComponent<ParticleSystemRenderer>().material = particleSystemMaterial;
        sparkPartSys.GetComponent<ParticleSystemRenderer>().material = trailSparkMaterial;
        trailSystem.material = trailMaterial;

        particleSys.gameObject.SetActive(false);
        
        Shader shader = particleSystemMaterial.shader;
        _psMatColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColor"));
        _psMatEmissColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_EmissionColor"));
        
        shader = trailMaterial.shader;
        _trailMatColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColor"));
        _trialMatEmissColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_EmissionColor"));
        
        shader = trailSparkMaterial.shader;
        _psSparkMatColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColor"));
        _psSparkMatEmissColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_EmissionColor"));
        
        _wireframePulseFunc = PulseWireframe;
    }

    public void Activate(bool refreshColour = false)
    {
        particleSys.gameObject.SetActive(true);
        
        // wireframe.SetBaseColour(wireframeColour);
        // wireframe.SetHDRColour(wireframeHdrColour);

        // if (_wireframePulseCo != null)
        // {
        //     StopCoroutine(_wireframePulseCo);
        // }
        //
        // _wireframePulseCo = StartCoroutine(_wireframePulseFunc());

        if (!_colourHasBeenSet || refreshColour)
        {
            particleSystemMaterial.SetColor(_psMatColourID, particleColour);
            particleSystemMaterial.SetColor(_psMatEmissColourID, particleHdrColour);
            
            trailMaterial.SetColor(_trailMatColourID, trailColour);
            trailMaterial.SetColor(_trialMatEmissColourID, trailHdrColour);
            
            trailSparkMaterial.SetColor(_psSparkMatColourID, trailSparkColour);
            trailSparkMaterial.SetColor(_psSparkMatEmissColourID, trailSparkHdrColour);
            
            _colourHasBeenSet = true;
        }
    }

    private IEnumerator PulseWireframe()
    {
        // NOTE(Zack): this is currently unused for some reason??
        /*
        float intensityRange = wireframeIntensityRange.maxValue - wireframeIntensityRange.minValue;
        float cycleTime = pulseFrequency * 0.5f;
        float time = 0;
        bool increase = true;

        while (false)
        {
            if (time > cycleTime)
            {
                increase = !increase;
                time = 0;
            }

            if (increase)
            {
                wireframe.SetColour(CalculateHDRColour(wireframeHdrColour, wireframeIntensityRange.minValue + (intensityRange * EaseInOutUtil.Exponential(time / cycleTime))));
            }
            else
            {
                wireframe.SetColour(CalculateHDRColour(wireframeHdrColour, wireframeIntensityRange.minValue + (intensityRange * EaseInOutUtil.Exponential(1 - (time / cycleTime)))));
            }
            
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        */
        yield break;
    }

    private Color CalculateHDRColour(Color baseColour, float intensity)
    {
        float power = maths.Pow2(intensity);
        return baseColour * power;
    }

    public void Deactivate()
    {
        particleSys.gameObject.SetActive(false);
        // if (_wireframePulseCo != null)
        // {
        //     StopCoroutine(_wireframePulseCo);
        //     _wireframePulseCo = null;
        // }
    }

}

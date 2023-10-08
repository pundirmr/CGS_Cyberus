using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct GlitchEffectParams {
    [Range(0f, 1f)] public float DigitalIntensity; // = 0.36f;
    [Range(0f, 1f)] public float AnalogScanLine;   // = 0.248f;
    [Range(0f, 1f)] public float AnalogVertical;   // = 0.093f;
    [Range(0f, 1f)] public float AnalogHorizontal; // = 0f;
    [Range(0f, 1f)] public float AnalogColourDrift;// = 0.235f;
}

public class GlitchEffectController : MonoBehaviour {
    public AnalogGlitchFeature Analog   = default;
    public DigitalGlitchFeature Digital = default;
    
    public delegate IEnumerator LerpToGlitchParamsDel(GlitchEffectParams effectParams, float duration);
    public delegate IEnumerator LerpDisableGlitchDel(float duration);
    public LerpToGlitchParamsDel LerpToGlitchParams;
    public LerpDisableGlitchDel LerpDisableGlitch;

    private void Awake() {
        Debug.Assert(Analog  != null, "Analog render feature is null",  this);
        Debug.Assert(Digital != null, "Digital render feature is null", this);
        
        LerpToGlitchParams = __LerpToGlitchParams;
        LerpDisableGlitch  = __LerpDisableGlitch;
    }
    
    public void SetParams(GlitchEffectParams @params)
    {
        Digital.Intensity      = @params.DigitalIntensity;
        Analog.ScanLineJitter  = @params.AnalogScanLine;
        Analog.VerticalJump    = @params.AnalogVertical;
        Analog.HorizontalShake = @params.AnalogHorizontal;
        Analog.ColorDrift      = @params.AnalogColourDrift;
    }
    
    private IEnumerator __LerpToGlitchParams(GlitchEffectParams effect, float duration) {
        // Start values
        float startIntensity       = Digital.Intensity;
        float startScanLineJitter  = Analog.ScanLineJitter;
        float startVerticalJump    = Analog.VerticalJump;
        float startHorizontalShake = Analog.HorizontalShake;
        float startColorDrift      = Analog.ColorDrift;
        
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            Digital.Intensity      = maths.Lerp(startIntensity,       effect.DigitalIntensity,  t);
            Analog.ScanLineJitter  = maths.Lerp(startScanLineJitter,  effect.AnalogScanLine,    t);
            Analog.VerticalJump    = maths.Lerp(startVerticalJump,    effect.AnalogVertical,    t);
            Analog.HorizontalShake = maths.Lerp(startHorizontalShake, effect.AnalogHorizontal,  t);
            Analog.ColorDrift      = maths.Lerp(startColorDrift,      effect.AnalogColourDrift, t);

            yield return CoroutineUtil.WaitForUpdate;
        }
    }
    
    private IEnumerator __LerpDisableGlitch(float duration) {
        // Start values
        float startIntensity       = Digital.Intensity;
        float startScanLineJitter  = Analog.ScanLineJitter;
        float startVerticalJump    = Analog.VerticalJump;
        float startHorizontalShake = Analog.HorizontalShake;
        float startColorDrift      = Analog.ColorDrift;
 
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            Digital.Intensity      = maths.Lerp(startIntensity,       0f, t);
            Analog.ScanLineJitter  = maths.Lerp(startScanLineJitter,  0f, t);
            Analog.VerticalJump    = maths.Lerp(startVerticalJump,    0f, t);
            Analog.HorizontalShake = maths.Lerp(startHorizontalShake, 0f, t);
            Analog.ColorDrift      = maths.Lerp(startColorDrift,      0f, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        if (SceneHandler.SceneIndex != (int)Scene.GAME_SCENE) yield break;
        
        // REVIEW(WSWhitehouse): Should this function be responsible for changing renderers?
        StaticCamera.SetToDefaultRenderer();
    }
}

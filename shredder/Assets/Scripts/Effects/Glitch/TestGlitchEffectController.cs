using UnityEngine;

[ExecuteInEditMode]
sealed class TestGlitchEffectController : MonoBehaviour {
    [SerializeField] private DigitalGlitchFeature digitalGlitchFeature = default;
    [SerializeField] private AnalogGlitchFeature analogGlitchFeature = default;

    [Header("Digital")]
    [SerializeField, Range(0f, 1f)] private float intensity = 0f;

    [Header("Analog")]
    [SerializeField, Range(0f, 1f)] private float scanLineJitter = 0f;
    [SerializeField, Range(0f, 1f)] private float verticalJump = 0f;
    [SerializeField, Range(0f, 1f)] private float horizontalShake = 0f;
    [SerializeField, Range(0f, 1f)] private float colorDrift = 0f;

#if UNITY_EDITOR
    private void Update() {
        UpdateValues();
    }
    
    private void OnValidate() {
        UpdateValues();
    }
#endif
    
    private void UpdateValues() {
        digitalGlitchFeature.Intensity = intensity;

        analogGlitchFeature.ScanLineJitter = scanLineJitter;
        analogGlitchFeature.VerticalJump = verticalJump;
        analogGlitchFeature.HorizontalShake = horizontalShake;
        analogGlitchFeature.ColorDrift = colorDrift;
    }
}


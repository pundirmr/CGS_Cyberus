using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.UI;

// [RequireComponent(typeof(Renderer))]
public class DissolveEffect : MonoBehaviour
{
    public Color GradientColourOne
    {
        get => gradientColourOne;

        set
        {
            float factor = maths.Pow2(intensity);
            gradientColourOne = value * factor;
            _dissolveMatInst.SetColor(_gradientColourOneID, gradientColourOne);
        }
    }

    public Color GradientColourTwo
    {
        get => gradientColourTwo;

        set
        {
            float factor = maths.Pow2(intensity);
            gradientColourTwo = value * factor;
            _dissolveMatInst.SetColor(_gradientColourTwoID, gradientColourTwo);
        }
    }

    public Color GradientColourThree
    {
        get => gradientColourThree;

        set
        {
            float factor = maths.Pow2(intensity);
            gradientColourThree = value * factor;
            _dissolveMatInst.SetColor(_gradientColourThreeID, gradientColourThree);
        }
    }

    public Texture2D Texture
    {
        get => texture;

        set
        {
            texture = value;
            _dissolveMatInst.SetTexture(_textureID, texture);
        }
    }

    public Color BaseColour
    {
        get => baseColour;

        set
        {
            baseColour = value;
            _dissolveMatInst.SetColor(_baseColourID, baseColour);
        }
    }

    public float EdgeWidth
    {
        get => edgeWidth;

        set
        {
            edgeWidth = value;
            _dissolveMatInst.SetFloat(_edgeWidthID, edgeWidth);
        }
    }

    public float NoiseStrength
    {
        get => noiseStrength;

        set
        {
            noiseStrength = value;
            _dissolveMatInst.SetFloat(_noiseStrengthID, noiseStrength);
        }
    }

    public float NoiseScale
    {
        get => noiseScale;

        set
        {
            noiseScale = value;
            _dissolveMatInst.SetFloat(_noiseScaleID, noiseScale);
        }
    }

    public float NoiseSpeed
    {
        get => noiseSpeed;

        set
        {
            noiseSpeed = value;
            _dissolveMatInst.SetFloat(_noiseSpeedID, noiseSpeed);
        }
    }

    public float NoiseShape
    {
        get => noiseShape;

        set
        {
            noiseShape = value;
            _dissolveMatInst.SetFloat(_noiseShapeID, noiseShape);
        }
    }

    public float BaseColourStrength
    {
        get => baseColourStrength;

        set
        {
            baseColourStrength = maths.Clamp01(value);
            _dissolveMatInst.SetFloat(_tintID, baseColourStrength);
        }
    }

    [SerializeField] private bool isUi;
    [Header("References")]
    [SerializeField] private Material dissolveMat;
    [SerializeField] private ParticleSystem particleSys;
    [Header("Colours")]
    [Range(0f, 10f)][field: SerializeField] public float intensity;
    [SerializeField] private Color gradientColourOne;
    [SerializeField] private Color gradientColourTwo;
    [SerializeField] private Color gradientColourThree;
    [SerializeField] private Color baseColour;
    [SerializeField] private Texture2D texture;
    [Header("Noise")]
    [Range(0f, 5f)][SerializeField] private float edgeWidth;
    [Range(0f, 10f)][SerializeField] private float noiseStrength;
    [Range(0f, 50f)][SerializeField] private float noiseScale;
    [Range(0f, 5f)][SerializeField] private float noiseSpeed;
    [Range(-1f, 1)][SerializeField] private float noiseShape;
    [Header("Misc")]
    [SerializeField] private float dissolveEffectSpeed = 2f;
    [SerializeField] private Vector3 direction;
    [Range(0f, 1f)][SerializeField] private float baseColourStrength;

    private Material _dissolveMatInst;
    private Shader _dissolveShader;
    private Renderer _renderer;
    private ParticleSystemRenderer _psRenderer;
    private RectTransform _imageRect;
    private float _currentDissolveAmount;

    private int _dissolveAmountID;
    private int _baseColourID;
    private int _sizeID;
    private int _directionID;
    private int _gradientColourOneID;
    private int _gradientColourTwoID;
    private int _gradientColourThreeID;
    private int _edgeWidthID;
    private int _noiseStrengthID;
    private int _noiseScaleID;
    private int _noiseSpeedID;
    private int _noiseShapeID;
    private int _tintID;
    private int _textureID;
    private int _uiPositionID;

    private bool _canParticle;
    private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    public Material MaterialInstance { get => _dissolveMatInst; }

    // NOTE(WSWhitehouse): Caching function call in delegate
    public delegate IEnumerator DoDissolveEffectDel(bool useParticles);
    public DoDissolveEffectDel DoDissolveEffect;

    private void Awake()
    {
        DoDissolveEffect = __DoDissolveEffect;

        Debug.Assert(dissolveMat != null, $"Please assign DissolveMaterial on {gameObject.name}");
        _dissolveMatInst = new Material(dissolveMat);
        Debug.Assert(_dissolveMatInst.HasProperty("_Amount"), "Material Assigned did not have dissolve amount property and therefore is not a dissolve shader.");
        _dissolveShader = _dissolveMatInst.shader;

        _sizeID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Size"));
        _dissolveAmountID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Amount"));
        _baseColourID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Tint"));
        _directionID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Direction"));
        _edgeWidthID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_EdgeWidth"));
        _noiseStrengthID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_NoiseStrength"));
        _noiseScaleID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_NoiseScale"));
        _noiseSpeedID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_NoiseSpeed"));
        _noiseShapeID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_NoiseShape"));
        _tintID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Tint"));
        _textureID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_Texture"));
        _gradientColourOneID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_GradientColourOne"));
        _gradientColourTwoID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_GradientColourTwo"));
        _gradientColourThreeID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_GradientColourThree"));

        if (isUi)
        {
            _uiPositionID = _dissolveShader.GetPropertyNameId(_dissolveShader.FindPropertyIndex("_UIPosition"));
        }

        _dissolveMatInst.SetVector(_directionID, direction);

        if (!TryGetComponent(out _renderer))
        {
            if (!TryGetComponent(out Image image))
            {
                Debug.Assert(false, "Dissolve Effect requires a component of either Mesh Renderer or Image etc.");
            }

            image.material = _dissolveMatInst;
            // _canvasRenderer.SetMaterial(_dissolveMatInst, 0);
            var rect = image.transform as RectTransform;
            _dissolveMatInst.SetVector(_sizeID, rect.rect.size);
        }
        else
        {
            _renderer.material = _dissolveMatInst;
            _dissolveMatInst.SetVector(_sizeID, /*_renderer.bounds.size*/ _renderer.localBounds.size);
        }

        // float size = sizeVec.magnitude;
        // Log.Print(size.ToString("F"));
        // Vector3 sizeVec = float3Util.LinearFloatMultiply(_renderer.bounds.size, direction);

        GradientColourOne = new Color(gradientColourOne.r, gradientColourOne.g, gradientColourOne.b, 1);
        GradientColourTwo = new Color(gradientColourTwo.r, gradientColourTwo.g, gradientColourTwo.b, 1);
        GradientColourThree = new Color(gradientColourThree.r, gradientColourThree.g, gradientColourThree.b, 1);

        _dissolveMatInst.SetColor(_gradientColourOneID, GradientColourOne);
        _dissolveMatInst.SetColor(_gradientColourTwoID, GradientColourTwo);
        _dissolveMatInst.SetColor(_gradientColourThreeID, GradientColourThree);

        _dissolveMatInst.SetTexture(_textureID, texture);
        _dissolveMatInst.SetColor(_baseColourID, baseColour);
        _dissolveMatInst.SetFloat(_edgeWidthID, edgeWidth);
        _dissolveMatInst.SetFloat(_noiseStrengthID, noiseStrength);
        _dissolveMatInst.SetFloat(_noiseScaleID, noiseScale);
        _dissolveMatInst.SetFloat(_noiseSpeedID, noiseSpeed);
        _dissolveMatInst.SetFloat(_noiseShapeID, noiseShape);
        _dissolveMatInst.SetFloat(_tintID, baseColourStrength);

        if (particleSys != null)
        {
            _canParticle = true;
            _psRenderer = particleSys.GetComponent<ParticleSystemRenderer>();
            ToggleParticles(false);
        }
        else
        {
            Log.Warning($"No Particle system has been added to Dissolve effect on {gameObject.name}, effect will still function.");
        }

        SetDissolveAmount(0, false);
    }

    // public Material GetMaterialInstance() { return _dissolveMatInst;}

    public void SetDissolveAmount(float amount, bool updateParticles)
    {
        _currentDissolveAmount = maths.Clamp(amount, 0, 1);
        _dissolveMatInst.SetFloat(_dissolveAmountID, _currentDissolveAmount);
        if (_canParticle && updateParticles)
        {
            Vector3 pos = direction.normalized * GetPosAtAmount(_currentDissolveAmount);
            MoveParticleSystem(pos);
            UpdatePSColour(GetColourAtAmount(_currentDissolveAmount));
        }
    }

    public float GetPosAtAmount(float value)
    {
        Vector3 size    = _dissolveMatInst.GetVector(_sizeID);
        Vector3 sizeVec = float3Util.LinearFloatMultiply(size, direction);

        var t   = sizeVec.magnitude * value;
        var pos = -(sizeVec.magnitude * 0.5f) + t;
        return pos;
    }

    public void ToggleParticles(bool value)
    {
        if (!_canParticle)
        {
            return;
        }

        ParticleSystem.EmissionModule particleSysEmission = particleSys.emission;
        particleSysEmission.enabled = value;
    }

    public void SetBaseColour(Color colour) { _dissolveMatInst.SetColor(_baseColourID, colour); }

    // NOTE(WSWhitehouse): This is effectively a public function and should be called using the 
    // DoDissolveEffect function pointer which is assigned in Awake!
    private IEnumerator __DoDissolveEffect(bool useParticles)
    {
        SetDissolveAmount(0, false);
        ToggleParticles(true);
        float t = 0;

        while (t < 1)
        {
            SetDissolveAmount(t, useParticles);
            t += dissolveEffectSpeed * Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        ToggleParticles(false);
    }

    private void Update()
    {
        // ToggleParticles(true);
        // SetDissolveAmount(maths.TriangleWave(-1, 2, 4, 0, Time.time), true);
        // SetDissolveAmount(maths.TriangleWave(-0.25f, 1.25f, 2, 0, Time.time));

        if (!isUi)
        {
            return;
        }

        UpdateShaderWithUIPosition();
    }

    private void UpdateShaderWithUIPosition() { _dissolveMatInst.SetVector(_uiPositionID, _imageRect.position); }

    private Color GetColourAtAmount(float amount)
    {
        if (amount < 0.5f)
        {
            return Color.Lerp(GradientColourOne, GradientColourTwo, amount);
        }

        return Color.Lerp(GradientColourTwo, GradientColourThree, amount);
    }

    private void MoveParticleSystem(Vector3 pos)
    {
        ParticleSystem.ShapeModule particleSysShape = particleSys.shape;
        var                        lookVec          = particleSys.transform.localPosition - pos;
        // lookVec *= 10;
        if (lookVec.magnitude != 0)
        {
            particleSysShape.rotation = Quaternion.LookRotation(lookVec, Vector3.up).eulerAngles;
        }

        particleSys.transform.localPosition = pos;
    }

    private void UpdatePSColour(Vector4 colour) { _psRenderer.material.SetColor(Emission, colour); }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LaserEffectWave
{
    private Material _laserMat;
    private Coroutine _laserFireAnimCo;
    private Coroutine _laserIdleAnimCo;

    private delegate IEnumerator LerpToAmplitudeCoroutine(float speed);
    private LerpToAmplitudeCoroutine LerpToTargetAmplitudeFunc;
    private LerpToAmplitudeCoroutine LerpToIdleAmplitudeFunc;
    private Coroutine _lerpToIdleCo;

    private delegate IEnumerator LaserPulseCoroutine(float pulseAmplitude, float pulseSpeed);
    private LaserPulseCoroutine LaserPulseFunc;

    private string _suffix;

    private int _frequencyID;
    private int _amplitudeID;
    private int _colourID;
    // private int _colourHDRID;
    private int _clipPointID;
    private int _intensityID;
    private int _sizeID;

    private bool _animating;
    private bool _atTarget;
    private float _targetAmplitude;
    private float _idleAmplitude;
    private float _currentAmplitude;

    public LaserEffectWave(Material laserMat, string suffix)
    {
        _laserMat = laserMat;
        _suffix = suffix;
        Shader laserShader = laserMat.shader;

        StringBuilder sb = new StringBuilder(20);
        sb.Append("_Frequency");
        sb.Append(_suffix);
        _frequencyID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex(sb.ToString()));
        sb.Clear();
        sb.Append("_Amplitude");
        sb.Append(_suffix);
        _amplitudeID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex(sb.ToString()));
        sb.Clear();
        sb.Append("_Colour");
        sb.Append(_suffix);
        _colourID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex(sb.ToString()));
        sb.Clear();
        sb.Append("_Size");
        sb.Append(_suffix);
        _sizeID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex(sb.ToString()));
        sb.Clear();


        // sb.Append("_Colour");
        // sb.Append("One"); //This will be _suffix at some point
        // sb.Append("HDR");
        // _colourHDRID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex(sb.ToString()));
        // sb.Clear();
        // sb.Append("_ClipPoint");
        // sb.Append(_suffix);
        _clipPointID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex("_ClipPoint"));
        // _intensityID = laserShader.GetPropertyNameId(laserShader.FindPropertyIndex("_Intensity"));

        _currentAmplitude = _laserMat.GetFloat(_amplitudeID);
        // _targetAmplitude = _currentAmplitude;
        _idleAmplitude = _currentAmplitude;

        LerpToTargetAmplitudeFunc = LerpToAmplitude;
        LerpToIdleAmplitudeFunc = LerpToIdleAmplitude;
        LaserPulseFunc = LaserPulse;
    }

    public float GetSize() { return _laserMat.GetFloat(_sizeID); }
    public void  SetSize(float size) { _laserMat.SetFloat(_sizeID, size);}

    public void SetColour(Color colour)
    {
        _laserMat.SetColor(_colourID, colour);
    }

    public void SetAmplitude(float value)
    {
        _laserMat.SetFloat(_amplitudeID, value);
    }

    public float GetAmplitude()
    {
        return _laserMat.GetFloat(_amplitudeID);
    }

    public void SetFrequency(float value)
    {
        _laserMat.SetFloat(_frequencyID, value);
    }

    // public void SetHDRColour(Color colour)
    // {
    //     _laserMat.SetColor(_colourHDRID, colour);
    // }
    public void SetIntensity(float value)
    {
        _laserMat.SetFloat(_intensityID, value);
    }

    public void ChangeAmplitude(float targetAmplitude, float idleAmplitude, float speed)
    {
        _atTarget = false;
        _targetAmplitude = targetAmplitude;
        _idleAmplitude = idleAmplitude;
        CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref _laserFireAnimCo, LerpToTargetAmplitudeFunc(speed));
    }

    public void SetClipPoint(float newClipPoint)
    {
        _laserMat.SetFloat(_clipPointID, newClipPoint);
    }

    public float GetClipPoint()
    {
        return _laserMat.GetFloat(_clipPointID);
    }

    public void OnBeat(float additionalAmplitude, float speed)
    {
        if (_currentAmplitude > additionalAmplitude)
        {
            _atTarget = false;
            _targetAmplitude = additionalAmplitude;
            CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref _laserFireAnimCo, LerpToTargetAmplitudeFunc(speed));
        }
    }

    private IEnumerator LaserPulse(float pulseAmplitude, float pulseTime)
    {
        float time = 0;
        while (time < pulseTime)
        {
            _targetAmplitude -= pulseAmplitude * EaseInUtil.Exponential(time / pulseTime);
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        time = 0;
        while (time < pulseTime)
        {
            _targetAmplitude += pulseAmplitude * EaseOutUtil.Exponential(time / pulseTime);
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
    }

    private IEnumerator LerpToAmplitude(float speed)
    {
        float time = 0;
        CoroutineUtil.StopSafelyWithRef(StaticCoroutine.Mono, ref _laserIdleAnimCo);
        while (!_atTarget)
        {
            _currentAmplitude = maths.Lerp(_currentAmplitude, _targetAmplitude, EaseOutUtil.Quadratic(time / speed));
            _laserMat.SetFloat(_amplitudeID, _currentAmplitude);
            if (maths.FloatCompare(time, speed) || time > speed)
            {
                _laserMat.SetFloat(_amplitudeID, _targetAmplitude);
                // CoroutineUtil.StartSafelyWithRef(StaticCoroutine.Mono, ref _laserIdleAnimCo, LerpToIdleAmplitudeFunc(speed * 1.5f));
                _lerpToIdleCo = StaticCoroutine.Start(LerpToIdleAmplitudeFunc(speed));
                break;
            }
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
    }

    private IEnumerator LerpToIdleAmplitude(float speed)
    {
        float time = 0;
        _atTarget = true;
        while (_atTarget)
        {
            _currentAmplitude = maths.Lerp(_targetAmplitude, _idleAmplitude, EaseInUtil.Exponential(time / speed));
            _laserMat.SetFloat(_amplitudeID, _currentAmplitude);
            if (maths.FloatCompare(time, speed) || time > speed)
            {
                _laserMat.SetFloat(_amplitudeID, _idleAmplitude);
                break;
            }
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        _lerpToIdleCo = null;
    }
}

[RequireComponent(typeof(LineRenderer))]
public class LaserEffect : MonoBehaviour
{
    public Material LaserEffectMaterial => _laserMaterial;

    [SerializeField] private Material laserMaterialTemplate;
    [Range(0f, 10f)][SerializeField] private float maxIntensity;
    [SerializeField] private Laser laser;
    [SerializeField] private float laserFireTime;
    [SerializeField] private float laserIdleAmplitude;
    [SerializeField] private float laserFireAmplitude;
    [SerializeField] private float laserPulseAmplitude;
    [SerializeField] private float laserPulseTime;
    // [SerializeField] private float laserBeatAmplitude;
    // [SerializeField] private float laserNoteAmplitude;

    private Material _laserMaterial;
    private LineRenderer _lineRenderer;
    private LaserEffectWave[] _laserWaves = new LaserEffectWave[3];
    private LaserEchoHitIntensity[] _laserEchoHitIntensities;
    private PlayerID _playerID;
    private Vector3 _defaultLocalPosition;
    private float _revealTime;
    private float _colourAlpha = 1;
    private float _echoIntensityAnimTime;
    private float _echoIntensityAnimTimeOffset;
    private float _defaultIntensity;
    private bool _isEcho = false;

    private delegate IEnumerator WaitForValidPlayerCoroutine();
    private WaitForValidPlayerCoroutine WaitForValidPlayerFunc;
    private Coroutine _waitForValidPlayerCo;

    private delegate IEnumerator AnimateOnBlockHitCoroutine(float intensity);
    private AnimateOnBlockHitCoroutine AnimateOnBlockHitFunc;
    private Coroutine _animateOnBlockHitCo;

    private AnimateOnBlockHitCoroutine AnimateBackToDefaultFunc;
    private Coroutine _animateBackToDefaultCo;

    private delegate IEnumerator AnimateToPos(Vector3 pos, float time);
    private AnimateToPos AnimateToPosFunc;

    private void Awake()
    {
        _laserMaterial = new Material(laserMaterialTemplate);
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.material = _laserMaterial;

        _laserWaves[0] = new LaserEffectWave(_laserMaterial, "One");
        _laserWaves[1] = new LaserEffectWave(_laserMaterial, "Two");
        _laserWaves[2] = new LaserEffectWave(_laserMaterial, "Three");

        WaitForValidPlayerFunc = WaitForValidPlayer;
        AnimateOnBlockHitFunc = HitAnim;
        AnimateBackToDefaultFunc = BackToDefaultAnim;
        AnimateToPosFunc = AnimateToPosition;

        laser.OnLaserFireStarted += StartEffect;
        laser.OnLaserFireStopped += StopEffect;

        // REVIEW(WSWhitehouse): Should we be using the non-static version of the difficulty changed event?
        // Would mean it is invoked less as it would only be listening to its own player.
    }

    //NOTE(Felix): Might be an idea to use a struct as input...
    public void SetAsEchoLine(Vector3 localPos, float intensityPower, float echoIntensityAnimTime, float power, float alpha, LaserEchoHitIntensity[] laserEchoHitIntensities, float revealTime, float echoPulseAmplitude)
    {
        // MusicTrackPlayer.OnBeat += OnBeat;
        _lineRenderer.enabled = false;
        _isEcho = true;

        laserIdleAmplitude *= power;
        laserFireAmplitude *= power;
        laserPulseAmplitude *= echoPulseAmplitude;

        maxIntensity *= intensityPower;
        _defaultIntensity = maxIntensity;

        _colourAlpha = alpha;
        // Log.Print($"{maxIntensity}");
        foreach (LaserEffectWave laserWave in _laserWaves)
        {
            // laserWave.SetFrequency(frequency);
            laserWave.ChangeAmplitude(laserIdleAmplitude, laserIdleAmplitude, laserFireTime);
        }

        laser.OnLaserHit += StartHitAnim;

        _laserEchoHitIntensities = laserEchoHitIntensities;
        _echoIntensityAnimTime = echoIntensityAnimTime;

        _defaultLocalPosition = localPos;
        _revealTime = revealTime;

        // gameObject.SetActive(false);
    }

    private void StartHitAnim(Laser.LaserHitInfo hitInfo)
    {
        foreach (LaserEchoHitIntensity echoHitIntensity in _laserEchoHitIntensities)
        {
            if (echoHitIntensity.timing == hitInfo.Timing && _animateOnBlockHitCo == null)
            {
                if (_animateBackToDefaultCo != null)
                {
                    StopCoroutine(_animateBackToDefaultCo);
                    _animateBackToDefaultCo = null;
                    maxIntensity = _defaultIntensity;
                }
                _animateOnBlockHitCo = StartCoroutine(AnimateOnBlockHitFunc(echoHitIntensity.maxIntensity));
                return;
            }
        }
        // Log.Warning($"Laser Echo did not have a hit intensity for the players timing, please ensure Laser Echo Hit Intensities is set up correctly on {gameObject.transform.parent.name}");
    }

    private IEnumerator HitAnim(float intensity)
    {
        float time = 0;
        float dist = maths.Abs(maxIntensity - intensity);
        while (time < _echoIntensityAnimTime)
        {
            maxIntensity = _defaultIntensity + (dist * EaseOutUtil.Quadratic(time / _echoIntensityAnimTime));
            UpdateColours();

            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        _animateOnBlockHitCo = null;
        _animateBackToDefaultCo = StartCoroutine(AnimateBackToDefaultFunc(intensity));
    }

    private IEnumerator BackToDefaultAnim(float intensity)
    {
        float time = 0;
        float dist = maths.Abs(maxIntensity - intensity);
        while (time < _echoIntensityAnimTime)
        {
            maxIntensity -= _defaultIntensity + (dist * EaseInUtil.Exponential(time / _echoIntensityAnimTime));
            UpdateColours();

            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        maxIntensity = _defaultIntensity;
        UpdateColours();
        _animateBackToDefaultCo = null;
    }

    private IEnumerator AnimateToPosition(Vector3 localPos, float animTime)
    {
        _lineRenderer.enabled = true;
        float time = 0;
        Vector3 startPos = transform.localPosition;
        while (time < animTime)
        {
            transform.localPosition = float3Util.Lerp(startPos, localPos, EaseOutUtil.Quadratic(time / animTime));
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
    }

    private void InitPlayerColours()
    {
        SetPlayerColours(maxIntensity * 0.5f);
    }

    private void SetPlayerColours(float intensity)
    {
        for (int i = 0; i < 3; i++)
        {
            float factor = maths.Pow2(intensity);
            Color32[] colour = PlayerManager.PlayerData[_playerID.ID].HDRColourScheme.Colours;

            float saturation = colour == StaticData.ColourSchemesHDR[5].Colours ? 0f : 0.82f; // if black and white

            Color temp = colour[i];
            Color.RGBToHSV(temp * factor, out float hue, out float sat, out float vib);
            Color normalisedColour = Color.HSVToRGB(hue, saturation, vib, true);
            normalisedColour.a = _colourAlpha;
            _laserWaves[i].SetColour(normalisedColour);

        }
    }

    private void SetInactiveColours()
    {
        for (int i = 0; i < 3; i++)
        {
            Color colour = StaticData.ColourSchemes[laser.PlayerID.ID].Colours[i];
            float saturation = StaticData.ColourSchemes[laser.PlayerID.ID].Colours == StaticData.ColourSchemes[5].Colours ? 0f : 0.82f; // if black and white
            Color.RGBToHSV(colour, out float hue, out float sat, out float vib);
            Color normalisedColour = Color.HSVToRGB(hue, saturation, vib, true);
            normalisedColour.a = 0.25f;
            _laserWaves[i].SetColour(normalisedColour);
        }
    }

    private void OnPlayerDifficultyChanged()
    {
        if (!_playerID.IsValid) { return; }
        UpdateColours();
    }

    private void UpdateColours()
    {
        switch (_playerID.PlayerData.Difficulty)
        {
            case NoteDifficulty.EASY:
                SetPlayerColours(maxIntensity * 0.1f);
                break;
            case NoteDifficulty.MEDIUM:
                SetPlayerColours(maxIntensity * 0.5f);
                break;
            case NoteDifficulty.HARD:
                SetPlayerColours(maxIntensity);
                break;
            default:
                SetPlayerColours(maxIntensity * 0.5f);
                break;
        }
    }

    private IEnumerator WaitForValidPlayer()
    {
        _playerID = laser.PlayerID;
        SetInactiveColours();

        yield return PlayerManager.WaitForValidPlayer(_playerID.ID);
        
        _playerID.PlayerData.OnDifficultyUpdated += OnPlayerDifficultyChanged;

        InitPlayerColours();
        _waitForValidPlayerCo = null;
        if (!_isEcho)
        {
            yield break;
        }
        StartCoroutine(AnimateToPosFunc(_defaultLocalPosition, _revealTime));
    }

    private void OnEnable()
    {
        _waitForValidPlayerCo = StartCoroutine(WaitForValidPlayerFunc());
    }

    private void OnDestroy()
    {
        laser.OnLaserFireStarted -= StartEffect;
        laser.OnLaserFireStopped -= StopEffect;

        // MusicTrackPlayer.OnTrackStarted -= StartWaitForValidPlayer;
        
        if (_isEcho)
        {
            laser.OnLaserHit -= StartHitAnim;
        }

        if (_playerID.IsValid)
        {
           _playerID.PlayerData.OnDifficultyUpdated -= OnPlayerDifficultyChanged;
        }
    }

    private void StartEffect(Laser.LaserInfo info)
    {
        _laserWaves[info.colourIndex].ChangeAmplitude(laserFireAmplitude, laserIdleAmplitude, laserFireTime);
    }

    private void StopEffect(Laser.LaserInfo info)
    {
        // _laserWaves[info.colourIndex].ChangeAmplitude(laserIdleAmplitude, laserIdleAmplitude, laserFireSpeed);
    }
}

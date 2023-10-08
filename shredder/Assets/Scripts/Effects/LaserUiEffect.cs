using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LaserUiEffectValues
{
    public Vector3 position;
    public float revealTime;
    public float idleAmplitude;
    public float effectAmplitude;
    public float effectTime;
}

public class LaserUiEffect : MonoBehaviour
{
    // public Material LaserEffectMaterial => _laserMaterial;

    [SerializeField] private UiLaser laserPrefab;
    [SerializeField] private int laserCount;
    [Header("Positional Values")]
    [SerializeField] private Vector3 offsetStart;
    [SerializeField] private Vector3 offsetStep;
    [SerializeField] private float revealTimeStart;
    [SerializeField] private float revealTimeStep;
    [Header("Amplitude Values")]
    [SerializeField] private float idleAmplitudeStart;
    [SerializeField] private float effectAmplitudeStart;
    [SerializeField] private float idleAmplitudeStep;
    [SerializeField] private float effectAmplitudeStep;
    [Header("Effect Values")]
    [SerializeField] private float effectTime;
    [SerializeField] private float effectTimeStep;
    [Header("Intensity Values")]
    [SerializeField] private float intensityStart;
    [SerializeField] private float intensityStep;
    [Header("Alpha Values")]
    [SerializeField] private float alphaStart;
    [SerializeField] private float alphaStep;

    private UiLaser[] _lasers;
    // private Material _laserMaterial;
    private LineRenderer _lineRenderer;
    private UiLaser _mainLaser;
    private PlayerID _playerID;

    private void Awake()
    {
        _lasers = new UiLaser[laserCount];
        for (int i = 0; i < laserCount; i++)
        {
            var laser = Instantiate(laserPrefab, transform);
            laser.transform.localPosition = Vector3.zero;

            if (i != 0)
            {
                _lasers[i] = laser;
            }
            else
            {
                _lasers[i] = laser;
                _mainLaser = laser;
            }
        }
        enabled = false;
    }

    public void SetPlayerID(PlayerID player)
    {
        _playerID = player;
    }

    public void SetColours(Color32[] colours)
    {
        for (int i = 0; i < laserCount; i++)
        {
            float factor = maths.Pow2(intensityStart - (intensityStep * i));
            float alpha = alphaStart - (alphaStep * i);
            _lasers[i].SetColours(colours, factor, alpha);
        }
    }
    
    public void AnimateAmplitude(float multiplier)
    {
        for (int i = 0; i < laserCount; i++)
        {
            _lasers[i].AnimateAmplitude(multiplier);
        }    
    }

    private void OnEnable()
    {
        LaserUiEffectValues value = new LaserUiEffectValues
        {
            position = Vector3.zero,
            revealTime = 0,
            idleAmplitude = idleAmplitudeStart,
            effectAmplitude = effectAmplitudeStart,
            effectTime = effectTime
        };
        _mainLaser.StartReveal(value);
        for (int i = 1; i < laserCount; i++)
        {
            LaserUiEffectValues values = new LaserUiEffectValues
            {
                position = offsetStart + (offsetStep * (i - 1)),
                revealTime = revealTimeStart + (revealTimeStep * i),
                idleAmplitude = idleAmplitudeStart + (idleAmplitudeStep * i ),
                effectAmplitude = effectAmplitudeStart + (effectAmplitudeStep * i),
                effectTime = effectTime + (effectTimeStep * i),
            };
            _lasers[i].StartReveal(values);
        }
        //TODO(Felix): I think ive made the hdr colours in the wrong place in playerdata.

        var coloursScheme = PlayerManager.PlayerData[_playerID.ID].HDRColourScheme.Colours;
        for (int i = 0; i < laserCount; i++)
        {
            float factor = maths.Pow2(intensityStart - (intensityStep * i));
            float alpha = alphaStart - (alphaStep * i);
            _lasers[i].SetColours(coloursScheme, factor, alpha);
        }
    }

    private void OnDisable()
    {
        for (int i = 1; i < laserCount; i++)
        {
            _lasers[i].transform.localPosition = Vector3.zero;
        }
    }
}

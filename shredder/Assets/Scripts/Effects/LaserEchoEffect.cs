using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LaserEchoHitIntensity
{
    public LaserHitTiming timing;
    public float maxIntensity;
}

public class LaserEchoEffect : MonoBehaviour
{
    [SerializeField] private LaserEffect laserLinePrefab;
    [SerializeField] private int echoCount = 3;
    [SerializeField] private float echoStep = -0.1f;
    [SerializeField] private float echoIntensityStep = 0.1f;
    [SerializeField] private float echoPowerStep;
    [SerializeField] private LaserEchoHitIntensity[] laserEchoHitIntensities = new LaserEchoHitIntensity[5];
    [SerializeField] private float echoIntensityAnimTime = 1;
    [SerializeField] private float revealSpeedStart = 0.25f;
    [SerializeField] private float revealSpeedStep = 0.2f;
    [SerializeField] private float echoPulseAmplitudeStep;
    // [SerializeField] private float animOffsetStep = 0.1f;
    // [SerializeField] private float echoBeatPulsePower;
    // [SerializeField] private float echoClipPointStep = 0.1f;0

    private List<LaserEffect> _laserLines;
    private void Awake()
    {
        _laserLines = new List<LaserEffect>(echoCount);
        for (int i = 1; i < echoCount + 1; i++)
        {
            var pos = new Vector3(transform.position.x, transform.position.y + (echoStep * i), transform.position.z);
            LaserEffect inst = Instantiate(laserLinePrefab, transform);
            inst.transform.localPosition = Vector3.zero;
            // inst.transform.SetParent(transform);
            _laserLines.Add(inst);
            float alphaStep = 1f / (echoCount + 0.8f);
            _laserLines[i - 1].SetAsEchoLine(transform.InverseTransformPoint(pos) ,echoIntensityStep * (echoCount - i), echoIntensityAnimTime,echoPowerStep * i, 1 - (alphaStep*i), laserEchoHitIntensities, revealSpeedStart + (revealSpeedStep * (i - 1)), echoPulseAmplitudeStep * i);
        }
    }
}

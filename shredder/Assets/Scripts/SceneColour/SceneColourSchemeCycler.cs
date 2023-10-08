using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneColourSchemeCycler : MonoBehaviour
{
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private float time = 1;
    [SerializeField] private float waitTime = 1;
    
    private MusicTrack[] _tracks => StaticData.MusicTracks;
    private int _currentScheme = 0;
    
    private DelegateUtil.EmptyCoroutineDel LerpColourSchemes;
    private Coroutine lerpCo;
    private bool lerpStarted = false;
    
    private void Awake()
    {
        // to stop it being called in GameScene?
        if (!playOnAwake) { return; }

        // delegate allocation (probably not necessary)
        LerpColourSchemes = __LerpColourSchemes;

        // set the colour scheme
        _currentScheme = Random.Range(0, _tracks.Length);
        SceneColourer.UpdateSceneMaterialsColours(_tracks[_currentScheme]);
        
        // start the coroutine
        CoroutineUtil.StartSafelyWithRef(this, ref lerpCo, LerpColourSchemes());
        lerpStarted = true;        
    }

    private void OnDestroy()
    {
        if (!lerpStarted) return;
        CoroutineUtil.StopSafelyWithRef(this, ref lerpCo);
    }

    private IEnumerator __LerpColourSchemes() {
        while (true) {
            yield return SceneColourer.LerpToScheme(_tracks[_currentScheme].SceneColourScheme, time);

            // wrap the index for the current colour scheme
            _currentScheme += 1;
            _currentScheme = ArrayUtil.WrapIndex(_currentScheme, _tracks.Length);

            // wait for a period of time before we lerp to the next colour
            yield return CoroutineUtil.Wait(waitTime);
        }
    }
}

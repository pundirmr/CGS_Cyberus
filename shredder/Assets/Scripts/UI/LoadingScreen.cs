using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct LoadingRefs {
    public CanvasRenderer background;
    
    // info text borders
    public RectTransform corners;
    public Image textCorners;

    // loading screen info
    public TMP_Text text;
    public Image img;

    // bottom loading text
    public UITextGlow loadGlow;
    public TMP_Text loadingText;

    [NonSerialized] public float2 originalSize;
    [NonSerialized] public Color imgTransparent;
    [NonSerialized] public Color imgOpaque;
    public const float MinCornerWidth = 350f; // a hardcoded value to stop weird scaling on a screen
}

public class LoadingScreen : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LoadingRefs sceneRefs;

    /*[Header("SFX References")]
    [SerializeField] private AudioClip showSFX;
    [SerializeField] private AudioClip hideSFX;
    */

    [Header("Background Transition Settings")]
    [SerializeField] private float backgroundFadeInDuration  = 1.5f;
    [SerializeField] private float backgroundFadeOutDuration = 1.5f;
    
    [Header("Info Text Transition Settings")]
    [SerializeField] private float textWidthDuration  = 0.25f;
    [SerializeField] private float textHeightDuration = 0.1f;
    [SerializeField] private float textAlphaDuration  = 0.15f;

    [Header("Loading Text Transition Settings")]
    [SerializeField] private float loadingDuration = 1f;
    [SerializeField] private RangedFloat intensityVals = new (0f, 0.2f);
    [SerializeField] private float waitBetweenFlashes = 0.1f;


    
    // static access helpers
    public static Camera OverlayCamera;
    public static DelegateUtil.EmptyEventDel OnShowingLoadingScreen;
    public static DelegateUtil.EmptyEventDel OnHidingLoadingScreen;

    // alpha transitions for other elements are based off of the combined animation duration of the info text
    private float imgAlphaDuration     => textWidthDuration + textHeightDuration + textAlphaDuration;
    private float loadingAlphaDuration => imgAlphaDuration;
    
    private static LoadingScreen instance;
    private static LoadingRefs refs;
    private static string infoT;
    private static Sprite infoS;
    private static float2 textOriginalSize;
    
    // loading coroutines
    private DelegateUtil.EmptyCoroutineDel ShowLoading;
    private DelegateUtil.EmptyCoroutineDel HideLoading;
    
    // loading background coroutines
    private DelegateUtil.EmptyCoroutineDel ShowBackground;
    private DelegateUtil.EmptyCoroutineDel HideBackground;

    // loading info text coroutines
    private DelegateUtil.EmptyCoroutineDel ShowText;
    private DelegateUtil.EmptyCoroutineDel HideText;

    // loading info image coroutines
    private DelegateUtil.EmptyCoroutineDel ShowImage;
    private DelegateUtil.EmptyCoroutineDel HideImage;

    // loading text coroutines
    private DelegateUtil.EmptyCoroutineDel ShowLoadingText;
    private DelegateUtil.EmptyCoroutineDel HideLoadingText;

    // loading animation coroutines
    private DelegateUtil.EmptyCoroutineDel LoadingAnim;
    
    private bool subbed = false;
    private Coroutine loadingAnimCo;


    
    private void Awake() {
        if (instance != null) return;
        OverlayCamera = cam;
    }
        
    private void Start() {
        if (instance != null) {
            infoT = sceneRefs.text.text;
            infoS = sceneRefs.img.sprite;
            
            Canvas.ForceUpdateCanvases();
            textOriginalSize = new (sceneRefs.corners.GetWidth(), sceneRefs.corners.GetHeight());
            
            Destroy(this.gameObject);
            return;
        }

        // delegate allocations
        ShowLoading = __ShowLoading;
        HideLoading = __HideLoading;
        
        ShowBackground = __ShowBackground;
        HideBackground = __HideBackground;
        
        ShowText = __ShowText;
        HideText = __HideText;

        ShowImage = __ShowImage;
        HideImage = __HideImage;

        ShowLoadingText = __ShowLoadingText;
        HideLoadingText = __HideLoadingText;

        LoadingAnim = __LoadingAnim;
        
        // set the static variables
        instance = this;
        infoT    = sceneRefs.text.text;
        infoS    = sceneRefs.img.sprite;

        // set reference variables
        Canvas.ForceUpdateCanvases();
        refs                   = sceneRefs;
        textOriginalSize       = new (refs.corners.GetWidth(), refs.corners.GetHeight());
        refs.corners.SetSize(new (LoadingRefs.MinCornerWidth, 0f));

        // set transparency of entities
        refs.background.SetAlpha(0.0f);
        refs.text.alpha        = 0f;
        refs.imgTransparent    = new (refs.img.color.r, refs.img.color.g, refs.img.color.b, 0f);
        refs.imgOpaque         = refs.img.color;
        refs.img.color         = refs.imgTransparent;
        refs.textCorners.color = refs.imgTransparent; // NOTE(Zack): we're making the assumption that both images use the same base colour of [White]

        // setup events
        SceneLoad.OnLoadingStarted  += ShowLoadingScreen;
        SceneLoad.OnLoadingComplete += HideLoadingScreen;
        subbed = true;

        refs.loadGlow.SetOutlineColourAndUpdate(refs.imgTransparent);
        
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy() {
        if (!subbed) return;
        SceneLoad.OnLoadingStarted  -= ShowLoadingScreen;
        SceneLoad.OnLoadingComplete -= HideLoadingScreen;
    }


    private void ShowLoadingScreen() {
        // set the info text and sprite for the loading screen
        refs.text.text    = infoT;
        refs.img.sprite   = infoS;
        refs.originalSize = textOriginalSize;

        StartCoroutine(ShowLoading());
    }

    private void HideLoadingScreen() {
        StartCoroutine(HideLoading());
    }

    private IEnumerator __ShowLoading() {
        OnShowingLoadingScreen?.Invoke();
        
        yield return ShowBackground();
        
        StartCoroutine(ShowLoadingText());
        StartCoroutine(ShowText());
        StartCoroutine(ShowImage());
    }

    private IEnumerator __HideLoading() {
        StartCoroutine(HideLoadingText());
        StartCoroutine(HideText());
        StartCoroutine(HideImage());
        
        yield return HideBackground();
        
        OnHidingLoadingScreen?.Invoke();
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Background Fade Functions
    private IEnumerator __ShowBackground() {
        float elapsed = 0f;
        while (elapsed < backgroundFadeInDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / backgroundFadeInDuration;

            refs.background.SetAlpha(maths.Lerp(0.0f, 1.0f, t));

            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.background.SetAlpha(1.0f);
    }
    
    private IEnumerator __HideBackground() {
        float elapsed = 0f;
        while (elapsed < backgroundFadeOutDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / backgroundFadeOutDuration;

            refs.background.SetAlpha(maths.Lerp(1.0f, 0.0f, t));

            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.background.SetAlpha(0.0f);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Info Text Animation Functions
    private IEnumerator __ShowText() {
        // play show ui sfx
        // SFX.PlayUIScene(showSFX);
        AudioEventSystem.TriggerEvent("ShowSFX", null);

        
        float elapsed   = 0f;
        float endHeight = refs.originalSize.y;
        while (elapsed < textHeightDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textHeightDuration;

            // set height and opacity of the image
            float y = maths.Lerp(0f, endHeight, t);
            refs.corners.SetHeight(y);
            refs.textCorners.color = Colour.Lerp(refs.imgTransparent, refs.imgOpaque, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        // lerp the width to the original size of the text
        elapsed = 0f;
        float endWidth = refs.originalSize.x;
        while (elapsed < textWidthDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textWidthDuration;

            // set width of the corners
            float x = maths.Lerp(LoadingRefs.MinCornerWidth, endWidth, t);
            refs.corners.SetWidth(x);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        // lerp the text transparency
        elapsed = 0f;
        while (elapsed < textAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textWidthDuration;
            refs.text.alpha = t;
            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.text.alpha = 1f;

        
        yield break;
    }

    private IEnumerator __HideText() {
        // lerp the text transparency
        float elapsed = 0f;
        while (elapsed < textAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textWidthDuration;
            refs.text.alpha = 1f - t;
            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.text.alpha = 0f;


        // play show ui sfx
        // SFX.PlayUIScene(hideSFX);
        AudioEventSystem.TriggerEvent("HideSFX", null);

        
        // lerp the width to final value
        elapsed = 0f;
        float startWidth = refs.originalSize.x;
        while (elapsed < textWidthDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textWidthDuration;

            // set width of the corners
            float x = maths.Lerp(startWidth, LoadingRefs.MinCornerWidth, t); 
            refs.corners.SetWidth(x);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        // lerp the height and transparency to final values
        elapsed = 0f;
        float startHeight = refs.originalSize.y;
        while (elapsed < textHeightDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / textHeightDuration;

            // set height and opacity of the image
            float y = maths.Lerp(startHeight, 0f, t);
            refs.corners.SetHeight(y);
            refs.textCorners.color = Colour.Lerp(refs.imgOpaque, refs.imgTransparent, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        yield break;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Image Animation Functions
    private IEnumerator __ShowImage() {
        float elapsed = 0f;
        while (elapsed < imgAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / imgAlphaDuration;

            refs.img.color = Colour.Lerp(refs.imgTransparent, refs.imgOpaque, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        yield break;
    }

    private IEnumerator __HideImage() {
        float elapsed = 0f;
        while (elapsed < imgAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / imgAlphaDuration;

            refs.img.color = Colour.Lerp(refs.imgOpaque, refs.imgTransparent, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        yield break;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Loading Text Animation Functions
    private IEnumerator __ShowLoadingText() {
        float elapsed = 0f;
        while (elapsed < loadingAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / loadingAlphaDuration;

            var colour = Colour.Lerp(refs.imgTransparent, refs.imgOpaque, t);
            refs.loadGlow.SetOutlineColour(colour);
            refs.loadGlow.UpdateOutlineColour();

            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.loadGlow.SetOutlineColour(refs.imgOpaque);
        refs.loadGlow.UpdateOutlineColour();
        CoroutineUtil.StartSafelyWithRef(this, ref loadingAnimCo, LoadingAnim());
        yield break;
    }

    private IEnumerator __HideLoadingText() {
        CoroutineUtil.StopSafelyWithRef(this, ref loadingAnimCo);

        float startIntensity = refs.loadGlow.BaseIntensity;
        float elapsed = 0f;
        while (elapsed < loadingAlphaDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / loadingAlphaDuration;

            var colour = Colour.Lerp(refs.imgOpaque, refs.imgTransparent, t);
            float intensity = maths.Lerp(startIntensity, intensityVals.minValue, t);
            refs.loadGlow.SetOutlineIntensity(intensity);
            refs.loadGlow.SetOutlineColour(colour);
            refs.loadGlow.UpdateOutlineColour();
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        refs.loadGlow.SetOutlineIntensity(0f);
        refs.loadGlow.SetOutlineColour(refs.imgTransparent);
        refs.loadGlow.UpdateOutlineColour();
        yield break;
    }

    private IEnumerator __LoadingAnim() {
        while (true) {
            // lerp glow increase
            float start   = intensityVals.minValue;
            float end     = intensityVals.maxValue;
            float elapsed = 0f;
            float val     = 0f;
            while (elapsed < loadingDuration) {
                elapsed += Time.deltaTime;
                float t  = elapsed / loadingDuration;

                val = maths.Lerp(start, end, EaseOutUtil.Cubic(t));
                refs.loadGlow.SetOutlineIntensityAndUpdate(val);

                yield return CoroutineUtil.WaitForUpdate;
            }

            refs.loadGlow.SetOutlineIntensityAndUpdate(val);
            yield return CoroutineUtil.Wait(waitBetweenFlashes);

            // lerp glow decrease
            elapsed = 0f;
            val = 0f;
            start = intensityVals.maxValue;
            end = intensityVals.minValue;
            while (elapsed < loadingDuration) {
                elapsed += Time.deltaTime;
                float t  = elapsed / loadingDuration;

                val = maths.Lerp(start, end, EaseInUtil.Cubic(t));
                refs.loadGlow.SetOutlineIntensityAndUpdate(val);

                yield return CoroutineUtil.WaitForUpdate;
            }
            
            refs.loadGlow.SetOutlineIntensityAndUpdate(val);
        }
    }
    
#if UNITY_EDITOR
    private void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame) {
            ShowLoadingScreen();
        }

        if (Keyboard.current.oKey.wasPressedThisFrame) {
            HideLoadingScreen();
        }
    }
#endif
}

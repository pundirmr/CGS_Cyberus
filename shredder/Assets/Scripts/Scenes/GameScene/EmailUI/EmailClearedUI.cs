using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class EmailClearedUI : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform border;
    [SerializeField] private TMP_Text text;

    [Header("Animation Settings")]
    [ColorUsage(false, true), SerializeField] private Color clearedColour = new (255, 185, 46);
    [ColorUsage(false, true), SerializeField] private Color failedColour  = new (255, 0, 0);
    [SerializeField] private float colourDuration       = 0.5f;
    [SerializeField] private float heightRevealDuration = 0.25f;
    [SerializeField] private float widthRevealDuration  = 0.5f;

    [Header("SFX References")]
    [SerializeField] private AudioClip clearedSFX;
    [SerializeField] private AudioClip failedSFX;
    [SerializeField] private AudioClip allAIEliminatedSFX;
    

    public DelegateUtil.EmptyCoroutineDel OpenClearedUI;
    public DelegateUtil.EmptyCoroutineDel CloseClearedUI;
    
    private const string clearedText = "Device\nSecured";
    private const string failedText  = "Device\nVulnerable";
    private AudioClip audio;
    
    private float minWidth    = 100f;
    private float2 finalDims  = new ();
    private Color finalColour = new ();

    private void Awake() {
        OpenClearedUI  = __OpenClearedUI;
        CloseClearedUI = __CloseClearedUI;
    }

    private void Start() {
        Canvas.ForceUpdateCanvases();

        var rect    = text.rectTransform;
        finalDims.y = rect.GetHeight() + 200f;
        border.SetSize(new Vector2(minWidth, 0f));
        canvasGroup.alpha = 0f;
    }


    public void SetEmailCleared(bool cleared) {
        if (cleared) {
            text.text   = clearedText;
            finalColour = clearedColour;
            audio       = clearedSFX;
        } else {
            text.text   = failedText;
            finalColour = failedColour;
            audio       = failedSFX;
        }
        
        Canvas.ForceUpdateCanvases();
        var rect    = text.rectTransform;
        finalDims.x = rect.GetWidth() + 300f;
    }


    private IEnumerator __OpenClearedUI() {
        float elapsed = 0f;
        while (elapsed < heightRevealDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / heightRevealDuration;
            
            float y = maths.Lerp(0f, finalDims.y, t);
            border.SetHeight(y);
            canvasGroup.alpha = maths.Lerp(0f, 1f, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        border.SetHeight(finalDims.y);
        canvasGroup.alpha = 1f;

        
        elapsed = 0f;
        while (elapsed < widthRevealDuration) {
            elapsed += Time.deltaTime;
            float t  = EaseOutUtil.Exponential(elapsed / widthRevealDuration);

            float x = maths.Lerp(minWidth, finalDims.x, t);
            border.SetWidth(x);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        border.SetWidth(finalDims.x);

        
        // we wait before we change the colour of the text
        yield return CoroutineUtil.Wait(0.5f);


        // HACK(Zack): edge case handling
        if (GameManager.AllPlayersDead) {
            VoiceOver.PlayGlobal(allAIEliminatedSFX);
        } else {
            // play audio as to whether the players have cleared or not cleared the email
            VoiceOver.PlayGlobal(audio);
        }
        
        elapsed = 0f;
        Color startColour = text.color;
        while (elapsed < colourDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / colourDuration;

            var colour = Colour.Lerp(startColour, finalColour, t);
            text.color = colour;
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        text.color = finalColour;
    }

    private IEnumerator __CloseClearedUI() {
        float elapsed = 0f;
        while (elapsed < widthRevealDuration) {
            elapsed += Time.deltaTime;
            float t  = EaseOutUtil.Exponential(elapsed / widthRevealDuration);

            float x = maths.Lerp(finalDims.x, minWidth, t);
            border.SetWidth(x);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        border.SetWidth(minWidth);

        elapsed = 0f;
        while (elapsed < heightRevealDuration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / heightRevealDuration;
            
            float y = maths.Lerp(finalDims.y, 0f, t);
            border.SetHeight(y);
            canvasGroup.alpha = maths.Lerp(1f, 0f, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }
        border.SetHeight(0f);
        canvasGroup.alpha = 0f;
    }
}

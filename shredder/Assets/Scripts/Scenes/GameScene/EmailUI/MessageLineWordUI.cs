using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class MessageLineWordUI : MonoBehaviour {
    [Header("Prefab References")]
    public TMP_Text txt;
    public Image img;
    [SerializeField] private AberrationEffect aberrationEffect;
    
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset keyWordFont;
    [SerializeField] private TMP_FontAsset notKeyWordFont;
    [SerializeField] private FontStyles keyWordStyle;
    [SerializeField] private FontStyles notKeyWordStyle;

    [Header("Glitch Effect Settings")]
    [SerializeField] private Sprite[] glitchSprites;
    [SerializeField] private float glitchIterationTime = 0.4f;
    [SerializeField] private float glitchSpread        = 0.125f;
    [SerializeField] private int glitchSpreadCount     = 3;
    [SerializeField] private int aberrationAmountSize  = 5;
    
    [Header("Colour Settings")]
    [SerializeField] private Color keywordColour    = new (255, 185, 46);
    [SerializeField] private Color nonKeywordColour = Colour.WhiteOpaque;


    private DelegateUtil.EmptyCoroutineDel GlitchWordOffScreen;
    private DelegateUtil.EmptyCoroutineDel MoveGlitchImage;
    private Coroutine glitchCo;

    public bool TxtIsEnabled => txt.enabled;
    public bool ImgIsEnabled => img.enabled;
    [NonSerialized] public bool IsKeyWord;
    [NonSerialized] public int WordIndex;

    // static data
    private static float3[] glitchSpreadDirections = new float3[8];
    private static float3[] aberrationAmounts;

    private int[] glitchSpritesIndex;
    private float glitchSpreadDuration;
    
    static MessageLineWordUI() {
        glitchSpreadDirections[0] = float3Util.left;
        glitchSpreadDirections[1] = float3Util.right;
        glitchSpreadDirections[2] = float3Util.up;
        glitchSpreadDirections[3] = float3Util.down;
        glitchSpreadDirections[4] = new ( 1,  1, 0);
        glitchSpreadDirections[5] = new ( 1, -1, 0);
        glitchSpreadDirections[6] = new (-1,  1, 0);
        glitchSpreadDirections[7] = new (-1, -1, 0);
    }
    
    private void Awake() {
        Debug.Assert(glitchSprites.Length > 0, "No glitch sprites have been set in the inspector", this);

        // delegate allocations
        GlitchWordOffScreen = __GlitchWordOffScreen;
        MoveGlitchImage     = __MoveGlitchImage;

        
        // setup sprite index's
        glitchSpritesIndex = new int[glitchSprites.Length];
        for (int i = 0; i < glitchSprites.Length; ++i) {
            glitchSpritesIndex[i] = i;
        }
        
        ArrayUtil.Shuffle(glitchSpritesIndex, glitchSpritesIndex.Length);

        
        aberrationAmounts = new float3[aberrationAmountSize];
        for (int i = 0; i < aberrationAmountSize; ++i) {
            aberrationAmounts[i] = aberrationEffect.RandomAmount();
        }

        // disable the image
        img.enabled = false;
        glitchSpreadDuration = glitchIterationTime / glitchSpreadCount;
    }

    // set the imgs material
    private void Start() => img.material = aberrationEffect.GetMaterialInstance();
    
    //////////////////////////////////////////////////////////////////////////////////
    /////////// Setup Functions
    public void SetWord(string word, int wordIndex) {
        txt.text  = word;
        WordIndex = wordIndex;
        IsKeyWord = GameManager.SpamMessage.keyWords[wordIndex];

        if (IsKeyWord) {
            txt.color     = keywordColour;
            txt.font      = keyWordFont;
            txt.fontStyle = keyWordStyle;
        } else {
            txt.color     = nonKeywordColour;
            txt.font      = notKeyWordFont;
            txt.fontStyle = notKeyWordStyle;
        }
    }
 
    public void ResetWord() {
        // if this animation is currently playing when we are trying to reset the word we stop it
        CoroutineUtil.StopSafelyWithRef(this, ref glitchCo);
        txt.enabled = true;
        img.enabled = false;
    }



    //////////////////////////////////////////////////////////////////////////////////
    /////////// Animation Functions
    public void AnimateDisableWord() {
        CoroutineUtil.StartSafelyWithRef(this, ref glitchCo, GlitchWordOffScreen());
    }

    private IEnumerator __GlitchWordOffScreen() {
        img.enabled = true;
        for (int i = 0; i < glitchSprites.Length; ++i) {
            int index  = ArrayUtil.WrapIndex(i, glitchSprites.Length);
            img.sprite = glitchSprites[glitchSpritesIndex[index]];
            yield return MoveGlitchImage();

            txt.enabled = false;
        }

        img.enabled = false;
        glitchCo    = null;
        yield break;
    }

    private IEnumerator __MoveGlitchImage() {
        float3 defaultPos = this.transform.position;
        int sIndex  = random.Range(0, glitchSpreadDirections.Length); // spread index
        int2 aIndex = random.RangeInt2(0, aberrationAmounts.Length);  // aberration index

        float posTime = glitchSpreadDuration;
        float elapsed = 0f;
        while (elapsed < glitchSpreadDuration) {
            elapsed += Time.deltaTime;
            posTime += Time.deltaTime;
            
            if (posTime >= glitchSpreadDuration) {
                posTime = 0f; // reset value
                
                float3 pos = defaultPos + (glitchSpreadDirections[sIndex] * glitchSpread);
                img.rectTransform.position = pos;
                img.rectTransform.rotation = quaternion.identity;

                aberrationEffect.SetAmount(aberrationAmounts[aIndex.x], aberrationAmounts[aIndex.y]);

                // get new indexs
                ArrayUtil.Shuffle(glitchSpritesIndex, glitchSpritesIndex.Length);        
                aIndex = random.RangeInt2(0, aberrationAmounts.Length);  // aberration index
            }

            yield return CoroutineUtil.WaitForUpdate;
        }

        img.rectTransform.position = defaultPos;
    }
    
}

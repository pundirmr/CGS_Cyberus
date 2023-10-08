using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

[Serializable]
public struct SongCardAvatars {
    public GameObject parent;
    public Image img;
}

[Serializable]
public struct SongAvatarRefs {
    public SongCardAvatars[] avatars;
    public TMP_Text[] playerNumbers;

    public void SetAvatarImage(int index, ref Sprite sprite) {
        avatars[index].img.sprite = sprite;
    }
    
    public void EnableAvatarSelection(int index) {
        avatars[index].parent.SetActive(true);
        playerNumbers[index].gameObject.SetActive(true);
    }
    
    public void DisableAvatarSelection(int index) {
        avatars[index].parent.SetActive(false);
        playerNumbers[index].gameObject.SetActive(false);
    }
}

[Serializable]
public struct SongInfoRefs {
    public TMP_Text name;
    public TMP_Text songLength;
    public Image[] difficulty;

    // used to set the difficulty level of a track
    [SerializeField] private Sprite onImg;
    [SerializeField] private Sprite offImg;
    
    public void SetDifficulty(int difficultyLevel) {
        for (int i = 0; i < difficulty.Length; ++i) {
            ref var d = ref difficulty[i];

            if (i < difficultyLevel) {
                d.sprite = onImg;
            } else {
                d.sprite = offImg;
            }
        }
    }
}

[Serializable]
public struct SongImageRefs {
    public Image SongImg;   
    public GameObject SelectedText;

    public DelegateUtil.EmptyCoroutineDel SetSelected;
    
    public void Init() {
        SetSelected = __SetSelected;
        Unselect();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetImages(ref Sprite img) => SongImg.sprite = img;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Select() => SelectedText.SetActive(true);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Unselect() => SelectedText.SetActive(false);
    
    private IEnumerator __SetSelected() {
        bool visible = false;
        for (int i = 0; i < 5; ++i) {
            if (visible) {
                Unselect();
            } else {
                Select();
            }

            visible = !visible;
            yield return CoroutineUtil.Wait(0.75f);
        }
        
        yield break;
    }
}

public class TrackRefs : MonoBehaviour {
    [Header("Scene References")]
    public Transform parent;
    public TMP_Text cardNumber;
    public SongAvatarRefs avatarRefs;
    public SongInfoRefs infoRefs;
    public SongImageRefs imgRefs;

    [Header("Animation Settings")]
    [SerializeField] private CanvasRenderer[] renderers;
    [SerializeField] private float animationLength = 1f;
    [SerializeField] private float finalScale = 0.8f;

    public DelegateUtil.EmptyCoroutineDel FadeIntoBackground;
    
    public void Init() {
        FadeIntoBackground = __FadeIntoBackground;
        imgRefs.Init();
    }
    
    public void SetAvatarImage(int playerID, ref Sprite sprite) => avatarRefs.SetAvatarImage(playerID, ref sprite);
    public void EnableAvatarSelection(int playerID)             => avatarRefs.EnableAvatarSelection(playerID);
    public void DisableAvatarSelection(int playerID)            => avatarRefs.DisableAvatarSelection(playerID);
    public void SetSongImage(ref Sprite img)                    => imgRefs.SetImages(ref img);
    public void SetDifficultyLevel(int difficultyLevel)         => infoRefs.SetDifficulty(difficultyLevel);


    public IEnumerator SetFinalSongSelected() {
        yield return imgRefs.SetSelected();
    }

    private IEnumerator __FadeIntoBackground() {
        float elapsed     = 0f;
        float3 startScale = float3Util.ScalarInit(1f);
        float3 endScale   = float3Util.ScalarInit(finalScale);
        Color startColour = Colour.WhiteOpaque;
        Color endColour   = Colour.GreyOpaque;
        
        while (elapsed < animationLength) {
            elapsed += Time.deltaTime;
            float t  = elapsed / animationLength;
            t = EaseInOutUtil.Exponential(t);

            for (int i = 0; i < renderers.Length; ++i) {
                Color tmp = Colour.Lerp(startColour, endColour, t);
                renderers[i].SetColor(tmp);
            }

            parent.localScale = float3Util.Lerp(startScale, endScale, t);
            
            yield return CoroutineUtil.WaitForUpdate;
        }

        // ensure final values are set after lerp
        for (int i = 0; i < renderers.Length; ++i) {
            renderers[i].SetColor(endColour);
        }
        
        parent.localScale = endScale;
        
        yield break;
    }
}

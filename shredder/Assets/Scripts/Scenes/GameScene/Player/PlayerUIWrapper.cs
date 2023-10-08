using System.Collections;
using UnityEngine;

public class PlayerUIWrapper : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private CanvasGroup uiCanvas;

    public bool IsOpaque { get; private set; } = false;
    
    private void Awake() {
        uiCanvas.alpha = 0f;
    }

    public void LerpUIOpaque(float duration) {
        if (IsOpaque) return;
        StartCoroutine(LerpUtil.LerpCanvasGroupAlpha(uiCanvas, 1f, duration));
        IsOpaque = true;
    }

    public void LerpUITransparent(float duration) {
        if (!IsOpaque) return;
        StartCoroutine(LerpUtil.LerpCanvasGroupAlpha(uiCanvas, 0f, duration));
        IsOpaque = false;
    }
}

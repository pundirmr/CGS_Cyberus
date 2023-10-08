using UnityEngine;

public class LaneWalls : MonoBehaviour
{
  [Header("Scene References")] 
  [SerializeField] private CanvasGroup canvasGroup;

  private Coroutine lerpCoroutine;

  private void Awake()
  {
    canvasGroup.alpha = 0.0f;
  }

  public void FadeToOnScreen(float duration)  => CoroutineUtil.StartSafelyWithRef(this, ref lerpCoroutine, LerpUtil.LerpCanvasGroupAlpha(canvasGroup, 1.0f, duration));
  public void FadeToOffScreen(float duration) => CoroutineUtil.StartSafelyWithRef(this, ref lerpCoroutine, LerpUtil.LerpCanvasGroupAlpha(canvasGroup, 0.0f, duration));
}
using UnityEngine;

[ExecuteAlways]
public class AspectRatioScaler : MonoBehaviour
{
  public float height = 1080f;
  public Vector2 aspectRatio;
  
  private RectTransform rectTransform       => transform as RectTransform;
  private RectTransform parentRectTransform => rectTransform.parent as RectTransform;

  private void Awake()
  {
    Vector2 size = rectTransform.parent == null ? new Vector2(Screen.width, Screen.height) : parentRectTransform.GetSize();
    float ratio  = aspectRatio.x / aspectRatio.y;
    // float height = size.y;
    float width  = height * ratio;

    rectTransform.SetSize(new Vector2(width, height));
  }
  
#if UNITY_EDITOR
  private void OnValidate() => Awake();
#endif
}
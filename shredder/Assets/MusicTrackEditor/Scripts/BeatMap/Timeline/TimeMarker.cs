using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TimeMarker : MonoBehaviour
{
  public RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private Image markerImage;
  [SerializeField] private TMP_Text markerText;
  [Space]
  [SerializeField] private float2 secondSize;
  [SerializeField] private float2 halfSecondSize;
  [SerializeField] private float2 otherSize;
  
  public void Init(decimal time)
  {
    #nullable enable
    markerText.text = string.Empty;
    
    float2? size = null;
    
    if (time % 0.5m == 0)
    {
      size = halfSecondSize;
    }
    
    if (time % 1.0m == 0)
    {
      size = secondSize;
      
      int min, sec;
      maths.TimeToMinutesAndSeconds(time, out min, out sec);
      markerText.text = $"{min:00}:{sec:00}";
    }

    if (!size.HasValue)
    {
      size = otherSize;
    }

    markerImage.rectTransform.SetWidth(size.Value.x);
    markerImage.rectTransform.SetHeight(size.Value.y);
    #nullable restore
  }
}
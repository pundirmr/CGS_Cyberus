using Unity.Mathematics;
using UnityEngine;

public class WindowResizePoint : MonoBehaviour
{
  public RectTransform RectTransform => (RectTransform)transform;
  
  public bool2 movementAxis;
}
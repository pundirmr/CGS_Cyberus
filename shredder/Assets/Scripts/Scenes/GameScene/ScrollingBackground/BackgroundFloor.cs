using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* NOTE(WSWhitehouse):
 * This script is responsible for setting the background floor to face the right direction
 */

public class BackgroundFloor : MonoBehaviour
{
  [SerializeField] private ScrollingBackground scrollingBackground;
  [SerializeField] private Canvas canvas;
  
  private RectTransform canvasRT => (RectTransform)canvas.transform;

  private void Awake()
  {
    SetCanvasPosition();
  }
  
  public void SetCanvasPosition()
  {
    // NOTE(WSWhitehouse): Lerping to halfway between start and end point! Basically, a fancy way of getting the mid point
    canvasRT.position = float3Util.Lerp(scrollingBackground.StartPos, scrollingBackground.EndPos, 0.5f);
    
    scrollingBackground.LookTowardsNormal(canvas.transform);
    
    float height = float3Util.Distance(scrollingBackground.StartPos, scrollingBackground.EndPos);
    canvasRT.SetHeight(height);
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BackgroundFloor))]
public class BackgroundFloorEditor : Editor
{
  private BackgroundFloor Target => (BackgroundFloor)target;
  
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    if (GUILayout.Button("Set Canvas Position"))
    {
      Target.SetCanvasPosition();
    }
  }
}
#endif
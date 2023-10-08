using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class BackgroundPavement : MonoBehaviour
{
  [SerializeField] private ScrollingBackground scrollingBackground;

  private void Awake()
  {
    SetCanvasPosition();
  }
  
  public void SetCanvasPosition()
  {
    // NOTE(WSWhitehouse): Lerping to halfway between start and end point! Basically, a fancy way of getting the mid point
    transform.position = float3Util.Lerp(scrollingBackground.StartPos, scrollingBackground.EndPos, 0.5f);
    scrollingBackground.LookTowardsNormal(transform);
    transform.localEulerAngles += new Vector3(90.0f, 0.0f, 0.0f);
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BackgroundPavement))]
public class BackgroundPavementEditor : Editor
{
  private BackgroundPavement Target => (BackgroundPavement)target;
  
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    if (GUILayout.Button("Set Pavement Position"))
    {
      Target.SetCanvasPosition();
    }
  }
}
#endif
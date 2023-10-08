using UnityEngine;

/// <summary>
/// Handles moving the block down the appropriate lane
/// </summary>
public class ColourLane : MonoBehaviour
{
  [SerializeField] private Lane parentLane;
  [SerializeField] private Vector3 laneOffset = Vector3.zero;
  
  public Vector3 StartPos { get; private set; }
  public Vector3 EndPos   { get; private set; }

  public void Awake()
  {
    StartPos = parentLane.StartTransform.position + laneOffset;
    EndPos   = parentLane.EndTransform.position + laneOffset;
  }

#if UNITY_EDITOR
  private void OnDrawGizmos()
  {
    if (parentLane == null) return;
    if (parentLane.StartTransform == null || parentLane.EndTransform == null) return;

    Vector3 startPos = parentLane.StartTransform.position + laneOffset;
    Vector3 endPos   = parentLane.EndTransform.position + laneOffset;
    Gizmos.color = Color.red;
    Gizmos.DrawLine(startPos, endPos);
  }
#endif
}
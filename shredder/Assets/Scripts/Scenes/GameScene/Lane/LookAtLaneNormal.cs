using System.Runtime.CompilerServices;
using UnityEngine;

public class LookAtLaneNormal : MonoBehaviour
{
  [SerializeField] private Lane lane;
  
  private void Start()
  {
    LookAtNormal();
  }

  [ContextMenu("Look At Lane Normal")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void LookAtNormal()
  {
    transform.LookAt(transform.position + lane.LaneNormal);
  }
  
  [ContextMenu("Set pos on lane")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetPosOnLane()
  {
    transform.position = float3Util.PointAlongLine(lane.StartTransform.position, lane.EndTransform.position, 0.5f);
  }
}
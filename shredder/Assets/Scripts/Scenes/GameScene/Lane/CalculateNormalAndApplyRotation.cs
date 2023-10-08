using UnityEngine;
using Unity.Mathematics;

[ExecuteAlways]
public class CalculateNormalAndApplyRotation : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private Transform wall;

    private float3 normal;
    private quaternion rot;

    private void CalculatePositionAndRotation() {
        var side1 = end.position - start.position;
        var side2 = new float3(-1f, 0f, 0f);
        normal    = float3Util.Normalise(float3Util.Cross(side1, side2));
        rot       = Quaternion.LookRotation(normal, side1);

        wall.position   = end.position + (start.position - end.position) / 2;
        wall.rotation   = rot;
    }

    private void OnValidate() {
        CalculatePositionAndRotation();
    }
}

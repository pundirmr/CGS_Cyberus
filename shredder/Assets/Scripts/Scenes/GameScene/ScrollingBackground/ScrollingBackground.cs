using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
  [Header("Background")]
  [SerializeField] private BackgroundSection backgroundPrefab;
  [SerializeField] private float backgroundSize;
  
  // NOTE(WSWhitehouse): Using a constant rather than a inspector values to give
  // the compiler a chance to unroll the for loops! Feel free to change the number.
  private const int NumOfBackgroundSections = 14;
  
  [Header("Movement")]
  [SerializeField] private float moveSpeed;
  [SerializeField] private Transform startTransform;
  [SerializeField] private Transform endTransform;
  
  public Vector3 StartPos => startTransform.position;
  public Vector3 EndPos   => endTransform.position;
  
  private Vector3 _moveDir;
  private int _firstBackgroundIndex;
  private int _finalBackgroundIndex;
  private BackgroundSection[] _backgrounds = new BackgroundSection[NumOfBackgroundSections];

  private void Awake()
  {
    _moveDir = float3Util.DirectionNormalised(EndPos, StartPos);

    for (int i = 0; i < NumOfBackgroundSections; i++)
    {
      BackgroundSection bg  = Instantiate(backgroundPrefab, transform, false);
      bg.transform.position =  EndPos - (_moveDir * (backgroundSize * i));
      
      // NOTE(WSWhitehouse): Setting the background to look at the plane normal. Adding 90
      // degrees to the rotation of the X axis in order to get the up vector facing the lane
      // normal rather than the forward vector (which is what LookAt() does).
      LookTowardsNormal(bg.transform);
      bg.transform.localEulerAngles += new Vector3(90.0f, 0.0f, 0.0f);
      
      bg.ChooseSectionAssets();
      
      _backgrounds[i] = bg;
    }
    
    _firstBackgroundIndex = 0;
    _finalBackgroundIndex = NumOfBackgroundSections - 1;
  }

  private void FixedUpdate()
  {
    // NOTE(WSWhitehouse): To check if the background section has gone past the endPos we 
    // calculate the movement direction vector between the section's current pos and endPos.
    // Comparing that dir vector to the movement vector (_moveDir) of the sections we can see
    // if the block has moved past the end position. I dont know if this is the most efficient 
    // method of doing this, but it is axis-independent - meaning it doesnt matter which axis
    // the sections are moving in.
    
    BackgroundSection firstBg = _backgrounds[_firstBackgroundIndex];
    Vector3 pos               = firstBg.transform.position;
    Vector3 dir               = float3Util.DirectionNormalised(EndPos, pos);
    if (!float3Util.Compare(dir, _moveDir))
    {
      Vector3 newPos = _backgrounds[_finalBackgroundIndex].transform.position - (_moveDir * backgroundSize);
      firstBg.transform.position = newPos;
      firstBg.ChooseSectionAssets();
      
      _finalBackgroundIndex = _firstBackgroundIndex;
      _firstBackgroundIndex = ArrayUtil.WrapIndex(_firstBackgroundIndex + 1, NumOfBackgroundSections);
    }
    
    // Move all background sections
    for (int i = 0; i < NumOfBackgroundSections; i++)
    {
      _backgrounds[i].transform.position += _moveDir * (moveSpeed * Time.deltaTime);
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Vector3 CalculateNormalVector()
  {
    float3 side1 = EndPos - StartPos;
    float3 side2 = new float3(-1.0f, 0.0f, 0.0f);
    return float3Util.Normalise(float3Util.Cross(side1, side2));
  }

  // NOTE(WSWhitehouse): A small function to make a transform look towards the normal of the scrolling 
  // background. It makes the transform.forward vector face that way so some objects might need to be 
  // rotated interdependently so another vector faces the normal.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void LookTowardsNormal(Transform obj)
  {
    obj.LookAt(obj.position + CalculateNormalVector());
  }

#if UNITY_EDITOR
  private void OnDrawGizmosSelected()
  {
    if (startTransform == null || endTransform == null) return;
    
    Gizmos.color = Color.green;
    Gizmos.DrawLine(StartPos, EndPos);
    
    Vector3 normal = CalculateNormalVector();
    Gizmos.color = Color.red;
    Gizmos.DrawLine(StartPos, StartPos + normal);
    Gizmos.DrawLine(EndPos, EndPos + normal);
  }
#endif
}
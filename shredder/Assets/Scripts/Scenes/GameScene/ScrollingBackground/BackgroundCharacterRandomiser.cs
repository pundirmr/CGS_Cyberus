using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BackgroundCharacterRandomiser : MonoBehaviour
{
  [SerializeField] private Transform character;
  [SerializeField] private Transform[] characterPoints = Array.Empty<Transform>();
  
  private int _pointIndex = 0;
  private int[] _pointIndices;

  private void Awake()
  {
    _pointIndices = new int[characterPoints.Length];
    for (int i = 0; i < _pointIndices.Length; i++) { _pointIndices[i] = i; }
    
    ArrayUtil.Shuffle(_pointIndices, _pointIndices.Length);
  }

  private void OnEnable() => RandomiseCharacter();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Transform GetPoint()
  {
    Transform point = characterPoints[_pointIndices[_pointIndex]];
    _pointIndex = ArrayUtil.WrapIndex(_pointIndex + 1, _pointIndices.Length);
    
    return point;
  }

  private void RandomiseCharacter()
  {
    Transform point = GetPoint();
    character.transform.position = point.position;
  }
}
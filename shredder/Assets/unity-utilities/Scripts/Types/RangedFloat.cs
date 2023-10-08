/*----------------------------------------------------------------------------*/
/* Unity Utilities                                                            */
/* Copyright (C) 2022 Manifold Games - All Rights Reserved                    */
/*                                                                            */
/* Unauthorized copying of this file, via any medium is strictly prohibited   */
/* Proprietary and confidential                                               */
/* Do NOT release into public domain                                          */
/*                                                                            */
/* The above copyright notice and this permission notice shall be included in */
/* all copies or substantial portions of the Software.                        */
/*                                                                            */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR */
/* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,   */
/* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL    */
/* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER */
/* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING    */
/* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER        */
/* DEALINGS IN THE SOFTWARE.                                                  */
/*                                                                            */
/* Written by Manifold Games <hello.manifoldgames@gmail.com>                  */
/*----------------------------------------------------------------------------*/

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct RangedFloat
{
  public RangedFloat(float minValue = 0f, float maxValue = 1f)
  {
    this.minValue = minValue;
    this.maxValue = maxValue;
    
    #if UNITY_EDITOR
    EDITOR_unlockedMin = 0f;
    EDITOR_unlockedMax = 0f;
    EDITOR_rangeInit   = false;
    #endif
  }

  public float minValue;
  public float maxValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool InRange(float val) => val >= minValue && val <= maxValue;
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Random() => UnityEngine.Random.Range(minValue, maxValue);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Clamp(float val) => maths.Clamp(val, minValue, maxValue);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Lerp(float t) => maths.Lerp(minValue, maxValue, t);
  
  public override string ToString() => $"Min Value: {minValue.ToString(CultureInfo.CurrentCulture)}, Max Value: {maxValue.ToString(CultureInfo.CurrentCulture)}";
  
  // Editor
  #if UNITY_EDITOR
#pragma warning disable CS0414
  [SerializeField, HideInInspector] private float EDITOR_unlockedMin;
  [SerializeField, HideInInspector] private float EDITOR_unlockedMax;
  [SerializeField, HideInInspector] private bool  EDITOR_rangeInit;
#pragma warning restore CS0414
  #endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangedFloat), true)]
public class RangedFloatDrawer : PropertyDrawer
{
  private const float RangeBoundsLabelWidth = 40f;
  private RangedTypeAttribute _rangeAttribute;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    RangedTypeAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(RangedTypeAttribute), true) as RangedTypeAttribute[];
    _rangeAttribute = attributes.Length > 0 ? attributes[0] : null;
    
    label = EditorGUI.BeginProperty(position, label, property);
    position = EditorGUI.PrefixLabel(position, label);

    SerializedProperty minProperty = property.FindPropertyRelative("minValue");
    SerializedProperty maxProperty = property.FindPropertyRelative("maxValue");

    if (_rangeAttribute == null)
    {
      DefaultRangedFloatDrawer(position, property);
    }
    else
    {
      float minValue = minProperty.floatValue;
      float maxValue = maxProperty.floatValue;

      switch (_rangeAttribute.displayType)
      {
        case RangedTypeDisplayType.LockedRanges:
        {
          var rangeBoundsLabel1Rect = new Rect(position) {width = RangeBoundsLabelWidth};
          GUI.Label(rangeBoundsLabel1Rect, new GUIContent(minValue.ToString("F2")));
          position.xMin += RangeBoundsLabelWidth;

          var rangeBoundsLabel2Rect = new Rect(position);
          rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - RangeBoundsLabelWidth;
          GUI.Label(rangeBoundsLabel2Rect, new GUIContent(maxValue.ToString("F2")));
          position.xMax -= RangeBoundsLabelWidth;

          EditorGUI.BeginChangeCheck();
          EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, _rangeAttribute.min, _rangeAttribute.max);
          if (EditorGUI.EndChangeCheck())
          {
            minProperty.floatValue = minValue;
            maxProperty.floatValue = maxValue;
          }

          break;
        }

        case RangedTypeDisplayType.UnlockedRanges:
        {
          SerializedProperty minRangeProperty  = property.FindPropertyRelative("EDITOR_unlockedMin");
          SerializedProperty maxRangeProperty  = property.FindPropertyRelative("EDITOR_unlockedMax");
          SerializedProperty rangeInitProperty = property.FindPropertyRelative("EDITOR_rangeInit");
          
          if (!rangeInitProperty.boolValue)
          {
            rangeInitProperty.boolValue = true;
            minRangeProperty.floatValue = _rangeAttribute.min;
            maxRangeProperty.floatValue = _rangeAttribute.max;
          }
          
          EditorGUI.BeginChangeCheck();

          position.width /= 8f;
          float newMinRange = EditorGUI.FloatField(position, minRangeProperty.floatValue);

          position.x += position.width;

          EditorGUI.LabelField(position, minValue.ToString("F2"));
          position.x += position.width;

          position.width *= 4f;
          EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, minRangeProperty.floatValue, maxRangeProperty.floatValue);


          position.x += position.width + 1f;
          position.width /= 4f;
          EditorGUI.LabelField(position, maxValue.ToString("F2"));

          position.x += position.width;
          float newMaxRange = EditorGUI.FloatField(position, maxRangeProperty.floatValue);

          if (EditorGUI.EndChangeCheck())
          {
            minRangeProperty.floatValue = newMinRange;
            maxRangeProperty.floatValue = newMaxRange;
            minProperty.floatValue = Mathf.Clamp(minValue, minRangeProperty.floatValue, maxRangeProperty.floatValue);
            maxProperty.floatValue = Mathf.Clamp(maxValue, minRangeProperty.floatValue, maxRangeProperty.floatValue);
          }

          break;
        }

        default: throw new NotImplementedException();
      }
    }

    EditorGUI.EndProperty();
  }

  private static void DefaultRangedFloatDrawer(Rect position, SerializedProperty property)
  {
    position.width /= 4f;
    EditorGUIUtility.labelWidth /= 4f;
    position.width *= 4f;
    position.width *= 0.375f;
    
    EditorGUI.PropertyField(position, property.FindPropertyRelative("minValue"), new GUIContent("Min"));

    position.x += position.width;

    EditorGUI.PropertyField(position, property.FindPropertyRelative("maxValue"), new GUIContent("Max"));
  }
}
#endif
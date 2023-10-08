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
public struct RangedInt
{
  public RangedInt(int minValue = 0, int maxValue = 10)
  {
    this.minValue = minValue;
    this.maxValue = maxValue;
    
    #if UNITY_EDITOR
    EDITOR_unlockedMin = 0;
    EDITOR_unlockedMax = 0;
    EDITOR_rangeInit   = false;
    #endif
  }

  public int minValue;
  public int maxValue;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool InRange(int val) => val >= minValue && val <= maxValue;
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int Random() => UnityEngine.Random.Range(minValue, maxValue);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int Clamp(int val) => maths.Clamp(val, minValue, maxValue);
  
  public override string ToString() => $"Min Value: {minValue.ToString(CultureInfo.CurrentCulture)}, Max Value: {maxValue.ToString(CultureInfo.CurrentCulture)}";
  
  // Editor
  #if UNITY_EDITOR
#pragma warning disable CS0414
  [SerializeField, HideInInspector] private int EDITOR_unlockedMin;
  [SerializeField, HideInInspector] private int EDITOR_unlockedMax;
  [SerializeField, HideInInspector] private bool EDITOR_rangeInit;
#pragma warning restore CS0414
  #endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangedInt), true)]
public class RangedIntDrawer : PropertyDrawer
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
      DefaultRangedIntDrawer(position, property);
    }
    else
    {
      float minValue = minProperty.intValue;
      float maxValue = maxProperty.intValue;

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
            minProperty.intValue = Mathf.RoundToInt(minValue);
            maxProperty.intValue = Mathf.RoundToInt(maxValue);
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
            minRangeProperty.intValue   = (int)_rangeAttribute.min;
            maxRangeProperty.intValue   = (int)_rangeAttribute.max;
          }
          
          EditorGUI.BeginChangeCheck();

          position.width /= 8f;
          int newMinRange = EditorGUI.IntField(position, minRangeProperty.intValue);

          position.x += position.width;

          EditorGUI.LabelField(position, minValue.ToString("F2"));
          position.x += position.width;

          position.width *= 4f;
          EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, minRangeProperty.intValue, maxRangeProperty.intValue);


          position.x += position.width + 1f;
          position.width /= 4f;
          EditorGUI.LabelField(position, maxValue.ToString("F2"));

          position.x += position.width;
          int newMaxRange = EditorGUI.IntField(position, maxRangeProperty.intValue);

          if (EditorGUI.EndChangeCheck())
          {
            minRangeProperty.intValue = newMinRange;
            maxRangeProperty.intValue = newMaxRange;
            minProperty.intValue = Mathf.RoundToInt(Mathf.Clamp(minValue, minRangeProperty.intValue, maxRangeProperty.intValue));
            maxProperty.intValue = Mathf.RoundToInt(Mathf.Clamp(maxValue, minRangeProperty.intValue, maxRangeProperty.intValue));
          }

          break;
        }

        default: throw new NotImplementedException();
      }
    }

    EditorGUI.EndProperty();
  }

  private static void DefaultRangedIntDrawer(Rect position, SerializedProperty property)
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
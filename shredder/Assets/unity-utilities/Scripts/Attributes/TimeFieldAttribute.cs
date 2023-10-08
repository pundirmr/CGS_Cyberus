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
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Place this attribute on a float field to change it to a minute and second input field in the inspector.
/// </summary>

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TimeFieldAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TimeFieldAttribute))]
public class TimeFieldPropertyDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    GUIContent minLabel = new GUIContent("Min ", "Minutes");
    GUIContent secLabel = new GUIContent("Sec ", "Seconds");
    GUIContent rawLabel = new GUIContent("Raw ", "Raw time value.");
    
    float minLabelWidth = EditorUtil.CalculateLabelWidth(minLabel, EditorUtil.LabelStyleRightAligned);
    float secLabelWidth = EditorUtil.CalculateLabelWidth(secLabel, EditorUtil.LabelStyleRightAligned);
    float rawLabelWidth = EditorUtil.CalculateLabelWidth(secLabel, EditorUtil.LabelStyleRightAligned);
    
    // Calculate current minutes and seconds is dependant on the property type.
    int min, sec;
    
    // NOTE(WSWhitehouse): To add more property types add more branches here to calculate the current time.
    // Add another branch to the if statement at the bottom of this function to draw the raw value and update
    // the serialized property.
    if (property.propertyType == SerializedPropertyType.Float)
    {
      float currentTime = property.floatValue;
      maths.TimeToMinutesAndSeconds(currentTime, out min, out sec);
    }
    else
    {
      Log.Error($"Property Type ({property.propertyType.ToString()}) is not supported on TimeFieldAttribute!");
      EditorGUI.PropertyField(position, property, label);
      return;
    }

    EditorGUI.BeginProperty(position, label, property);
      
    // Calculate label and field rects
    const float fieldGap = 10f;
    float fieldWidth = (position.width - EditorGUIUtility.labelWidth - minLabelWidth - secLabelWidth - rawLabelWidth - (fieldGap * 3)) * 0.33333333f;

    Rect labelRect  = position;
    labelRect.width = EditorGUIUtility.labelWidth;
      
    Rect minLabelRect  = position; 
    minLabelRect.width = minLabelWidth;
    minLabelRect.x     = labelRect.x + EditorGUIUtility.labelWidth;
      
    Rect minRect  = position; 
    minRect.width = fieldWidth;
    minRect.x     = minLabelRect.x + minLabelWidth;
      
    Rect secLabelRect  = position; 
    secLabelRect.width = secLabelWidth;
    secLabelRect.x     = minRect.x + fieldWidth + fieldGap;
      
    Rect secRect  = position; 
    secRect.width = fieldWidth;
    secRect.x     = secLabelRect.x + secLabelWidth;
      
    Rect rawLabelRect  = position; 
    rawLabelRect.width = rawLabelWidth;
    rawLabelRect.x     = secRect.x + fieldWidth + (fieldGap * 2f);
      
    Rect rawRect  = position; 
    rawRect.width = fieldWidth;
    rawRect.x     = rawLabelRect.x + rawLabelWidth;
      
    EditorGUI.LabelField(labelRect, label);

    EditorGUI.LabelField(minLabelRect, minLabel, EditorUtil.LabelStyleRightAligned);
    int newMin = EditorGUI.IntField(minRect, min);
    EditorGUI.LabelField(secLabelRect, secLabel, EditorUtil.LabelStyleRightAligned);
    int newSec = EditorGUI.IntField(secRect, sec);
      
    // Calculate the time from the int values
    float intTime = (newMin * 60f) + newSec;
    
    if (property.propertyType == SerializedPropertyType.Float)
    {
      float currentTime = (min * 60f) + sec;
      
      EditorGUI.LabelField(rawLabelRect, rawLabel, EditorUtil.LabelStyleRightAligned);
      float newTime = EditorGUI.FloatField(rawRect, currentTime);

      // NOTE(WSWhitehouse): Check if new time or int time has changed and update the property if so
      if (maths.Abs(newTime - currentTime) >= 0.001) property.floatValue = newTime;
      if (maths.Abs(intTime - currentTime) >= 0.001) property.floatValue = intTime;
    }
    
    EditorGUI.EndProperty();
  }
}
#endif

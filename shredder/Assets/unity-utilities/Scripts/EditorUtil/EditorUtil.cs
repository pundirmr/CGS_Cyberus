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

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPropertyArray(SerializedProperty property)
  {
    string path    = property.propertyPath;
    int arrayIndex = path.LastIndexOf(".Array", StringComparison.Ordinal);
    return arrayIndex >= 0;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPropertyArray(SerializedProperty property, out int arrayIndex)
  {
    string path = property.propertyPath;
    arrayIndex  = path.LastIndexOf(".Array", StringComparison.Ordinal);
    return arrayIndex >= 0;
  }

  /// <summary>
  /// Gets the current target from the Property Drawer
  /// </summary>
  /// <param name="propertyDrawer"></param>
  /// <param name="property"></param>
  /// <param name="propertyIndex"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns>The Target</returns>
  public static T PropertyDrawerGetTarget<T>(PropertyDrawer propertyDrawer, SerializedProperty property, out int propertyIndex)
  {
    // NOTE(WSWhitehouse): We need to do something special if its an array
    if (IsPropertyArray(property))
    {
      string path = property.propertyPath;
      int openBracketIndex  = path.LastIndexOf('[') + 1;
      int closeBracketIndex = path.LastIndexOf(']');

      string indexStr = new string(path.ToCharArray(), openBracketIndex , closeBracketIndex - openBracketIndex);
      propertyIndex   = int.Parse(indexStr);
      
      // HACK(WSWhitehouse): Casting to IEnumerable here as this supports Lists and Arrays. The ToArray() call
      // is used to get the current element at the index specified above. Probably a prettier way of doing this!
      return ((IEnumerable<T>)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject)).ToArray()[propertyIndex];
    }
 
    propertyIndex = -1;
    return (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject);
  }
  
  // NOTE(WSWhitehouse): This is here because I always forget where single line height is stored
  public static float SingleLineHeight => EditorGUIUtility.singleLineHeight;
  public static float HalfLineHeight   => SingleLineHeight * 0.5f;
  
  // NOTE(WSWhitehouse): The width of an indent. This value is taken from an internal constant in EditorGUI
  public const float IndentWidth = 15f;
  
  // NOTE(WSWhitehouse): Gets the current indent from the current indent level
  public static float IndentOffset => EditorGUI.indentLevel * IndentWidth;

  // NOTE(WSWhitehouse): Disables the GUI and returns bool of its previously enabled state. Can be used to return the editor to its previous state
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool DisableGUI()
  {
    bool enabledCache = GUI.enabled;
    GUI.enabled = false;
    return enabledCache;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ResetGUI(bool cache)
  {
    GUI.enabled = cache;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rect GetCurrentRect() => GUILayoutUtility.GetLastRect();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rect Space(Rect rect)
  {
    rect.y += SingleLineHeight * 0.5f;
    return rect;
  }

  /// <summary>
  /// Returns an area of the current editor window with the requested height
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rect GetRectArea(float height)
  {
    Rect lastRect = GUILayoutUtility.GetLastRect();
    return GUILayoutUtility.GetRect(lastRect.width, height);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BeginFoldoutHeaderGroup(Rect rect, ref bool foldout, GUIContent guiContent)
  {
    foldout = EditorGUI.Foldout(rect, foldout, guiContent);
    if (GUI.Button(rect, string.Empty, GUIStyle.none)) foldout = !foldout;
  }
  
  public static GUIStyle LabelStyleBold          => new GUIStyle("boldLabel");
  public static GUIStyle LabelStyleCenterAligned => new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
  public static GUIStyle LabelStyleRightAligned  => new GUIStyle("label") { alignment = TextAnchor.MiddleRight  };

  /// <summary>
  /// Calculates the width of the label.
  /// </summary>
  /// <param name="style">Optional. Leave empty for no style.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateLabelWidth(GUIContent label, GUIStyle style = null)
  {
    // NOTE(WSWhitehouse): Cannot use a null style so setting it to none instead.
    if (style == null) style = GUIStyle.none;
    return style.CalcSize(label).x;
  }

  /// <summary>
  /// Calculates the width of the label.
  /// </summary>
  /// <param name="style">Optional. Leave empty for no style.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateLabelWidth(string label, GUIStyle style = null) => CalculateLabelWidth(new GUIContent(label), style);
}

#endif
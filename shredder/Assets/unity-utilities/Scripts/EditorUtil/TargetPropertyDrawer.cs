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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A small wrapper class around the Property Drawer. Includes a property for the Target (similar to <see cref="UnityEditor.Editor.target"/>)
/// and a Target Index to help with arrays/lists. The Index is set to -1 if the target does not belong to an array or list. Automatically
/// calculates the height of the GUI and is supported across arrays/lists. Override <see cref="OnGUIRender"/> rather than <see cref="OnGUI"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the Target. Should be identical to the type used in the <see cref="UnityEditor.CustomPropertyDrawer"/> attribute
/// used above this and a typical Property Drawer class.
/// </typeparam>
public abstract class TargetPropertyDrawer<T> : PropertyDrawer
{
  /// <summary>
  /// The Target that this Property Drawer is using
  /// </summary>
  protected T Target { get; private set; }
  
  /// <summary>
  /// The index of the Target in an array/list. Equals -1 if the Target does not belong to an array/list.
  /// </summary>
  protected int TargetIndex { get; private set; }
  
  private Dictionary<int, float> _height = new Dictionary<int, float>();

  public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // Set up Target and Index
    {
      Target = EditorUtil.PropertyDrawerGetTarget<T>(this, property, out int index);
      TargetIndex = index;
    }
    
    float startPos = position.y;
    
    // Render user GUI
    OnGUIRender(ref position, property, label);
    
    // Calculate the gui height at the end of the user's GUI and add/update dictionary
    float guiHeight = position.y - startPos;
    if (_height.ContainsKey(TargetIndex))
    {
      _height[TargetIndex] = guiHeight;
    }
    else
    {
      _height.Add(TargetIndex, guiHeight);
    }
  }

  public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
  {
    EditorUtil.PropertyDrawerGetTarget<T>(this, property, out int index);
    return _height.ContainsKey(index) ? _height[index] : 0.0f;
  }
  
  public sealed override bool CanCacheInspectorGUI(SerializedProperty property) => false;
  
  /// <summary>
  /// Override this function to render GUI Code. Acts the same as the <see cref="OnGUI"/> function.
  /// </summary>
  protected abstract void OnGUIRender(ref Rect position, SerializedProperty property, GUIContent label);
}
#endif

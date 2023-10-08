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
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditorInternal;
#endif

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class UnitySceneAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UnitySceneAttribute))]
public class UnityScenePropertyDrawer : PropertyDrawer
{
  private const string NoScene = "<NoScene>";
  
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
    
    List<string> sceneList        = new List<string>(scenes.Length + 1);
    List<string> sceneDisplayName = new List<string>(scenes.Length + 1);
      
    sceneList.Add(NoScene);
    sceneDisplayName.Add(NoScene);
      
    for (int i = 0; i < scenes.Length; i++)
    {
      string sceneName = Path.GetFileNameWithoutExtension(scenes[i].path);
      sceneList.Add(sceneName);
      sceneDisplayName.Add($"{i}: {sceneName}");
    }

    if (property.propertyType == SerializedPropertyType.String)
    {
      string propertyString = property.stringValue;
      int index = 0;
      for (int i = 1; i < sceneList.Count; i++)
      {
        if (sceneList[i] != propertyString) continue;

        index = i; break;
      }
      
      index = EditorGUI.Popup(position, label.text, index, sceneDisplayName.ToArray());
      
      EditorGUI.BeginProperty(position, label, property);
      property.stringValue = index >= 1 ? sceneList[index] : string.Empty;
      EditorGUI.EndProperty();
    }
    else if (property.propertyType == SerializedPropertyType.Integer)
    {
      int index = property.intValue + 1;
      index = EditorGUI.Popup(position, label.text, index, sceneDisplayName.ToArray());
      
      EditorGUI.BeginProperty(position, label, property);
      property.intValue = index >= 1 ? index - 1 : -1;
      EditorGUI.EndProperty();
    }
    else
    {
      EditorGUI.PropertyField(position, property, label);
    }
  }
}
#endif

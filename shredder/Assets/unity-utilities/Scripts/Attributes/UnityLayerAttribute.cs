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

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class UnityLayerAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UnityLayerAttribute))]
public class UnityLayerPropertyDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    if (property.propertyType == SerializedPropertyType.String)
    {
      int layerInt = LayerMask.NameToLayer(property.stringValue);
      
      // NOTE(WSWhitehouse): Set layer to default if it doesnt exist!
      if (layerInt < 0)
      {
        layerInt = 0;
      }
      
      EditorGUI.BeginProperty(position, label, property);
      property.stringValue = LayerMask.LayerToName(EditorGUI.LayerField(position, label, layerInt));
      EditorGUI.EndProperty();
    }
    else if (property.propertyType == SerializedPropertyType.Integer)
    {
      EditorGUI.BeginProperty(position, label, property);
      property.intValue = EditorGUI.LayerField(position, label, property.intValue);
      EditorGUI.EndProperty();
    }
    else
    {
      EditorGUI.PropertyField(position, property, label);
    }
  }
}
#endif
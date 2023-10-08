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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class ConfigurableScrollRect : ScrollRect
{
  [Header("Options")]
  public bool enableDragging = true;

  public override void OnBeginDrag(PointerEventData eventData)
  {
    if (!enableDragging) return;
    base.OnBeginDrag(eventData);
  }

  public override void OnDrag(PointerEventData eventData)
  {
    if (!enableDragging) return;
    base.OnDrag(eventData);
  }

  public override void OnEndDrag(PointerEventData eventData)
  {
    if (!enableDragging) return;
    base.OnEndDrag(eventData);
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConfigurableScrollRect))]
public class ConfigurableScrollRectEditor : ScrollRectEditor
{
  private ConfigurableScrollRect Target => target as ConfigurableScrollRect;
  
  private SerializedProperty _enableDraggingProperty;
  
  protected override void OnEnable()
  {
    base.OnEnable();
    
    _enableDraggingProperty = serializedObject.FindProperty(nameof(Target.enableDragging));
  }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    serializedObject.Update();
    
    EditorGUILayout.PropertyField(_enableDraggingProperty);
    
    serializedObject.ApplyModifiedProperties();
  }
}
#endif
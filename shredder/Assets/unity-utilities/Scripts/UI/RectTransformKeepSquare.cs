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

using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class RectTransformKeepSquare : MonoBehaviour
{
  [SerializeField] private bool2 axisToMatch;
  
  #if UNITY_EDITOR
  [SerializeField] private bool runInEditor = true;
  private void OnValidate() => Update();
  #endif
  
  private RectTransform rectTransform => (RectTransform)transform;

  private void Update()
  {
    #if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      if (!runInEditor) return;
    }
    else
    {
      if (!rectTransform.hasChanged) return;
      rectTransform.hasChanged = false;
    }
    
    #else
    if (!rectTransform.hasChanged) return;
    rectTransform.hasChanged = false;
    #endif

    if (axisToMatch.x)
    {
      rectTransform.SetHeight(rectTransform.GetWidth());
    }

    if (axisToMatch.y)
    {
      rectTransform.SetWidth(rectTransform.GetHeight());
    }
  }
}
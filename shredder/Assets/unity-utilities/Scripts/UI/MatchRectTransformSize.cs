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

/// <summary>
/// Place this MonoBehaviour on a RectTransform object and assign another one into the rectTransform field. When the
/// other transform has changed this will update. The size offset field is added to the size of this rect transform in
/// case this one should be bigger/smaller than the target by an amount.
/// </summary>
public class MatchRectTransformSize : MonoBehaviour
{
  [SerializeField] private RectTransform rectTransform;
  [SerializeField] private Vector2 sizeOffset;
  
  private RectTransform _thisRectTransform => (RectTransform)transform;
  
  private void Update()
  {
    if (!rectTransform.hasChanged) return;
    rectTransform.hasChanged = false;

    _thisRectTransform.SetSize(rectTransform.GetSize() + sizeOffset);
  }
}
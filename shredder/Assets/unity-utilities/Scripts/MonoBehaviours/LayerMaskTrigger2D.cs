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

/* NOTE(WSWhitehouse):
 * This script detects triggers between gameobjects on certain layers,
 * if the object that collided is not included in the layermask the
 * trigger is ignored. It includes events for the first/last collision
 * inside its collision volume as well as an event for every time the
 * trigger is performed.
 */

public class LayerMaskTrigger2D : MonoBehaviour
{
  [SerializeField] private LayerMask layerMask;
  
  // NOTE(WSWhitehouse): Pass through collider2D that triggered the event
  public delegate void LayerMaskTrigger2DEvent(Collider2D collider);
  public LayerMaskTrigger2DEvent OnFirstEnter;
  public LayerMaskTrigger2DEvent OnLastExit;

  public LayerMaskTrigger2DEvent OnAnyEnter;
  public LayerMaskTrigger2DEvent OnAnyExit;

  public int ColliderCount { get; private set; } = 0;
  
  private void OnTriggerEnter2D(Collider2D other)
  {
    // NOTE(WSWhitehouse): If colliders layer isn't included in layerMask then ignore it 
    if (layerMask != (layerMask | (1 << other.gameObject.layer))) return;

    ColliderCount++;
    OnAnyEnter?.Invoke(other);

    if (ColliderCount == 1) // First collider to enter
    {
      OnFirstEnter?.Invoke(other);
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    // NOTE(WSWhitehouse): If colliders layer isn't included in layerMask then ignore it
    if (layerMask != (layerMask | (1 << other.gameObject.layer))) return;

    ColliderCount--;
    OnAnyExit?.Invoke(other);

    if (ColliderCount <= 0) // Last collider to exit
    {
      OnLastExit?.Invoke(other);
    }
  }
}
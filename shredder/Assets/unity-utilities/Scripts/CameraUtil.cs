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

using System.Runtime.CompilerServices;
using UnityEngine;

public static class CameraUtil
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetCameraSize(Camera camera)
  {
    float height = camera.orthographicSize * 2f;
    float width  = height * camera.aspect;
    return new Vector2(width, height);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bounds GetCameraBounds(Camera camera)
  {
    Vector2 size = GetCameraSize(camera);
    return new Bounds(camera.transform.position, new Vector3(size.x, size.y, 0));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void GetCameraCorners(Camera camera, out Vector3 topLeft, out Vector3 bottomLeft, out Vector3 topRight, out Vector3 bottomRight)
  {
    Bounds camBounds = GetCameraBounds(camera);
    Vector3 center   = camBounds.center;
    Vector3 extents  = camBounds.extents;

    topLeft     = new Vector3(center.x + extents.x, center.y - extents.y);
    bottomLeft  = new Vector3(center.x - extents.x, center.y - extents.y);
    topRight    = new Vector3(center.x + extents.x, center.y + extents.y);
    bottomRight = new Vector3(center.x - extents.x, center.y + extents.y);
  }
}
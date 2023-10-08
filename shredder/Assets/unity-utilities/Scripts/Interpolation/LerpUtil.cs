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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Unity.Mathematics;

public static class LerpUtil
{
  // delegates
  public delegate IEnumerator RotCoroutineDel([NotNull] Transform transform, quaternion endRot, float duration, Action onCompleted = null);
  public delegate IEnumerator CanvasAlphaCoroutineDel([NotNull] CanvasGroup canvasGroup, float endAlpha, float duration, Action onCompleted = null);
    
  public static DelegateUtil.LerpTransFloat3Coroutine LerpScale;
  public static DelegateUtil.LerpTransVecCoroutine LerpLocalPosition;
  public static DelegateUtil.LerpTransFloat3Coroutine LerpPosition;
  public static DelegateUtil.LerpTransFloat3Coroutine LerpPositionUnscaled;
  public static RotCoroutineDel LerpRotation;
  public static CanvasAlphaCoroutineDel LerpCanvasGroupAlpha;

  static LerpUtil()
  {
    LerpScale            = __LerpScale;
    LerpLocalPosition    = __LerpLocalPosition;
    LerpPosition         = __LerpPosition;
    LerpPositionUnscaled = __LerpPositionUnscaled;
    LerpRotation         = __LerpRotation;
    LerpCanvasGroupAlpha = __LerpCanvasGroupAlpha;
  }
    
  ////////////////////////////////////////////////////////////////////////////////////
  ///// Lerps
  public static IEnumerator __LerpScale([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null)
  {
    float timeElapsed  = 0.0f;
    float3 startValue = transform.localScale;

    while (timeElapsed < duration)
    {
      transform.localScale = float3Util.Lerp(startValue, endScale, timeElapsed / duration);
      timeElapsed         += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    transform.localScale = endScale;
    onCompleted?.Invoke();
  }
  
  public static IEnumerator __LerpLocalPosition([NotNull] Transform transform, Vector3 endPos, float duration, Action onCompleted = null)
  {
    float timeElapsed  = 0.0f;
    float3 startValue = transform.localPosition;

    while (timeElapsed < duration)
    {
      transform.localPosition = float3Util.Lerp(startValue, endPos, timeElapsed / duration);
      timeElapsed            += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    transform.localPosition = endPos;
    onCompleted?.Invoke();
  }
  
  public static IEnumerator __LerpPosition([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null)
  {
    float timeElapsed  = 0.0f;
    float3 startValue = transform.position;

    while (timeElapsed < duration)
    {
      transform.position = float3Util.Lerp(startValue, endPos, timeElapsed / duration);
      timeElapsed       += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    transform.position = endPos;
    onCompleted?.Invoke();
  }

  public static IEnumerator __LerpPositionUnscaled([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null)
  {
    float timeElapsed = 0.0f;
    float3 startValue = transform.position;

    while (timeElapsed < duration)
    {
      transform.position = float3Util.Lerp(startValue, endPos, timeElapsed / duration);
      timeElapsed       += Time.unscaledDeltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }

    transform.position = endPos;
    onCompleted?.Invoke();
  }

  public static IEnumerator __LerpRotation([NotNull] Transform transform, quaternion endRot, float duration, Action onCompleted = null) 
  {
    float elapsed = 0f;
    quaternion startValue = transform.rotation;

    while (elapsed < duration) 
    {
      transform.rotation = quaternionUtil.Lerp(startValue, endRot, elapsed / duration);
      elapsed           += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }

    transform.rotation = endRot;
    onCompleted?.Invoke();
  }
  
  public static IEnumerator __LerpCanvasGroupAlpha([NotNull] CanvasGroup canvasGroup, float endAlpha, float duration, Action onCompleted = null)
  {
    float timeElapsed  = 0.0f;
    float startValue = canvasGroup.alpha;

    while (timeElapsed < duration)
    {
      canvasGroup.alpha = maths.Lerp(startValue, endAlpha, timeElapsed / duration);
      timeElapsed      += Time.deltaTime;
      yield return CoroutineUtil.WaitForUpdate;
    }

    canvasGroup.alpha = endAlpha;
    onCompleted?.Invoke();
  }
}

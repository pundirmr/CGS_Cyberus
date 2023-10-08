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
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

public static class EaseInUtil {
    /// Constants
    private const float s = 1.70158f;

    /// delegate allocations
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionQuadratic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionCubic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionQuartic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionQuintic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionSinusoidal;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionExponential;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionCircular;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionBack;
    public static DelegateUtil.LerpTransFloat3Coroutine ToPositionElastic;

    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionQuadratic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionCubic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionQuartic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionQuintic;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionSinusoidal;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionExponential;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionCircular;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionBack;
    public static DelegateUtil.LerpTransFloat3Coroutine ToLocalPositionElastic;

    public static DelegateUtil.LerpTransFloat3Coroutine ScaleQuadratic;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleCubic;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleQuartic;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleQuintic;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleSinusoidal;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleExponential;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleCircular;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleBack;
    public static DelegateUtil.LerpTransFloat3Coroutine ScaleElastic;

    static EaseInUtil() {
        ToPositionQuadratic   = __ToPositionQuadratic;
        ToPositionCubic       = __ToPositionCubic;
        ToPositionQuartic     = __ToPositionQuartic;
        ToPositionQuintic     = __ToPositionQuintic;
        ToPositionSinusoidal  = __ToPositionSinusoidal;
        ToPositionExponential = __ToPositionExponential;
        ToPositionCircular    = __ToPositionCircular;
        ToPositionBack        = __ToPositionBack;
        ToPositionElastic     = __ToPositionElastic;

        ToLocalPositionQuadratic   = __ToLocalPositionQuadratic;
        ToLocalPositionCubic       = __ToLocalPositionCubic;
        ToLocalPositionQuartic     = __ToLocalPositionQuartic;
        ToLocalPositionQuintic     = __ToLocalPositionQuintic;
        ToLocalPositionSinusoidal  = __ToLocalPositionSinusoidal;
        ToLocalPositionExponential = __ToLocalPositionExponential;
        ToLocalPositionCircular    = __ToLocalPositionCircular;
        ToLocalPositionBack        = __ToLocalPositionBack;
        ToLocalPositionElastic     = __ToLocalPositionElastic;

        ScaleQuadratic   = __ScaleQuadratic;
        ScaleCubic       = __ScaleCubic;
        ScaleQuartic     = __ScaleQuartic;
        ScaleQuintic     = __ScaleQuintic;
        ScaleSinusoidal  = __ScaleSinusoidal;
        ScaleExponential = __ScaleExponential;
        ScaleCircular    = __ScaleCircular;
        ScaleBack        = __ScaleBack;
        ScaleElastic     = __ScaleElastic;
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Quadratic(float val) => maths.Square(val);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cubic(float val) => maths.Cube(val);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Quartic(float val) => maths.Cube(val) * val;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Quintic(float val) => Quartic(val) * val;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sinusoidal(float val) => 1f - math.cos(val * (maths.Pi * 0.5f));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Exponential(float val) => val <= 0f ? val : maths.Pow2(10f * (val - 1f));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Circular(float val) => 1f - maths.FastSqrt(1f - val * val);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Back(float val) {
        const float c3 = s + 1f;

        return c3 * val * val * val - s * val * val;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Elastic(float val) {
        return 0.5f * math.sin(13f * (maths.Pi * 0.5f) * (2f * val)) * maths.Pow2(10f * ((2f * val) - 1f));
    }

    ////////////////////////////////////////////////////////////////////////////////
    /////////// Position
    private static IEnumerator __ToPositionQuadratic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Quadratic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToPositionCubic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Cubic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToPositionQuartic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Quartic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToPositionQuintic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Quintic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToPositionSinusoidal([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Sinusoidal(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToPositionExponential([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Exponential(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToPositionCircular([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Circular(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToPositionBack([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Back(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToPositionElastic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.position;
        while (elapsed < duration) {
            transform.position = float3Util.Lerp(start, endPos, Elastic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.position = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToLocalPositionQuadratic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Quadratic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToLocalPositionCubic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Cubic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToLocalPositionQuartic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Quartic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToLocalPositionQuintic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Quintic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToLocalPositionSinusoidal([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Sinusoidal(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToLocalPositionExponential([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Exponential(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ToLocalPositionCircular([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Circular(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToLocalPositionBack([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Back(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ToLocalPositionElastic([NotNull] Transform transform, float3 endPos, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localPosition;
        while (elapsed < duration) {
            transform.localPosition = float3Util.Lerp(start, endPos, Elastic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localPosition = endPos;
        onCompleted?.Invoke();
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    /////////// Scale
    private static IEnumerator __ScaleQuadratic([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Quadratic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ScaleCubic([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Cubic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ScaleQuartic([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Quartic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ScaleQuintic([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Quintic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ScaleSinusoidal([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Sinusoidal(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ScaleExponential([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Exponential(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
    
    private static IEnumerator __ScaleCircular([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Circular(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ScaleBack([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Back(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }

    private static IEnumerator __ScaleElastic([NotNull] Transform transform, float3 endScale, float duration, Action onCompleted = null) {
        float elapsed = 0.0f;
        float3 start  = transform.localScale;
        while (elapsed < duration) {
            transform.localScale = float3Util.Lerp(start, endScale, Elastic(elapsed / duration));
            elapsed           += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        transform.localScale = endScale;
        onCompleted?.Invoke();
    }
}

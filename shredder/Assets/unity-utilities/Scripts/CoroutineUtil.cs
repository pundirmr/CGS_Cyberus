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

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class CoroutineUtil
{
  static CoroutineUtil()
  {
    WaitForFixedUpdate = new WaitForFixedUpdate();
    WaitForEndOfFrame  = new WaitForEndOfFrame();
    WaitForUpdate      = null;

    // delegate function pointer setup
    Wait         = __Wait;
    WaitUnscaled = __WaitUnscaled;

    WaitForMultipleEndOfFrames = __WaitForMultipleEndOfFrames;
    WaitForMultipleUpdates     = __WaitForMultipleUpdates;
  }
  
  // Yield Instructions
  public static readonly WaitForFixedUpdate WaitForFixedUpdate;
  public static readonly WaitForEndOfFrame  WaitForEndOfFrame;
  public static readonly YieldInstruction   WaitForUpdate;

  // Pre-Allocation of Coroutine Functions
  public delegate IEnumerator WaitDurationCoroutine(float duration);
  public static readonly WaitDurationCoroutine Wait;
  public static readonly WaitDurationCoroutine WaitUnscaled;

  public delegate IEnumerator WaitForAmountCoroutine(int numOfFrames);
  public static readonly WaitForAmountCoroutine WaitForMultipleEndOfFrames;
  public static readonly WaitForAmountCoroutine WaitForMultipleUpdates;
    
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void StartSafelyWithRef(MonoBehaviour mono, ref Coroutine reference, IEnumerator routine)
  {
    if (reference != null)
    {
      mono.StopCoroutine(reference);
      reference = null;
    }

    reference = mono.StartCoroutine(routine);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void StopSafelyWithRef(MonoBehaviour mono, ref Coroutine reference) 
  {
    if (reference == null) return;
    mono.StopCoroutine(reference);
    reference = null;
  }

  private static IEnumerator __Wait(float duration)
  {
    float timer = 0.0f;
    while (true)
    {
      timer += Time.deltaTime;
      if (timer >= duration) break;
      
      yield return WaitForUpdate;
    }
  }
  
  private static IEnumerator __WaitUnscaled(float duration)
  {
    float timer = 0.0f;
    while (true)
    {
      timer += Time.unscaledDeltaTime;
      if (timer >= duration) break;
      
      yield return WaitForUpdate;
    }
  }

  private static IEnumerator __WaitForMultipleEndOfFrames(int frames)
  {
    for (int i = 0; i < frames; i++)
    {
      yield return WaitForEndOfFrame;
    }
  }
  
  private static IEnumerator __WaitForMultipleUpdates(int frames)
  {
    for (int i = 0; i < frames; i++)
    {
      yield return WaitForUpdate;
    }
  }
}

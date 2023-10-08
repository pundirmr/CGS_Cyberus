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
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Unity.Mathematics;

public static class DelegateUtil
{
  // Common Delegates
  public delegate void EmptyEventDel();
  public delegate IEnumerator EmptyCoroutineDel();
  public delegate IEnumerator LerpTransVecCoroutine([NotNull] Transform transform, Vector3 vec, float duration, Action onCompleted = null);
  public delegate IEnumerator LerpTransFloat3Coroutine([NotNull] Transform transform, float3 vec, float duration, Action onCompleted = null);
  
  /// <summary>
  /// Invoke the action with a try catch. Logs exceptions to console.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void TryCatchInvoke(this Action del)
  {
    try                 { del.Invoke(); }
    catch (Exception e) { Log.Error($"{e.Message}\n{e.StackTrace}"); }
  }
  
  /// <summary>
  /// Invoke the action with a try catch. Logs exceptions to console.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void TryCatchInvoke<T>(this Action<T> del, T arg)
  {
    try                 { del.Invoke(arg); }
    catch (Exception e) { Log.Error($"{e.Message}\n{e.StackTrace}"); }
  }
}

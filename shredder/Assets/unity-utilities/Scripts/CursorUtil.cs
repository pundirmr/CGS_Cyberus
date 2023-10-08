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

public static class CursorUtil
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    Lock();
  }

  /// <summary>
  /// Unlock and show the cursor
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Unlock()
  {
#if UNITY_EDITOR
    Log.Warning("Unlock cursor has been called, but this is not available in the editor!");
    return;
#endif
    
    Cursor.visible   = true;
    Cursor.lockState = CursorLockMode.None;
  }

  /// <summary>
  /// Lock and hide the cursor
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Lock()
  {
#if UNITY_EDITOR
    Log.Warning("Lock cursor has been called, but this is not available in the editor!");
    return;
#endif
    
    Cursor.visible   = false;
    Cursor.lockState = CursorLockMode.Locked;
  }

  /// <summary>
  /// Sets the cursor lock through a boolean.
  /// </summary>
  /// <param name="lock">Should the cursor be locked</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetLock(bool @lock)
  {
    if (@lock) { Lock();   }
    else       { Unlock(); }
  }
}
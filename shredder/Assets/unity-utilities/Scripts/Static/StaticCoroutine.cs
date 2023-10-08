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
using UnityEngine;

/* NOTE(WSWhitehouse):
 * This class allows coroutines to be started/stopped in a static context.
 * It creates a GameObject in the scene and adds a MonoBehaviour on that cannot
 * be accessed outside this class. This is done automatically when calling a
 * coroutine function - there is no setup or initialisation required beforehand.
 */

public static class StaticCoroutine
{
  private class StaticCoroutineBehaviour : MonoBehaviour
  {
    public static StaticCoroutineBehaviour Instance { get; set; } = null;

    private void OnDestroy()
    {
      if (Instance != this) return;

      StopAllCoroutines();
      Instance = null;
    }
  }

  static StaticCoroutine()
  {
    GameObject obj = new GameObject("StaticCoroutine");
    StaticCoroutineBehaviour.Instance = obj.AddComponent<StaticCoroutineBehaviour>();
    Object.DontDestroyOnLoad(obj);
  }
  
  // Get access to the mono behind the Static Coroutine, can be used inside CoroutineUtil functions
  public static MonoBehaviour Mono => StaticCoroutineBehaviour.Instance;

  public static Coroutine Start(IEnumerator coroutine)
  {
    if (StaticCoroutineBehaviour.Instance == null) return null;
    return StaticCoroutineBehaviour.Instance.StartCoroutine(coroutine);
  }

  public static void Stop(IEnumerator coroutine)
  {
    if (StaticCoroutineBehaviour.Instance == null) return;
    StaticCoroutineBehaviour.Instance.StopCoroutine(coroutine);
  }
  
  public static void Stop(Coroutine coroutine)
  {
    if (StaticCoroutineBehaviour.Instance == null) return;
    StaticCoroutineBehaviour.Instance.StopCoroutine(coroutine);
  }
}
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
using UnityEngine.SceneManagement;

public static class SceneHandler
{
  public static bool IsLoading => _asyncLoad != null;
  public static bool ActivatingSceneWhenReady => IsLoading && _asyncLoad.allowSceneActivation;
  public static int SceneIndex { get; private set; }
  public static int PreviousSceneIndex { get; private set; } = -1;

  public delegate void DefaultSceneEvent();
  public static DefaultSceneEvent OnSceneLoadingStarted;
  public static DefaultSceneEvent OnSceneLoadingEnded;
  public static DefaultSceneEvent OnSceneActivated;

  public delegate void SceneProgressEvent(float progress);
  public static SceneProgressEvent OnSceneLoadingProgress;

  private delegate IEnumerator LoadSceneAdditiveDel(float delay);
  private static LoadSceneAdditiveDel LoadSceneAdditiveCoroutine;
  private static DelegateUtil.EmptyCoroutineDel LoadSceneCoroutine;

  private static AsyncOperation _asyncLoad = null;
  private const float sceneCleanupDelay    = 2f;

  static SceneHandler()
  {
    SceneIndex = SceneManager.GetActiveScene().buildIndex;

    // delegate allocations
    LoadSceneCoroutine         = __LoadSceneCoroutine;
    LoadSceneAdditiveCoroutine = __LoadSceneAdditiveCoroutine;
  }

  public static void LoadScene(int sceneIndex, bool activateSceneWhenReady = true)
  {
    Debug.Assert(IsSceneIndexValid(sceneIndex), "Trying to load scene with invalid index!");
    Debug.Assert(!IsLoading, "Already loading a scene!");

    PreviousSceneIndex = SceneIndex;
    SceneIndex = sceneIndex;
    _asyncLoad = SceneManager.LoadSceneAsync(SceneIndex, LoadSceneMode.Single);
    _asyncLoad.allowSceneActivation = activateSceneWhenReady;
    
    StaticCoroutine.Start(LoadSceneCoroutine());
  }

  public static void LoadSceneAdditive(int sceneIndex, float cleanupDelay = sceneCleanupDelay, bool activateSceneWhenReady = true)
  {
    Debug.Assert(IsSceneIndexValid(sceneIndex), "Trying to load scene with invalid index!");
    Debug.Assert(!IsLoading, "Already loading a scene!");

    PreviousSceneIndex = SceneIndex;
    SceneIndex    = sceneIndex;
    _asyncLoad    = SceneManager.LoadSceneAsync(SceneIndex, LoadSceneMode.Additive);
    _asyncLoad.allowSceneActivation = activateSceneWhenReady;
    
    StaticCoroutine.Start(LoadSceneAdditiveCoroutine(cleanupDelay));
  }

  public static void ActivateScene()
  {
    if (!IsLoading) return;
    _asyncLoad.allowSceneActivation = true;
  }

  private static IEnumerator __LoadSceneCoroutine()
  {
    OnSceneLoadingStarted?.Invoke();

    while (_asyncLoad.progress <= 0.9f)
    {
      OnSceneLoadingProgress?.Invoke(_asyncLoad.progress);
      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    OnSceneLoadingEnded?.Invoke();

    // NOTE(WSWhitehouse): Wait here until the scene is activated
    while (!_asyncLoad.isDone) yield return CoroutineUtil.WaitForFixedUpdate;

    _asyncLoad = null;
    OnSceneActivated?.Invoke();
  }

  private static IEnumerator __LoadSceneAdditiveCoroutine(float delay)
  {
    OnSceneLoadingStarted?.Invoke();

    while (_asyncLoad.progress <= 0.9f)
    {
      OnSceneLoadingProgress?.Invoke(_asyncLoad.progress);
      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    OnSceneLoadingEnded?.Invoke();

    // NOTE(WSWhitehouse): Wait here until the scene is activated
    while (!_asyncLoad.isDone) yield return CoroutineUtil.WaitForFixedUpdate;
    
    _asyncLoad = null;
    OnSceneActivated?.Invoke();
    
    yield return CoroutineUtil.Wait(delay);
    SceneManager.UnloadSceneAsync(PreviousSceneIndex);
  }
    
  private static bool IsSceneIndexValid(int sceneIndex)
  {
    return sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings;
  }
}

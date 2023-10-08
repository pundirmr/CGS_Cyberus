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

public static class StaticUpdate
{
  private class StaticUpdateBehaviour : MonoBehaviour
  {
    public static StaticUpdateBehaviour Instance { get; set; } = null;

    private void OnDestroy()
    {
      if (Instance != this) return;
      Instance = null;
    }

    private void Update()
    {
      _onUpdate?.Invoke();
    }
  }
  
  static StaticUpdate()
  {
    GameObject obj = new GameObject("StaticUpdate");
    StaticUpdateBehaviour.Instance = obj.AddComponent<StaticUpdateBehaviour>();
    UnityEngine.Object.DontDestroyOnLoad(obj);
  }

  public delegate void UpdateDelegate();
  private static event UpdateDelegate _onUpdate;
  public static event UpdateDelegate OnUpdate
  {
    add    => _onUpdate += value;
    remove => _onUpdate -= value;
  }
}

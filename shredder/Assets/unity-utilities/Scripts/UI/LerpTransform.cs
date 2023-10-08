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
using Unity.Mathematics;
using UnityEngine;

public class LerpTransform : MonoBehaviour {
    [Header("Position References")]
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform endTransform;
    [SerializeField] private bool setToStartPosOnAwake = true;
    [SerializeField] private bool useGlobalPosition    = false;

    [Header("Lerp Settings")]
    [SerializeField] private float duration = 1f;

    private delegate IEnumerator LerpDel(float3 s, float3 e, float duration);
    private LerpDel LerpFunc;

    public delegate void LerpEvent();
    public LerpEvent OnLerpStarted;
    public LerpEvent OnLerpFinished;
    
    private Coroutine lerpCo;
    private float3 start;       
    private float3 end;       
    
    private void Awake() {
        Debug.Assert(startTransform != null, "Start position has not been set in the inspector", this);
        Debug.Assert(endTransform   != null, "End position has not been set in the inspector",   this);
        
        if (useGlobalPosition) {
            LerpFunc = LerpGlobal;
            start    = startTransform.position;
            end      = endTransform.position;
        } else {
            LerpFunc = LerpLocal;
            start    = startTransform.localPosition;
            end      = endTransform.localPosition;
        }
        
        if (!setToStartPosOnAwake) return;
        if (useGlobalPosition) {
            this.transform.position = start;
        } else {
            this.transform.localPosition = start;
        }
    }

    public void LerpToEnd() {
        CoroutineUtil.StartSafelyWithRef(this, ref lerpCo, LerpFunc(start, end, duration));
    }

    public void LerpToStart() {
        CoroutineUtil.StartSafelyWithRef(this, ref lerpCo, LerpFunc(end, start, duration));
    }

    public void StopLerp() {
        CoroutineUtil.StopSafelyWithRef(this, ref lerpCo);
    }

    private IEnumerator LerpLocal(float3 s, float3 destination, float duration) {
        this.transform.localPosition = s;
        OnLerpStarted?.Invoke();
        yield return LerpUtil.LerpLocalPosition(this.transform, destination, duration);
        OnLerpFinished?.Invoke();
    }

    private IEnumerator LerpGlobal(float3 s, float3 e, float duration) {
        this.transform.position = s;
        OnLerpStarted?.Invoke();
        yield return LerpUtil.LerpLocalPosition(this.transform, e, duration);
        OnLerpFinished?.Invoke();
    }
}

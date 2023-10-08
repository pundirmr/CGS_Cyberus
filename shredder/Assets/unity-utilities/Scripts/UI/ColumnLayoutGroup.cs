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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

[ExecuteAlways]
public class ColumnLayoutGroup : MonoBehaviour {
    [Serializable]
    internal struct Padding {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    [Header("Layout Settings")]
    [SerializeField] private RectTransform parent; // NOTE(Zack): this is what's used to calculate the adjustment of children
    [SerializeField] private Padding padding;
    [SerializeField] private float spacing = 50;
    [SerializeField] private bool effectInactiveObjects = false;

    [Header("Runtime Settings")]
    [SerializeField] private bool calculateOnStart = false;

    public delegate void SetupEvent();
    public SetupEvent OnUISetupFinished;

#if UNITY_EDITOR
    [Header("Debug Only Settings")]
    [SerializeField, Tooltip("Force calculation of UI layout")] private bool calculate = false;
    [SerializeField, Tooltip("USE WITH CAUTION, will run every 'Frame' in editor")] private bool autoCalc = false;
    
    private void Update() {
        if (autoCalc) calculate = true;
        if (!calculate) return;
        calculate = false;
        CalculateUILayout();
    }

    private void OnValidate() => CalculateUILayout();
#endif

    
    private void Start() {
        if (!calculateOnStart) return;
        CalculateUILayout();
    }
    
    public void CalculateUILayout() {
        Debug.Assert(parent != null, "You have not set the parent object in which to base calculations off of in the inspector.", this);

        // we force canvases to be updated before any calculations happen
        Canvas.ForceUpdateCanvases();
        
        // if we have no children we return
        int maxCount = this.transform.childCount;
        if (maxCount == 0) return;

        // NOTE(Zack): we default to checking that the children are active in the hierarchy
        int count = 0;
        List<RectTransform> valid = new (maxCount);
        for (int i = 0; i < maxCount; ++i) {
            var child = transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy && !effectInactiveObjects) continue;
            valid.Add((RectTransform)child);
            count += 1;
        }
        
        // if we have no active children we return
        if (count == 0) return;

        // Child Size Setup (UNSCALED)
        float pw            = parent.GetWidth();
        float ph            = parent.GetHeight();
        float total_spacing = spacing * (count - 1);
        float base_width    = (pw / (float)count) - (padding.left / count) - (padding.right / count) - (total_spacing / count);
        float base_height   = ph - padding.top - padding.bottom;
        for (int i = 0; i < count; ++i) {
            valid[i].SetWidth(base_width);
            valid[i].SetHeight(base_height);
        }

        // Child Position Setup (SCALED)
        float scaled_width  = valid[0].GetScaledWidth();
        float scaled_height = valid[0].GetScaledHeight();
        float scale_x       = parent.lossyScale.x;
        float scale_y       = parent.lossyScale.y;
        
        float x = (scaled_width * 0.5f)  + (padding.left   * scale_x);
        float y = (scaled_height * 0.5f) + (padding.bottom * scale_y);
        
        float3 start = new float3(x, y, 0f);
        for (int i = 0; i < count; ++i) {
            valid[i].position = start;
            start.x += scaled_width + (spacing * scale_x);
        }

        OnUISetupFinished?.Invoke();
    }
}

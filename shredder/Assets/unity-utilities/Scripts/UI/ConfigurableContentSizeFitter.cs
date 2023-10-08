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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))] [ExecuteAlways]
public class ConfigurableContentSizeFitter : UIBehaviour, ILayoutSelfController
{
  [SerializeField] private ContentSizeFitter.FitMode m_HorizontalFit = ContentSizeFitter.FitMode.Unconstrained;
  
  public ContentSizeFitter.FitMode horizontalFit
  {
    get => m_HorizontalFit;
    set
    {
      if (SetStruct(ref m_HorizontalFit, value)) SetDirty();
    }
  }

  [SerializeField] private ContentSizeFitter.FitMode m_VerticalFit = ContentSizeFitter.FitMode.Unconstrained;
  
  public ContentSizeFitter.FitMode verticalFit
  {
    get => m_VerticalFit;
    set
    {
      if (SetStruct(ref m_VerticalFit, value)) SetDirty();
    }
  }
  
  [SerializeField] private Vector2 maxSize = Vector2.zero;
  [SerializeField] private Vector2 minSize = Vector2.zero;

  [System.NonSerialized] private RectTransform m_Rect;

  private RectTransform rectTransform
  {
    get
    {
      if (m_Rect == null) m_Rect = GetComponent<RectTransform>();
      return m_Rect;
    }
  }

  // field is never assigned warning
#pragma warning disable 649
  private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

  protected override void OnEnable()
  {
    base.OnEnable();
    SetDirty();
  }

  protected override void OnDisable()
  {
    m_Tracker.Clear();
    LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    base.OnDisable();
  }

  protected override void OnRectTransformDimensionsChange()
  {
    SetDirty();
  }

  private void HandleSelfFittingAlongAxis(int axis)
  {
    ContentSizeFitter.FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
    if (fitting == ContentSizeFitter.FitMode.Unconstrained)
    {
      // Keep a reference to the tracked transform, but don't control its properties:
      m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
      return;
    }

    m_Tracker.Add(this, rectTransform,
      (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));
    
    float min = axis == 0 ? minSize.x : minSize.y;
    float max = axis == 0 ? maxSize.x : maxSize.y;
    if (max <= 0) max = float.MaxValue; // NOTE(WSWhitehouse): If max is 0 then ignore it by setting it to the max value
    
    float size = fitting == ContentSizeFitter.FitMode.MinSize ? LayoutUtility.GetMinSize(m_Rect, axis) : LayoutUtility.GetPreferredSize(m_Rect, axis);
    
    // NOTE(WSWhitehouse): Lock size to min and max
    if (size < min) size = min;
    if (size > max) size = max;
    
    rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
  }
  
  public virtual void SetLayoutHorizontal()
  {
    m_Tracker.Clear();
    HandleSelfFittingAlongAxis(0);
  }
  
  public virtual void SetLayoutVertical()
  {
    HandleSelfFittingAlongAxis(1);
  }

  private void SetDirty()
  {
    if (!IsActive())
      return;

    LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
  }
  
  private static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
  {
    if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
      return false;
 
    currentValue = newValue;
    return true;
  }

#if UNITY_EDITOR
  protected override void OnValidate()
  {
    SetDirty();
  }
#endif
}

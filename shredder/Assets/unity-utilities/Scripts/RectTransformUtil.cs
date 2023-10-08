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
using System.Runtime.CompilerServices;
using UnityEngine;

public static class RectTransformUtil
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetPositionFromWorldTransform(RectTransform uiElement, RectTransform parent, Camera camera, Transform worldTransform) => 
                                                                SetPositionFromWorldTransform(uiElement, parent, camera, worldTransform.position);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetPositionFromWorldTransform(RectTransform uiElement, RectTransform parent, Camera camera, Vector3 worldPosition)
  {
    // https://forum.unity.com/threads/world-space-to-canvas-space.460185/
    
    Vector2 viewportPos = camera.WorldToViewportPoint(worldPosition);
    Vector2 screenPos   = new Vector2(
      ((viewportPos.x * parent.sizeDelta.x) - (parent.sizeDelta.x * 0.5f)),
      ((viewportPos.y * parent.sizeDelta.y) - (parent.sizeDelta.y * 0.5f)));
    
    uiElement.anchoredPosition = screenPos;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetPositionFromWorldTransform(RectTransform parent, Camera camera, Vector3 worldPosition)
  {
    // https://forum.unity.com/threads/world-space-to-canvas-space.460185/
    
    Vector2 viewportPos = camera.WorldToViewportPoint(worldPosition);
    Vector2 screenPos   = new Vector2(
                                      ((viewportPos.x * parent.sizeDelta.x) - (parent.sizeDelta.x * 0.5f)),
                                      ((viewportPos.y * parent.sizeDelta.y) - (parent.sizeDelta.y * 0.5f)));
    
    return screenPos;
  }

  // --- EXTENSION METHODS --- //

  [Flags]
  public enum Anchor
  {
    NONE    = 0,
    MIDDLE  = 1 << 0, // 1
    TOP     = 1 << 1, // 2
    BOTTOM  = 1 << 2, // 4
    LEFT    = 1 << 3, // 8
    RIGHT   = 1 << 4, // 16
  }
  
  // NOTE(WSWhitehouse): These are used inside the SetPivotAndAnchors function below. They are used to loop
  // through all possible values of the enum flag and set the appropriate anchor position on the rect transform.
  private static readonly Anchor[] AnchorEnumValues = Enum.GetValues(typeof(Anchor)) as Anchor[];
  private static int AnchorEnumCount => AnchorEnumValues.Length;
  
  /// <summary>
  /// Sets the pivot and anchor of the RectTransform based on the provided anchor flags. The enum flags can be combined
  /// to set the anchor position to the side or corner of the transform. Example:
  /// <code>
  /// SetPivotAndAnchors(transform, Anchor.TOP | Anchor.LEFT);   // Will set anchor to top left corner
  /// SetPivotAndAnchors(transform, Anchor.TOP | Anchor.MIDDLE); // Will set anchor to top middle side
  ///                                                            // etc...
  /// </code>
  /// </summary>
  /// <param name="anchor">The flags used to calculate the transforms anchor position</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetPivotAndAnchors(this RectTransform trans, Anchor anchor)
  {
    // NOTE(WSWhitehouse): NONE is 0 on the enum, which means there are no other flags set, if 
    // this is the case then return out the function as there is no need to calculate anything.
    // The anchor position will stays the same.
    if (anchor == Anchor.NONE) return;
    
    // NOTE(WSWhitehouse): Setting the values to what they already were, just in case the flags that were
    // passed in only effect one element of the vector.
    float anchorX = trans.pivot.x;
    float anchorY = trans.pivot.y;

    for (int i = 0; i < AnchorEnumCount; i++)
    {
      Anchor flag = AnchorEnumValues[i];

      // Check if anchor has this flag, if not continue
      if ((anchor & flag) == 0) continue;

      switch (flag)
      {
        // NOTE(WSWhitehouse): The middle enum value is at the very beginning of the anchor enum
        // therefore it will be run first in this loop. So if there are other flags set, they will 
        // overwrite the change that the middle case set.
        case Anchor.MIDDLE:
        {
          anchorX = 0.5f;
          anchorY = 0.5f;
          break;
        }
        
        case Anchor.TOP:
        {
          anchorY = 1.0f;
          break;
        }
        
        case Anchor.BOTTOM:
        {
          anchorY = 0.0f;
          break;
        }
        
        case Anchor.LEFT:
        {
          anchorX = 0.0f;
          break;
        }
        
        case Anchor.RIGHT:
        {
          anchorX = 1.0f;
          break;
        }
        
        default: throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
      }
    }
    
    SetPivotAndAnchors(trans, new Vector2(anchorX, anchorY));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetPivotAndAnchors(this RectTransform trans, Vector2 vec2)
  {
    trans.pivot     = vec2;
    trans.anchorMin = vec2;
    trans.anchorMax = vec2;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Overlaps(this RectTransform a, RectTransform b) => a.GetWorldRect().Overlaps(b.GetWorldRect());
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse) => a.GetWorldRect().Overlaps(b.GetWorldRect(), allowInverse);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetWidth(this RectTransform trans) => trans.rect.width;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetHeight(this RectTransform trans) => trans.rect.height;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetSize(this RectTransform trans) => trans.rect.size;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetScaledWidth(this RectTransform trans) => trans.rect.width * trans.lossyScale.x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetScaledHeight(this RectTransform trans) => trans.rect.height * trans.lossyScale.y;
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetScaledSize(this RectTransform trans) => new Vector2(GetScaledWidth(trans), GetScaledHeight(trans));
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos)
  {
    trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos)
  {
    trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width),
      newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos)
  {
    trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width),
      newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos)
  {
    trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width),
      newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos)
  {
    trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width),
      newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetSize(this RectTransform trans, Vector2 newSize)
  {
    Vector2 oldSize   = trans.rect.size;
    Vector2 deltaSize = newSize - oldSize;
    trans.offsetMin  -= new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
    trans.offsetMax  += new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetWidth(this RectTransform trans, float newSize)
  {
    SetSize(trans, new Vector2(newSize, trans.rect.size.y));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetHeight(this RectTransform trans, float newSize)
  {
    SetSize(trans, new Vector2(trans.rect.size.x, newSize));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MoveToWorldSpacePosition(this RectTransform trans, Canvas parentCanvas, Vector3 worldPos)
  {
    //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
    Vector3 screenPos = parentCanvas.worldCamera.WorldToScreenPoint(worldPos);

    //Convert the screenpoint to ui rectangle local point
    RectTransformUtility.ScreenPointToLocalPointInRectangle(trans, screenPos, parentCanvas.worldCamera, out Vector2 movePos);
    
    //Convert the local point to world point
    trans.position = trans.TransformPoint(movePos);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rect GetWorldRect(this RectTransform trans)
  {
    // Calculate Size
    Vector2 sizeDelta = trans.sizeDelta;
    float width  = sizeDelta.x * trans.lossyScale.x;
    float height = sizeDelta.y * trans.lossyScale.y;

    // Calculate Position
    Vector3 position = trans.TransformPoint(trans.rect.center);
    float x = position.x - width  * 0.5f;
    float y = position.y - height * 0.5f;
            
    return new Rect(x, y, width, height);
  }
}

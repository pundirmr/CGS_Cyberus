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
using UnityEngine;

public enum RangedTypeDisplayType
{
  LockedRanges,
  UnlockedRanges
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class RangedTypeAttribute : PropertyAttribute
{
  // NOTE(WSWhitehouse): We're using float for the min and max even though this attribute can be used on an integer type.
  // This is so its compatible with most ranged types. As we can round floating point numbers to integers internally.
  public RangedTypeAttribute(float min, float max, RangedTypeDisplayType displayType = RangedTypeDisplayType.UnlockedRanges)
  {
    this.min = min;
    this.max = max;
    this.displayType = displayType;
  }

  public float min;
  public float max;
  public RangedTypeDisplayType displayType;
}
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
using Unity.Mathematics;

[Serializable]
public struct RangedFloat2
{
  // NOTE(WSWhitehouse): This is the default constructor, the unused variable is here so we dont get an error
  // as structs cant have explicit parameterless constructors (fucking why C#??)
  public RangedFloat2(bool unused = true)
  {
    x = new RangedFloat(0.0f, 0.0f);
    y = new RangedFloat(0.0f, 0.0f);
  }
  
  public RangedFloat2(float2 minValue, float2 maxValue)
  {
    x = new RangedFloat(minValue.x, maxValue.x);
    y = new RangedFloat(minValue.y, maxValue.y);
  }

  public RangedFloat x;
  public RangedFloat y;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool InRange(float2 val)
  {
    return (val.x >= x.minValue && val.x <= x.minValue) && 
           (val.y >= y.minValue && val.y <= y.maxValue);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float2 Random()
  {
    float xVal = UnityEngine.Random.Range(x.minValue, x.maxValue);
    float yVal = UnityEngine.Random.Range(y.minValue, y.maxValue);
    return new float2(xVal, yVal);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float2 Clamp(float2 val) => float2Util.Clamp(val, new float2(x.minValue, y.minValue), new float2(x.maxValue, y.maxValue));
  
  public override string ToString() => $"X: ({x.ToString()}), Y: ({y.ToString()})";
}

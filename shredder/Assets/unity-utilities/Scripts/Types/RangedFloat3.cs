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
public struct RangedFloat3
{
  // NOTE(WSWhitehouse): This is the default constructor, the unused variable is here so we dont get an error
  // as structs cant have explicit parameterless constructors (fucking why C#??)
  public RangedFloat3(bool unused = true)
  {
    x = new RangedFloat(0.0f, 0.0f);
    y = new RangedFloat(0.0f, 0.0f);
    z = new RangedFloat(0.0f, 0.0f);
  }
  
  public RangedFloat3(float3 minValue, float3 maxValue)
  {
    x = new RangedFloat(minValue.x, maxValue.x);
    y = new RangedFloat(minValue.y, maxValue.y);
    z = new RangedFloat(minValue.z, maxValue.z);
  }

  public RangedFloat x;
  public RangedFloat y;
  public RangedFloat z;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool InRange(float3 val)
  {
    return (val.x >= x.minValue && val.x <= x.minValue) && 
           (val.y >= y.minValue && val.y <= y.maxValue) &&
           (val.z >= z.minValue && val.z <= z.maxValue);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float3 Random()
  {
    float xVal = UnityEngine.Random.Range(x.minValue, x.maxValue);
    float yVal = UnityEngine.Random.Range(y.minValue, y.maxValue);
    float zVal = UnityEngine.Random.Range(z.minValue, z.maxValue);
    return new float3(xVal, yVal, zVal);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float3 Clamp(float3 val) => float3Util.Clamp(val, new float3(x.minValue, y.minValue, z.minValue), 
                                                           new float3(x.maxValue, y.maxValue, z.maxValue));
  
  public override string ToString() => $"X: ({x.ToString()}), Y: ({y.ToString()})";
}
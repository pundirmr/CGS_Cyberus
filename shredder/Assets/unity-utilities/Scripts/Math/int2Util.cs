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

using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

public static class int2Util {
  // ------ Constants ------ //
  public static readonly int2 zero = new int2(0, 0);
  public static readonly int2 one  = new int2(1, 1);
  
  // ----- Util ----- //
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int2 Clamp(int2 val, int2 min, int2 max) {
    int x = maths.Clamp(val.x, min.x, max.x);
    int y = maths.Clamp(val.y, min.y, max.y);
    return new int2(x, y);
  }
  
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int2 Clamp(int2 val, int min, int max) {
    int x = maths.Clamp(val.x, min, max);
    int y = maths.Clamp(val.y, min, max);
    return new int2(x, y);
  }
}
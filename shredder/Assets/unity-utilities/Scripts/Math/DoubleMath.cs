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

public static partial class maths {
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Abs(double val) => math.asdouble(math.asulong(val) & 0x7fffffffffffffff);
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double a, double b) {
        double minus = a - b; 
        long abShift = math.aslong(minus) >> 63;
        ulong result = (ulong)((math.aslong(a) & abShift) | (math.aslong(b) & ~abShift));
        return math.asdouble(result);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double a, double b) {
        double minus = a - b;
        long abShift = math.aslong(minus) >> 63;
        ulong result = (ulong)((math.aslong(a) & ~abShift) | (math.aslong(b) & abShift));
        return math.asdouble(result);
    }
  
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(double val) => (val >= 0) ? (double)((long)(val + 0.5f)) : (double)((long)(val - 0.5f));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Floor(double val) {
        double r = val % 1.0;
        if (r >= 0) return val - r;
        return val - (1 + r);
    }
  
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Ceil(double val) => -Floor(-val);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp(double val, double min, double max) => Max(min, Min(max, val));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp01(double val) => Max(0f, Min(1f, val));
  
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoubleCompare(double a, double b, double epsilon = Epsilon) {
        return Abs(a - b) < epsilon;
    }
}

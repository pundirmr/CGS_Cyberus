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
using Unity.Mathematics;
using Unity.Burst;


// TODO(Zack): add more useful functions
// REVIEW(Zack): do we need to cast stuff to 64 so much??
public static partial class random {
    private static uint[] __rngState = { 0, 3579545447, 340436397, 842436295 };
    private const int intBitMask = 0x7FFFFFFF;

    private static uint S0 { get => __rngState[0]; set => __rngState[0] = value; }
    private static uint S1 { get => __rngState[1]; set => __rngState[1] = value; }
    private static uint S2 { get => __rngState[2]; set => __rngState[2] = value; }
    private static uint S3 { get => __rngState[3]; set => __rngState[3] = value; }

    // division constants
    private const double maxIntDivInclusive = 1.0 / ((double)int.MaxValue);
    private const double maxIntDivExclusive = 1.0 / ((double)int.MaxValue + 1.0);
    private const double maxUintDivInclusive = 1.0 / ((double)uint.MaxValue);
    private const double maxUintDivExclusive = 1.0 / ((double)uint.MaxValue + 1);
        
    static random() {
        S0 = (uint)(DateTime.Now.Ticks);
        Debug.Assert(S0 != 0, "State cannot be seeded with a value of 0");
    }

    private static uint __NextState() {
        uint temp = (S0 ^ (S0 << 11));

        // shuffle state down
        S0 = S1;
        S1 = S2;
        S2 = S3;
        S3 = (S3 ^ (S3 >> 19)) ^ (temp ^ (temp >> 8));
        return S3;
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    /// Basic Random Int Functions
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Int()   => intBitMask & (int)__NextState();
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Uint() => (uint)(intBitMask & __NextState());


    //////////////////////////////////////////////////////////////////////////////////////////
    /// Basic Random Float Functions (returns value in Range of [0] -> [1])
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Float()    => (float)(maxIntDivInclusive * Int());
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Float64() => (double)(maxIntDivInclusive * Int());
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Double()  => Float64();


    ///////////////////////////////////////////////////////////////////////////////////////////
    // Ranged Integer Functions [min] = inclusive, [max] = exclusive
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Range(int min, int max) {
        return min + (int)((maxUintDivExclusive * (double)__NextState()) * (double)((long)max - (long)min));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Range(uint min, uint max) {
        return min + (uint)((maxUintDivExclusive * (double)__NextState()) * (double)((long)max - (long)min));        
    }


    //////////////////////////////////////////////////////////////////////////////////////////
    /// Ranged Float Functions [min] = inclusive, [max] = inclusive
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Range(float min, float max) {
        return min + (float)((maxUintDivInclusive * (double)__NextState()) * (double)((long)max - (long)min));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Range(double min, double max) {
        return min + (double)((maxUintDivInclusive * (double)__NextState()) * (double)((long)max - (long)min));
    }


    /// Ranged int Functions [min] = inclusive, [max] = inclusive
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 RangeInt2(int min, int max) {
        int x = Range(min, max);
        int y = Range(min, max);
        return new (x, y);
    }
    
    /// Ranged Float Functions [min] = inclusive, [max] = inclusive
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 RangeFloat2(float min, float max) {
        float x = Range(min, max);
        float y = Range(min, max);
        return new (x, y);
    }
}

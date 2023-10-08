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
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

public static partial class maths {
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this int val) {
        return val % 2 == 0;
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOdd(this int val) {
        return !IsEven(val);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(int val) {
        uint u         = math.asuint(val);
        int complement = val >> 31;
        int add        = (int)(u >> 31);
        return (val ^ complement) + add;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b) {
        int abShift = (a - b) >> 31;
        return (a & abShift) | (b & ~abShift);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b) {
        int abShift = (a - b) >> 31;
        return (a & ~abShift) | (b & abShift);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int val, int min, int max) => Max(min, Min(max, val));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Lerp(int start, int end, int amount) => (1 - amount) * start + end * amount;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InverseLerp(int start, int end, int value) => (value - start) / (end - start);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Remap(int inputMin, int inputMax, int outputMin, int outputMax, int value) {
        int t = InverseLerp(inputMin, inputMax, value);
        return Clamp(Lerp(outputMin, outputMax, t), outputMin, outputMax);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RemapUnClamped(int inputMin, int inputMax, int outputMin, int outputMax, int value) {
        int t = InverseLerp(inputMin, inputMax, value);
        return Lerp(outputMin, outputMax, t);
    }
    
    /// <summary/> returns the percent of 'value'. E.g percent=50, value=10 return 5
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetPercentage(int percent, int value) => (percent / 100) * value;

    /// <summary/> returns the percentage that 'value' is of 'maxValue' E.g value=5, maxValue=10 return 50%
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PercentageOfMax(int value, int maxValue) => (value / maxValue) * 100;
}

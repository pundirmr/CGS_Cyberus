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
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

public static partial class maths {
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TimeToMinutesAndSeconds(float time, out int minutes, out int seconds) {
         minutes = (int)Floor(time / 60f);
         seconds = (int)time - (60 * minutes);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TimeToMinutesAndSeconds(double time, out int minutes, out int seconds) {
        minutes = (int)Floor(time / 60.0);
        seconds = (int)time - (60 * minutes);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TimeToMinutesAndSeconds(decimal time, out int minutes, out int seconds) {
        minutes = (int)System.Math.Floor(time / 60.0m);
        seconds = (int)time - (60 * minutes);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClosestAngleOnCircle(Transform center, float3 comparison, float3 look, float r) {
        float3 pos  = center.position;
        float diffx = comparison.x - pos.x;
        float diffz = comparison.z - pos.z;
        float len   = FastSqrt((diffx * diffx) + (diffz * diffz));

        float x          = pos.x + (diffx / (len * r));
        float z          = pos.z + (diffz / (len * r));
        float3 point     = new float3(x, pos.y, z);
        float3 direction = float3Util.Direction(point, pos);
        return float3Util.UnsignedAngle(direction, center.InverseTransformPoint(look));
    }

    /// <summary/> Precise version uses math.sqrt() instead of our own FastSqrt()
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClosestAngleOnCirclePrecise(Transform center, float3 comparison, float3 look, float r) {
        float3 pos  = center.position;
        float diffx = comparison.x - pos.x;
        float diffz = comparison.z - pos.z;
        float len   = math.sqrt((diffx * diffx) + (diffz * diffz));

        float x          = pos.x + (diffx / (len * r));
        float z          = pos.z + (diffz / (len * r));
        float3 point     = new float3(x, pos.y, z);
        float3 direction = float3Util.Direction(point, pos);
        return float3Util.UnsignedAnglePrecise(direction, center.InverseTransformPoint(look));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SineWave(float amplitude, float frequency, float t) => amplitude * (frequency * t);
    
    /// <summary>
    /// Triangle Wave.
    /// </summary>
    /// <param name="t">The position along the wave.</param>
    /// <returns></returns>
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float TriangleWave(float minLevel, float maxLevel, float wavesPerUnitTime, float frequencyOffset, float t) {
        float remainder = (t - frequencyOffset) % wavesPerUnitTime;
        float pos       = remainder / wavesPerUnitTime;
        if (pos < 0.5f) {
            return Lerp(minLevel, maxLevel, pos * 2f);
        } else {
            return Lerp(maxLevel, minLevel, (pos - 0.5f) * 2f);
        }  
    }
    
    /// <summary>
    /// Get the decimal part of a floating point number. For example inputting "43.542f" will return "542". 
    /// </summary>
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDecimalRemainder(float fvalue) {
        // https://www.codeproject.com/Questions/5187181/Csharp-get-decimal-part-of-a-float-without-roundin
        const int SIGN_MASK = ~int.MinValue;
        double dplaces;
        try {
            decimal dvalue = Convert.ToDecimal(fvalue);
            dplaces = (double)((decimal.GetBits(dvalue)[3] & SIGN_MASK) >> 16);
            return (int)((dvalue - Math.Truncate(dvalue)) * (int)Math.Pow(10d, dplaces));
        }
        catch (Exception ex) {
            throw new TypeInitializationException($"fvalue ({fvalue}) cannot be converted", ex);
        }
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<int> GetIntDigits(int num, int approxDigitCount = 5)
    {
        List<int> listOfInts = new List<int>(approxDigitCount);
        while(num > 0)
        {
            listOfInts.Add(num % 10);
            num /= 10;
        }
        
        listOfInts.Reverse();
        return listOfInts;
    }
}

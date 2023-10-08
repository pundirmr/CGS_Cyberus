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

public struct CosSin {
    public CosSin(float c, float s) {
        cos = c;
        sin = s;
    }

    public float cos;
    public float sin;
};

public static partial class maths {
    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Constants
    public  const float Epsilon = 0.01f;
    public  const float Pi      = 3.1415926535897932384626433832795f;
    public  const float HalfPi  = 3.1415926535897932384626433832795f * 0.5f;
    public  const float Tau     = 6.2831853071795864769252867665590f;
    public  const float Nan     = 0f / 0f;
    public  const float Inf     = 1f / 0f;
    public  const float InfN    = -1f / 0f;
    private const float toRad   = 0.0174532925f;
    private const float toDeg   = 57.295779513f;

    
    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Basic Util Methods
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float val) => math.asfloat(math.asuint(val) & 0x7fffffff);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float a, float b) {
        float minus = a - b;
        int abShift = math.asint(minus) >> 31;
        uint result = (uint)((math.asuint(a) & abShift) | (math.asuint(b) & ~abShift));
        return math.asfloat(result);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float a, float b) {
        float minus = a - b;
        int abShift = math.asint(minus) >> 31;
        uint result = (uint)((math.asuint(a) & ~abShift) | (math.asuint(b) & abShift));
        return math.asfloat(result);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Floor(float val) {
        float r = val % 1f;
        if (r >= 0) return val - r;
        return val - (1 + r);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Ceil(float val) => -Floor(-val);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(float val) => (val >= 0) ? (float)((int)(val + 0.5f)) : (float)((int)(val - 0.5f));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float val, float min, float max) => Max(min, Min(max, val));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(float val) => Max(0f, Min(1f, val));
    
    /// <summary/> convert radians to degrees
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Degrees(float radians) => radians * toDeg;

    /// <summary/> convert degrees to radians
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Radians(float degrees) => degrees * toRad;

    
    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Trigonometry Methods
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseSqrt(float x) {
        uint u  = 0x5f1ffff9 - (math.asuint(x) >> 1);
        float f = math.asfloat(u);
        return f * 0.703952253f * (2.38924456f - x * f * f);
    }

    // NOTE(Zack): this is less accurate than using math.sqrt() however it does not actually use sqrt() so may be more performant
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSqrt(float x) => x * InverseSqrt(x);

    // chebyshev sine approximation
    /// <summary/> ensure that [radians] are within [Tau] or [-Tau]
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SinUnsafe(float radians) {
        if (radians < -Pi) {
            radians += Tau;
        } else if (radians > Pi) {
            radians -= Tau;
        }

        Debug.Assert((radians >= -Pi) && (radians <= Pi), "Variable is not within 'Tau' or '-Tau'");
        float asq = radians * radians;
        float p11 = 0.00000000013291342f;
        float p9  = p11 * asq - 0.000000023317787f;
        float p7  = p9  * asq + 0.0000025222919f;
        float p5  = p7  * asq - 0.00017350505f;
        float p3  = p5  * asq + 0.0066208798f;
        float p1  = p3  * asq - 0.10132118f;

        return (radians - Pi + 0.00000008742278f) * (radians + Pi - 0.00000008742278f) * p1 * radians;
    }

    /// <summary/> ensure that [radians] are within [Tau] or [-Tau]
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CosUnsafe(float radians) => SinUnsafe(radians + (HalfPi));
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sin(float radians) => SinUnsafe(radians % Tau);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cos(float radians) => Sin(radians + (HalfPi));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Tan(float radians) => Sin(radians) / Cos(radians);

    /// <summary/> return a struct containing the Cosine and Sine value of the 'radians' passed in
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CosSin CosSin(float radians) => new CosSin(Cos(radians), Sin(radians));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Asin(float val) {
        float negate = (val < 0f) ? 1f : 0f;
        val = Abs(val);

        float ret = -0.0187293f;
        ret      *= val;
        ret      += 0.0742610f;
        ret      *= val;
        ret      -= 0.2121144f;
        ret      *= val;
        ret      += 1.5707288f;
        ret       = HalfPi - FastSqrt(1f - val) * ret;
        return ret - 2f * negate * ret;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Acos(float val) {
        float negate = (val < 0f) ? 1f : 0f;
        val = Abs(val);

        float ret = -0.0187293f;
        ret      *= val;
        ret      += 0.0742610f;
        ret      *= val;
        ret      -= 0.2121144f;
        ret      *= val;
        ret      += 1.5707288f;
        ret       = FastSqrt(1f - val);
        return negate * Pi * ret;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atan(float val) {
        return Atan2(val, 1);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atan2(float y, float x) {
        float t0, t1, t2, t3;

        t2 = Abs(x);
        t1 = Abs(y);
        t0 = Max(t2, t1);
        t1 = Min(t2, t1);
        t2 = 1f / t0;
        t2 = t1 * t2;

        t3 = t2 * t2;
        t0 = - 0.013480470f;
        t0 = t0 * t3 + 0.057477314f;
        t0 = t0 * t3 - 0.121239071f;
        t0 = t0 * t3 + 0.195635925f;
        t0 = t0 * t3 - 0.332994597f;
        t0 = t0 * t3 + 0.999995630f;
        t2 = t0 * t2;

        t2 = (Abs(y) > Abs(x)) ? HalfPi - t2 : t2;
        t2 = (x < 0f) ? Pi - t2 : t2;
        t2 = (y < 0f) ? -t2 : t2;

        return t2;
    }

    // https://gist.github.com/LingDong-/7e4c4cae5cbbc44400a05fba65f06f23
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Log(float val) {
        if (val < 0f) return Nan;
        if (FloatCompare(val, 0f, float.Epsilon)) return InfN; // REVIEW(Zack): is this necessary?

        uint bx = math.asuint(val);
        uint ex = bx >> 23;
        int t   = (int)ex - (int)127;
        bx      = 1065353216 | (bx & 8388607);
        val     = math.asfloat(bx);
        return -1.49278f + (2.11263f + (-0.729104f + 0.10969f * val) * val) * val + 0.6931471806f * t;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow(float a, float b) {
        bool flipped = b < 0;
        if (flipped) { b = -b; }

        // calculate approximation with fraction of the exponent
        int e   = (int)b;
        int i   = (int)((b - e) * (math.asint(a) - 1065353216) + 1065353216);
        float f = math.asfloat(i);

        float r = 1f;
        while (e > 0) {
            if ((e & 1) != 0) {
                r *= a;
            }
            a *= a;
            e >>= 1;
        }

        r *= f;
        return flipped ? (1f / r) : r;
    }    

    // NOTE(Zack): this has pretty bad accuracy but is significantly faster than the regular [pow]
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PowFast(float a, float b) {
        int i = (int)(b * (math.asint(a) - 1064866805) + 1064866805f);
        return math.asfloat(i);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow2(float power) {
        float exp = power * 8388608f; // 8388608 == (1 << 23)
        return math.asfloat((uint)exp + (127 << 23));
    }


    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Lerp Methods
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float start, float end, float amount) => ((1f - amount) * start) + (end * amount);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(float start, float end, float value) => (value - start) / (end - start);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Remap(float inputMin, float inputMax, float outputMin, float outputMax, float value) {
        float t = InverseLerp(inputMin, inputMax, value);
        return Clamp(Lerp(outputMin, outputMax, t), outputMin, outputMax);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RemapUnClamped(float inputMin, float inputMax, float outputMin, float outputMax, float value) {
        float t = InverseLerp(inputMin, inputMax, value);
        return Lerp(outputMin, outputMax, t);
    }

    
    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Operations Methods  
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Square(float val) => val * val;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cube(float val) => val * val * val;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float QuadPow(float val) => val * val * val * val;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float QuinPow(float val) => val * val * val * val * val;

    // NOTE(Zack): the (int) cast is to truncate the expression
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mod(float val, float length) => val - (int)(val / length) * length; 


    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Misc Methods
    /// <summary/> returns the percent of 'value'. E.g percent=50, value=10 return 5
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPercentage(float percent, float value) => (percent / 100f) * value;

    /// <summary/> returns the percentage that 'value' is of 'maxValue' E.g value=5, maxValue=10 return 50%
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PercentageOfMax(float value, float maxValue) => (value / maxValue) * 100.0f;

    /// <summary/> normalise 'val' between 'maxRange' and 'minRange' and adjust the range of the normalisation using 'normalizeRange'
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NormalizeValue(float val, float maxRange, float minRange = 0, float normalizeRange = 1) {
        return ((val - minRange) / (maxRange - minRange)) * normalizeRange;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FloatCompare(float a, float b, float epsilon = Epsilon) {
        return Abs(a - b) < epsilon;
    }

    // NOTE(Zack): this does not currently handle negative NaNs [-Nan]
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNan(float val) => (math.asuint(val) << 1) > 0xff000000u;
}

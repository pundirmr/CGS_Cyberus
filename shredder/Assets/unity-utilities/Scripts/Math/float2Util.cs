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

public static class float2Util {
    // ------ Constants ------ //
    public static readonly float2 zero = new float2(0f, 0f);
    public static readonly float2 one  = new float2(1f, 1f);
    
    // ------ Up and Down Casting Helpers ------ //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToFloat3(this float2 value) => new float3(value.x, value.y, 0);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 ToFloat4(this float2 value) => new float4(value.x, value.y, 0, 0);

    // ------ Util ------ //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Compare(float2 lhs, float2 rhs, float epsilon = maths.Epsilon) {
        return maths.Abs(DistanceSquared(lhs, rhs)) < epsilon;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Clamp(float2 val, float2 min, float2 max) {
        float x = maths.Clamp(val.x, min.x, max.x);
        float y = maths.Clamp(val.y, min.y, max.y);
        return new float2(x, y);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Clamp(float2 val, float min, float max) {
        float x = maths.Clamp(val.x, min, max);
        float y = maths.Clamp(val.y, min, max);
        return new float2(x, y);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float2 lhs, float2 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        return maths.FastSqrt((x * x) + (y * y));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistancePrecise(float2 lhs, float2 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        return math.sqrt((x * x) + (y * y));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(float2 lhs, float2 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        return (x * x) + (y * y);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Rotate(float2 v, float degrees) {
        float radians = maths.Radians(degrees);
        CosSin cs     = maths.CosSin(radians);

        float x = (v.x * cs.cos) - (v.y * cs.sin);
        float y = (v.x * cs.sin) + (v.y * cs.cos);
        return new float2(x, y);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 PositionOnCircle(float2 center, float r, float degrees) {
        float radians = maths.Radians(degrees);
        CosSin cs     = maths.CosSin(radians);

        float2 pos = center;
        pos.x     += r * cs.cos;
        pos.y     += r * cs.sin;
        return pos;
    }

    // forward   : the vector we're testing against to get an angle from.
    // direction : the vector that we're trying to get an angle from. NOTE(Zack): DO NOT NORMALIZE THE VECTORS
    //
    // forward
    // ---------
    // \
    //  \ direction
    //   \
    //    \
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UnsignedAngle(float2 forward, float2 direction) {
        float cross   = Cross(forward, direction);
        float dot     = Dot(forward, direction);
        float radians = maths.Abs(math.atan2(cross, dot));
        return maths.Degrees(radians);
    }

    // forward   : the vector we're testing against to get an angle from.
    // direction : the vector that we're trying to get an angle from. NOTE(Zack): DO NOT NORMALIZE THE VECTORS
    //
    // forward
    // ---------
    // \
    //  \ direction
    //   \
    //    \
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignedAngle(float2 forward, float2 direction) {
        float cross   = Cross(forward, direction);
        float dot     = Dot(forward, direction);
        float radians = math.atan2(cross, dot);
        return maths.Degrees(radians);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Normalise(float2 v) {
        float length  = Length(v);
        float2 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 NormalisePrecise(float2 v) {
        float length  = LengthPrecise(v);
        float2 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Direction(float2 target, float2 current) {
        float2 diff = target - current;
        return diff;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 DirectionNormalised(float2 target, float2 current) {
        float2 diff = target - current;
        return Normalise(diff);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 DirectionNormalisedPrecise(float2 target, float2 current) {
        float2 diff = target - current;
        return NormalisePrecise(diff);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Lerp(float2 start, float2 end, float amount) {
        float2 result = zero;
        result.x = start.x + (amount * (end.x - start.x));
        result.y = start.y + (amount * (end.y - start.y));
        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InRadius(float2 v, float r) => (LengthSquared(v)) < (r * r);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(float2 v) => maths.FastSqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthPrecise(float2 v) => math.sqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSquared(float2 v) => (v.x * v.x) + (v.y * v.y);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross(float2 lhs, float2 rhs) => (lhs.x * rhs.y) - (lhs.y * rhs.x);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(float2 lhs, float2 rhs) => (lhs.x * rhs.x) + (lhs.y * rhs.y);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Abs(float2 v) => new float2(maths.Abs(v.x), maths.Abs(v.y));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Floor(float2 v) => new float2(maths.Floor(v.x), maths.Floor(v.y));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 Ceil(float2 v) => new float2(maths.Ceil(v.x), maths.Ceil(v.y));
}

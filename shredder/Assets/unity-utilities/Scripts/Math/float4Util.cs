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

public static class float4Util {
    // ------ Constants ------ //
    public static readonly float4 zero = new float4(0f, 0f, 0f, 0f);
    public static readonly float4 one  = new float4(1f, 1f, 1f, 1f);
    
    // ------ Up and Down Casting Helpers ------ //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 ToFloat2(this float4 value) => new float2(value.x, value.y);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToFloat3(this float4 value) => new float3(value.x, value.y, value.z);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion ToQuaternion(this float4 value) => new quaternion(value.x, value.y, value.z, value.w);
    
    // ----- Util ----- //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Compare(float4 lhs, float4 rhs, float epsilon = maths.Epsilon) {
        return maths.Abs(DistanceSquared(lhs, rhs)) < epsilon;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float4 lhs, float4 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        float w = lhs.w - rhs.w;
        return maths.FastSqrt((x * x) + (y * y) + (z * z) + (w * w));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistancePrecise(float4 lhs, float4 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        float w = lhs.w - rhs.w;
        return math.sqrt((x * x) + (y * y) + (z * z) + (w * w));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(float4 lhs, float4 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        float w = lhs.w - rhs.w;
        return (x * x) + (y * y) + (z * z) + (w * w);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Normalise(float4 v) {
        float length  = Length(v);
        float4 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
            result.z *= iLength;
            result.w *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 NormalisePrecise(float4 v) {
        float length  = LengthPrecise(v);
        float4 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
            result.z *= iLength;
            result.w *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(float4 v1, float4 v2) {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z) + (v1.w * v2.w);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Lerp(float4 start, float4 end, float amount) {
        float4 result = zero;
        result.x = start.x + (amount * (end.x - start.x));
        result.y = start.y + (amount * (end.y - start.y));
        result.z = start.z + (amount * (end.z - start.z));
        result.w = start.w + (amount * (end.w - start.w));
        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InRadius(float4 v, float r) => (LengthSquared(v)) < (r * r);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(float4 v) => maths.FastSqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthPrecise(float4 v) => math.sqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSquared(float4 v) => (v.x * v.x) + (v.y * v.y) + (v.z * v.z) + (v.w * v.w);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Floor(float4 v) => new float4(maths.Floor(v.x), maths.Floor(v.y), maths.Floor(v.z), maths.Floor(v.w));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Ceil(float4 v) => new float4(maths.Ceil(v.x), maths.Ceil(v.y), maths.Ceil(v.z), maths.Ceil(v.w));
}

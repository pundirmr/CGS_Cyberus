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

public static class quaternionUtil {
    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Constants
    public static readonly quaternion identity = new quaternion(0f, 0f, 0f, 1f);
    public static readonly quaternion zero     = new quaternion(0f, 0f, 0f, 0f);
    public static readonly quaternion one      = new quaternion(1f, 1f, 1f, 1f);


    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Up and Down Casting Helpers
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToFloat3(this quaternion q) => new float3(q.value.x, q.value.y, q.value.z);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 ToRotor4(this quaternion q) => new float4(q.value.z, -q.value.y, q.value.x, q.value.w);


    //////////////////////////////////////////////////////////////////////////////////////////
    //////// Methods
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Compare(quaternion lhs, quaternion rhs, float epsilon = maths.Epsilon) {
        return maths.Abs(DistanceSquared(lhs, rhs)) < epsilon;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(quaternion lhs, quaternion rhs) {
        float x = lhs.value.x - rhs.value.x;
        float y = lhs.value.y - rhs.value.y;
        float z = lhs.value.z - rhs.value.z;
        float w = lhs.value.w - rhs.value.w;
        return maths.FastSqrt((x * x) + (y * y) + (z * z) + (w * w));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistancePrecise(quaternion lhs, quaternion rhs) {
        float x = lhs.value.x - rhs.value.x;
        float y = lhs.value.y - rhs.value.y;
        float z = lhs.value.z - rhs.value.z;
        float w = lhs.value.w - rhs.value.w;
        return math.sqrt((x * x) + (y * y) + (z * z) + (w * w));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(quaternion lhs, quaternion rhs) {
        float x = lhs.value.x - rhs.value.x;
        float y = lhs.value.y - rhs.value.y;
        float z = lhs.value.z - rhs.value.z;
        float w = lhs.value.w - rhs.value.w;
        return (x * x) + (y * y) + (z * z) + (w * w);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion Normalise(quaternion v) {
        float length      = Length(v);
        quaternion result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.value.x *= iLength;
            result.value.y *= iLength;
            result.value.z *= iLength;
            result.value.w *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion NormalisePrecise(quaternion v) {
        float length      = LengthPrecise(v);
        quaternion result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.value.x *= iLength;
            result.value.y *= iLength;
            result.value.z *= iLength;
            result.value.w *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion Lerp(quaternion start, quaternion end, float amount) {
        quaternion result = identity;
        result.value.x = maths.Lerp(start.value.x, end.value.x, amount);
        result.value.y = maths.Lerp(start.value.y, end.value.y, amount);
        result.value.z = maths.Lerp(start.value.z, end.value.z, amount);
        result.value.w = maths.Lerp(start.value.w, end.value.w, amount);
        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion Nlerp(quaternion start, quaternion end, float amount) {
        float dot = Dot(start, end);
        if(dot < 0.0f)
        {
            end.value = -end.value;
        }

        return Normalise(new quaternion(Lerp(start, end, amount).value));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(quaternion q1, quaternion q2) => float4Util.Dot(q1.value, q2.value);    

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InRadius(quaternion v, float r) => (LengthSquared(v)) < (r * r);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(quaternion v) => maths.FastSqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthPrecise(quaternion v) => math.sqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSquared(quaternion v) => (v.value.x * v.value.x) + (v.value.y * v.value.y) + (v.value.z * v.value.z) + (v.value.w * v.value.w);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Floor(float4 v) => new float4(maths.Floor(v.x), maths.Floor(v.y), maths.Floor(v.z), maths.Floor(v.w));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 Ceil(float4 v) => new float4(maths.Ceil(v.x), maths.Ceil(v.y), maths.Ceil(v.z), maths.Ceil(v.w));
}

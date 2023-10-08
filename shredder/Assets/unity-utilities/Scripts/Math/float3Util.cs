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

public static class float3Util {
    // ------ Constants ------ //
    public static readonly float3 zero    = new float3( 0f, 0f, 0f);
    public static readonly float3 one     = new float3( 1f, 1f, 1f);
    public static readonly float3 up      = new float3( 0f, 1f, 0f);
    public static readonly float3 down    = new float3( 0f, -1f, 0f);
    public static readonly float3 right   = new float3( 1f, 0f, 0f);
    public static readonly float3 left    = new float3(-1f, 0f, 0f);
    public static readonly float3 forward = new float3( 0f, 0f, 1f);
    public static readonly float3 back    = new float3( 0f, 0f, -1f);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScalarInit(float val) => new float3(val, val, val);
    
    // ------ Up and Down Casting Helpers ------ //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 ToFloat2(this float3 value) => new float2(value.x, value.y);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 ToFloat4(this float3 value) => new float4(value.x, value.y, value.z, 0f);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion ToQuaternion(this float3 value) => new quaternion(value.x, value.y, value.z, 0f);

    // ------ Util ------ //
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Compare(float3 lhs, float3 rhs, float epsilon = maths.Epsilon) {
        return maths.Abs(DistanceSquared(lhs, rhs)) < epsilon;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CompareXY(float3 lhs, float3 rhs, float epsilon = maths.Epsilon) {
        float2 lh = new (lhs.x, lhs.y);
        float2 rh = new (rhs.x, rhs.y);
        return float2Util.Compare(lh, rh, epsilon);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CompareXZ(float3 lhs, float3 rhs, float epsilon = maths.Epsilon) {
        float2 lh = new (lhs.x, lhs.z);
        float2 rh = new (rhs.x, rhs.z);
        return float2Util.Compare(lh, rh, epsilon);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CompareYZ(float3 lhs, float3 rhs, float epsilon = maths.Epsilon) {
        float2 lh = new (lhs.y, lhs.z);
        float2 rh = new (rhs.y, rhs.z);
        return float2Util.Compare(lh, rh, epsilon);
    }   
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Clamp(float3 val, float3 min, float3 max) {
        float x = maths.Clamp(val.x, min.x, max.x);
        float y = maths.Clamp(val.y, min.y, max.y);
        float z = maths.Clamp(val.z, min.z, max.z);
        return new float3(x, y, z);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Clamp(float3 val, float min, float max) {
        float x = maths.Clamp(val.x, min, max);
        float y = maths.Clamp(val.y, min, max);
        float z = maths.Clamp(val.z, min, max);
        return new float3(x, y, z);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Add(float3 lhs, float3 rhs) {
        float x = lhs.x + rhs.x;
        float y = lhs.y + rhs.y;
        float z = lhs.z + rhs.z;
        return new float3(x, y, z);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Subtract(float3 lhs, float3 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        return new float3(x, y, z);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Square(float3 v) {
        float x = maths.Square(v.x);
        float y = maths.Square(v.y);
        float z = maths.Square(v.z);
        return new float3(x, y, z);
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float3 lhs, float3 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        return maths.FastSqrt((x * x) + (y * y) + (z * z));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistancePrecise(float3 lhs, float3 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        return math.sqrt((x * x) + (y * y) + (z * z));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(float3 lhs, float3 rhs) {
        float x = lhs.x - rhs.x;
        float y = lhs.y - rhs.y;
        float z = lhs.z - rhs.z;
        return (x * x) + (y * y) + (z * z);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
        float3 direction = Direction(point, pivot);
        direction        = quaternion.Euler(angles).ToFloat3() * direction;
        float3 result    = direction + pivot;
        return result;
    }

    // pass in position and apply return value to rotation
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateAroundX(float3 pos, float degrees) {
        float angle = maths.Radians(degrees);
        CosSin cs   = maths.CosSin(angle);

        float ny = cs.cos * pos.y - cs.sin * pos.z;
        float nz = cs.cos * pos.z + cs.sin * pos.y;
        return new float3(pos.x, ny, nz);
    }

    // pass in position and apply return value to rotation
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateAroundY(float3 pos, float degrees) {
        float angle = maths.Radians(degrees);
        CosSin cs   = maths.CosSin(angle);

        float nx = cs.cos * pos.x + cs.sin * pos.z;
        float nz = cs.cos * pos.z - cs.sin * pos.x;
        return new float3(nx, pos.y, nz);
    }

    // pass in position and apply return value to rotation
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateAroundZ(float3 pos, float degrees) {
        float angle = maths.Radians(degrees);
        CosSin cs   = maths.CosSin(angle);

        float nx = cs.cos * pos.x - cs.sin * pos.y;
        float ny = cs.cos * pos.y + cs.sin * pos.x;
        return new float3(nx, ny, pos.z);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateByQuaternion(float3 pos, quaternion q) {
        // convert unity quaternion to geometric rotor
        float4 rotor = q.ToRotor4();

        // basis vector
        float bx = (rotor.w * pos.x) + (pos.y * rotor.x) + (pos.z * rotor.y);
        float by = (rotor.w * pos.y) - (pos.x * rotor.x) + (pos.z * rotor.z);
        float bz = (rotor.w * pos.z) - (pos.x * rotor.y) - (pos.y * rotor.z);

        // tri-vector
        float tri = (pos.x * rotor.z) - (pos.y * rotor.y) + (pos.z * rotor.x);

        // calculate new rotated position
        float3 result;
        result.x = (rotor.w * bx) + (rotor.x * by)   + (rotor.y * bz)  + (rotor.z * tri);
        result.y = (rotor.w * by) - (rotor.x * bx)   - (rotor.y * tri) + (rotor.z * bz);
        result.z = (rotor.w * bz) + (rotor.x * tri)  - (rotor.y * bx)  - (rotor.z * by);

        return result;
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
    public static float UnsignedAngle(float3 forward, float3 direction) {
        float3 cross  = Cross(forward, direction);
        float len     = Length(cross);
        float dot     = Dot(forward, direction);
        float radians = maths.Abs(math.atan2(len, dot));
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
    public static float UnsignedAnglePrecise(float3 forward, float3 direction) {
        float3 cross  = Cross(forward, direction);
        float len     = LengthPrecise(cross);
        float dot     = Dot(forward, direction);
        float radians = maths.Abs(math.atan2(len, dot));
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
    public static float SignedAngle(float3 forward, float3 direction) {
        float3 cross  = Cross(forward, direction);
        float len     = Length(cross);
        float dot     = Dot(forward, direction);
        float radians = math.atan2(len, dot);
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
    public static float SignedAnglePrecise(float3 forward, float3 direction) {
        float3 cross  = Cross(forward, direction);
        float len     = LengthPrecise(cross);
        float dot     = Dot(forward, direction);
        float radians = math.atan2(len, dot);
        return maths.Degrees(radians);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Normalise(float3 v) {
        float length  = Length(v);
        float3 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
            result.z *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 NormalisePrecise(float3 v) {
        float length  = LengthPrecise(v);
        float3 result = v;
        if (length > 0f) {
            float iLength = 1f / length;
            result.x *= iLength;
            result.y *= iLength;
            result.z *= iLength;
        }

        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Direction(float3 target, float3 current) {
        float3 diff = target - current;
        return diff;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 DirectionNormalised(float3 target, float3 current) {
        float3 diff = target - current;
        return Normalise(diff);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 DirectionNormalisedPrecise(float3 target, float3 current) {
        float3 diff = target - current;
        return NormalisePrecise(diff);
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Lerp(float3 start, float3 end, float amount) {
        float3 result = zero;
        result.x = maths.Lerp(start.x, end.x, amount);
        result.y = maths.Lerp(start.y, end.y, amount);
        result.z = maths.Lerp(start.z, end.z, amount);
        return result;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Nlerp(float3 start, float3 end, float amount) {
        return Normalise(Lerp(start, end, amount));
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InRadius(float3 v, float r) => (LengthSquared(v)) < (r * r);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(float3 v) => maths.FastSqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthPrecise(float3 v) => math.sqrt(LengthSquared(v));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSquared(float3 v) => (v.x * v.x) + (v.y * v.y) + (v.z * v.z);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Cross(float3 lhs, float3 rhs) {
        float x = (lhs.y * rhs.z - lhs.z * rhs.y);
        float y = (lhs.z * rhs.x - lhs.x * rhs.z);
        float z = (lhs.z * rhs.y - lhs.y * rhs.x);
        return new float3(x, y, z);
    }
                                
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(float3 lhs, float3 rhs) => (lhs.x * rhs.x) + (lhs.y * rhs.y) + (lhs.z * rhs.z);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Negate(float3 v) => new (-v.x, -v.y, -v.z);
        
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Floor(float3 v) => new float3(maths.Floor(v.x), maths.Floor(v.y), maths.Floor(v.z));

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Ceil(float3 v) => new float3(maths.Ceil(v.x), maths.Ceil(v.y), maths.Ceil(v.z));

    /// <summary>
    /// Get a point along a line. t is distance along line normalised between 0 and 1.
    /// </summary>
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 PointAlongLine(float3 startPos, float3 endPos, float t) => startPos + ((endPos - startPos) * t);

    /// <summary>
    /// Multiplies each element of vector a by its respective element in b.
    /// <br/><b>
    /// a.x * b.x.<br/>
    /// a.y * b.y.<br/>
    /// a.z * b.z.<br/>
    /// </b>
    /// </summary>
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 LinearFloatMultiply(float3 a, float3 b)
    {
        float3 output;
        output.x = a.x * b.x;
        output.y = a.y * b.y;
        output.z = a.z * b.z;
        return output;
    }

}

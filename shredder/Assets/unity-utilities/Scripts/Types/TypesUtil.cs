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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst; 
using Unity.Mathematics;
using UnityEngine;

public static class TypesUtil {    
    /////////////////////////////////////////////////////////////////
    /// Bool Extensions
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this bool value) => value ? 1 : 0;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllTrue(this bool2 val) => val.x && val.y;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllTrue(this bool3 val) => val.x && val.y && val.z;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllTrue(this bool4 val) => val.x && val.y && val.z && val.w;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllTrue(this IList<bool> valArr)
    {
        for (int i = 0; i < valArr.Count; i++) { if (!valArr[i]) return false; }
        return true;
    }
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyTrue(this bool2 val) => val.x || val.y;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyTrue(this bool3 val) => val.x || val.y || val.z;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyTrue(this bool4 val) => val.x || val.y || val.z || val.w;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyTrue(this IList<bool> valArr)
    {
        for (int i = 0; i < valArr.Count; i++) { if (valArr[i]) return true; }
        return false;
    }

    /////////////////////////////////////////////////////////////////
    /// Int Extensions
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(this int value) => value >= 1;
    
    /////////////////////////////////////////////////////////////////
    /// Vector Extensions
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector(this float2 value) => (Vector2)value;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector(this float3 value) => (Vector3)value;
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector(this float4 value) => (Vector4)value;
}

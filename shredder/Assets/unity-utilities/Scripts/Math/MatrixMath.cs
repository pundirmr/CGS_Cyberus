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

public static class mat4 {
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 LookToLH(float3 pos, float3 dir, float3 up) {
        float3 z_axis = float3Util.Normalise(dir); // forward
        float3 x_axis = float3Util.Normalise(float3Util.Cross(up, z_axis)); // right
        float3 y_axis = float3Util.Cross(z_axis, x_axis); // up

        float4x4 val = new ();
        val.c0.x = x_axis.x;
        val.c0.y = y_axis.x;
        val.c0.z = z_axis.x;
        val.c0.w = 0f;

        val.c1.x = x_axis.y;
        val.c1.y = y_axis.y;
        val.c1.z = z_axis.y;
        val.c1.w = 0f;
        
        val.c2.x = x_axis.z;
        val.c2.y = y_axis.z;
        val.c2.z = z_axis.z;
        val.c2.w = 0f;
        
        val.c3.x = -float3Util.Dot(x_axis, pos);
        val.c3.y = -float3Util.Dot(y_axis, pos);
        val.c3.z = -float3Util.Dot(z_axis, pos);
        val.c3.w = 1f;

        return val;
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 LookToRH(float3 pos, float3 dir, float3 up) {
        dir = float3Util.Negate(dir);
        return LookToLH(pos, dir, up);
    }
}

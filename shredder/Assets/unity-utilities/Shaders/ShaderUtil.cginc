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

// ----- SHADER UTIL -----
// This is a file that includes shader utility functions that can be included in
// any Unity shader. This file includes any files that are required from Unity.
// Can be included using a simple include statement, for example:
//    #include "Assets/unity-utilities/Shaders/ShaderUtil.cginc"

#ifndef SHADER_UTIL_CGINC
#define SHADER_UTIL_CGINC

#include "UnityCG.cginc"

inline float3 ShaderUtil_GetObjWorldScale()
{
  float4 modelX = float4(1.0, 0.0, 0.0, 0.0);
  float4 modelY = float4(0.0, 1.0, 0.0, 0.0);
  float4 modelZ = float4(0.0, 0.0, 1.0, 0.0);
                 
  float4 modelXInWorld = mul(unity_ObjectToWorld, modelX);
  float4 modelYInWorld = mul(unity_ObjectToWorld, modelY);
  float4 modelZInWorld = mul(unity_ObjectToWorld, modelZ);
                 
  float scaleX = length(modelXInWorld);
  float scaleY = length(modelYInWorld);
  float scaleZ = length(modelZInWorld);
  
  return float3(scaleX, scaleY, scaleZ);
}

#endif // SHADER_UTIL_CGINC
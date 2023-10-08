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

public static class TextureUtil
{
  public struct TextureInfo
  {
    public int stride;

    public int rOffset;
    public int gOffset;
    public int bOffset;
    public int aOffset;

    public const int UnusedAlpha = -1;
    public bool isAlphaUsed => aOffset != UnusedAlpha;
  }
  
  /// <summary>
  /// Check if the texture format is supported on TextureUtil functions. This should be
  /// updated whenever a new texture format is supported/used in any function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsTextureFormatSupported(TextureFormat format)
  {
    // Texture Format
    bool formatSupported = format switch
    {
      TextureFormat.RGB24  => true,
      TextureFormat.RGBA32 => true,
      TextureFormat.ARGB32 => true,
      TextureFormat.BGRA32 => true,
      _ => false
    };

    if (formatSupported) return true;
    
    Log.Error($"This Texture Format ({format.ToString()}) is not supported in button encoding.\n" + 
              $"Supported Texture Formats: \n\t- {nameof(TextureFormat.RGB24)}\n\t- {nameof(TextureFormat.RGBA32)}\n\t- {nameof(TextureFormat.ARGB32)}\n\t- {nameof(TextureFormat.BGRA32)}");
   
    return false;
  }
  
  /// <summary>
  /// Check if the texture format is supported on TextureUtil functions. This should be
  /// updated whenever a new texture format is supported/used in any function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsTextureFormatSupported(Texture2D texture) => IsTextureFormatSupported(texture.format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TextureInfo GetTexture2DInfo(Texture2D texture)
  {
    return texture.format switch
    {
      TextureFormat.RGB24  => new TextureInfo() { stride = 3, rOffset = 0, gOffset = 1, bOffset = 2, aOffset = TextureInfo.UnusedAlpha },
      TextureFormat.RGBA32 => new TextureInfo() { stride = 4, rOffset = 0, gOffset = 1, bOffset = 2, aOffset = 3 },
      TextureFormat.ARGB32 => new TextureInfo() { stride = 4, rOffset = 1, gOffset = 2, bOffset = 3, aOffset = 0 },
      TextureFormat.BGRA32 => new TextureInfo() { stride = 4, rOffset = 2, gOffset = 1, bOffset = 0, aOffset = 3 },
      _ => default(TextureInfo)
    };
  }
}
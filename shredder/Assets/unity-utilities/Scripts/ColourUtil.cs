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

public static class Colour {
    public static readonly Color WhiteOpaque      = new (1, 1, 1, 1);
    public static readonly Color BlackOpaque      = new (0, 0, 0, 1);
    public static readonly Color GreyOpaque       = new (0.5f, 0.5f, 0.5f, 1);
    public static readonly Color WhiteTransparent = new (1, 1, 1, 0);
    public static readonly Color BlackTransparent = new (0, 0, 0, 0);
    public static readonly Color GreyTransparent  = new (0.5f, 0.5f, 0.5f, 0);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Lerp(Color start, Color end, float amount) {
        float r = maths.Lerp(start.r, end.r, amount);
        float g = maths.Lerp(start.g, end.g, amount);
        float b = maths.Lerp(start.b, end.b, amount);
        float a = maths.Lerp(start.a, end.a, amount);
        return new Color(r, g, b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color InverseLerp(Color start, Color end, float amount) {
        float r = maths.InverseLerp(start.r, end.r, amount);
        float g = maths.InverseLerp(start.g, end.g, amount);
        float b = maths.InverseLerp(start.b, end.b, amount);
        float a = maths.InverseLerp(start.a, end.a, amount);
        return new Color(r, g, b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Remap(int inputMin, int inputMax, Color outputMin, Color outputMax, int value) {
        int t = maths.InverseLerp(inputMin, inputMax, value);
        return Lerp(outputMin, outputMax, t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Remap(float inputMin, float inputMax, Color outputMin, Color outputMax, float value) {
        float t = maths.InverseLerp(inputMin, inputMax, value);
        return Lerp(outputMin, outputMax, t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Invert(Color color, bool useAlpha = true) {
        float alpha = useAlpha ? color.a : 1.0f;
        
        float r = alpha - color.r;
        float g = alpha - color.g;
        float b = alpha - color.b;
        return new Color(r, g, b, color.a);
    }

    // returns a new colour with the changed alpha
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ChangeAlpha(Color col, float a) => new (col.r, col.g, col.b, a);
    
    // returns a fully opaque colour, based on the colour passed in
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Opaque(Color col) => new (col.r, col.g, col.b, 1);

    // returns a full transparent colour based on the colour passed in
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Transparent(Color col) => new (col.r, col.g, col.b, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ToHDR(Color col, float intensity)
    {
        Color outCol = col;
        float power  = maths.Pow2(intensity);
        outCol      *= power;
        outCol.a    = col.a;
        return outCol;
    }
}

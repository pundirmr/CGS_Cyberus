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

public static class Bits {
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag(uint bitFlag, uint flag) => ((bitFlag & flag) != 0);
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAllFlags(uint bitFlag, uint flag) => ((bitFlag & flag) == flag);
    
    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoesNotHaveFlag(uint bitFlag, uint flag) => ((bitFlag & flag) == 0);

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AddFlag(uint bitFlag, uint flag) => bitFlag | flag;

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RemoveFlag(uint bitFlag, uint flag) => bitFlag & ~flag;

    public const uint Bit1  = 0x00000001;
    public const uint Bit2  = 0x00000002;
    public const uint Bit3  = 0x00000004;
    public const uint Bit4  = 0x00000008;
    public const uint Bit5  = 0x00000010;
    public const uint Bit6  = 0x00000020;
    public const uint Bit7  = 0x00000040;
    public const uint Bit8  = 0x00000080;
    public const uint Bit9  = 0x00000100;
    public const uint Bit10 = 0x00000200;
    public const uint Bit11 = 0x00000400;
    public const uint Bit12 = 0x00000800;
    public const uint Bit13 = 0x00001000;
    public const uint Bit14 = 0x00002000;
    public const uint Bit15 = 0x00004000;
    public const uint Bit16 = 0x00008000;
    public const uint Bit17 = 0x00010000;
    public const uint Bit18 = 0x00020000;
    public const uint Bit19 = 0x00040000;
    public const uint Bit20 = 0x00080000;
    public const uint Bit21 = 0x00100000;
    public const uint Bit22 = 0x00200000;
    public const uint Bit23 = 0x00400000;
    public const uint Bit24 = 0x00800000;
    public const uint Bit25 = 0x01000000;
    public const uint Bit26 = 0x02000000;
    public const uint Bit27 = 0x04000000;
    public const uint Bit28 = 0x08000000;
    public const uint Bit29 = 0x10000000;
    public const uint Bit30 = 0x20000000;
    public const uint Bit31 = 0x40000000;
    public const uint Bit32 = 0x80000000;

    public const ulong Bit33 = 0x0000000100000000;
    public const ulong Bit34 = 0x0000000200000000;
    public const ulong Bit35 = 0x0000000400000000;
    public const ulong Bit36 = 0x0000000800000000;
    public const ulong Bit37 = 0x0000001000000000;
    public const ulong Bit38 = 0x0000002000000000;
    public const ulong Bit39 = 0x0000004000000000;
    public const ulong Bit40 = 0x0000008000000000;
    public const ulong Bit41 = 0x0000010000000000;
    public const ulong Bit42 = 0x0000020000000000;
    public const ulong Bit43 = 0x0000040000000000;
    public const ulong Bit44 = 0x0000080000000000;
    public const ulong Bit45 = 0x0000100000000000;
    public const ulong Bit46 = 0x0000200000000000;
    public const ulong Bit47 = 0x0000400000000000;
    public const ulong Bit48 = 0x0000800000000000;
    public const ulong Bit49 = 0x0001000000000000;
    public const ulong Bit50 = 0x0002000000000000;
    public const ulong Bit51 = 0x0004000000000000;
    public const ulong Bit52 = 0x0008000000000000;
    public const ulong Bit53 = 0x0010000000000000;
    public const ulong Bit54 = 0x0020000000000000;
    public const ulong Bit55 = 0x0040000000000000;
    public const ulong Bit56 = 0x0080000000000000;
    public const ulong Bit57 = 0x0100000000000000;
    public const ulong Bit58 = 0x0200000000000000;
    public const ulong Bit59 = 0x0400000000000000;
    public const ulong Bit60 = 0x0800000000000000;
    public const ulong Bit61 = 0x1000000000000000;
    public const ulong Bit62 = 0x2000000000000000;
    public const ulong Bit63 = 0x4000000000000000;
    public const ulong Bit64 = 0x8000000000000000;

    public const uint BitMask1  = 0x00000001;
    public const uint BitMask2  = 0x00000003;
    public const uint BitMask3  = 0x00000007;
    public const uint BitMask4  = 0x0000000f;
    public const uint BitMask5  = 0x0000001f;
    public const uint BitMask6  = 0x0000003f;
    public const uint BitMask7  = 0x0000007f;
    public const uint BitMask8  = 0x000000ff;
    public const uint BitMask9  = 0x000001ff;
    public const uint BitMask10 = 0x000003ff;
    public const uint BitMask11 = 0x000007ff;
    public const uint BitMask12 = 0x00000fff;
    public const uint BitMask13 = 0x00001fff;
    public const uint BitMask14 = 0x00003fff;
    public const uint BitMask15 = 0x00007fff;
    public const uint BitMask16 = 0x0000ffff;
    public const uint BitMask17 = 0x0001ffff;
    public const uint BitMask18 = 0x0003ffff;
    public const uint BitMask19 = 0x0007ffff;
    public const uint BitMask20 = 0x000fffff;
    public const uint BitMask21 = 0x001fffff;
    public const uint BitMask22 = 0x003fffff;
    public const uint BitMask23 = 0x007fffff;
    public const uint BitMask24 = 0x00ffffff;
    public const uint BitMask25 = 0x01ffffff;
    public const uint BitMask26 = 0x03ffffff;
    public const uint BitMask27 = 0x07ffffff;
    public const uint BitMask28 = 0x0fffffff;
    public const uint BitMask29 = 0x1fffffff;
    public const uint BitMask30 = 0x3fffffff;
    public const uint BitMask31 = 0x7fffffff;

    public const ulong BitMask32 = 0x00000000ffffffff;
    public const ulong BitMask33 = 0x00000001ffffffff;
    public const ulong BitMask34 = 0x00000003ffffffff;
    public const ulong BitMask35 = 0x00000007ffffffff;
    public const ulong BitMask36 = 0x0000000fffffffff;
    public const ulong BitMask37 = 0x0000001fffffffff;
    public const ulong BitMask38 = 0x0000003fffffffff;
    public const ulong BitMask39 = 0x0000007fffffffff;
    public const ulong BitMask40 = 0x000000ffffffffff;
    public const ulong BitMask41 = 0x000001ffffffffff;
    public const ulong BitMask42 = 0x000003ffffffffff;
    public const ulong BitMask43 = 0x000007ffffffffff;
    public const ulong BitMask44 = 0x00000fffffffffff;
    public const ulong BitMask45 = 0x00001fffffffffff;
    public const ulong BitMask46 = 0x00003fffffffffff;
    public const ulong BitMask47 = 0x00007fffffffffff;
    public const ulong BitMask48 = 0x0000ffffffffffff;
    public const ulong BitMask49 = 0x0001ffffffffffff;
    public const ulong BitMask50 = 0x0003ffffffffffff;
    public const ulong BitMask51 = 0x0007ffffffffffff;
    public const ulong BitMask52 = 0x000fffffffffffff;
    public const ulong BitMask53 = 0x001fffffffffffff;
    public const ulong BitMask54 = 0x003fffffffffffff;
    public const ulong BitMask55 = 0x007fffffffffffff;
    public const ulong BitMask56 = 0x00ffffffffffffff;
    public const ulong BitMask57 = 0x01ffffffffffffff;
    public const ulong BitMask58 = 0x03ffffffffffffff;
    public const ulong BitMask59 = 0x07ffffffffffffff;
    public const ulong BitMask60 = 0x0fffffffffffffff;
    public const ulong BitMask61 = 0x1fffffffffffffff;
    public const ulong BitMask62 = 0x3fffffffffffffff;
    public const ulong BitMask63 = 0x7fffffffffffffff;
}

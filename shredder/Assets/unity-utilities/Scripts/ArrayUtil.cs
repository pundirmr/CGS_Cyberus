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

public static class ArrayUtil
{
  private static Unity.Mathematics.Random S_Random;
  
  static ArrayUtil()
  {
    S_Random = new ((uint)UnityEngine.Random.Range(0, uint.MaxValue));
  }

  // NOTE(Zack): increment array index before passing into function
  // NOTE(Zack): this function has less instructions than the below varient, but is potentially less safe for negative numbers
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int WrapIndex(int index, int arraySize)
  {
    // https://stackoverflow.com/a/10184756/13195883
    return (index + arraySize) % arraySize;
  }
    
  // NOTE(Zack): increment array index before passing into function
  // NOTE(Zack): this function is safer for negative numbers than the above varient, however it requires more instructions
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int WrapIndexSafe(int index, int arraySize)
  {
    // https://stackoverflow.com/a/10184756/13195883
    return ((index % arraySize) + arraySize) % arraySize;
  }

  // NOTE(Zack): maps a 2D array index to a 1D array
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Index2DTo1D(int x, int y, int width) => y * width + x;

  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Index2DTo1D(int2 i, int width) => Index2DTo1D(i.x, i.y, width);

  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int2 Index1DTo2D(int i, int width) => new int2(i % width, i / width);
  
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]  
  public static void Swap<T>(IList<T> array, int index1, int index2)
  {
    // NOTE(WSWhitehouse): If you're using Rider and see a suggestion
    // here ignore it as it's so difficult to read and uses a very
    // specific language feature! You know who you are...
    T temp        = array[index1];
    array[index1] = array[index2];
    array[index2] = temp;
  }

  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]  
  public static bool IsValidIndex<T>(int index, IList<T> arr)
  {
    if (index < 0)          return false;
    if (index >= arr.Count) return false;
    
    return true;
  }
  
  /// <summary>
  /// Shuffles an array using the Fischer-Yates/Knuth shuffle.
  /// </summary>
  /// <param name="array">The array to shuffle</param>
  /// <param name="shuffleCount">How many values to shuffle, starting from index 0.</param>
  /// <param name="rnd">The random generator to use, defaults to `new Unity.Mathematics.Random()`</param>
  /// <typeparam name="T">The type of array.</typeparam>
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Shuffle<T>(IList<T> array, int shuffleCount, Unity.Mathematics.Random rnd) 
  {
    // https://stackoverflow.com/questions/2301015/shuffle-listt/2301091#2301091
    // https://stackoverflow.com/questions/19600435/generating-2-random-numbers-that-are-different-c-sharp
    
    for (int i = shuffleCount; i > 1; i--) 
    {
      int pos = rnd.NextInt(i);
      Swap(array, i - 1, pos);
    }
  }
  
  /// <summary>
  /// Shuffles an array using the Fischer-Yates/Knuth shuffle.
  /// </summary>
  /// <param name="array">The array to shuffle</param>
  /// <param name="shuffleCount">How many values to shuffle, starting from index 0.</param>
  /// <typeparam name="T">The type of array.</typeparam>
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Shuffle<T>(IList<T> array, int shuffleCount) => Shuffle(array, shuffleCount, S_Random);
}

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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using Unity.Burst;
using UnityEngine;
using Random = UnityEngine.Random;

public class StringUtil
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    CharToString = new Dictionary<char, string>(char.MaxValue);
    for (char i = char.MinValue; i < char.MaxValue; i++)
    {
      CharToString.Add(i, i.ToString());
    }
  }
  
  // NOTE(WSWhitehouse): Used to cut-down on string allocations at runtime from chars
  public static Dictionary<char, string> CharToString { get; private set; }
  
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char CharToUpper(char c)
  {
    if (c < 'a' || c > 'z') return c;
    return (char)((int)c - ((int)'a' - (int)'A'));
  }
  
  [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char CharToLower(char c)
  {
    if (c < 'A' || c > 'Z') return c;
    return (char)((int)c + ((int)'a' - (int)'A'));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
  {
    if (string.IsNullOrWhiteSpace(text)) return string.Empty;

    StringBuilder newText = new StringBuilder(text.Length * 2);

    newText.Append(text[0]);

    for (int i = 1; i < text.Length; i++)
    {
      if (char.IsUpper(text[i]))
      {
        if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) ||
            (preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 &&
             !char.IsUpper(text[i + 1])))
        {
          newText.Append(' ');
        }
      }

      newText.Append(text[i]);
    }

    return newText.ToString();
  }

  // NOTE(WSWhitehouse): This function is **very** bad for performance
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> SentenceToWords(string sentence, bool includePunctuation)
  {
    // https://stackoverflow.com/a/16734675/13195883

    if (includePunctuation)
    {
      return sentence.Split().Where(x => x != string.Empty);
    }

    char[] punctuation = sentence.Where(char.IsPunctuation).Distinct().ToArray();
    return sentence.Split().Select(x => x.Trim(punctuation)).Where(x => x != string.Empty);
  }
  
  // NOTE(WSWhitehouse): This function is **very** bad for performance
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RemovePunctuation(string str)
  {
    char[] punctuation = str.Where(char.IsPunctuation).Distinct().ToArray();
    return str.Trim(punctuation);
  }

  /// <summary>
  /// Creates a dictionary of approximate widths of glyphs in a font. Depending on the font it may be accurate or it may not but its faster than using a TMP text field.
  /// <br/>
  /// <b>
  /// > Source: https://forum.unity.com/threads/calculate-width-of-a-text-before-without-assigning-it-to-a-tmp-object.758867/
  /// </b>
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Dictionary<char, float> CreateGlyphWidthApproximationDictionary(TMP_FontAsset font,
    float fontSize,
    FontStyles fontStyle)
  {
    Dictionary<char, float> fontGlyphWidth = new Dictionary<char, float>(font.glyphTable.Count);

    float width = 0;
    float pointSizeScale = fontSize / (font.faceInfo.pointSize * font.faceInfo.scale);
    float emScale = fontSize * 0.01f;

    float styleSpacingAdjustment = (fontStyle & FontStyles.Bold) == FontStyles.Bold
      ? font.boldSpacing
      : 0;
    float normalSpacingAdjustment = font.normalSpacingOffset;

    foreach (var character in font.characterTable)
    {
      width = character.glyph.metrics.horizontalAdvance * pointSizeScale +
              (styleSpacingAdjustment + normalSpacingAdjustment) * emScale;
      fontGlyphWidth.Add((char)character.unicode, width);
    }

    return fontGlyphWidth;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string StringsToString(IEnumerable<string> words, int allocSize = 100)
  {
    StringBuilder sb = new StringBuilder(allocSize);
    foreach (string word in words)
    {
      sb.Append(word);
      sb.Append(' ');
    }

    return sb.ToString();
  }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TMPSetToBinary(TMP_Text tmp) {
        StringBuilder sb = new (tmp.text);

        // REVIEW(Zack): probably a more efficient way to do this
        // we set all the characters to a binary format to begin with
        for (int i = 0; i < sb.Length; ++i) {
            if (sb[i] == ' ') continue;
            sb[i] = (char)Random.Range('0', '2');
        }
        tmp.text = sb.ToString();
    }
    
    // TODO(Zack): make this use a matrix effect reveal instead of just setting all text to the final form instantly
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerator TMPBinaryFlicker(TMP_Text text, int flickerAmount, RangedFloat waitTimes) {
        string original  = text.text;
        StringBuilder sb = new StringBuilder(text.text);
        int length       = text.text.Length;

        for (int i = 0; i < flickerAmount; ++i) {
            for (int ci = 0; ci < length; ++ci) {
                if (sb[ci] == ' ') continue;
                sb[ci] = (char)Random.Range('0', '2');
            }
            
            text.text = sb.ToString();
            yield return CoroutineUtil.Wait(waitTimes.Random());
        }

        text.text = original;
        yield break;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerator TMPBinaryReveal(TMP_Text tmp, string originalText = null, float letterDelay = 0.001f) {       
        string original = originalText == null ? tmp.text : originalText;
        tmp.text         = original;
        StringBuilder sb = new (tmp.text);

        // Debug.AssertFormat(original.Length == tmp.text.Length, "Lengths of the strings of TMP and the original text are not the same length");

        float timer = 0f;
        float offset = 0f;
        for (int letter = 0; letter < original.Length; ++letter) {
            for (int i = letter; i < original.Length; ++i) {
                if (sb[i] == ' ') continue;
                sb[i] = (char)Random.Range('0', '2');
                tmp.text = sb.ToString();

                timer = 0f;
                while (timer + offset < letterDelay) {
                    timer += Time.fixedDeltaTime;
                    yield return CoroutineUtil.WaitForFixedUpdate;
                }

                offset += timer - letterDelay;
            }

            offset     = 0f;
            sb[letter] = original[letter];
            tmp.text   = sb.ToString();
            yield return CoroutineUtil.WaitForFixedUpdate;
        }

        sb = sb.Replace(sb.ToString(), original);
        yield break;
    }
}

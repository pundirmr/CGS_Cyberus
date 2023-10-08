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

using TMPro;
using UnityEngine;

/// <summary>
/// Place this MonoBehaviour on a TMP_Text object. Select another TMP_Text object in the <see cref="textToMatch"/>
/// field, this TMP_Text will match the font size of the other - it is updated whenever the font is changed using
/// a Dirty Layout callback.
/// </summary>
[RequireComponent(typeof(TMP_Text)), ExecuteInEditMode]
public class MatchFontSize : MonoBehaviour
{
  [SerializeField] private TMP_Text textToMatch;

  private TMP_Text _text;

  private void Awake()
  {
    _text = GetComponent<TMP_Text>();
  }

  private void OnEnable()
  {
    Debug.Assert(textToMatch != null, "Text To Match is null! Please assign one.", this);
    textToMatch.RegisterDirtyLayoutCallback(SetFontSize);
    SetFontSize();
  }

  private void OnDisable()
  {
    textToMatch.UnregisterDirtyLayoutCallback(SetFontSize);
  }

  private void SetFontSize()
  {
    _text.enableAutoSizing = false;
    _text.fontSize         = textToMatch.fontSize;
  }
  
#if UNITY_EDITOR
  private void OnValidate()
  {
    if (Application.isPlaying) return;
    if (textToMatch == null)   return;
    Awake();
    SetFontSize();
  }
#endif
}
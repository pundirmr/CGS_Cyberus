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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Debug  = UnityEngine.Debug;
using Object = UnityEngine.Object;

// NOTE(WSWhitehouse): Comment to disable Debug Log/Warning/Error calls being marked as expensive in JetBrains Rider:
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

// Wrapper class for Unity's debug logging. Normal logging occurs during release builds, which is not
// desirable. This class only allows printing to the console in debug or development builds. "Debug.Log"
// calls have been updated to "Log.Print" to be more clear.

public static class Log
{
  //////////////////////////////////////////////////////////////////////////////////////////
  //////// Print Methods  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Print(string message) => Debug.Log(message);
  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Print(object message) => Debug.Log(message);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
  public static void Print(object message, Object context) => Debug.Log(message, context);

  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void PrintFormat(string format, params object[] args) => Debug.LogFormat(format, args);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void PrintFormat(Object context, string format, params object[] args) => Debug.LogFormat(context, format, args);
  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void PrintFormat(LogType logType, LogOption logOptions, Object context, string format, params object[] args) => Debug.LogFormat(logType, logOptions, context, format, args);

    

  //////////////////////////////////////////////////////////////////////////////////////////
  //////// Warning Methods  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Warning(string message) => Debug.LogWarning(message);
  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Warning(object message) => Debug.LogWarning(message);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Warning(object message, Object context) => Debug.LogWarning(message, context);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void WarningFormat(string format, params object[] args) => Debug.LogWarningFormat(format, args);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void WarningFormat(Object context, string format, params object[] args) => Debug.LogWarningFormat(context, format, args);


    
  //////////////////////////////////////////////////////////////////////////////////////////
  //////// Error Methods  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Error(string message) => Debug.LogError(message);
  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Error(object message) => Debug.LogError(message);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void Error(object message, Object context) => Debug.LogError(message, context);
  
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void ErrorFormat(string format, params object[] args) => Debug.LogErrorFormat(format, args);
    
  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("DEVELOPMENT_BUILD")]
  public static void ErrorFormat(Object context, string format, params object[] args) => Debug.LogErrorFormat(context, format, args);
}

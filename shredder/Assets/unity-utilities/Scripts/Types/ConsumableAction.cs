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

using System;
using System.Collections.Generic;

public struct ConsumableAction
{
  public delegate void Delegate();
  private List<Delegate> _events;
  private bool _consumeEvent;
  
  public ConsumableAction(int capacity = 0)
  {
    _events = new List<Delegate>(capacity);
    _consumeEvent = false;
  }

  public void Add(Delegate func)    => _events.Add(func);
  public void Remove(Delegate func) => _events.Remove(func);

  public void Invoke()
  {
    // Reset consume event
    _consumeEvent = false;
       
    for (int i = 0; i < _events.Count; i++)
    {
      _events[i].Invoke();
      
      // Check if the event has been consumed, if so stop invoking
      if (_consumeEvent) break;
    }
  }
  
  /// <summary>
  /// Invoke the action with a try catch. Logs exceptions to console.
  /// </summary>
  public void TryCatchInvoke()
  {
    // Reset consume event
    _consumeEvent = false;

    try
    {
      for (int i = 0; i < _events.Count; i++)
      {
        _events[i].Invoke();

        // Check if the event has been consumed, if so stop invoking
        if (_consumeEvent) break;
      }
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
    }
  }

  public void Consume() => _consumeEvent = true;

  public static ConsumableAction operator+(ConsumableAction action, Delegate del)
  {
    action.Add(del);
    return action;
  }
  
  public static ConsumableAction operator-(ConsumableAction action, Delegate del)
  {
    action.Remove(del);
    return action;
  }
}

public struct ConsumableAction<T>
{
  public delegate void Delegate(T arg);
  private List<Delegate> _events;
  private bool _consumeEvent;
  
  public ConsumableAction(int capacity = 0)
  {
    _events = new List<Delegate>(capacity);
    _consumeEvent = false;
  }

  public void Add(Delegate func)    => _events.Add(func);
  public void Remove(Delegate func) => _events.Remove(func);

  public void Invoke(T arg)
  {
    // Reset consume event
    _consumeEvent = false;
       
    for (int i = 0; i < _events.Count; i++)
    {
      _events[i].Invoke(arg);
      
      // Check if the event has been consumed, if so stop invoking
      if (_consumeEvent) break;
    }
  }
  
  /// <summary>
  /// Invoke the action with a try catch. Logs exceptions to console.
  /// </summary>
  public void TryCatchInvoke(T arg)
  {
    // Reset consume event
    _consumeEvent = false;

    try
    {
      for (int i = 0; i < _events.Count; i++)
      {
        _events[i].Invoke(arg);

        // Check if the event has been consumed, if so stop invoking
        if (_consumeEvent) break;
      }
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
    }
  }

  public void Consume() => _consumeEvent = true;

  public static ConsumableAction<T> operator+(ConsumableAction<T> action, Delegate del)
  {
    action.Add(del);
    return action;
  }
  
  public static ConsumableAction<T> operator-(ConsumableAction<T> action, Delegate del)
  {
    action.Remove(del);
    return action;
  }
}
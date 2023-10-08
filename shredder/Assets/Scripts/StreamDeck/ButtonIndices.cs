using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Defining Input types
using DeckInput   = ConsumableAction<int>;
using ButtonInput = ConsumableAction;

[Serializable]
public struct ButtonIndices : IEnumerable<int>
{
  public ButtonIndices(int size = StreamDeck.ButtonTotalCount)
  {
    indices = new int[size];
  }
  
  [SerializeField] public int[] indices;

  public int this[int i]
  {
    get => indices[i];
    set => indices[i] = value;
  }

  public int Count   => indices.Length;
  public int[] Get() => indices;
  
  // Util
  public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)indices).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
 
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Contains(int index) => indices.Contains(index);
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public List<int> ToList()       => indices.ToList();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindArrayIndexOfButtonIndex(int index)
  {
    for (int i = 0; i < indices.Length; i++)
    {
      if (indices[i] == index) return i;
    }
    
    return -1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SubscribeToButtonPerformed(int streamDeckIndex, ButtonInput.Delegate func)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    foreach (int index in indices)
    {
      streamDeck.OnButtonPerformed[index] += func;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SubscribeToButtonCancelled(int streamDeckIndex, ButtonInput.Delegate func)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    foreach (int index in indices)
    {
      streamDeck.OnButtonCancelled[index] += func;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void UnsubscribeFromButtonPerformed(int streamDeckIndex, ButtonInput.Delegate func)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    
    #if UNITY_EDITOR
    if (streamDeck == null) return;
    #endif
    
    foreach (int index in indices)
    {
      streamDeck.OnButtonPerformed[index] -= func;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void UnsubscribeFromButtonCancelled(int streamDeckIndex, ButtonInput.Delegate func)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    
    #if UNITY_EDITOR
    if (streamDeck == null) return;
    #endif
    
    foreach (int index in indices)
    {
      streamDeck.OnButtonCancelled[index] -= func;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetButtonColour(int streamDeckIndex, StreamDeckColour colour)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    foreach (int index in indices)
    {
      streamDeck.SetButtonColour(index, colour);
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetButtonImage(int streamDeckIndex, ButtonTexture tex)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    foreach (int index in indices)
    {
      streamDeck.SetButtonImage(index, tex);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetMultiButtonImage(int streamDeckIndex, MultiButtonTexture tex)
  {
    Debug.Assert(indices.Length > 0, "Indices have not been set, or enumerated");
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    streamDeck.SetMultiButtonImage(indices[0], tex);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ClearButtons(int streamDeckIndex)
  {
    StreamDeck streamDeck = StreamDeckManager.StreamDecks[streamDeckIndex];
    foreach (int index in indices)
    {
      streamDeck.ClearButton(index);
    }
  }

  [Conditional("UNITY_EDITOR")]
  public void DebugLog()
  {
    Debug.Log("Logging Stream Deck Indices");

    foreach (int index in indices)
    {
      int2 index2D = StreamDeckManager.Index1DTo2D(index);
      Debug.Log($"int: {index}   int2: {index2D.x}, {index2D.y}");
    }
  }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonIndices))]
public class ButtonIndicesDrawer : PropertyDrawer
{
  private const float GridSize = 32f;
  private static readonly Color32 SelectedColor = new Color32(138, 201, 38, 255);

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    float startX = position.x;
    float startY = position.y;

    EditorGUI.BeginProperty(position, label, property);
    
    SerializedProperty indices = property.FindPropertyRelative("indices");

    // Draw label
    position   = EditorGUI.PrefixLabel(position, label);
    position.x = startX;
    position.y = startY + EditorUtil.SingleLineHeight;
    
    List<int> ints = new List<int>(indices.arraySize);
    
    for (int i = 0; i < indices.arraySize; i++)
    {
      SerializedProperty child = indices.GetArrayElementAtIndex(i);
      ints.Add(child.intValue);
    }
    
    // Draw grid
    DrawButtonGrid(ref position, ref ints);
    
    indices.arraySize = ints.Count;
    for (int i = 0; i < ints.Count; i++)
    {
      SerializedProperty child = indices.GetArrayElementAtIndex(i);
      child.intValue = ints[i];
    }
    
    EditorGUI.EndProperty();
  }

  public override bool CanCacheInspectorGUI(SerializedProperty property) => false;
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => (GridSize * StreamDeck.ButtonYCount) + (EditorUtil.SingleLineHeight * 2);

  private static void DrawButtonGrid(ref Rect startPos, ref List<int> index)
  {
    Color standardCol = GUI.color;
    
    Rect gridRect   = startPos;
    gridRect.width  = GridSize;
    gridRect.height = GridSize;

    float startX = gridRect.x;

    for (int i = 0; i < StreamDeck.ButtonYCount; i++)
    {
      for (int j = 0; j < StreamDeck.ButtonXCount; j++)
      {
        int thisIndex = StreamDeckManager.Index2DTo1D(j, i);
        
        if (index.Contains(thisIndex)) GUI.color = SelectedColor;

        if (GUI.Button(gridRect, " "))
        {
          if (index.Contains(thisIndex))
          {
            index.Remove(thisIndex);
          }
          else
          {
            index.Add(thisIndex);
          }
          
          index.Sort();
        }

        GUI.color = standardCol;
        gridRect.x += gridRect.width;
      }
      
      gridRect.x = startX;
      gridRect.y += GridSize;
    }
    
    gridRect.width  = startPos.width;
    gridRect.height = startPos.height;
    startPos = gridRect;
  }
}
#endif

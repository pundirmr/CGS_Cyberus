using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ButtonIndexAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonIndexAttribute))]
public class ButtonIndexPropertyDrawer : PropertyDrawer
{
  private const float GridSize = 32f;
  private static readonly Color32 SelectedColor = new Color32(138, 201, 38, 255);

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    float startX = position.x;
    float startY = position.y;

    EditorGUI.BeginProperty(position, label, property);
    
    if (EditorUtil.IsPropertyArray(property))
    {
      Log.Error("You are using the ButtonIndexAttribute on an array type. Use ButtonIndexArrayAttribute instead.");
    }
   
    // Draw label
    position   = EditorGUI.PrefixLabel(position, label);
    position.x = startX;
    position.y = startY + EditorUtil.SingleLineHeight;

    if (property.propertyType == SerializedPropertyType.Integer)
    {
      int val = property.intValue;
      DrawButtonGrid(ref position, ref val);
      property.intValue = val;
    }
    else if (property.propertyType == SerializedPropertyType.Vector2)
    {
      int val = StreamDeckManager.Index2DTo1D((int2)(float2)property.vector2Value);
      DrawButtonGrid(ref position, ref val);
      property.vector2Value = (float2)StreamDeckManager.Index1DTo2D(val);
    }
    else
    {
      EditorGUI.PropertyField(position, property, label);
    }

    EditorGUI.EndProperty();
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return (GridSize * StreamDeck.ButtonYCount) + EditorUtil.SingleLineHeight;
  }

  private void DrawButtonGrid(ref Rect position, ref int index)
  {
    Color standardCol = GUI.color;
    
    Rect gridRect   = position;
    gridRect.width  = GridSize;
    gridRect.height = GridSize;

    float startX = gridRect.x;
    int currentIndex = index;
    
    for (int i = 0; i < StreamDeck.ButtonYCount; i++)
    {
      for (int j = 0; j < StreamDeck.ButtonXCount; j++)
      {
        int thisIndex = StreamDeckManager.Index2DTo1D(j, i);
        if (currentIndex == thisIndex) GUI.color = SelectedColor;

        if (GUI.Button(gridRect, " "))
        {
          index = currentIndex != thisIndex ? thisIndex : -1;
        }

        GUI.color = standardCol;
        gridRect.x += gridRect.width;
      }
      
      gridRect.x = startX;
      gridRect.y += GridSize;
    }
    
    position.x = gridRect.x;
    position.y = gridRect.y;
  }
}
#endif

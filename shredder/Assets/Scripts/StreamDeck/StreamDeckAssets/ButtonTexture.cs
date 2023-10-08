using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Color = UnityEngine.Color;
#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Button Texture", menuName = "Stream Deck/Button Texture", order = 0)]
public class ButtonTexture : ScriptableObject
{
  public Texture2D texture;
  public Color colourMultiplier = Color.white;
  
  public List<StreamDeckPacket> packets = new List<StreamDeckPacket>();

  public void GeneratePackets()
  {
    Image<Bgr24> image = StreamDeckInternal.Texture2DToButtonImage(texture, colourMultiplier);
    byte[] jpegTextureData = StreamDeckInternal.ConvertButtonImageToJpegAlloc(image);
    image.Dispose();

    packets.Clear();
    packets = StreamDeckPacket.GenerateData(jpegTextureData);
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ButtonTexture))]
public class ButtonTextureEditor : Editor
{
  private ButtonTexture Target => (ButtonTexture)target;

  private bool _isDirty = false;
  private ButtonTexture _copy;
  
  private SerializedProperty _textureProperty;
  private SerializedProperty _colourMultiplierProperty;
  private SerializedProperty _packetsProperty;

  private void OnEnable()
  {
    CreateCopy();

    _textureProperty          = serializedObject.FindProperty(nameof(Target.texture));
    _colourMultiplierProperty = serializedObject.FindProperty(nameof(Target.colourMultiplier));
    _packetsProperty          = serializedObject.FindProperty(nameof(Target.packets));
    
    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
  }

  private void OnDisable()
  {
    if (_copy != null) DestroyImmediate(_copy);
    
    EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    
    if (!_isDirty) return;
    if (EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes! Do you want to save?", "Save", "Discard"))
    {
      Save();
    }
  }
  
  private void OnPlayModeStateChanged(PlayModeStateChange state)
  {
    if (!_isDirty) return;
    
    // NOTE(WSWhitehouse): Only care about exiting edit mode state
    if (state != PlayModeStateChange.ExitingEditMode) return;
    
    EditorApplication.isPlaying = false;
    if (!EditorUtility.DisplayDialog("Play Mode is disabled!", "Can't enter play mode while there are still pending changes on a Stream Deck Asset!", "Okay", "Save Now"))
    {
      Save();
    }
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(_textureProperty);
    EditorGUILayout.PropertyField(_colourMultiplierProperty);
    if (EditorGUI.EndChangeCheck())
    {
      _isDirty = true;
    }

    using (new EditorGUI.DisabledScope(true))
    {
      EditorGUILayout.PropertyField(_packetsProperty);
    }

    EditorGUILayout.Space();

    using (new EditorGUI.DisabledScope(Target.texture == null))
    {
      if (GUILayout.Button("Generate Packets"))
      {
        EditorUtility.DisplayProgressBar("Generating Packet Data", "Generating...", 0.5f);
        serializedObject.ApplyModifiedProperties();
        Target.GeneratePackets();
        serializedObject.Update();
        EditorUtility.ClearProgressBar();
        
        _isDirty = true;
      }
    }

    // Save & Discard
    {
      EditorGUILayout.BeginHorizontal("box");

      using (new EditorGUI.DisabledScope(!_isDirty))
      {
        if (GUILayout.Button("Save"))
        {
          Save();
        }

        if (GUILayout.Button("Discard"))
        {
          // Reset target from copy
          Target.texture          = _copy.texture;
          Target.colourMultiplier = _copy.colourMultiplier;
          Target.packets          = new List<StreamDeckPacket>(_copy.packets.Select(StreamDeckPacket.Copy));
          
          _isDirty = false;
        }
      }
      
      EditorGUILayout.EndHorizontal();
    }
    
    serializedObject.ApplyModifiedProperties();
  }

  private void Save()
  {
    CreateCopy();
    _isDirty = false;
    
    serializedObject.ApplyModifiedPropertiesWithoutUndo();
    
    // Reload asset database
    string assetPath = AssetDatabase.GetAssetPath(Target);
    AssetDatabase.ImportAsset(assetPath);
    AssetDatabase.SaveAssets(); 
    AssetDatabase.Refresh();
  }

  private void CreateCopy()
  {
    if (_copy == null)
    {
      _copy = CreateInstance<ButtonTexture>();
    }
    
    _copy.texture          = Target.texture;
    _copy.colourMultiplier = Target.colourMultiplier;
    _copy.packets          = new List<StreamDeckPacket>(Target.packets.Select(StreamDeckPacket.Copy));
  }
}
#endif
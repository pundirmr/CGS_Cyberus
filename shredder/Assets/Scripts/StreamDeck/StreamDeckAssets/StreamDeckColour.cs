using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Color = UnityEngine.Color;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Stream Deck Colour", menuName = "Stream Deck/Colour", order = 0)]
public class StreamDeckColour : ScriptableObject
{
  public Color colour;
  public Color colourMultiplier = Color.white;
  public List<StreamDeckPacket> buttonPackets = new List<StreamDeckPacket>();
  public List<StreamDeckPacket> deckPackets   = new List<StreamDeckPacket>();

  public void GeneratePackets()
  {
    Image<Bgr24> image     = StreamDeckInternal.ColourToButtonImage(colour, colourMultiplier);
    byte[] jpegTextureData = StreamDeckInternal.ConvertButtonImageToJpegAlloc(image);
    image.Dispose();

    List<StreamDeckPacket> generatedPackets = StreamDeckPacket.GenerateData(jpegTextureData);

    // Button Packets
    buttonPackets.Clear();
    buttonPackets = generatedPackets;

    // Deck Packets
    deckPackets.Clear();
    for (int i = 0; i < StreamDeck.ButtonTotalCount; i++)
    {
      foreach (StreamDeckPacket packet in generatedPackets)
      {
        // Clone packet 
        StreamDeckPacket newPacket = StreamDeckPacket.Copy(packet);

        newPacket.SetButtonIndex((byte)i);
        deckPackets.Add(newPacket);
      }
    }
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StreamDeckColour))]
public class ButtonColourEditor : Editor
{
  private StreamDeckColour Target => (StreamDeckColour)target;

  private SerializedProperty _colourProperty;
  private SerializedProperty _colourMultiplierProperty;
  private SerializedProperty _buttonPacketsProperty;
  private SerializedProperty _deckPacketsProperty;
  
  private bool _isDirty = false;
  private StreamDeckColour _copy;

  private void OnEnable()
  {
    CreateCopy();

    _colourProperty           = serializedObject.FindProperty(nameof(Target.colour));
    _colourMultiplierProperty = serializedObject.FindProperty(nameof(Target.colourMultiplier));
    _buttonPacketsProperty    = serializedObject.FindProperty(nameof(Target.buttonPackets));
    _deckPacketsProperty      = serializedObject.FindProperty(nameof(Target.deckPackets));
    
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
    EditorGUILayout.PropertyField(_colourProperty);
    EditorGUILayout.PropertyField(_colourMultiplierProperty);
    if (EditorGUI.EndChangeCheck())
    {
      _isDirty = true;
    }

    using (new EditorGUI.DisabledScope(true))
    {
      EditorGUILayout.PropertyField(_buttonPacketsProperty);
      EditorGUILayout.PropertyField(_deckPacketsProperty);
    }

    EditorGUILayout.Space();
    
    if (GUILayout.Button("Generate Packets"))
    {
      EditorUtility.DisplayProgressBar("Generating Packet Data", "Generating...", 0.5f);
      serializedObject.ApplyModifiedProperties();
      Target.GeneratePackets();
      serializedObject.Update();
      EditorUtility.ClearProgressBar();
      
      _isDirty = true;
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
          Target.colour           = _copy.colour;
          Target.colourMultiplier = _copy.colourMultiplier;
          Target.buttonPackets    = new List<StreamDeckPacket>(_copy.buttonPackets.Select(StreamDeckPacket.Copy));
          Target.deckPackets      = new List<StreamDeckPacket>(_copy.deckPackets.Select(StreamDeckPacket.Copy));
          
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
      _copy = CreateInstance<StreamDeckColour>();
    }
    
    _copy.colour           = Target.colour;
    _copy.colourMultiplier = Target.colourMultiplier;
    _copy.buttonPackets    = new List<StreamDeckPacket>(Target.buttonPackets.Select(StreamDeckPacket.Copy));
    _copy.deckPackets      = new List<StreamDeckPacket>(Target.deckPackets.Select(StreamDeckPacket.Copy));
  }
}
#endif
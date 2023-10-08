using System;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Color = UnityEngine.Color;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ButtonPacket
{
  public ButtonPacket() { this.packets = new List<StreamDeckPacket>(); }
  public ButtonPacket(List<StreamDeckPacket> packets) { this.packets = packets; }

  public List<StreamDeckPacket> packets;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetButtonIndex(byte index)
  {
    foreach (StreamDeckPacket packet in packets)
    {
      packet.SetButtonIndex(index);
    }
  }

  public static ButtonPacket Copy(ButtonPacket other)
  {
    ButtonPacket clone = new ButtonPacket();
    if (other.packets == null) return clone;
      
    clone.packets = new List<StreamDeckPacket>(other.packets.Select(StreamDeckPacket.Copy));
    return clone;
  }
}

[CreateAssetMenu(fileName = "New Multi Button Texture", menuName = "Stream Deck/Multi Button Texture", order = 0)]
public class MultiButtonTexture : ScriptableObject
{
  public Texture2D texture;
  public Color colourMultiplier     = Color.white;
  public int2 buttonCount           = int2Util.one;
  public List<ButtonPacket> packets = new List<ButtonPacket>();

  public void GeneratePackets()
  {
    packets.Clear();
    
    Image<Bgr24> deckImage = StreamDeckInternal.Texture2DToImage(texture, colourMultiplier);
    int textureWidth  = StreamDeck.FullButtonSize * buttonCount.x - StreamDeck.ButtonGapSize;
    int textureHeight = StreamDeck.FullButtonSize * buttonCount.y - StreamDeck.ButtonGapSize;
    StreamDeckInternal.ResizeImage(ref deckImage, textureWidth, textureHeight);
    List<Image<Bgr24>> buttonImages = StreamDeckInternal.ImageToButtonImages(deckImage, buttonCount);

    for (int i = 0; i < buttonImages.Count; i++)
    {
      byte[] jpegTextureData  = StreamDeckInternal.ConvertButtonImageToJpegAlloc(buttonImages[i]);
      ButtonPacket packetData = new ButtonPacket(StreamDeckPacket.GenerateData(jpegTextureData));
      packets.Add(packetData);

      buttonImages[i].Dispose();
      buttonImages[i] = null;
    }

    deckImage.Dispose();
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MultiButtonTexture))]
public class MultiButtonTextureEditor : Editor
{
  private MultiButtonTexture Target => (MultiButtonTexture)target;

  private const float GridSize = 32f;
  
  private bool _isDirty = false;
  private MultiButtonTexture _copy;
  
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
    
    EditorGUI.BeginChangeCheck();
    int x = EditorGUILayout.IntSlider(new GUIContent("Button X Count"), Target.buttonCount.x, 1, StreamDeck.ButtonXCount);
    int y = EditorGUILayout.IntSlider(new GUIContent("Button Y Count"), Target.buttonCount.y, 1, StreamDeck.ButtonYCount);
    if (EditorGUI.EndChangeCheck())
    {
      Target.buttonCount = new int2(x, y);
      _isDirty = true;
    }

    // Draw button visualisation
    {
      // Display warning if button count equals (1,1)
      if ((Target.buttonCount == int2Util.one).AllTrue())
      {
        EditorGUILayout.HelpBox(
          "This texture is the size of one button! Please use a Button Texture for better performance.",
          MessageType.Warning);
      }

      // Display warning if button count equals the stream deck button count
      if ((Target.buttonCount == StreamDeck.ButtonCount).AllTrue())
      {
        EditorGUILayout.HelpBox(
          "This texture is the size of the entire Stream Deck! Please use a Deck Texture for better performance.",
          MessageType.Warning);
      }

      EditorGUILayout.LabelField("Button Visualisation", EditorStyles.boldLabel);
      Rect currentRect = EditorUtil.GetRectArea(GridSize * Target.buttonCount.y);
      DrawButtonGrid(ref currentRect, Target.buttonCount);
    }

    EditorGUILayout.Space();

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
          Target.packets          = new List<ButtonPacket>(_copy.packets.Select(ButtonPacket.Copy));
          
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
      _copy = CreateInstance<MultiButtonTexture>();
    }
    
    _copy.texture          = Target.texture;
    _copy.colourMultiplier = Target.colourMultiplier;
    _copy.packets          = new List<ButtonPacket>(Target.packets.Select(ButtonPacket.Copy));
  }

  private static void DrawButtonGrid(ref Rect position, int2 buttonCount)
  {
    Rect gridRect   = position;
    gridRect.width  = GridSize;
    gridRect.height = GridSize;

    float startX = gridRect.x;

    for (int y = 0; y < buttonCount.y; y++)
    {
      for (int x = 0; x < buttonCount.x; x++)
      {
        GUI.Button(gridRect, " ");
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
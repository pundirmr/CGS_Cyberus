using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "ColourScheme", menuName = "Colour Scheme", order = 1)]
public class ColourScheme : ScriptableObject {
    [ColorUsage(true, true)] public Color32[] Colours = new Color32[3];
    [ColorUsage(true, true)] public Color32 CardBGColour;
    [NonSerialized] public StreamDeckColour[] StreamDeckColours;
    public ButtonTexture StreamDeckTexture;
    
    public int ColourCount => Colours.Length;

    [HelpBox("The materials used for in game lasers use this template material with the colour changed to one of the colours in the array above.")]
    [SerializeField] private Material colMaterialTemplate;
    [Space]
    [HelpBox("Used to create a gradient material for this ColourScheme")]
    [SerializeField] private Material gradMaterialTemplate;
        
    [NonSerialized] public Material GradientMaterial;
    
    public void Init() {
        // set alpha values to 255
        for (int i = 0; i < Colours.Length; ++i) {
            Colours[i].a = 255;
        }
        
        // Gradient Material
        Debug.Assert(gradMaterialTemplate != null, $"Gradient Template Material is null", this);

        GradientMaterial = new Material(gradMaterialTemplate);
        
        GradientMaterial.SetColor("_Color1", Colours[0]);
        GradientMaterial.SetColor("_Color2", Colours[1]);
        GradientMaterial.SetColor("_Color3", Colours[2]);
        
        // Materials
        Debug.Assert(colMaterialTemplate != null, $"Colour Material Template is null", this);
        _colourMaterials = new Material[Colours.Length];
        for (int i = 0; i < _colourMaterials.Length; i++) 
        {
            _colourMaterials[i] = new Material(colMaterialTemplate) { color = Colours[i] };
        }

        // Stream Deck Colours
        StreamDeckColours = new StreamDeckColour[Colours.Length];
        for (int i = 0; i < StreamDeckColours.Length; i++)
        {
            StreamDeckColours[i] = CreateInstance<StreamDeckColour>();
            StreamDeckColours[i].colour = Colours[i];
            StreamDeckColours[i].GeneratePackets();
        }
        
    }
    
    private Material[] _colourMaterials = null;
    public Material[] ColourMaterials => _colourMaterials;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int RandomIndex() => Random.Range(0, Colours.Length);
}

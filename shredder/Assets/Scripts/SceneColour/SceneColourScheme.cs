using System;
using UnityEngine;

public enum SceneColourType
{
    Default = -1,
    HighlightHDR,
    PavementBackground,
    LampPostBackground,
    BuildingBackground,
    BuildingFacade,
    TextColour,
    CharacterBaseColour,
    CharacterGlowColour,
    SelectedCharacterBaseColour,
    SelectedCharacterGlowColour,
    logoColour,
}

[Serializable]
public class SceneColour
{
    public SceneColour(Color colour, SceneColourType type = SceneColourType.Default)
    {
        Colour = colour;
        ColourType = type;
    }

    public SceneColour()
    {
        Colour = Color.magenta;
        ColourType = SceneColourType.Default;
    }

    public SceneColourType ColourType;
    [ColorUsage(true, true)] public Color Colour;
}

[Serializable]
public struct SceneColourScheme
{
    // [SerializeField] public SceneColour[] sceneColours;
    [SerializeField] public Sprite backgroundSprite;
    [Header("Colours")]
    [SerializeField] [ColorUsage(true, true)] private Color defaultColour;
    [SerializeField] [ColorUsage(true, true)] private Color highlightHDRColour;
    [SerializeField] [ColorUsage(true, true)] private Color pavementBackgroundColour;
    [SerializeField] [ColorUsage(true, true)] private Color lampPostBackgroundColour;
    [SerializeField] [ColorUsage(true, true)] private Color buildingBackgroundColour;
    [SerializeField] [ColorUsage(true, true)] private Color buildingFacadeColour;
    [SerializeField] [ColorUsage(true, true)] private Color textColour;
    [SerializeField] [ColorUsage(true, true)] private Color characterBaseColour;
    [SerializeField] [ColorUsage(true, true)] private Color characterGlowColour;
    [SerializeField] [ColorUsage(true, true)] private Color characterSelectedBaseColour;
    [SerializeField] [ColorUsage(true, true)] private Color characterSelectedGlowColour;
    [SerializeField] [ColorUsage(true, true)] private Color logoColour;

    public SceneColourScheme(int unused = 0)
    {
        // sceneColours = new[] { new SceneColour()};
        backgroundSprite = Sprite.Create(Texture2D.blackTexture, new Rect(0,0, 1, 1), Vector2.zero);
        defaultColour = new Color(0,0,0, 1);
        highlightHDRColour = new Color(0,0,0, 1);
        pavementBackgroundColour = new Color(0,0,0, 1);
        lampPostBackgroundColour = new Color(0,0,0, 1);
        buildingBackgroundColour = new Color(0,0,0, 1);
        buildingFacadeColour = new Color(0,0,0, 1);
        textColour = new Color(0,0,0, 1);
        characterBaseColour = new Color(0,0,0, 1);
        characterGlowColour = new Color(0,0,0, 1);
        characterSelectedBaseColour = new Color(0,0,0, 1);
        characterSelectedGlowColour = new Color(0,0,0, 1);
        logoColour = new Color(0,0,0, 1);
    }

    public Color Find(SceneColourType type)
    {
        switch (type)
        {
            case SceneColourType.logoColour:
                return logoColour;
            case SceneColourType.HighlightHDR:
                return  highlightHDRColour;
            case SceneColourType.PavementBackground:
                return  pavementBackgroundColour;
            case SceneColourType.LampPostBackground:
                return  lampPostBackgroundColour;
            case SceneColourType.BuildingBackground:
                return  buildingBackgroundColour;
            case SceneColourType.BuildingFacade:
                return buildingFacadeColour;
            case SceneColourType.TextColour:
                return textColour;
            case SceneColourType.CharacterBaseColour:
                return characterBaseColour;
            case SceneColourType.CharacterGlowColour:
                return characterGlowColour;
            case SceneColourType.SelectedCharacterBaseColour:
                return characterSelectedBaseColour;
            case SceneColourType.SelectedCharacterGlowColour:
                return characterSelectedGlowColour;
            case SceneColourType.Default:
            default:
                return defaultColour;
        }
    }

    public SceneColourScheme LerpColours(SceneColourScheme other, float t)
    {
        SceneColourScheme returnScheme = new SceneColourScheme
        {
            defaultColour = Color.Lerp(defaultColour, other.defaultColour, t),
            highlightHDRColour = Color.Lerp(highlightHDRColour, other.highlightHDRColour, t),
            pavementBackgroundColour = Color.Lerp(pavementBackgroundColour, other.pavementBackgroundColour, t),
            lampPostBackgroundColour = Color.Lerp(lampPostBackgroundColour, other.lampPostBackgroundColour, t),
            buildingBackgroundColour = Color.Lerp(buildingBackgroundColour, other.buildingBackgroundColour, t),
            buildingFacadeColour = Color.Lerp(buildingFacadeColour, other.buildingFacadeColour, t),
            textColour = Color.Lerp(textColour, other.textColour, t),
            characterBaseColour = Color.Lerp(characterBaseColour, other.characterBaseColour, t),
            characterGlowColour = Color.Lerp(characterGlowColour, other.characterGlowColour, t),
            characterSelectedBaseColour = Color.Lerp(characterSelectedBaseColour, other.characterSelectedBaseColour, t),
            characterSelectedGlowColour = Color.Lerp(characterSelectedGlowColour, other.characterSelectedGlowColour, t),
            logoColour = Color.Lerp(logoColour, other.logoColour, t),
            backgroundSprite = other.backgroundSprite,
        };
        return returnScheme;
    }
}

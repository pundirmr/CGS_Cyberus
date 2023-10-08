using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class SceneMeshRenderer
{
    [Serializable]
    public struct ShaderVariableNameAndColour
    {
        public string shaderVariableName;
        public SceneColourType type;
    }

    public MeshRenderer MeshRenderer;
    public ShaderVariableNameAndColour[] shaderVariables;
}

[Serializable]
public class SceneImage
{
    public Image image;
    public SceneColourType type;
}

public class SceneColourer : MonoBehaviour
{
    // public static SceneColourScheme defaultScheme = new SceneColourScheme();
    public static SceneColourScheme currentColourScheme;

    //Materials
    public static Material LampPostMaterial;
    private static int _lampPostBaseColourID;
    private static int _lampPostColourID;

    public static Material PavementMaterial;
    private static int _pavementBaseColourID;
    private static int _pavementColourID;

    public static Material BuildingMaterial;
    private static int _buildingBaseColourID;
    private static int _buildingColourID;
    
    public static  Material BackgroundMaterial;
    private static int _backgroundMatTextAID;
    private static int _backgroundMatTextBID;
    private static int _backgroundMatLerpValID;
    
    public static Material TextMaterial;
    private static int _textMatOutlineColID;
    private static int _textMatGlowColID;
    
    public static Material OtherTextMaterial;
    private static int _otherTextColourID;

    public delegate void UpdateSceneColours(SceneColourScheme scheme);
    public static UpdateSceneColours OnUpdateSceneColours;

    public delegate IEnumerator LerpToColourScheme(SceneColourScheme colourScheme, float lerpTime);
    public static LerpToColourScheme LerpToScheme;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        currentColourScheme = new SceneColourScheme(0);
        
        LampPostMaterial = new Material(Resources.Load<Material>("SceneMaterials/LampPostMat"));
        PavementMaterial = new Material(Resources.Load<Material>("SceneMaterials/PavementMat"));
        BuildingMaterial = new Material(Resources.Load<Material>("SceneMaterials/BuildingMat"));
        BackgroundMaterial = new Material(Resources.Load<Material>("SceneMaterials/BackgroundMat"));
        TextMaterial = new Material(Resources.Load<Material>("SceneMaterials/TextMat"));
        OtherTextMaterial = new Material(Resources.Load<Material>("SceneMaterials/OtherTextMat"));

        Shader shader = LampPostMaterial.shader;
        _lampPostColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Colour"));
        _lampPostBaseColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColour"));

        shader = PavementMaterial.shader;
        _pavementColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Colour"));
        _pavementBaseColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColour"));
        
        shader = BuildingMaterial.shader;
        _buildingColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Colour"));
        _buildingBaseColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_BaseColour"));
        
        shader = BackgroundMaterial.shader;
        _backgroundMatTextAID = shader.GetPropertyNameId(shader.FindPropertyIndex("_TextA"));
        _backgroundMatTextBID = shader.GetPropertyNameId(shader.FindPropertyIndex("_TextB"));
        _backgroundMatLerpValID= shader.GetPropertyNameId(shader.FindPropertyIndex("_LerpVal"));
        
        shader = TextMaterial.shader;
        _textMatOutlineColID = shader.GetPropertyNameId(shader.FindPropertyIndex("_OutlineColor"));
        _textMatGlowColID = shader.GetPropertyNameId(shader.FindPropertyIndex("_GlowColor"));
        
        shader = OtherTextMaterial.shader;
        _otherTextColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_FaceColor"));
        
        LerpToScheme = __LerpToScheme;
    }

    public static void UpdateSceneMaterialsColours(MusicTrack track)
    {
        SceneColourScheme scheme = track.SceneColourScheme;

        UpdateMaterials(scheme, -1);

        currentColourScheme = scheme;
    }

    // NOTE(Zack): this is wrapped in an #ifdef because it is for testing only, and just incase I forget to remove it after testing
#if UNITY_EDITOR
    public static void UpdateSceneMaterialsColours(SceneColourScheme scheme) {
        // update most of the scene colours
        UpdateMaterials(scheme, -1);

        // this updates the [Characters] and [Building Facades]
        OnUpdateSceneColours?.Invoke(scheme);
        
        currentColourScheme = scheme;
    }
#endif

    private static void UpdateMaterials(SceneColourScheme scheme, float backgroundLerpValue)
    {
        // if (scheme.sceneColours.Length == 0) { scheme = defaultScheme; }

        LampPostMaterial.SetColor(_lampPostColourID, scheme.Find(SceneColourType.HighlightHDR));
        LampPostMaterial.SetColor(_lampPostBaseColourID, scheme.Find(SceneColourType.LampPostBackground));
        
        PavementMaterial.SetColor(_pavementColourID, scheme.Find(SceneColourType.HighlightHDR));
        PavementMaterial.SetColor(_pavementBaseColourID, scheme.Find(SceneColourType.PavementBackground));

        BuildingMaterial.SetColor(_buildingColourID, scheme.Find(SceneColourType.HighlightHDR));
        BuildingMaterial.SetColor(_buildingBaseColourID, scheme.Find(SceneColourType.BuildingBackground));
        
        TextMaterial.SetColor(_textMatOutlineColID, scheme.Find(SceneColourType.HighlightHDR));
        TextMaterial.SetColor(_textMatGlowColID, scheme.Find(SceneColourType.HighlightHDR));
        
        OtherTextMaterial.SetColor(_otherTextColourID, scheme.Find(SceneColourType.TextColour));

        if (maths.FloatCompare(backgroundLerpValue, -1))
        {
            BackgroundMaterial.SetTexture(_backgroundMatTextAID, scheme.backgroundSprite.texture);
            BackgroundMaterial.SetTexture(_backgroundMatTextBID, scheme.backgroundSprite.texture);
            BackgroundMaterial.SetFloat(_backgroundMatLerpValID, 0.5f);
        }
        else
        {
            BackgroundMaterial.SetTexture(_backgroundMatTextAID, currentColourScheme.backgroundSprite != null ? currentColourScheme.backgroundSprite.texture : scheme.backgroundSprite.texture);
            BackgroundMaterial.SetTexture(_backgroundMatTextBID, scheme.backgroundSprite.texture);
            BackgroundMaterial.SetFloat(_backgroundMatLerpValID, backgroundLerpValue);
        }
    }

    private static IEnumerator __LerpToScheme(SceneColourScheme colourScheme, float lerpTime)
    {
        float time = 0;
        SceneColourScheme tScheme; 
        SceneColourScheme startScheme = currentColourScheme;
        
        while (time < lerpTime)
        {
            float t = time / lerpTime;

            // update most of the scene colours
            tScheme = startScheme.LerpColours(colourScheme, t);
            UpdateMaterials(tScheme, t);

            // this updates the [Characters] and [Building Facades]
            OnUpdateSceneColours?.Invoke(tScheme);

            
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        // update most of the Scene Colours
        UpdateMaterials(colourScheme, 1);
        
        // this updates the [Characters] and [Building Facades]
        OnUpdateSceneColours?.Invoke(colourScheme);

        currentColourScheme = colourScheme;
    }
}

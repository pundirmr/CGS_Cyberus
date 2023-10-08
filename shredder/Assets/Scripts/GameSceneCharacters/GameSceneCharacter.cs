using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameSceneCharacterElements
{
    public Image characterImage;
    public Sprite characterHeadShot;
    public Transform lockOnTarget;

    private Material _imageMaterial;
    private int _glowColourID;
    private int _deltaXID;
    private int _deltaYID;

    public void SetImageColour(Color colour, Color glowColour)
    {
        if (_imageMaterial == null)
        {
            Setup();
        }

        characterImage.color = colour;
        _imageMaterial.SetColor(_glowColourID, glowColour);
    }

    public void SetImageLineThickness(float xThickness, float yThickness)
    {
        if (_imageMaterial == null)
        {
            Setup();
        }

        _imageMaterial.SetFloat(_deltaXID, xThickness);
        _imageMaterial.SetFloat(_deltaYID, yThickness);
    }

    public Color GetImageGlowColour()
    {
        if (_imageMaterial == null)
        {
            Setup();
        }

        return _imageMaterial.GetColor(_glowColourID);
    }

    private void Setup()
    {
        _imageMaterial = new Material(characterImage.material);
        Shader shader = _imageMaterial.shader;
        _glowColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_GlowColour"));
        _deltaXID = shader.GetPropertyNameId(shader.FindPropertyIndex("_DeltaX"));
        _deltaYID = shader.GetPropertyNameId(shader.FindPropertyIndex("_DeltaY"));
        characterImage.material = _imageMaterial;
    }

}

public class GameSceneCharacter : MonoBehaviour
{
    [SerializeField] private GameSceneCharacterElements[] characters;
    [SerializeField] private BackgroundObjectRandomiser randomiser;
    [Header("Animation Variables")]
    [SerializeField] private Color selectedCharacterBaseColour;
    [SerializeField][ColorUsage(true, true)] private Color selectedCharacterGlowColour;
    [SerializeField] private float animTime = 2;
    [SerializeField] private float flashCount = 3;
    [SerializeField] private float xLineThickness;
    [SerializeField] private float yLineThickness;

    public delegate IEnumerator CharacterPickAnim();
    public CharacterPickAnim PickAnim;

    private delegate IEnumerator CharacterFlashAnim(float flashTime, Color startBaseColour, Color lineStartColour);
    private CharacterFlashAnim _flashAnim;

    // private SceneColourScheme scheme;
    private int _currentActiveCharacter;
    private bool _nothingChosen;

    public Image     GetActiveCharacterImage()        {return characters[_currentActiveCharacter].characterImage;}
    public Sprite    GetActiveCharacterHeadShot()     {return characters[_currentActiveCharacter].characterHeadShot;}
    public Color     GetActiveCharacterGlowColour()   {return selectedCharacterGlowColour;}
    public Transform GetActiveCharacterLockOnTarget() { return characters[_currentActiveCharacter].lockOnTarget;}

    private void Awake()
    {
        PickAnim = StartSelectionEffects;
        _flashAnim = Flash;
        CharacterPicker.Characters.Add(this);
        randomiser.OnAssetSelected += OnAssetSelected;
        SceneColourer.OnUpdateSceneColours += OnUpdateSceneColours;
    }
    
    private void Start()
    {
        SceneColourScheme scheme = SceneColourer.currentColourScheme;
        UpdateColours(scheme);
    }

    private void UpdateColours(SceneColourScheme scheme)
    {
        Color baseColour = scheme.Find(SceneColourType.CharacterBaseColour);
        Color glowColour = scheme.Find(SceneColourType.CharacterGlowColour);
        foreach (var character in characters)
        {
            character.SetImageColour(baseColour, glowColour);
        }
    }

    private void OnUpdateSceneColours(SceneColourScheme scheme)
    {
        UpdateColours(scheme);
    }

    private void OnDestroy()
    {
        randomiser.OnAssetSelected -= OnAssetSelected;
        SceneColourer.OnUpdateSceneColours -= OnUpdateSceneColours;
        CharacterPicker.Characters.Remove(this);
    }

    private void OnAssetSelected(int assetIndex)
    {
        if (assetIndex == -1) //if randomiser has chosen an nothing then remove ourselves from the list so we dont get chosen.
        {
            if (!_nothingChosen)
            {
                CharacterPicker.Characters.Remove(this);
            }
            _nothingChosen = true;
            return;
        }

        _currentActiveCharacter = assetIndex;
    }

    private IEnumerator StartSelectionEffects()
    {
        if (_nothingChosen)
        {
            Log.Error("Character Picker has chosen an empty character, this shouldn't happen");
            yield break;
        }

        Color imageColour = characters[_currentActiveCharacter].characterImage.color;
        Color imageGlowColour = characters[_currentActiveCharacter].GetImageGlowColour();
        float time = animTime / flashCount;

        for (int i = 0; i < flashCount; i++)
        {
            yield return _flashAnim(time, imageColour, imageGlowColour);
        }

        time = 0;
        while (time < animTime * 0.5f) //lerp to glow to highlight character in scene
        {
            Color tImageColour = Color.Lerp(imageColour, selectedCharacterBaseColour, time / animTime);
            Color tImageGlowColour = Color.Lerp(imageGlowColour, selectedCharacterGlowColour, time / animTime);

            float txThickness = maths.Lerp(0, xLineThickness, time / animTime);
            float tyThickness = maths.Lerp(0, yLineThickness, time / animTime);

            characters[_currentActiveCharacter].SetImageColour(tImageColour, tImageGlowColour);
            characters[_currentActiveCharacter].SetImageLineThickness(txThickness, tyThickness);
            yield return CoroutineUtil.WaitForUpdate;
            time += Time.deltaTime;
        }
        characters[_currentActiveCharacter].SetImageColour(selectedCharacterBaseColour, selectedCharacterGlowColour);
        characters[_currentActiveCharacter].SetImageLineThickness(xLineThickness, yLineThickness);
    }

    private IEnumerator Flash(float flashTime, Color startBaseColour, Color lineStartColour)
    {
        float time = 0;
        while (time < animTime * 0.45f) //lerp to glow
        {
            Color tImageColour = Color.Lerp(startBaseColour, selectedCharacterBaseColour, time / animTime);
            Color tImageGlowColour = Color.Lerp(lineStartColour, selectedCharacterGlowColour, time / animTime);

            float txThickness = maths.Lerp(0, xLineThickness, time / animTime);
            float tyThickness = maths.Lerp(0, yLineThickness, time / animTime);

            characters[_currentActiveCharacter].SetImageColour(tImageColour, tImageGlowColour);
            characters[_currentActiveCharacter].SetImageLineThickness(txThickness, tyThickness);
            yield return CoroutineUtil.WaitForUpdate;
            time += Time.deltaTime;
        }
        characters[_currentActiveCharacter].SetImageColour(selectedCharacterBaseColour, selectedCharacterGlowColour);
        characters[_currentActiveCharacter].SetImageLineThickness(xLineThickness, yLineThickness);
        yield return CoroutineUtil.Wait(animTime * 0.1f);
        time = 0;
        while (time < animTime * 0.45f) //lerp back
        {
            Color tImageColour = Color.Lerp(selectedCharacterBaseColour, startBaseColour, time / animTime);
            Color tImageGlowColour = Color.Lerp(selectedCharacterGlowColour, lineStartColour, time / animTime);

            float txThickness = maths.Lerp(xLineThickness, 0, time / animTime);
            float tyThickness = maths.Lerp(yLineThickness, 0, time / animTime);

            characters[_currentActiveCharacter].SetImageColour(tImageColour, tImageGlowColour);
            characters[_currentActiveCharacter].SetImageLineThickness(txThickness, tyThickness);
            yield return CoroutineUtil.WaitForUpdate;
            time += Time.deltaTime;
        }
        characters[_currentActiveCharacter].SetImageColour(startBaseColour, lineStartColour);
        characters[_currentActiveCharacter].SetImageLineThickness(0, 0);
    }
}

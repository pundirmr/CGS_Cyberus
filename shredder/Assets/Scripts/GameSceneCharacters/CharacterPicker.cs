using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPicker : MonoBehaviour
{
    public static List<GameSceneCharacter> Characters = new List<GameSceneCharacter>(100);
    
    [SerializeField] private Transform selectionCrossHair;
    [SerializeField] private GameSceneCharacterUI uiCharacter;

    public delegate IEnumerator PickCharacter();
    public PickCharacter Pick;
    
    private GameSceneCharacter closestCharacter;

    private void Awake()
    {
        Pick = __Pick;
    }

    public GameSceneCharacter Select()
    {
        float dist = float.PositiveInfinity;
        closestCharacter = Characters[0];
        foreach (GameSceneCharacter character in Characters)
        {
            float charDist = maths.Abs((selectionCrossHair.transform.position - character.transform.position).magnitude);
            if (charDist < dist)
            {
                closestCharacter = character;
                dist = charDist;
            }
        }
        return closestCharacter;
    }

    private IEnumerator __Pick()
    {
        yield return closestCharacter.PickAnim();
        yield return CoroutineUtil.Wait(2);
    }

    public void ShowUiCharacter()
    {
        StartCoroutine(uiCharacter.ShowUiCharacter( closestCharacter.GetActiveCharacterHeadShot(), closestCharacter.GetActiveCharacterImage().color, closestCharacter.GetActiveCharacterGlowColour()));
    }

    private void OnDestroy()
    {
        // Characters.Clear();
    }
}

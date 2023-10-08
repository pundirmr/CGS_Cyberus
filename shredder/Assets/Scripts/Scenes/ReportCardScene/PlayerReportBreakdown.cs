using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerReportBreakdown : MonoBehaviour {        
    [Header("Prefab References")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private PlayerReportGrade grade;
    [SerializeField] private PlayerReportStats stats;

    /*[Header("SFX References")]
    [SerializeField] private AudioClip transitionSFX;
    */
    

    // components
    private PlayerID id;
    
    private static bool transitionSFXPlayed = false;
    
    private void Awake() {
        SFX.DEBUG_CreateSFXInstance();
        
        // components
        id = GetComponent<PlayerID>();

        // initialize components
        grade.Initialize(id);
        stats.Initialize(id);
        
        // we disable the main ui content on start for players that haven't joined the game
        contentParent.SetActive(false);
    }
    
    private IEnumerator Start() {
        yield return PlayerManager.WaitForValidPlayer(id.ID);
        StreamDeckManager.StreamDecks[id.ID].ClearDeck();



        // NOTE(Zack): we don't show a player if they have not joined before the [Report Scene]
        if (id.PlayerData.SceneJoinedOn == Scene.REPORT_SCENE && (Scene)SceneHandler.PreviousSceneIndex == Scene.GAME_SCENE) {
            yield break;
        }

        
        
        // we enable the content parent
        contentParent.SetActive(true);

        // setup relevant ui
        Canvas.ForceUpdateCanvases();
        grade.SetupUI();
        stats.SetupUI();

        // calculate the relevant stats and grade for the player
        grade.CalculateGrade();
        stats.CalculateStats();

        
        // while the countdown has not started we wait to start animating the player stats
        while (!SceneCountdown.HasStarted) yield return CoroutineUtil.WaitForUpdate;
        StartAnimatingScene();
        yield break;
    }

    private void OnDestroy() {
        transitionSFXPlayed = false;
    }
    


    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////// Animation Functions
    private void StartAnimatingScene() => StartCoroutine(AnimateScene());

    private IEnumerator AnimateScene() {
        // Enumerate the grade
        yield return grade.EnumerateGrade();
        yield return grade.MoveLaserOffTop();
        StartCoroutine(grade.HideGrade());

        // Enumerate the stats
        stats.SetStatsVisible();

        // doing this check because it's called for all players otherwise, and we only want it to play once
        // NOTE(Zack): this is a super dirty way of implementing this, but we're at the end of the project so....
        if (!transitionSFXPlayed) {
            transitionSFXPlayed = true;
            //SFX.PlayUIScene(transitionSFX);
            AudioEventSystem.TriggerEvent("PlayerResultsTransitionSFX", null);
        }
        
        yield return stats.ShowSeperationLinesAndAvatar();
        yield return stats.EnumerateTimingStatsAndBars();
        yield return stats.ShowSpamAndCombo();
        yield break;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/*This is the audio engine for the MainMenu, potential to use an interface and have on enable and on disable as
required implmentations if wanting to use different audio engines to reduce amount of code per audio engine.
Note: When using the AudioEventSystem to trigger events in this script the arguments are: string eventInstance and GameObject objectToAttachTo   ---  
Gameobject argument is only needed if you want to make an event 3D, set the argument to the object you would like to attach the event to for distance based attenuation ----
If 2D event, set GameObject argument to null on event trigger call*/

public class AudioEngine : MonoBehaviour
{

    public FmodEventReferences fmodEventReferences; // Use this to supply desired set of fmod event references
    public FmodRouting fmodRouting;
    public EventPlaybackState eventPbState;
    public FmodParameters fmodParams;

    public static AudioEngine audioEngineInstance => s_audioEngineInstance; //This static instance will be used to access other FMOD fuctionality found in seperate scripts (DO NOT USE TO START AND STOP EVENTS, USE THE AUDIO EVENT SYSTEM FOR THIS)
    private static AudioEngine s_audioEngineInstance;




    void Awake()
    {
        s_audioEngineInstance = this;
    }

    void Start()
    {
        fmodRouting.SetUpBuses();
    }

    //Start listening for events
    void OnEnable()
    {

        AudioEventSystem.StartListening("StartMenuMusic", StartMenuMusic);
        AudioEventSystem.StartListening("StopMenuMusic", StopMenuMusic);
        AudioEventSystem.StartListening("StartShowSFX", StartShowSFX);
        AudioEventSystem.StartListening("StartHideSFX", StartHideSFX);
        AudioEventSystem.StartListening("StartTitleTransitionSFX", StartTitleTransitionSFX);
        AudioEventSystem.StartListening("StartTransitionInSFX", StartTransitionInSFX);
        AudioEventSystem.StartListening("StartTransitionOutSFX", StartTransitionOutSFX);
        AudioEventSystem.StartListening("StartNewAvatarChosenSFX", StartNewAvatarChosenSFX);
        AudioEventSystem.StartListening("StartSameAvatarChosenSFX", StartSameAvatarChosenSFX);
        AudioEventSystem.StartListening("StartCardsTransitionInSFX", StartCardsTransitionInSFX);
        AudioEventSystem.StartListening("StartCardsTransitionOutSFX", StartCardsTransitionOutSFX);
        AudioEventSystem.StartListening("StartCardsScaleBackSFX", StartCardsScaleBackSFX);
        AudioEventSystem.StartListening("StartRouletteTickSFX", StartRouletteTickSFX);
        AudioEventSystem.StartListening("StartRouletteSelectedSFX", StartRouletteSelectedSFX);
        AudioEventSystem.StartListening("StartCyberusVO", StartCyberusVO);


        //Avatar Select Scene
        AudioEventSystem.StartListening("StartAvatarSelectVO", StartAvatarSelectVO);

        //Game Scene
        AudioEventSystem.StartListening("StartGameMusic", StartGameMusic);
        AudioEventSystem.StartListening("PreGameEmailMessageSFX", PreGameEmailMessageSFX);
        AudioEventSystem.StartListening("PostGameEmailShowSFX", PostGameEmailShowSFX);
        AudioEventSystem.StartListening("PostGameEmailHideSFX", PostGameEmailHideSFX);
        AudioEventSystem.StartListening("PostGameEmailKeywordsSFX", PostGameEmailKeywordsSFX);
        AudioEventSystem.StartListening("PostGameEmailKeywordEnumerateSFX", PostGameEmailKeywordEnumerateSFX);
        AudioEventSystem.StartListening("PostGameEmailStaticLoop", PostGameEmailStaticLoop);
        AudioEventSystem.StartListening("PostGameEmailClearedShowSFX", PostGameEmailClearedShowSFX);
        AudioEventSystem.StartListening("PostGameEmailClearedHide", PostGameEmailClearedHide);
        AudioEventSystem.StartListening("WarningClaxonSFX", WarningClaxonSFX);
        AudioEventSystem.StartListening("SearchingScanSFX", SearchingScanSFX);
        AudioEventSystem.StartListening("ClearLaneSFX", ClearLaneSFX);
        AudioEventSystem.StartListening("HeartAberrationSFX", HeartAberrationSFX);
        AudioEventSystem.StartListening("DifficultyChangeSFX", DifficultyChangeSFX);
        AudioEventSystem.StartListening("StartPostGameBackgroundMusicLoop", StartPostGameBackgroundMusicLoop);
        AudioEventSystem.StartListening("StopPostGameBackgroundMusicLoop", StopPostGameBackgroundMusicLoop);
        AudioEventSystem.StartListening("SpamAttackVO", SpamAttackVO);






        //Results Scene
        AudioEventSystem.StartListening("PlayerResultsTransitionSFX", PlayerResultsTransitionSFX);
        AudioEventSystem.StartListening("PlayerResultsEnumerateSFX", PlayerResultsEnumerateSFX);
        AudioEventSystem.StartListening("MoveLaserOffScreenSFX", MoveLaserOffScreenSFX);
        AudioEventSystem.StartListening("StaticStopSFX", StaticStopSFX);
        AudioEventSystem.StartListening("BarsSFX", BarsSFX);

        //Player voice over events
        AudioEventSystem.StartListening("PlayerJoinedVO", PlayerJoinedVO);
        AudioEventSystem.StartListening("PlayerDisconnectedVO", PlayerDisconnectedVO);



        

    }

    //Disable listeners for events 
    void OnDisable()
    {


        AudioEventSystem.StopListening("StartMenuMusic", StartMenuMusic);
        AudioEventSystem.StopListening("StopMenuMusic", StopMenuMusic);
        AudioEventSystem.StopListening("StartShowSFX", StartShowSFX);
        AudioEventSystem.StopListening("StartHideSFX", StartHideSFX);
        AudioEventSystem.StopListening("StartTitleTransitionSFX", StartTitleTransitionSFX);
        AudioEventSystem.StopListening("StartCyberusVO", StartCyberusVO);


        AudioEventSystem.StopListening("StartTransitionInSFX", StartTransitionInSFX);
        AudioEventSystem.StopListening("StartTransitionOutSFX", StartTransitionOutSFX);
        AudioEventSystem.StopListening("StartNewAvatarChosenSFX", StartNewAvatarChosenSFX);
        AudioEventSystem.StopListening("StartSameAvatarChosenSFX", StartSameAvatarChosenSFX);
        AudioEventSystem.StopListening("StartSameAvatarChosenSFX", StartSameAvatarChosenSFX);
        AudioEventSystem.StopListening("StartCardsTransitionInSFX", StartCardsTransitionInSFX);
        AudioEventSystem.StopListening("StartCardsTransitionOutSFX", StartCardsTransitionOutSFX);
        AudioEventSystem.StopListening("StartCardsScaleBackSFX", StartCardsScaleBackSFX);
        AudioEventSystem.StopListening("StartRouletteTickSFX", StartRouletteTickSFX);
        AudioEventSystem.StopListening("StartRouletteSelectedSFX", StartRouletteSelectedSFX);


        //Avatar Select scene
        AudioEventSystem.StopListening("StartAvatarSelectVO", StartAvatarSelectVO);
        
        //Game Scene 
        AudioEventSystem.StopListening("StartGameMusic", StartGameMusic);
        AudioEventSystem.StopListening("PreGameEmailMessageSFX", PreGameEmailMessageSFX);
        AudioEventSystem.StopListening("PostGameEmailShowSFX", PostGameEmailShowSFX);
        AudioEventSystem.StopListening("PostGameEmailHideSFX", PostGameEmailHideSFX);
        AudioEventSystem.StopListening("PostGameEmailKeywordsSFX", PostGameEmailKeywordsSFX);
        AudioEventSystem.StopListening("PostGameEmailKeywordEnumerateSFX", PostGameEmailKeywordEnumerateSFX);
        AudioEventSystem.StopListening("PostGameEmailStaticLoop", PostGameEmailStaticLoop);
        AudioEventSystem.StopListening("PostGameEmailClearedShowSFX", PostGameEmailClearedShowSFX);
        AudioEventSystem.StopListening("PostGameEmailClearedHide", PostGameEmailClearedHide);
        AudioEventSystem.StopListening("WarningClaxonSFX", WarningClaxonSFX);
        AudioEventSystem.StopListening("SearchingScanSFX", SearchingScanSFX);
        AudioEventSystem.StopListening("ClearLaneSFX", ClearLaneSFX);
        AudioEventSystem.StopListening("HeartAberrationSFX", HeartAberrationSFX);
        AudioEventSystem.StopListening("DifficultyChangeSFX", DifficultyChangeSFX);
        AudioEventSystem.StopListening("StartPostGameBackgroundMusicLoop", StartPostGameBackgroundMusicLoop);
        AudioEventSystem.StopListening("StopPostGameBackgroundMusicLoop", StopPostGameBackgroundMusicLoop);
        AudioEventSystem.StopListening("SpamAttackVO", SpamAttackVO);

        //Results Scene
        AudioEventSystem.StopListening("PlayerResultsTransitionSFX", PlayerResultsTransitionSFX);
        AudioEventSystem.StopListening("PlayerResultsEnumerateSFX", PlayerResultsEnumerateSFX);
        AudioEventSystem.StopListening("MoveLaserOffScreenSFX", MoveLaserOffScreenSFX);
        AudioEventSystem.StopListening("StaticStopSFX", StaticStopSFX);
        AudioEventSystem.StopListening("BarsSFX", BarsSFX);

        //Player voice over events
        AudioEventSystem.StopListening("PlayerJoinedVO", PlayerJoinedVO);
        AudioEventSystem.StopListening("PlayerDisconnectedVO", PlayerDisconnectedVO);

    }

    //Creates instance of music looping fmod event and then starts it
    private void StartMenuMusic(GameObject objectToAttachTo) 
    {
        fmodEventReferences.MenuMusicInstance();
        fmodEventReferences.menuMusicInstance.start();
    }

    private void StopMenuMusic(GameObject objectToAttachTo) 
    {
        fmodEventReferences.menuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);//Stop loop
    }

    private void StartCyberusVO(GameObject objectToAttachTo)
    {
        fmodEventReferences.CyberusVOInstance();
        fmodEventReferences.cyberusVOInstance.start();
        fmodEventReferences.cyberusVOInstance.release();

    }

    private void StartShowSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.ShowSFXInstance();
        fmodEventReferences.showSFXInstance.start();
        fmodEventReferences.showSFXInstance.release();
    }

    private void StartHideSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.HideSFXInstance();
        fmodEventReferences.hideSFXInstance.start();
        fmodEventReferences.hideSFXInstance.release();
    }

    private void StartCountDownSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.CountDownSFXInstance();
        fmodEventReferences.countDownSFXInstance.start();
        fmodEventReferences.countDownSFXInstance.release();
    }

    private void StartTitleTransitionSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.TitleTransitionSFXInstance();
        fmodEventReferences.titleTransitionSFXInstance.start();
        fmodEventReferences.titleTransitionSFXInstance.release();
    }

    private void StartTransitionInSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.TransitionInSFXInstance();
        fmodEventReferences.transitionInSFXInstance.start();
        fmodEventReferences.transitionInSFXInstance.release();
    }

    private void StartTransitionOutSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.TransitionInSFXInstance();
        fmodEventReferences.transitionInSFXInstance.start();
        fmodEventReferences.transitionInSFXInstance.release();
    }

    private void StartNewAvatarChosenSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.NewAvatarChosenSFXInstance();
        fmodEventReferences.newChosenSFXInstance.start();
        fmodEventReferences.newChosenSFXInstance.release();
    }

    private void StartSameAvatarChosenSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.SameAvatarChosenSFXInstance();
        fmodEventReferences.sameChosenSFXInstance.start();
        fmodEventReferences.sameChosenSFXInstance.release();
    }

    //Avatar SelectScene
    private void StartAvatarSelectVO(GameObject objectToAttachTo)
    {
        fmodEventReferences.AvatarSelectVOInstance();
        fmodEventReferences.avatarSelectVOInstance.start();
        fmodEventReferences.avatarSelectVOInstance.release();
    }


    // Track select scene
    private void StartCardsTransitionInSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.CardsTransitionInSFXInstance();
        fmodEventReferences.cardsTransitionInSFXInstance.start();
        fmodEventReferences.cardsTransitionInSFXInstance.release();
    }

    private void StartCardsTransitionOutSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.CardsTransitionOutSFXInstance();
        fmodEventReferences.cardsTransitionOutSFXInstance.start();
        fmodEventReferences.cardsTransitionOutSFXInstance.release();
    }

    private void StartCardsScaleBackSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.CardsScaleBackSFXInstance();
        fmodEventReferences.cardsScaleBackSFXInstance.start();
        fmodEventReferences.cardsScaleBackSFXInstance.release();
    }

    private void StartRouletteTickSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.RouletteTickSFXInstance();
        fmodEventReferences.rouletteTickSFXInstance.start();
        fmodEventReferences.rouletteTickSFXInstance.release();
    }

    private void StartRouletteSelectedSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.RouletteSelectedSFXInstance();
        fmodEventReferences.rouletteSelectedSFXInstance.start();
        fmodEventReferences.rouletteSelectedSFXInstance.release();
    }

    public float GetCurrentPlaybackTime()
    {
        int position = 0;
        fmodEventReferences.gameMusicInstance.getTimelinePosition(out position);
        return position / 1000.0f;
    }


    //Game Scene
    private void StartGameMusic(GameObject objectToAttachTo)
    {
        fmodEventReferences.GameSceneMusicInstance();
        fmodEventReferences.gameMusicInstance.start();
        fmodEventReferences.gameMusicInstance.release();

    }
    private void PreGameEmailMessageSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PreGameEmailMessageSFXInstance();
        fmodEventReferences.preGameEmailMessageSFXInstance.start();
        fmodEventReferences.preGameEmailMessageSFXInstance.release();
    }

    private void PostGameEmailShowSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameEmailShowSFXInstance();
        fmodEventReferences.postGameEmailShowSFXInstance.start();
        fmodEventReferences.postGameEmailShowSFXInstance.release();
    }

    private void PostGameEmailHideSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameEmailHideSFXInstance();
        fmodEventReferences.postGameEmailHideSFXInstance.start();
        fmodEventReferences.postGameEmailHideSFXInstance.release();
    }

    private void PostGameEmailKeywordsSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameEmailKeywordsSFXInstance();
        fmodEventReferences.postGameEmailKeywordsSFXInstance.start();
        fmodEventReferences.postGameEmailKeywordsSFXInstance.release();
    }

    private void PostGameEmailKeywordEnumerateSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameEmailKeywordEnumerateSFXInstance();
        fmodEventReferences.postGameEmailKeywordEnumerateSFXInstance.start();
        fmodEventReferences.postGameEmailKeywordEnumerateSFXInstance.release();
    }

    private void PostGameEmailStaticLoop(GameObject objectToAttachTo) ///////////////////////////////////// Need to provide event not string and start loop and find where to release loop with fade
    {
        fmodEventReferences.PostGameEmailStaticLoopInstance();
        fmodEventReferences.postGameEmailStaticLoopInstance.start();


    }

    private void PostGameEmailClearedShowSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.postGameEmailClearedShowSFXInstance.start();
        fmodEventReferences.postGameEmailClearedShowSFXInstance.release();
        fmodEventReferences.PostGameEmailClearedShowSFXInstance();
    }

    private void PostGameEmailClearedHide(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameEmailClearedHideSFXInstance();
        fmodEventReferences.postGameEmailClearedHideSFXInstance.start();
        fmodEventReferences.postGameEmailClearedHideSFXInstance.release();
    }


    private void WarningClaxonSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.WarningClaxonSFXInstance();
        fmodEventReferences.warningClaxonSFXInstance.start();
        fmodEventReferences.warningClaxonSFXInstance.release();
    }


    private void SearchingScanSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.SearchingScanSFXInstance();
        fmodEventReferences.searchingScanSFXInstance.start();
        fmodEventReferences.searchingScanSFXInstance.release();
    }

    private void SpamAttackVO(GameObject objectToAttachTo)
    {
        fmodEventReferences.SpamAttackVOInstance();
        fmodEventReferences.spamAttackVOInstance.start();
        fmodEventReferences.spamAttackVOInstance.release();
    }
    private void ClearLaneSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.ClearLaneSFXInstance();
        fmodEventReferences.clearLaneSFXInstance.start();
        fmodEventReferences.clearLaneSFXInstance.release();
    }


    private void HeartAberrationSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.HeartAberrationSFXInstance();
        fmodEventReferences.heartAberrationSFXInstance.start();
        fmodEventReferences.heartAberrationSFXInstance.release();
    }


    private void DifficultyChangeSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.DifficultyChangeSFXInstance();
        fmodEventReferences.difficultyChangeSFXInstance.start();
        fmodEventReferences.difficultyChangeSFXInstance.release();
    }

    private void StartPostGameBackgroundMusicLoop(GameObject objectToAttachTo)
    {
        fmodEventReferences.PostGameBackgroundMusicLoopInstance();
        fmodEventReferences.postGameBackgroundMusicLoopInstance.start();

    }

    private void StopPostGameBackgroundMusicLoop(GameObject objectToAttachTo)
    {

        fmodEventReferences.postGameBackgroundMusicLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);//Stop loop
        fmodEventReferences.postGameBackgroundMusicLoopInstance.release(); //Release instance from memory

    }

    //Results Scene

    private void PlayerResultsTransitionSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PlayerResultsTransitionSFXInstance();
        fmodEventReferences.playerResultsTransitionSFXInstance.start();
        fmodEventReferences.playerResultsTransitionSFXInstance.release();
    }

    private void PlayerResultsEnumerateSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.PlayerResultsEnumerateSFXInstance();
        fmodEventReferences.playerResultsEnumerateSFXInstance.start();
        fmodEventReferences.playerResultsEnumerateSFXInstance.release();
    }

    private void MoveLaserOffScreenSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.MoveLaserOffScreenSFXInstance();
        fmodEventReferences.moveLaserOffScreenSFXInstance.start();
        fmodEventReferences.moveLaserOffScreenSFXInstance.release();
    }

    private void StaticStopSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.StaticStopSFXInstance();
        fmodEventReferences.staticStopSFXInstance.start();
        fmodEventReferences.staticStopSFXInstance.release();
    }

    private void BarsSFX(GameObject objectToAttachTo)
    {
        fmodEventReferences.BarsSFXInstance();
        fmodEventReferences.barsSFXInstance.start();
        fmodEventReferences.barsSFXInstance.release();
    }

    //Player Voice Over events
    private void PlayerJoinedVO(GameObject objectToAttachTo)
    {
        fmodEventReferences.PlayerJoinedVOInstance();
        fmodEventReferences.playerJoinedVOInstance.start();
        fmodEventReferences.playerJoinedVOInstance.release();
    }

    private void PlayerDisconnectedVO(GameObject objectToAttachTo)
    {
        fmodEventReferences.PlayerDisconnectedVOInstance();
        fmodEventReferences.playerDisconnectedVOInstance.start();
        fmodEventReferences.playerDisconnectedVOInstance.release();
    }

}

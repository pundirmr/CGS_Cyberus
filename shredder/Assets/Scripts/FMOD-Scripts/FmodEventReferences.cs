using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is where string references to all the event instances will go and instances will be initialized

[CreateAssetMenu(menuName = "SO/Audio/EventReferences", fileName = "New Event Reference Sheet")]
public class FmodEventReferences : ScriptableObject
{
    //Setting inspector heading
    [Header("Music")]
    //Declaring music instance and event string
    [SerializeField] private string menuMusicEventName = null;
    public FMOD.Studio.EventInstance menuMusicInstance;

    [Header("Non-Diagetic Audio")]
    [SerializeField] private string showSFXEventName = null;
    public FMOD.Studio.EventInstance showSFXInstance;

    [SerializeField] private string hideSFXEventName = null;
    public FMOD.Studio.EventInstance hideSFXInstance;

    [SerializeField] private string countDownSFXEventName = null;
    public FMOD.Studio.EventInstance countDownSFXInstance;


    [SerializeField] private string titleTransitionSFXEventName = null;
    public FMOD.Studio.EventInstance titleTransitionSFXInstance;

    [SerializeField] private string transitionInSFXEventName = null;
    public FMOD.Studio.EventInstance transitionInSFXInstance;


    [SerializeField] private string transitionOutSFXEventName = null;
    public FMOD.Studio.EventInstance transitionOutSFXInstance;

    [SerializeField] private string newChosenSFXEventName = null;
    public FMOD.Studio.EventInstance newChosenSFXInstance;

    [SerializeField] private string cyberusVOEventName = null;
    public FMOD.Studio.EventInstance cyberusVOInstance;


    [SerializeField] private string sameChosenSFXEventName = null;
    public FMOD.Studio.EventInstance sameChosenSFXInstance;

    [Header("Avatar Select Scene Audio")]
    public FMOD.Studio.EventInstance avatarSelectVOInstance;
    public FMODUnity.EventReference avatarSelectVOEvent;



    [Header("Track Select Scene Audio")]
    [SerializeField] private string cardsTransitionInSFXEventName = null;
    public FMOD.Studio.EventInstance cardsTransitionInSFXInstance;

    [SerializeField] private string cardsTransitionOutSFXEventName = null;
    public FMOD.Studio.EventInstance cardsTransitionOutSFXInstance;

    [SerializeField] private string cardsScaleBackSFXEventName = null;
    public FMOD.Studio.EventInstance cardsScaleBackSFXInstance;

    [SerializeField] private string rouletteTickSFXEventName = null;
    public FMOD.Studio.EventInstance rouletteTickSFXInstance;

    [SerializeField] private string rouletteSelectedSFXEventName = null;
    public FMOD.Studio.EventInstance rouletteSelectedSFXInstance;


    [Header("Game Scene Audio")]

    public FMOD.Studio.EventInstance gameMusicInstance;
    public FMODUnity.EventReference gameMusicEvent;

    [SerializeField] private string preGameEmailMessageSFXEventName = null;
    public FMOD.Studio.EventInstance preGameEmailMessageSFXInstance;

    [SerializeField] private string postGameEmailShowSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailShowSFXInstance;

    [SerializeField] private string postGameEmailHideSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailHideSFXInstance;

    [SerializeField] private string postGameEmailKeywordsSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailKeywordsSFXInstance;

    [SerializeField] private string postGameEmailKeywordEnumerateSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailKeywordEnumerateSFXInstance;

    [SerializeField] private string postGameEmailStaticLoopEventName = null; //Looping Event NEED TO CREATE STOP EVENT
    public FMOD.Studio.EventInstance postGameEmailStaticLoopInstance;

    [SerializeField] private string postGameEmailClearedShowSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailClearedShowSFXInstance;

    [SerializeField] private string postGameEmailClearedHideSFXEventName = null;
    public FMOD.Studio.EventInstance postGameEmailClearedHideSFXInstance;

    [SerializeField] private string warningClaxonSFXEventName = null;
    public FMOD.Studio.EventInstance warningClaxonSFXInstance;

    [SerializeField] private string searchingScanSFXEventName = null;
    public FMOD.Studio.EventInstance searchingScanSFXInstance;

    public FMOD.Studio.EventInstance spamAttackVOInstance;
    public FMODUnity.EventReference spamAttackEvent;

    [SerializeField] private string clearLaneSFXEventName = null;
    public FMOD.Studio.EventInstance clearLaneSFXInstance;

    public FMOD.Studio.EventInstance heartAberrationSFXInstance;
    public FMODUnity.EventReference heartAberrationSFXEvent;

    [SerializeField] private string difficultyChangeSFXEventName = null;
    public FMOD.Studio.EventInstance difficultyChangeSFXInstance;

    public FMOD.Studio.EventInstance postGameBackgroundMusicLoopInstance;
    public FMODUnity.EventReference postGameBackgroundMusicLoopEvent;


    [Header("Results Scene Audio")]

    [SerializeField] private string playerResultsTransitionSFXEventName = null;
    public FMOD.Studio.EventInstance playerResultsTransitionSFXInstance;

    [SerializeField] private string playerResultsEnumerateSFXEventName;
    public FMOD.Studio.EventInstance playerResultsEnumerateSFXInstance;

    [SerializeField] private string moveLaserOffScreenSFXEventName;
    public FMOD.Studio.EventInstance moveLaserOffScreenSFXInstance;

    [SerializeField] private string staticStopSFXEventName;
    public FMOD.Studio.EventInstance staticStopSFXInstance;

    [SerializeField] private string barsSFXEventName;
    public FMOD.Studio.EventInstance barsSFXInstance;

    //Player Voice
    [SerializeField] private string playerJoinedVOEventName;
    public FMOD.Studio.EventInstance playerJoinedVOInstance;

    [SerializeField] private string playerDisconnectedVOEventName;
    public FMOD.Studio.EventInstance playerDisconnectedVOInstance;







    public void MenuMusicInstance()
    {
        menuMusicInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/" + menuMusicEventName);

    }

    public void CyberusVOInstance()
    {
        cyberusVOInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/" + cyberusVOEventName);
    }

    public void ShowSFXInstance()
    {
        showSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ReusedUI/" + showSFXEventName);
    }

    public void HideSFXInstance()
    {
        hideSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ReusedUI/" + hideSFXEventName);
    }

    public void CountDownSFXInstance()
    {
        countDownSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ReusedUI/ReusedUI/" + countDownSFXEventName);
    }

    public void TitleTransitionSFXInstance()
    {
        titleTransitionSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ReusedUI/" + titleTransitionSFXEventName);
    }

    public void TransitionInSFXInstance()
    {
        transitionInSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + transitionInSFXEventName);
    }

    public void TransitionOutSFXInstance()
    {
        transitionOutSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + transitionOutSFXEventName);
    }

    public void NewAvatarChosenSFXInstance()
    {
        newChosenSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/AvatarSelectScene/" + newChosenSFXEventName);
    }

    public void SameAvatarChosenSFXInstance()
    {
        sameChosenSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/AvatarSelectScene/" + sameChosenSFXEventName);
    }


    //Avatar Select scenet
    public void AvatarSelectVOInstance()
    {
        avatarSelectVOInstance = FMODUnity.RuntimeManager.CreateInstance(avatarSelectVOEvent);
    }


    //Track selected scene
    public void CardsTransitionInSFXInstance()
    {
        cardsTransitionInSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/SelectTrackScene/" + cardsTransitionInSFXEventName);
    }

    public void CardsTransitionOutSFXInstance()
    {
        cardsTransitionOutSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/SelectTrackScene/" + cardsTransitionOutSFXEventName);
    }

    public void CardsScaleBackSFXInstance()
    {
        cardsScaleBackSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/SelectTrackScene/" + cardsScaleBackSFXEventName);
    }

    public void RouletteTickSFXInstance()
    {
        rouletteTickSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/SelectTrackScene/" + rouletteTickSFXEventName);
    }

    public void RouletteSelectedSFXInstance()
    {
        rouletteSelectedSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/SelectTrackScene/" + rouletteSelectedSFXEventName);
    }



    //Game Scene
    public void GameSceneMusicInstance()
    {
        gameMusicInstance = FMODUnity.RuntimeManager.CreateInstance(gameMusicEvent);
    }
    public void PreGameEmailMessageSFXInstance()
    {
        preGameEmailMessageSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + preGameEmailMessageSFXEventName);
    }

    public void PostGameEmailShowSFXInstance()
    {
        postGameEmailShowSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailShowSFXEventName);
    }

    public void PostGameEmailHideSFXInstance()
    {
        postGameEmailHideSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailHideSFXEventName);
    }

    public void PostGameEmailKeywordsSFXInstance()
    {
        postGameEmailKeywordsSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailKeywordsSFXEventName);
    }

    public void PostGameEmailKeywordEnumerateSFXInstance()
    {
        postGameEmailKeywordEnumerateSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailKeywordEnumerateSFXEventName);
    }

    public void PostGameEmailStaticLoopInstance() ///////////////////////////////////// Need to provide event not string and start loop and find where to release loop with fade
    {
        postGameEmailStaticLoopInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailStaticLoopEventName);

    }

    public void PostGameEmailClearedShowSFXInstance()
    {
        postGameEmailClearedShowSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailClearedShowSFXEventName);
    }

    public void PostGameEmailClearedHideSFXInstance()
    {
        postGameEmailClearedHideSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + postGameEmailClearedHideSFXEventName);
    }


    public void WarningClaxonSFXInstance()
    {
        warningClaxonSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + warningClaxonSFXEventName);
    }


    public void SearchingScanSFXInstance()
    {
        searchingScanSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + searchingScanSFXEventName);
    }

    public void SpamAttackVOInstance()
    {
        spamAttackVOInstance = FMODUnity.RuntimeManager.CreateInstance(spamAttackEvent);
    }

    public void ClearLaneSFXInstance()
    {
        clearLaneSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + clearLaneSFXEventName);
    }


    public void HeartAberrationSFXInstance()
    {
        heartAberrationSFXInstance = FMODUnity.RuntimeManager.CreateInstance(heartAberrationSFXEvent);
    }


    public void DifficultyChangeSFXInstance()
    {
        difficultyChangeSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/GameScene/" + difficultyChangeSFXEventName);
    }

    public void PostGameBackgroundMusicLoopInstance()
    {
        postGameBackgroundMusicLoopInstance = FMODUnity.RuntimeManager.CreateInstance(postGameBackgroundMusicLoopEvent);
    }

    //Results Scene

    public void PlayerResultsTransitionSFXInstance()
    {
        playerResultsTransitionSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + playerResultsTransitionSFXEventName);
    }

    public void PlayerResultsEnumerateSFXInstance()
    {
        playerResultsEnumerateSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + playerResultsEnumerateSFXEventName);
    }

    public void MoveLaserOffScreenSFXInstance()
    {
        moveLaserOffScreenSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + moveLaserOffScreenSFXEventName);
    }

    public void StaticStopSFXInstance()
    {
        staticStopSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + staticStopSFXEventName);
    }

    public void BarsSFXInstance()
    {
        barsSFXInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/ResultsScene/" + barsSFXEventName);
    }

    //Player voice over's instances
    public void PlayerJoinedVOInstance()
    {
        playerJoinedVOInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/PlayerVOs/" + playerJoinedVOEventName);
    }

    public void PlayerDisconnectedVOInstance()
    {
        playerDisconnectedVOInstance = FMODUnity.RuntimeManager.CreateInstance("Event:/PlayerVOs/" + playerDisconnectedVOEventName);
    }







}

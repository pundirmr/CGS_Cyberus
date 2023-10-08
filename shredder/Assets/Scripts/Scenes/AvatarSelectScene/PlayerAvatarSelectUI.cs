using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

// NOTE(Zack): this is to attempt to stop so much memory being deallocated each scene transition
public static class StaticSDAvatarScene {
    private const int NumOfAvatars = 6;
    private const int NumOfColours = 6;

    public static readonly ConsumableAction.Delegate[][] AvatarLambdas = new [] {
        new ConsumableAction.Delegate[NumOfAvatars],
        new ConsumableAction.Delegate[NumOfAvatars],
        new ConsumableAction.Delegate[NumOfAvatars]
    };

    public static readonly ConsumableAction.Delegate[][] ColourLambdas = new [] {
        new ConsumableAction.Delegate[NumOfColours],
        new ConsumableAction.Delegate[NumOfColours],
        new ConsumableAction.Delegate[NumOfColours]
    };
}

[Serializable]
public struct AvatarUIRefs {
    public GameObject[] objs;
    public Image[] avatars;
    public AberrationEffect[] imageEffects;
    public Material[] imageMaterials;
    public LaserUiEffect laserEffect;
    public TMP_Text avatarName;
    public TMP_Text playerNumber;
    public Image gradient;

    public void SetActive(bool active) {
        for (int i = 0; i < objs.Length; ++i) {
            objs[i].SetActive(active);
        }
    }

    public void SetAvatars(ref Sprite[] sprites) {
        for (int i = 0; i < avatars.Length; ++i) {
            ref var a = ref avatars[i];
            a.sprite = sprites[i];
        }
    }

    public void SetAvatarColour(ref Color32[] colours) {
        for (int i = 0; i < avatars.Length; ++i) {
            ref var a = ref avatars[i];
            a.color = colours[i];
        }
    }
}

[RequireComponent(typeof(PlayerID))]
public class PlayerAvatarSelectUI : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private AvatarUIRefs ui;

    [Header("Stream Deck")]
    [SerializeField] private ButtonIndices[] avatarButtons;
    [SerializeField] private ButtonIndices[] colourButtons;
    [SerializeField] private ButtonIndices   idleButtons;

    [Header("Aberration Effect Settings")]
    [SerializeField] private float effectTime                                 = 1;
    [SerializeField] private int aberrationAmountSize                         = 6;
    [SerializeField] private float effectFlickerTime                          = 0.1f;
    [SerializeField] private float avatarRepeatTransitionAberrationMultiplier = 0.25f;
    [SerializeField] private float colourTransitionAberrationMultiplier       = 0.1f;
    [SerializeField] private float duplicateColourSelectionMultiplier         = 0.25f;
    
    [Header("Misc SFX References")]
    [SerializeField] private string[] avatarsVOName;

    /*
    [SerializeField] private AudioClip newChosenSFX;
    [SerializeField] private AudioClip sameChosenSFX; */

    [Header("Idle Button Settings")]
    [SerializeField] private LaserUiEffect laser;
    [SerializeField] private float laserAmplitudeReduction = 4;

    
    // components
    private PlayerID id;
    private PlayerData data => id.PlayerData;
    
    private const int unchosenIndex = -1;
    private int avatarIndex => id.PlayerData.AvatarIndex;
    private int colourIndex => id.PlayerData.ColourIndex;

    
    private Coroutine lerpCo;

    private delegate IEnumerator UITransition(int index);
    private UITransition SetAvatar;
    private UITransition SetColour;
    private Coroutine _avatarTransitionCo;
    private Coroutine _colourTransitionCo;

    private delegate IEnumerator AberrationDel(int uiIndex, bool resetAfter, float effectMultiplier, bool useTint = true);
    private AberrationDel AberrationEffect;

    private bool subbedToPlayerManager = false;
    private bool subbedToDisconnect = false;
    private bool inputSub = false;

    private StreamDeck deck => StreamDeckManager.StreamDecks[id.ID];
    
    private void Awake() {
        SFX.DEBUG_CreateSFXInstance();
        VoiceOver.DEBUG_CreateSFXInstance();
        
        
        // delegate allocations
        SetAvatar        = __SetAvatar;
        SetColour        = __SetColour;
        AberrationEffect = __AberrationEffect;

        // components
        id = GetComponent<PlayerID>();
        
        // set the avatar ui to disabled
        ui.SetActive(false);
        
        // event setup
        PlayerManager.onPlayerJoined += OnPlayerJoined;
        subbedToPlayerManager = true;
    }

    private void OnDestroy() {        
        UnsubFromInputAndCountdown();
        UnsubFromPlayerManager();
        
        // clear the streamdeck ready for the next scene, and check that the indexs have been set within an acceptable range
        if (id.IsValid) {
            Debug.Assert(id.PlayerData.AvatarIndex > -1 && id.PlayerData.AvatarIndex < 6);
            Debug.Assert(id.PlayerData.ColourIndex > -1 && id.PlayerData.ColourIndex < 6);
            deck.ClearDeck(); // this causes a null ref if we stop running unity but is not an issue in gameplay
        }

        // unsub from the setting of the stream deck ui
        if (!subbedToDisconnect) return;
        deck.OnConnect -= SetStreamDeckImages;
    }

    private void Start() {
        // all players coming from [MainMenu] will pass this check.
        // all uninitialized players will call [Setup] from the [OnPlayerJoined] function
        if (!PlayerManager.IsPlayerValid(id.ID)) return;
        StartCoroutine(Setup());
    }

    // players joining mid way through [AvatarSelect] will initialize using this function
    private void OnPlayerJoined(int playerID) {
        // this should hopefully stop players joining mid transition
        if (SceneCountdown.IsFinished) return;
        if (id.ID != playerID) return;

        // unsub so that we aren't listening to this call again
        UnsubFromPlayerManager();        
        StartCoroutine(Setup());
    }


    
    
    //////////////////////////////////////////////////////////////////////////////////////////////////
    /////////// Setup Functions    
    private IEnumerator Setup() {
        // clear the stream deck ready for new layout
        if (StreamDeckManager.StreamDeckCount > 0) //added
        {
            StreamDeckManager.StreamDecks[id.ID].ClearDeck();
        }
        

        // setup the tv ui for the player
        ui.SetActive(true);
        ui.SetAvatars(ref id.PlayerData.Avatar.Sprites); // NOTE(Zack): relying PlayerData being initialized correctly
        ui.SetAvatarColour(ref id.PlayerData.ColourScheme.Colours);
        
        ui.gradient.color    = id.PlayerData.ColourScheme.CardBGColour;
        ui.avatarName.text   = id.PlayerData.Avatar.Name;
        ui.playerNumber.text = StaticStrings.IDs[id.ID];


        // we wait to setup the streamdeck until the transition has finished
        while (!UICardTransition.FinishedTransitioningOnScreen) yield return CoroutineUtil.WaitForUpdate;

        if (StreamDeckManager.StreamDeckCount > 0) //added
        {
            // set the streamdeck ui
            SetStreamDeckImages();

            // sub to the streamdeck reconnecting after a disconnect
            deck.OnConnect += SetStreamDeckImages;
        }


        subbedToDisconnect = true;
        
        // setup abberation and laser ui effects
        SetupUIEffects();
             
        SubscribeToInputAndCountdown();
        UnsubFromPlayerManager();        
        yield break;
    }

    private void SetStreamDeckImages() {
        // setup avatar selection buttons
        for (int i = 0; i < avatarButtons.Length; ++i) {
            avatarButtons[i].SetButtonImage(id.ID, StaticData.StreamDeckAvatars[i][colourIndex]);
        }

        // setup colour selection buttons
        for (int i = 0; i < colourButtons.Length; ++i) {
            colourButtons[i].SetButtonImage(id.ID, StaticData.ColourSchemes[i].StreamDeckTexture);
        }
    }

    private void SetupUIEffects() {
       // NOTE(Zack): we set the laser effects player ID before we attempt to do anything with the effects
        ui.laserEffect.SetPlayerID(id);
        ui.laserEffect.enabled = true; // enabling this component starts the laser effect

        // set aberation materials
        for (int i = 0; i < 3; i++) {
            ui.avatars[i].material = ui.imageMaterials[id.ID];
        }

        // call setup for the aberration materials
        for (int i = 0; i < 3; i++) {
            ui.imageEffects[i].Setup();
        }
    }

    private bool debugWithoutControls = false; //added
    public void SetDebugFlag() //added
    {
        debugWithoutControls = true;
    }
    public void DebugConfirmAvatarSelection() //added
    {
        ConfirmAvatarSelection();
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////
    /////////// Input Sub Functions
    private void SubscribeToInputAndCountdown() {

        if (!debugWithoutControls) //added
        {
            if (inputSub) return;

            for (int i = 0; i < avatarButtons.Length; ++i)
            {
                ref ConsumableAction.Delegate lambda = ref StaticSDAvatarScene.AvatarLambdas[id.ID][i];
                int index = i; // we cache a local copy of the index for [closure] purposes
                lambda = delegate () {
                    CoroutineUtil.StartSafelyWithRef(this, ref _avatarTransitionCo, SetAvatar(index));
                };

                // add lambda delegation to array so we can clean them up later
                //commented//avatarButtons[i].SubscribeToButtonPerformed(id.ID, lambda);
            }

            for (int i = 0; i < colourButtons.Length; ++i)
            {
                ref ConsumableAction.Delegate lambda = ref StaticSDAvatarScene.ColourLambdas[id.ID][i];
                int index = i; // we cache a local copy of the index for [closure] purposes
                lambda = delegate () {
                    CoroutineUtil.StartSafelyWithRef(this, ref _colourTransitionCo, SetColour(index));
                };

                // add lambda delegation to array so we can clean them up later
                //commented//colourButtons[i].SubscribeToButtonPerformed(id.ID, lambda);
            }

            // sub idle buttons to sfx and laser effet
            //commented//idleButtons.SubscribeToButtonPerformed(id.ID, OnIdleButtonPressed);
        }


        // sub from scene countdown event
        SceneCountdown.OnCountdownFinished += ConfirmAvatarSelection;
        inputSub = true;
    }

    private void UnsubFromInputAndCountdown() {
        if (!inputSub) return;

        for (int i = 0; i < avatarButtons.Length; ++i) {
            ref ConsumableAction.Delegate lambda = ref StaticSDAvatarScene.AvatarLambdas[id.ID][i];
            avatarButtons[i].UnsubscribeFromButtonPerformed(id.ID, lambda);
        }

        for (int i = 0; i < colourButtons.Length; ++i) {
            ref ConsumableAction.Delegate lambda = ref StaticSDAvatarScene.ColourLambdas[id.ID][i];
            colourButtons[i].UnsubscribeFromButtonPerformed(id.ID, lambda);
        }

        // unsub from idle input
        idleButtons.UnsubscribeFromButtonPerformed(id.ID, OnIdleButtonPressed);
        
        // unsub from scene countdown event
        SceneCountdown.OnCountdownFinished -= ConfirmAvatarSelection;
        inputSub = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnsubFromPlayerManager() {
        if (!subbedToPlayerManager) return;
        PlayerManager.onPlayerJoined -= OnPlayerJoined;
        subbedToPlayerManager = false;
    }


    
    
    //////////////////////////////////////////////////////////////////////////////////////////////////
    /////////// Avatar and Colour Selection Functions    
    private IEnumerator __SetAvatar(int index) {
        // if the countdown has finished we ignore any button inputs
        if (SceneCountdown.IsFinished) yield break;

        // if we're choosing the same avatar we do a small aberration effect and return
        if (avatarIndex == index) {
            // play the sfx for when a player has chosen the same avatar
            //SFX.PlayUIScene(sameChosenSFX);
            AudioEventSystem.TriggerEvent("StartSameAvatarChosenSFX", null);
            
            StartCoroutine(AberrationEffect(0, false, avatarRepeatTransitionAberrationMultiplier));
            StartCoroutine(AberrationEffect(1, false, avatarRepeatTransitionAberrationMultiplier));
            yield return AberrationEffect(2, false, avatarRepeatTransitionAberrationMultiplier);
        
            StartCoroutine(AberrationEffect(0, true, avatarRepeatTransitionAberrationMultiplier));
            StartCoroutine(AberrationEffect(1, true, avatarRepeatTransitionAberrationMultiplier));
            yield return AberrationEffect(2, true, avatarRepeatTransitionAberrationMultiplier);
            yield break;
        }


        // player SFX for choosing a new avatar
        AudioEventSystem.TriggerEvent("StartNewAvatarChosenSFX", null);
        // SFX.PlayUIScene(newChosenSFX);

        AudioEngine.audioEngineInstance.fmodParams.SetGlobalParamByName("AvatarSelected", avatarsVOName[index]);
        AudioEventSystem.TriggerEvent("StartAvatarSelectVO", null);
        //  VoiceOver.Play(id.ID, avatarsSFX[index]);


        // set selected avatar to the index passed in
        data.AvatarIndex = index;
        
        StartCoroutine(AberrationEffect(0, false, 1));
        StartCoroutine(AberrationEffect(1, false, 1));
        yield return AberrationEffect(2, false, 1);

        // set on screen avatar and it's name
        ui.SetAvatars(ref StaticData.Avatars[index].Sprites);
        ui.avatarName.text = StaticData.Avatars[index].Name;

        StartCoroutine(AberrationEffect(0, true, 1));
        StartCoroutine(AberrationEffect(1, true, 1));
        yield return AberrationEffect(2, true, 1);

        _avatarTransitionCo = null;
    }


    private IEnumerator __SetColour(int index) {
        // if the countdown has finished we ignore any button inputs
        if (SceneCountdown.IsFinished) yield break;

        // if we're choosing the same colour we animate the player laser ui and return
        if (colourIndex == index) {
            // play the sfx for when a player has chosen the same colour
            //  SFX.PlayUIScene(sameChosenSFX);
            AudioEventSystem.TriggerEvent("StartSameAvatarChosenSFX", null);


            ui.laserEffect.AnimateAmplitude(duplicateColourSelectionMultiplier);
            yield break; 
        }

        // player sfx for choosing a new colour
        AudioEventSystem.TriggerEvent("StartNewAvatarChosenSFX", null);
        //SFX.PlayUIScene(newChosenSFX);

        // set on screen avatar colour scheme
        ui.SetAvatarColour(ref StaticData.ColourSchemes[index].Colours);
        ui.gradient.color = StaticData.ColourSchemes[index].CardBGColour;

        ui.laserEffect.SetColours(StaticData.ColourSchemesHDR[index].Colours);

        // set the colour indexes
        data.ColourIndex = index;

        // set the streamdeck avatar colours to be the colour the player has chosen
        for (int i = 0; i < avatarButtons.Length; ++i) {
            avatarButtons[i].SetButtonImage(id.ID, StaticData.StreamDeckAvatars[i][colourIndex]);
        }
        
        StartCoroutine(AberrationEffect(0, false, colourTransitionAberrationMultiplier));
        StartCoroutine(AberrationEffect(1, false, colourTransitionAberrationMultiplier));
        yield return AberrationEffect(2, false, colourTransitionAberrationMultiplier);
        
        StartCoroutine(AberrationEffect(0, true, colourTransitionAberrationMultiplier));
        StartCoroutine(AberrationEffect(1, true, colourTransitionAberrationMultiplier));
        yield return AberrationEffect(2, true, colourTransitionAberrationMultiplier);
    }


    private void ConfirmAvatarSelection() {
        UnsubFromInputAndCountdown();

        //// Colour Scheme ////
        ref ColourScheme colourRef    = ref StaticData.ColourSchemes[colourIndex];
        ref ColourScheme colourRefHDR = ref StaticData.ColourSchemesHDR[colourIndex];

        // set player data colour scheme
        data.ColourScheme    = colourRef;
        data.HDRColourScheme = colourRefHDR;

        // set on screen avatar colour scheme
        ui.SetAvatarColour(ref colourRef.Colours);
        ui.gradient.color = colourRef.CardBGColour;

        //// Avatar ////
        ref Avatar avatarRef = ref StaticData.Avatars[avatarIndex];
        data.Avatar          = avatarRef;

        // set on screen avatar and it's name
        ui.SetAvatars(ref avatarRef.Sprites);
        ui.avatarName.text = avatarRef.Name;

        // confirm selections, This starts the transition out of the AvatarScene
        ConfirmSelection.ConfirmChoices();
    }


    
    //////////////////////////////////////////////////////////////////////////////////////////////////
    /////////// Effects Functions
    private IEnumerator __AberrationEffect(int uiIndex, bool resetAfter, float effectMultiplier, bool useTint = true) {
        if (useTint) {
            ui.imageEffects[uiIndex].SetUseTint(true);
        }
        
        var aberrationAmounts = new Vector3[aberrationAmountSize];
        for (int i = 0; i < aberrationAmountSize; i++) {
            aberrationAmounts[i] = ui.imageEffects[uiIndex].RandomAmount(effectMultiplier);
        }

        float time = 0;
        float flickerTime = effectFlickerTime + 1;
        while (time < effectTime) {
            if (flickerTime > effectFlickerTime) {
                ui.imageEffects[uiIndex].SetAmount(aberrationAmounts[0], aberrationAmounts[1]);
                ArrayUtil.Shuffle(aberrationAmounts, aberrationAmountSize);
                flickerTime = 0;
            }
            
            time += Time.deltaTime;
            flickerTime += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
        
        if (resetAfter) {
            ui.imageEffects[uiIndex].SetAmount(Vector3.zero, Vector3.zero);
            ui.imageEffects[uiIndex].SetUseTint(false);
        }
    }


    private void OnIdleButtonPressed() {
        laser.AnimateAmplitude(laserAmplitudeReduction);
        //SFX.PlayUIScene(sameChosenSFX);
        AudioEventSystem.TriggerEvent("StartSameAvatarChosenSFX", null);
    }
}

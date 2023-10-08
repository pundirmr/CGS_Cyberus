using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;


// NOTE(Zack): this is to attempt to stop so much memory being deallocated each scene transition
public static class StaticSDTrackScene {
    private const int NumOfButtons = 7;
    
    public static readonly ConsumableAction.Delegate[][] ButtonLambdas = new [] {
        new ConsumableAction.Delegate[NumOfButtons * 2], // NOTE(Zack): we double num of buttons as we have 2 arrays to account for
        new ConsumableAction.Delegate[NumOfButtons * 2],
        new ConsumableAction.Delegate[NumOfButtons * 2]
    };
}


[RequireComponent(typeof(PlayerID))]
public class PlayerTrackSelect : MonoBehaviour {
    [Header("Stream Deck Settings")]
    [SerializeField] private ButtonIndices[] buttons;
    [SerializeField] private ButtonIndices[] numbers;
    [SerializeField] private ButtonIndices   idleButtons;

   /* [Header("SFX References")]
    [SerializeField] private AudioClip newChosenSFX;
    [SerializeField] private AudioClip sameChosenSFX; */

    
    // components
    private PlayerID id;
    private PlayerSelectionCountdown selection;

    private bool setup = false;
    private bool trackSelectionInProgress = false;
    
    
    // track selection variables
    private const int unselected = -1;
    private int selectedTrack    = unselected;

    private StreamDeck deck => StreamDeckManager.StreamDecks[id.ID];


    
    private void Awake() {
        SFX.DEBUG_CreateSFXInstance();
        VoiceOver.DEBUG_CreateSFXInstance();
        
        
        // get components
        id        = GetComponent<PlayerID>();
        selection = GetComponent<PlayerSelectionCountdown>();

        Debug.Assert(id        != null, "PlayerID is null",                 this);
        Debug.Assert(selection != null, "PlayerSelectionCountdown is null", this);


        // event subscriptoins
        PlayerManager.onPlayerJoined      += PlayerJoined;        
        TrackSelect.OnTransitionFinished  += Setup;
        TrackSelect.OnTrackProcessStarted += OnTrackProcessStarted;
    }

    private void OnDestroy() {
        PlayerManager.onPlayerJoined      -= PlayerJoined;
        TrackSelect.OnTrackProcessStarted -= OnTrackProcessStarted;

        // if we setup the stream deck we unsub from events and clear the stream deck
        if (setup) return;
        UnsubFromInput();
        if (id.IsValid) {
            deck.ClearDeck();
            deck.OnConnect -= OnStreamDeckConnected;
        }    
    }
    
    private void PlayerJoined(int playerID) {
        if (playerID != id.ID || trackSelectionInProgress) return;
        Setup();
    }
    
    private void Setup() {
        // if this id is not valid return and will wait for this player to join and setup their stream deck using the [PlayerJoined] function instead
        if (!id.IsValid) return;

        // ensure that we cannot accidently setup the streamdeck a second time
        if (setup || trackSelectionInProgress) return;
        setup = true;
        
        // if the scene countdown has finished we don't setup the stream deck
        if (SceneCountdown.IsFinished) return;

        // setup the streamdeck textures
        for (int i = 0; i < buttons.Length; ++i) {
            buttons[i].SetButtonImage(id.ID, StaticData.MusicTracks[i].DeckUnpressed);
            numbers[i].SetButtonImage(id.ID, StaticData.StreamDeckNumbers[i + 1]);
        }

        // sub to the streamdeck reconnecting after a disconnect
        deck.OnConnect += OnStreamDeckConnected;

        // set the sprite on the song cards
        TrackSelect.SetAvatarSpriteOnSongCards(id.ID);

        SubToInput();
    }

    private void SubToInput() {
        // setup button input
        int inputIndex = 0;
        for (int i = 0; i < buttons.Length; ++i) {
            ref var lambda = ref StaticSDTrackScene.ButtonLambdas[id.ID][inputIndex];
            int index = i; // NOTE(Zack): closure caching of index
            lambda = delegate() { SetTrack(index); };
            buttons[i].SubscribeToButtonPerformed(id.ID, lambda);
            inputIndex += 1;
        }

        for (int i = 0; i < numbers.Length; ++i) {
            ref var lambda = ref StaticSDTrackScene.ButtonLambdas[id.ID][inputIndex];
            int index = i; // NOTE(Zack): closure caching of index
            lambda = delegate() { SetTrack(index); };
            numbers[i].SubscribeToButtonPerformed(id.ID, lambda);
            inputIndex += 1;
        }

        // sub to idle button inputs
        idleButtons.SubscribeToButtonPerformed(id.ID, OnIdleButtonPressed);
    }

    private void UnsubFromInput() {
        // setup the album artwork button inputs
        int inputIndex = 0;
        for (int i = 0; i < buttons.Length; ++i) {
            ref var lambda = ref StaticSDTrackScene.ButtonLambdas[id.ID][inputIndex];
            buttons[i].UnsubscribeFromButtonPerformed(id.ID, lambda);
            inputIndex += 1;
        }

        // setup the number button inputs
        for (int i = 0; i < numbers.Length; ++i) {
            ref var lambda = ref StaticSDTrackScene.ButtonLambdas[id.ID][inputIndex];
            numbers[i].UnsubscribeFromButtonPerformed(id.ID, lambda);
            inputIndex += 1;            
        }

        // unsub from idle button inputs
        idleButtons.UnsubscribeFromButtonPerformed(id.ID, OnIdleButtonPressed);
    }


    private void OnStreamDeckConnected() {
        for (int i = 0; i < buttons.Length; ++i) {
            // if a player has selected this track we set it to pressed on stream deck connected
            if (selectedTrack == i) {
                buttons[i].SetButtonImage(id.ID, StaticData.MusicTracks[i].DeckPressed);
            } else {
                buttons[i].SetButtonImage(id.ID, StaticData.MusicTracks[i].DeckUnpressed);            
            }
            
            numbers[i].SetButtonImage(id.ID, StaticData.StreamDeckNumbers[i]);
        }
    }

    private void OnTrackProcessStarted() {
        trackSelectionInProgress = true;
        // NOTE(Zack): we unsub from this here instead of in [OnDestroy] so that players will not be registered to inputs again
        TrackSelect.OnTransitionFinished -= Setup;
    }

    

    private void OnIdleButtonPressed() {
        if (SceneCountdown.IsFinished || !SceneCountdown.HasStarted || trackSelectionInProgress) return;
        //SFX.PlayUIScene(sameChosenSFX);
        AudioEventSystem.TriggerEvent("StartSameAvatarChosenSFX", null);
    }
    
    
    private void SetTrack(int track) {
        if (SceneCountdown.IsFinished || !SceneCountdown.HasStarted || trackSelectionInProgress) return;

        // if player has already chosen this track we play the relevant sfx
        if (track == selectedTrack) {
            //SFX.PlayUIScene(sameChosenSFX);
            AudioEventSystem.TriggerEvent("StartSameAvatarChosenSFX", null);
            return;
        }

        // if we have previously chosen a track then we unselect it
        if (selectedTrack != unselected) {
            TrackSelect.UnSelectTrack(selectedTrack, id.ID);
            buttons[selectedTrack].SetButtonImage(id.ID, StaticData.MusicTracks[selectedTrack].DeckUnpressed);
        }


        // play new chosen sfx
        // SFX.PlayUIScene(newChosenSFX);
        AudioEventSystem.TriggerEvent("StartNewAvatarChosenSFX", null);


        // we always do this so that the first input will always set the pressed button image
        // set the chosen button as selected
        buttons[track].SetButtonImage(id.ID, StaticData.MusicTracks[track].DeckPressed);
        
        // set track
        selectedTrack = track;

        // countdown locking in of the player having made a choice
        selection.OnButtonPressed();
                
        // set the track as the one we want to select
        TrackSelect.SelectTrack(selectedTrack, id.ID);
    }
}

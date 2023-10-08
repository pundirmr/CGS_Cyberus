using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class TrackSelect : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private TrackRefs[] trackCards;

   /* [Header("SFX References")]
    [SerializeField] private AudioClip sceneTitleSFX;
    [SerializeField] private AudioClip cardsTransitionInSFX;
    [SerializeField] private AudioClip cardsTransitionOutSFX;
    [SerializeField] private AudioClip cardsScaleBackSFX;
    [SerializeField] private AudioClip rouletteTickSFX;
    [SerializeField] private AudioClip rouletteSelectedSFX;
    [SerializeField] private AudioClip[] trackVoiceOverSFXs;
   */
    
    [Header("Transitions References")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private RectTransform[] lineOneRects;
    [SerializeField] private RectTransform[] lineTwoRects;
    [SerializeField] private RectTransform lineOneOffScreenPos;
    [SerializeField] private RectTransform lineTwoOffScreenPos;

    [Header("Transitions Settings")]
    [SerializeField] private float lerpDuration = 1f;

    [Header("Roulette Effect Settings")]
    [SerializeField] private RangedInt rouletteIterationRange = new (12, 20);
    [SerializeField, Range(0.75f, 0f)] private float rouletteWaitBetween          = 0.75f;
    [SerializeField, Range(0.1f, 0f)]  private float rouletteMinWait              = 0.1f;
    [SerializeField, Range(0.09f, 0f)] private float rouletteAnimationLength      = 0.09f;
    [SerializeField, Range(1.1f, 3f)]  private float rouletteWaitAdjustmentFactor = 1.75f;

    
    
    // transition onto screen variables
    private float3[] lineOneOnScreenPositions;
    private float3[] lineTwoOnScreenPositions;
    private static int transitionsFinished = 0;

    // song card variables
    private static TrackRefs[] songCards;

    // track selection variables
    private const int unchosenTrack = -1;
    private const int randomTrack   = -1;
    private static int chosenCount  = 0;
    private static int[] Tracks;        // int == track, index == playerID
    private static int ChosenTrack = 0; // also the index into the array of song cards

    // animation variables
    private static int[] tracksToLoopThrough;
    
    // track selection events
    public static DelegateUtil.EmptyEventDel OnTrackProcessStarted;
    public static DelegateUtil.EmptyEventDel OnTrackChosen;

    // transition events
    public static DelegateUtil.EmptyEventDel OnTransitionFinished;

    // transition coroutines
    private DelegateUtil.LerpTransFloat3Coroutine MoveOnScreen;
    private DelegateUtil.LerpTransFloat3Coroutine MoveOffScreen;

    // anim coroutines
    private static DelegateUtil.LerpTransFloat3Coroutine ScaleUp;
    private static DelegateUtil.LerpTransFloat3Coroutine ScaleDown;
    private static DelegateUtil.EmptyCoroutineDel TrackRoulette;
    private static DelegateUtil.EmptyCoroutineDel RouletteEffect;
    private static DelegateUtil.EmptyCoroutineDel FadeUnselectedCards;
    private static DelegateUtil.EmptyCoroutineDel ConfirmChoice;
    private static DelegateUtil.EmptyCoroutineDel HighlightSelected;

    // anim variables
    private static float wait;
    private static float animlength;
    private static float minWait;
    private static float maxWait;
    private static float originalWaitAdjustment;
    private static RangedInt iterationRange;
    private static float3 originalScale = new (1f, 1f, 1f);
    private static float3 scaleUp       = new (1.05f, 1.05f, 1.05f);

    // sfx variables
    private bool playedOnce = false;
  /*  private static AudioClip scaleBackSFX;
    private static AudioClip tickSFX;
    private static AudioClip selectedSFX;
  */
    private static AudioClip[] trackSFXs;
    

    
    static TrackSelect() {
        // static delegate allocations
        ScaleUp             = EaseInOutUtil.ScaleQuadratic;
        ScaleDown           = EaseInOutUtil.ScaleQuadratic;
        TrackRoulette       = __TrackRoulette;
        RouletteEffect      = __RouletteEffect;
        FadeUnselectedCards = __FadeUnselectedCards;
        ConfirmChoice       = __ConfirmChoice;
        HighlightSelected   = __HighlightSelected;

        // initialize the Tracks array
        Tracks = new int[PlayerManager.MaxPlayerCount];
    }
    
    private void Awake() {
        MoveOnScreen  = EaseOutUtil.ToLocalPositionCubic;
        MoveOffScreen = EaseInUtil.ToPositionCubic;

        // static variable setup
        wait                   = rouletteWaitBetween;
        maxWait                = rouletteWaitBetween;
        minWait                = rouletteMinWait;
        originalWaitAdjustment = rouletteWaitAdjustmentFactor;
        iterationRange         = rouletteIterationRange;
        animlength             = rouletteAnimationLength;

        /*// static sfx
        scaleBackSFX = cardsScaleBackSFX;
        tickSFX      = rouletteTickSFX;
        selectedSFX  = rouletteSelectedSFX;
        trackSFXs    = trackVoiceOverSFXs;
        */
        
        
        for (int i = 0; i < trackCards.Length; ++i) {
            // set the card number
            trackCards[i].cardNumber.text = StaticStrings.Nums[i + 1];

            // set music track info
            ref var track = ref StaticData.MusicTracks[i];
            trackCards[i].infoRefs.name.text = track.TrackName;

            int mins, secs;
            maths.TimeToMinutesAndSeconds(track.TrackDuration, out mins, out secs);
            
            // if seconds is less than 10 seconds add a 0 infront of the character 
            if (secs < 10) {
                trackCards[i].infoRefs.songLength.text = $"{mins}:{StaticStrings.ZeroNums[secs]}";                
            } else {
                trackCards[i].infoRefs.songLength.text = $"{mins}:{secs}";
            }


            trackCards[i].SetDifficultyLevel(track.TrackOverallDifficulty);
            trackCards[i].SetSongImage(ref track.Sprite);
        }

        songCards = trackCards;

        // initialize Tracks, to be unchosen
        chosenCount = 0;
        for (int i = 0; i < Tracks.Length; ++i) {
            Tracks[i] = unchosenTrack;
        }
    }

    private void Start() {
        Canvas.ForceUpdateCanvases();
        
        // set song cards to off screen
        lineOneOnScreenPositions = new float3[lineOneRects.Length];
        lineTwoOnScreenPositions = new float3[lineTwoRects.Length];
        
        // set on screen positions for first line and second line
        for (int i = 0; i < lineOneRects.Length; ++i) {
            lineOneOnScreenPositions[i] = lineOneRects[i].localPosition;           
            lineTwoOnScreenPositions[i] = lineTwoRects[i].localPosition;
        }

        grid.enabled = false;
        
        // set the rects to be off screen
        for (int i = 0; i < lineOneRects.Length; ++i) {
            lineOneRects[i].position = lineOneOffScreenPos.position;
            lineTwoRects[i].position = lineTwoOffScreenPos.position;
        }

        // initialize the song cards
        foreach (var card in songCards) card.Init();
    }

    public void MoveTracksOnScreen() {
        // SFX.PlayUIScene(cardsTransitionInSFX);
        AudioEventSystem.TriggerEvent("StartCardsTransitionInSFX", null);
        StartCoroutine(DelayedTransitionOn());
    }

    public void MoveTracksOffScreen() {
        //SFX.PlayUIScene(cardsTransitionOutSFX);
        AudioEventSystem.TriggerEvent("StartCardsTransitionOutSFX", null);
        StartCoroutine(DelayedTransitionOff());
    }

    private IEnumerator DelayedTransitionOn() {
        for (int i = 0; i < lineOneRects.Length; ++i) {
            StartCoroutine(MoveOnScreen(lineOneRects[i], lineOneOnScreenPositions[i], lerpDuration, IncrementTransitionsFinished));
            StartCoroutine(MoveOnScreen(lineTwoRects[i], lineTwoOnScreenPositions[i], lerpDuration, IncrementTransitionsFinished));
            yield return CoroutineUtil.Wait(0.5f);
        }
        
        yield break;
    }

    private IEnumerator DelayedTransitionOff() {
        for (uint i = (uint)(lineOneRects.Length - 1); i < lineOneRects.Length; --i) {
            StartCoroutine(MoveOffScreen(lineOneRects[i], lineOneOffScreenPos.position, lerpDuration, IncrementTransitionsFinished));
            StartCoroutine(MoveOffScreen(lineTwoRects[i], lineTwoOffScreenPos.position, lerpDuration, IncrementTransitionsFinished));
            yield return CoroutineUtil.Wait(0.5f);
        }
        yield break;
    }

    private void IncrementTransitionsFinished() {
        transitionsFinished += 1;

        // if all transitions have finished we invoke an event, and reset count
        if (transitionsFinished >= trackCards.Length) {
            transitionsFinished = 0;
            OnTransitionFinished?.Invoke();

            if (!playedOnce) {
                playedOnce = true;
              //  VoiceOver.PlayGlobal(sceneTitleSFX);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetAvatarSpriteOnSongCards(int playerID) {
        ref var data   = ref PlayerManager.PlayerData[playerID];
        ref var sprite = ref StaticData.AvatarUISprites[data.AvatarIndex][data.ColourIndex];
        for (int i = 0; i < songCards.Length; ++i) {
            
            songCards[i].SetAvatarImage(playerID, ref sprite);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SelectTrack(int track, int playerID) {
        // enable the players avatar on their chosen track
        songCards[track].EnableAvatarSelection(playerID);

        // increment chosen count if this is the first track that a player has chosen
        if (Tracks[playerID] == unchosenTrack) chosenCount += 1;
        
        // set track
        Tracks[playerID] = track;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnSelectTrack(int track, int playerID) {
        songCards[track].DisableAvatarSelection(playerID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPlayerTrackChoices() {
        OnTrackProcessStarted?.Invoke();

        SetupValidTrackChoices();
        StartRoulette();
    }

    private static void SetupValidTrackChoices() {
        bool IsInBitFlag(int flag, int track) {
            int val = (1 << (track + 1));
            if ((flag & val) != 0) return true; // if we're already part of the bit flag we return true
            return false;
        }
        
        int validPlayerCount = PlayerManager.ValidPlayerIDs.Count;
        int validCount   = 0;
        int trackBitFlag = 0;
        // we check the number of valid player choices
        for (int i = 0; i < validPlayerCount; ++i) {
            int track = Tracks[PlayerManager.ValidPlayerIDs[i]];
            if (track == unchosenTrack || IsInBitFlag(trackBitFlag, track)) continue;

            // we add this track to the bit flag
            trackBitFlag |= (1 << (track + 1));

            // increment the validCount
            validCount += 1;
            Log.Print($"Track: {track + 1} chosen");
        }

        // NOTE(Zack): if we have not got any valid choices we randomly choose a track and return out of the function
        if (validCount == 0) {
            tracksToLoopThrough = new int[1];
            tracksToLoopThrough[0] = random.Range(0, songCards.Length);
            return;
        }
            
        // initialize the array to the correct size
        tracksToLoopThrough = new int[validCount];
        trackBitFlag = 0;
        
        // setup the valid choices
        for (int i = 0, index = 0; i < validPlayerCount; ++i) {
            int track = Tracks[PlayerManager.ValidPlayerIDs[i]];
            if (track == unchosenTrack|| IsInBitFlag(trackBitFlag, track)) continue;

            // set the track
            tracksToLoopThrough[index] = track;

            // add to bit flag
            trackBitFlag |= (1 << (track + 1));

            // increment index
            index += 1;
        }
        

        // we sort the array indexs to be in ascending order e.g (0..2..4)
        for (int i = 0; i < tracksToLoopThrough.Length; ++i) {
            for (int j = (i + 1); j < tracksToLoopThrough.Length; ++j) {
                if (tracksToLoopThrough[i] <= tracksToLoopThrough[j]) continue;

                // swap the values
                int tmp = tracksToLoopThrough[i];
                tracksToLoopThrough[i] = tracksToLoopThrough[j];
                tracksToLoopThrough[j] = tmp;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StartRoulette() {
        StaticCoroutine.Start(TrackRoulette());
    }

    private static IEnumerator __TrackRoulette() {
        // fade unselected cards
        yield return FadeUnselectedCards();

        // we default to the first track chosen, and in the case that only one track has been chosen we use this
        ChosenTrack = 6;// tracksToLoopThrough[0];
        
        // we do the roulette effect if there are more than one player that has chosen
        if (tracksToLoopThrough.Length > 1) {
            yield return RouletteEffect();
        } else {
            // else we scale up the selected song
            //SFX.PlayUIScene(tickSFX);
            AudioEventSystem.TriggerEvent("StartRouletteTickSFX", null);
            yield return ScaleUp(songCards[6].parent, scaleUp, animlength * 4f);
        }

        yield return ConfirmChoice();
        yield return CoroutineUtil.Wait(1.5f);
        OnTrackChosen?.Invoke();
        yield break;
    }

    private static IEnumerator __FadeUnselectedCards() {
        yield return CoroutineUtil.Wait(0.5f);

        // Scale Back SFX
        //SFX.PlayUIScene(scaleBackSFX);
        AudioEventSystem.TriggerEvent("StartCardsScaleBackSFX", null);
        
        // we fade the cards that haven't been chosen into the background slightly
        for (int i = 0; i < songCards.Length; ++i) {
            if (TrackHasBeenChosen(i)) continue;
            StaticCoroutine.Start(songCards[i].FadeIntoBackground());
        }

        yield return CoroutineUtil.Wait(2f);
        yield break;
    }

    private static bool TrackHasBeenChosen(int i) {
        foreach (var track in tracksToLoopThrough) {
            if (track == i) return true;
        }

        return false;
    }
    
    private static IEnumerator __RouletteEffect() {
        float waitAdjustment = 1f / originalWaitAdjustment;

        int index      = 0;
        int track      = 0;
        int j          = 0;
        int previous   = 0;
        int iterations = random.Range(17, 20);
        int halfIter   = iterations / 2;
        
        for (int i = 0; i < iterations; ++i) {
            if (i == halfIter) waitAdjustment = originalWaitAdjustment;

            // if we're on the last iteration we slow down the anim by half
            if (i == (iterations - 1)) animlength *= 4f;
            
            track    = tracksToLoopThrough[index];
            j        = ArrayUtil.WrapIndex(index - 1, tracksToLoopThrough.Length);
            previous = tracksToLoopThrough[j];

            // wrap index
            index += 1;
            index = ArrayUtil.WrapIndex(index, tracksToLoopThrough.Length);
            
            StaticCoroutine.Start(ScaleUp(songCards[track].parent, scaleUp, animlength));
            StaticCoroutine.Start(ScaleDown(songCards[previous].parent, originalScale, animlength));
            //SFX.PlayUIScene(tickSFX);
            AudioEventSystem.TriggerEvent("StartRouletteTickSFX", null);
    
            yield return CoroutineUtil.Wait(wait);
            wait *= waitAdjustment;
            wait  = maths.Clamp(wait, minWait, maxWait);
        }

        // we set the track to the final track that the iterations landed on
        ChosenTrack = track;
        yield return CoroutineUtil.Wait(0.5f);
        yield break;
    }

    private static IEnumerator __ConfirmChoice() {
        // SFX.PlayUIScene(selectedSFX);
        AudioEventSystem.TriggerEvent("StartRouletteSelectedSFX", null);
        yield return CoroutineUtil.Wait(0.5f);
        //VoiceOver.PlayGlobal(trackSFXs[ChosenTrack]);
        yield return HighlightSelected();

        // set music track player track
        GameManager.Track = StaticData.MusicTracks[6];
        AudioEngine.audioEngineInstance.fmodParams.SetGlobalParamByLabelName("SelectedTrack", GameManager.Track.TrackName);
        Debug.Log(GameManager.Track.TrackName);


        SceneColourer.UpdateSceneMaterialsColours(GameManager.Track);
        yield break;
    }
    
    private static IEnumerator __HighlightSelected() {
        yield return songCards[6].SetFinalSongSelected();        
        yield break;
    }
}

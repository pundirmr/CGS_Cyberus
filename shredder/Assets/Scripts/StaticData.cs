using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public static class StaticData {
    public static bool Initialised { get; private set; } = false;
    
    public static Avatar[] Avatars;
    public static ColourScheme[] ColourSchemes;
    public static ColourScheme[] ColourSchemesHDR;
    public static MusicTrack[] MusicTracks;
    public static Dictionary<int, ButtonTexture[]> StreamDeckAvatars; // int == avatar type, ButtonTexture[] == colour varients for avatar
    public static Dictionary<int, Sprite[]> AvatarUISprites; // int == avatar type, Sprite[] == colour varients for avatar
    public static ButtonTexture[] StreamDeckNumbers;
    public static AudioClip AIEliminatedSFX;
    
        
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        Avatars           = Resources.LoadAll<Avatar>("AvatarsCards");
        ColourSchemes     = Resources.LoadAll<ColourScheme>("ColourSchemes");        
        ColourSchemesHDR  = Resources.LoadAll<ColourScheme>("ColourSchemesHDR");
        MusicTracks       = Resources.LoadAll<MusicTrack>("MusicTracks");
        StreamDeckNumbers = Resources.LoadAll<ButtonTexture>("StreamDeckNumbers");
        AIEliminatedSFX   = Resources.Load<AudioClip>("SFX/AIEliminated/AiEliminated"); // HACK(Zack): 

        
        // ensure that all of the arrays have been loaded correctly
        Debug.Assert(Avatars.Length           == 6,    "Avatars not loaded correctly");
        Debug.Assert(ColourSchemes.Length     == 6,    "ColourSchemes not loaded correctly");
        Debug.Assert(ColourSchemesHDR.Length  == 6,    "ColourSchemesHDR not loaded correctly");
        Debug.Assert(MusicTracks.Length       == 7,    "Music Tracks not loaded correctly");
        Debug.Assert(StreamDeckNumbers.Length == 10,   "StreamDeckNumbers not loaded correctly");
        Debug.Assert(AIEliminatedSFX          != null, "AIEliminated not loaded correctly");
        

        InitStreamDeckAvatars();
        InitAvatarUISprites();
        
        // init all static data info
        foreach (var scheme in ColourSchemes) scheme.Init();
        foreach (var avatar in Avatars)       avatar.Init();
        foreach (var track in MusicTracks)    track.Init();
        
        Initialised = true;
    }


    private static void InitStreamDeckAvatars() {
        // the order of these are the order in which the indexs line up for avatar selection
        ButtonTexture[] cerb  = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Cerberus");
        ButtonTexture[] bas   = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Basilisk");
        ButtonTexture[] strix = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Strix");
        ButtonTexture[] lycan = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Lycan");
        ButtonTexture[] chim  = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Chimera");        
        ButtonTexture[] siren = Resources.LoadAll<ButtonTexture>("StreamDeckAvatars/Siren");

        // ensure that all of the arrays have been loaded correctly 
        Debug.Assert(cerb.Length  == 6, "Cerberus StreamDeck Textures not loaded correctly");
        Debug.Assert(bas.Length   == 6, "Basilisk StreamDeck Textures not loaded correctly");
        Debug.Assert(strix.Length == 6, "Strix StreamDeck Textures not loaded correctly");
        Debug.Assert(lycan.Length == 6, "Lycan StreamDeck Textures not loaded correctly");
        Debug.Assert(chim.Length  == 6, "Chimera StreamDeck Textures not loaded correctly");
        Debug.Assert(siren.Length == 6, "Siren StreamDeck Textures not loaded correctly");

        StreamDeckAvatars = new (6);

        StreamDeckAvatars.Add(0, cerb);
        StreamDeckAvatars.Add(1, bas);
        StreamDeckAvatars.Add(2, strix);
        StreamDeckAvatars.Add(3, lycan);
        StreamDeckAvatars.Add(4, chim);
        StreamDeckAvatars.Add(5, siren);
        Debug.Assert(StreamDeckAvatars.Count == 6);

    
        // NOTE(Zack): this for loop is to ensure that indexs into every array will work (not necessary but i'm paranoid
#if UNITY_EDITOR
        for (int i = 0; i < StreamDeckAvatars.Count; ++i) {
            for (int j = 0; j < 6; ++j) {
                var temp = StreamDeckAvatars[i][j];
                StreamDeckAvatars[i][j] = temp;
            }
        }
#endif
    }

    private static void InitAvatarUISprites() {
        // the order of these are the order in which the indexs line up for avatar selection
        Sprite[] cerb  = Resources.LoadAll<Sprite>("AvatarSmallUI/Cerberus");
        Sprite[] bas   = Resources.LoadAll<Sprite>("AvatarSmallUI/Basilisk");
        Sprite[] strix = Resources.LoadAll<Sprite>("AvatarSmallUI/Strix");
        Sprite[] lycan = Resources.LoadAll<Sprite>("AvatarSmallUI/Lycan");        
        Sprite[] chim  = Resources.LoadAll<Sprite>("AvatarSmallUI/Chimera");
        Sprite[] siren = Resources.LoadAll<Sprite>("AvatarSmallUI/Siren");

        // ensure all of the arrays have been loaded correctly
        Debug.Assert(cerb.Length  == 6, "Cerberus Small UI Sprites not loaded correctly");       
        Debug.Assert(bas.Length   == 6, "Basilisk Small UI Sprites not loaded correctly");
        Debug.Assert(strix.Length == 6, "Strix Small UI Sprites not loaded correctly");
        Debug.Assert(lycan.Length == 6, "Lycan Small UI Sprites not loaded correctly");
        Debug.Assert(chim.Length  == 6, "Chimera Small UI Sprites not loaded correctly");
        Debug.Assert(siren.Length == 6, "Siren Small UI Sprites not loaded correctly");

        AvatarUISprites = new (6);

        AvatarUISprites.Add(0, cerb);
        AvatarUISprites.Add(1, bas);
        AvatarUISprites.Add(2, strix);
        AvatarUISprites.Add(3, lycan);
        AvatarUISprites.Add(4, chim);
        AvatarUISprites.Add(5, siren);
        Debug.Assert(AvatarUISprites.Count == 6);

        
        // NOTE(Zack): this for loop is to ensure that indexs into every array will work (not necessary but i'm paranoid
#if UNITY_EDITOR
        for (int i = 0; i < AvatarUISprites.Count; ++i) {
            for (int j = 0; j < 6; ++j) {
                var temp = AvatarUISprites[i][j];
                AvatarUISprites[i][j] = temp;
            }
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodRouting : MonoBehaviour
{

    //Bus Variables 
    public FMOD.Studio.Bus masterBus;
    public FMOD.Studio.Bus musicBus;
    public FMOD.Studio.Bus sfxBus;
    public FMOD.Studio.Bus voBus;


   
    public void SetUpBuses()
    {
        //Setting bus variables to string paths in fmod
        masterBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBus");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBus/MusicBus");
        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBus/SFXBus");
        voBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBus/VoiceOverBus");
    }
}

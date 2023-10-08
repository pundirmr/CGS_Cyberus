using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*This class will be able to return the playback state of a given eventInstance.
  To use this script reference it in the desired part of the audio engine then use this reference and '.PlaybackState(eventInstance) and then you can use it to compare against different fmod event playing states. e.g. != FMOD.Studio.PLAYBACKSTATE.PLAYING*/
public class EventPlaybackState : MonoBehaviour
{
    

    public FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance eventInstance)
    {
        FMOD.Studio.PLAYBACK_STATE pbState;
       
        eventInstance.getPlaybackState(out pbState);

        return pbState;
    }
}

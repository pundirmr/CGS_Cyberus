using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine;

public class AudioEventSystem : MonoBehaviour
{
    private Dictionary<string, Action<GameObject>> eventDictionary; //Declaring Dictionary with string of event name, as well as a gameObject parameter

    private static AudioEventSystem audioEventSystem; //Will be used to populate dictionary

    public static AudioEventSystem instance //Will be used to start and stop listening and as well as for triggering events
    {
        get
        {
            if (!audioEventSystem) // If there is no eventManager reference, findObjectOftype event manager, if still no reference throw error, else if reference found initialize dictionary
            {
                audioEventSystem = FindObjectOfType(typeof(AudioEventSystem)) as AudioEventSystem;

                if (!audioEventSystem)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    audioEventSystem.Init();
                }
            }

            return audioEventSystem; //Return reference to eventManager
        }
    }

    void Init()
    {
        //Initialize dictionary
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, Action<GameObject>>();
        }
    }

    public static void StartListening(string eventName, Action<GameObject> listener)
    {
        Action<GameObject> thisEvent = null; //Make thisEvent instance null 
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) //Find parameter values for eventDictionary, then add listener to thisEvent
        {
            thisEvent += listener;
        }
        else //If no value found from dictionary, then create an event and add listener, and add instance to dictionary
        {
            thisEvent += listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action<GameObject> listener)
    {
        if (audioEventSystem == null) return; //Return if no eventMANAGER
        Action<GameObject> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) //Get values from dictionary then remove current listner from thisEvent
        {
            thisEvent -= listener;
        }
    }

    public static void TriggerEvent(string eventName, GameObject triggerObject)
    {
        Action<GameObject> thisEvent = null; //Make this eventNull
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) //Get event from dictionray and then invoke its methods/functions
        {
            thisEvent.Invoke(triggerObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugHelper : MonoBehaviour
{
    public GameObject[] JoinButtons;
    private PlayerAvatarSelectUI[] playerAvatarSelectUI;

    /// <summary>
    /// Emulate player join with player id. player id are 0,1,2
    /// </summary>
    /// <param name="playerId"></param>
    public void JoinPlayerWithId(int playerId)
    {
        PlayerManager.DebugJoin(playerId);
        JoinButtons[playerId].SetActive(false);
    }

    public void ContinueToTrackSelection()
    {
        if (playerAvatarSelectUI.Length>0)
        {
            for(int i=0;i<playerAvatarSelectUI.Length;i++)
                playerAvatarSelectUI[i].DebugConfirmAvatarSelection();
        }
    }

    /// <summary>
    /// https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    ///https://forum.unity.com/threads/scenemanager-sceneloaded-method-showing-overload-error.975708/
    /// </summary>
    /// 
    #region SceneManager
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("OnDisable");
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "AvatarSelect")
        {
            playerAvatarSelectUI = FindObjectsOfType<PlayerAvatarSelectUI>();
            if (playerAvatarSelectUI.Length > 0)
            {
                for (int i = 0; i < playerAvatarSelectUI.Length; i++)
                    playerAvatarSelectUI[i].SetDebugFlag();
            }
        }


        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        //disable button for which player has already joined
        for(int i = 0; i < PlayerManager.ValidPlayerIDs.Count; i++)
        {
            JoinButtons[PlayerManager.ValidPlayerIDs[i]].SetActive(false);
        }
    }

    #endregion
}

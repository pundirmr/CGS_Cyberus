using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDisablePressToJoin : MonoBehaviour {
    [SerializeField] private PlayerID id;
    [SerializeField] private GameObject parent;
    [SerializeField] private Image border;
    [SerializeField] private bool overrideDisable = false;

    private bool subbed = false;
    private Color borderColor;

    private void Start() {
        if (id == null) id = GetComponent<PlayerID>();
        Debug.Assert(id != null);

        // disable the sprite if the player is already valid (joined before the Report Scene)
        if (id.IsValid) {
            parent.SetActive(false);
            return;
        }

        if (border != null) {
            borderColor = border.color;
            border.color = new Color(borderColor.r, borderColor.g, borderColor.b, 0.25f);
        }

        // if we're overriding this we keep the sprite on screen
        if (overrideDisable) return;
        PlayerManager.onPlayerJoined += DisableObjects;
        subbed = true;
    }

    private void OnDestroy() {
        if (!subbed) return;
        PlayerManager.onPlayerJoined -= DisableObjects;
    }

    private void DisableObjects(int playerID) {
        if (playerID != id.ID) return;
        parent.SetActive(false);

        if (border != null) {
            border.color = borderColor;     
        }
        
        PlayerManager.onPlayerJoined -= DisableObjects;
        subbed = false;
    }            
}

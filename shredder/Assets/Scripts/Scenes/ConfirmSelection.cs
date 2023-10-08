using System;
using System.Collections;
using UnityEngine;

public class ConfirmSelection : MonoBehaviour {
    public static bool IsValid { get; private set; } = false;
    public static bool AllChosen       = false;
    public static bool ChoicesLockedIn = false;

    // confirmation event
    public static DelegateUtil.EmptyEventDel OnChoicesConfirmed;
    public static DelegateUtil.EmptyEventDel OnChoicesLockedIn;

    private static bool  signaledChosen = false;
    private static uint idBitFlag       = 0;
    private static short chosenCount = 0;

    private static float delay;
    
    private void Awake() {
        IsValid         = true;
        AllChosen       = false;
        ChoicesLockedIn = false;

        signaledChosen  = false;
        chosenCount     = 0;
        idBitFlag       = 0;
    }

    private void OnDestroy() {
        IsValid = false;
    }
    
    public static void AddPlayer(int id) {
        uint val = (uint)(1 << (id + 1));
        if (Bits.HasFlag(idBitFlag, val)) return; // if value is already in the bit flag we return

        idBitFlag = Bits.AddFlag(idBitFlag, val);
        chosenCount += 1;

        if (chosenCount == PlayerManager.PlayerCount && !AllChosen) {
            AllChosen = true;
            OnChoicesLockedIn?.Invoke();
        }
    }

    public static void RemovePlayer(int id) {
        if (AllChosen) return;
        
        uint val = (uint)(1 << (id + 1));
        if (Bits.DoesNotHaveFlag(idBitFlag, val)) return; // if value is not in the bit flag we return

        idBitFlag = Bits.RemoveFlag(idBitFlag, val);
        chosenCount -= 1;
    }

    public static void ConfirmChoices() {
        if (ChoicesLockedIn || signaledChosen) return;
        signaledChosen = true;
        OnChoicesConfirmed?.Invoke();
    }
}

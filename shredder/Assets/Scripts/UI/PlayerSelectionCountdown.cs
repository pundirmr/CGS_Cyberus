using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionCountdown : MonoBehaviour {
    [Header("Countdown Settings")]
    [SerializeField] private float countdownLength = 3f;

    private PlayerID id;
    private Coroutine countdownCo;
    private float delay = 0f;

    private DelegateUtil.EmptyCoroutineDel Countdown;
    
    private void Awake() => id = GetComponent<PlayerID>();

    private void Start() {
        Countdown = __Countdown;
    }

    private IEnumerator __Countdown() {
        bool locked = false;
        for (;;) {
            yield return CoroutineUtil.WaitForUpdate;
            if (locked) break;

            delay += Time.deltaTime;
            if (delay < countdownLength) continue;
            locked = true;
            ConfirmSelection.AddPlayer(id.ID);
            Log.Print("Countdown Finished");
        }

        countdownCo = null;
        yield break;
    }

    public void OnButtonPressed() {
        if (countdownCo == null) {
            StartCountdownCo();
        }
        
        ConfirmSelection.RemovePlayer(id.ID);
        delay = 0f;
    }

    public void StartCountdownCo() {
        CoroutineUtil.StartSafelyWithRef(this, ref countdownCo, Countdown());        
    }
}

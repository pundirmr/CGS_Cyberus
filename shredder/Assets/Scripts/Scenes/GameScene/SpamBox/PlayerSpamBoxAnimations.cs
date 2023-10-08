using System.Collections;
using UnityEngine;

public class PlayerSpamBoxAnimations : MonoBehaviour {
    [Header("Prefab References")]
    [SerializeField] private PlayerID id;
    [SerializeField] private PlayerSpamBoxHealthAnim healthAnim;
    [SerializeField] private PlayerSpamBoxDifficultyAnim difficultyAnim;

    private DelegateUtil.EmptyCoroutineDel DifficultyChangedCo;
    private DelegateUtil.EmptyCoroutineDel HealthChangedCo;
    private Coroutine healthCo;
    private Coroutine difficultyCo;

    private bool subbed = false;
    
    private void Awake() {
        // delegate allocations
        DifficultyChangedCo = __DifficultyChangedCo;
        HealthChangedCo     = __HealthChangedCo;
    }
    
    private IEnumerator Start() {
        yield return PlayerManager.WaitForValidPlayer(id.ID);
        id.PlayerData.OnDifficultyUpdated   += OnDifficultyChanged;
        id.PlayerData.OnPlayerHealthUpdated += OnPlayerHealthUpdated;
        subbed = true;
    }

    private void OnDestroy() {
        if (!subbed) return;
        id.PlayerData.OnDifficultyUpdated   -= OnDifficultyChanged;
        id.PlayerData.OnPlayerHealthUpdated -= OnPlayerHealthUpdated;
    }

    private void OnDifficultyChanged() {
        if (difficultyCo != null) return;
        CoroutineUtil.StartSafelyWithRef(this, ref difficultyCo, DifficultyChangedCo());
    }

    private IEnumerator __DifficultyChangedCo() {
        // enforce a simple animation "Queue" so that the difficulty anim cannot play whilst the health anim is playing
        while (healthCo != null) yield return CoroutineUtil.WaitForUpdate;

        yield return difficultyAnim.EntireAnimation();
        difficultyCo = null;
        yield break;
    }
    
    private void OnPlayerHealthUpdated() {
        if (healthCo != null) return;
        CoroutineUtil.StartSafelyWithRef(this, ref healthCo, HealthChangedCo());        
    }

    private IEnumerator __HealthChangedCo() {
         // enforce a simple animation "Queue" so that the health anim cannot play whilst the difficulty anim is playing
        while (difficultyCo != null) yield return CoroutineUtil.WaitForUpdate;

        yield return healthAnim.EntireAnimation();
        healthCo = null;        
        yield break;
    }
}

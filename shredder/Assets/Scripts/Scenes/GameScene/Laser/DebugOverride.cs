using UnityEngine;

public class DebugOverride : MonoBehaviour 
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Laser Hit Override")]
    [SerializeField] private bool shouldOverride   = false;
    [SerializeField] private LaserHitTiming timing = LaserHitTiming.PERFECT;
    [SerializeField] private bool useRandomHits    = false;
    [SerializeField] private RangedInt randomHitTimingMinMax = new (1, 8);

    public static bool LaserTimingOverride = false;
    public static bool UseRandomHits;
    public static LaserHitTiming LaserHitTiming;
    public static LaserHitTiming RandomHitTiming => ((LaserHitTiming)(1 << RandomHitTimingMinMax.Random()));
    private static RangedInt RandomHitTimingMinMax;
    
    [Header("Player Difficulty Override")]
    [SerializeField] private bool ignoreDifficulty = false;

    public static bool IgnoreDifficulty;

    private void Awake()      => UpdateValues();
    private void OnValidate() => UpdateValues();

    private void UpdateValues()
    {
        // Laser Hit Override
        LaserTimingOverride   = shouldOverride;
        UseRandomHits         = useRandomHits;
        LaserHitTiming        = timing;
        RandomHitTimingMinMax = randomHitTimingMinMax;
        
        // Player Difficulty Override
        IgnoreDifficulty = ignoreDifficulty;
    }
#endif
}


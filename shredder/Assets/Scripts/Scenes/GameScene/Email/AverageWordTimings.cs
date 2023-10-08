using System.Collections.Generic;
using UnityEngine;

public class AverageTimingInfo {
    // int == number of times hit on this hit timing
    public Dictionary<LaserHitTiming, int> Timings = new (7);
}

public static class AverageWordTimings {
    // int == wordIndex
    public static Dictionary<int, AverageTimingInfo> Words = new (10);

    public static void SetupTimingDictionary(SpamMessage spam) {
        Words.Clear();
        
        for (int i = 0; i < spam.wordsWithoutPunctuation.Length; ++i) {
            if (!spam.keyWords[i]) continue;
            
            AverageTimingInfo newInfo = new ();
            Words.Add(i, newInfo);
        }
    }
}

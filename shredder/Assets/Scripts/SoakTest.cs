using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoakTest : MonoBehaviour
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private static bool _isFirst = false;
    
    [Header("On/Off")]
    [SerializeField] private bool Active;
    [Space]
    [SerializeField] private Vector2 mainMenuWaitTime;
    [SerializeField] private bool onFirstTimeInstantLoadNextScene;
    [SerializeField] private int numberOfPlayersToJoin;
    [Header("Logging")]
    [SerializeField] private bool DoLog;
    [SerializeField] [TimeField] private float LogEverySecs = 60;
    [SerializeField] private string filename = "SoakTestMemoryProfilerLog";
    [SerializeField] private string extension = ".txt";
    
    private static ProfilerRecorder memoryRecorder;
    private static bool _profilerInited = false;
    private static float LogTime;
    private static string folderPath;
    private static string filePath;
    private static long lastMemUsage = -1;
    private static long memUsageDelta = 0;

    private delegate IEnumerator WaitOnMainMenu(float time);
    private WaitOnMainMenu WaitFunc;
    private Coroutine WaitCo;
    
    private static IEnumerator LogMemoryUsage()
    {
        if (!_profilerInited)
        {
            memoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            // memoryRecorder.Start();
        }

        bool first = true;
        Directory.CreateDirectory(folderPath);
        
        while (true)
        {
            yield return CoroutineUtil.Wait(LogTime);

            //Write to file
            long memUsageNow = memoryRecorder.LastValue;
            long memDelta = 0;
            if (lastMemUsage == -1)
            {
                lastMemUsage = memUsageNow;
            }
            else
            {
                memDelta = memUsageNow - lastMemUsage;
                memUsageDelta += first ? 0 : memDelta;
                if (first)
                {
                    first = false;
                }
                lastMemUsage = memUsageNow;
            }
            if (!memoryRecorder.Valid)
            {
                Log.Error("Memory Recorder is not valid");
            }
            
            StreamWriter writer = new StreamWriter(filePath, true);
            writer.WriteLine($"--- Memory Usage at {DateTime.Now.ToString(new CultureInfo("en-GB"))} ---");
            writer.WriteLine($"Current: {memUsageNow  / (1024 * 1024)} MB");
            writer.WriteLine($"Delta from previous: {memDelta / (1024 * 1024)} MB");
            writer.WriteLine($"Total Delta is: {memUsageDelta / (1024 * 1024)} MB");
            writer.WriteLine(" ");
            writer.Close();
            Log.Print("Made Mem Log.");
        }
    }
    

    private void Awake()
    {
        WaitFunc = WaitFor;
        LogTime = LogEverySecs;
    }

    private void OnDestroy()
    {
        if (WaitCo != null)
        {
            StopCoroutine(WaitCo);
        }
    }

    private void Start()
    {
        if (!Active)
        {
            return;
        }
        
        if (DoLog && !_isFirst)
        {
            folderPath = Path.Combine(Application.persistentDataPath, "MemoryLogs");
            var timeSpan = DateTime.Now.TimeOfDay;
            string filetime = $"{timeSpan.Hours:00}-{timeSpan.Minutes:00}-{timeSpan.Seconds:00}" ;
            filePath = Path.Combine(folderPath, filename + "-" + filetime + extension);
            StaticCoroutine.Start(LogMemoryUsage());
        }
        
        if (!_isFirst && onFirstTimeInstantLoadNextScene)
        {
            WaitCo = StartCoroutine(WaitFunc(5));
            _isFirst = true;
            return;
        }
        
        float time = Random.Range(mainMenuWaitTime.x, mainMenuWaitTime.y);
        int mins, secs;
        maths.TimeToMinutesAndSeconds(time, out mins, out secs);
        Log.Error($"THIS IS NOT AN ERROR: Soak Test waiting for {mins}:{secs}");
        WaitCo = StartCoroutine(WaitFunc(time));
    }

    private IEnumerator WaitFor(float waitTime)
    {
        float time = 0;
        while (time < waitTime)
        {
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }

        for (int i = 0; i < numberOfPlayersToJoin; i++)
        {
            PlayerManager.Join(i);
        }
        WaitCo = null;
    }
#endif
}

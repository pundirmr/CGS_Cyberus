using UnityEngine;

public static class LockFPS
{
  private const int TargetFrameRate = -1;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Init()
  {
    QualitySettings.vSyncCount  = 0;
    Application.targetFrameRate = TargetFrameRate;
  }
}
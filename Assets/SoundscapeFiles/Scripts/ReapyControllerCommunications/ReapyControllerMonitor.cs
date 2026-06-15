using System;
using UnityEngine;

public class ReapyControllerMonitor
{
    public static long startDelayTimestamp0;
    public static long startDelayTimestamp1;

    public static void startstartDelayCount()
    {
        startDelayTimestamp0 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static void countstartDelay()
    {
        startDelayTimestamp1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long timePassed = startDelayTimestamp1 - startDelayTimestamp0;
        Debug.Log($"[ReapyControllerMonitor] Audios playing with a delay of {timePassed}ms");
    }
}

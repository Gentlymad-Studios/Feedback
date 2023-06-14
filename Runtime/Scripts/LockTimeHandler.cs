using System;
using System.Threading;
using UnityEngine;

public class LockTimeHandler {

    public static bool locked = false;
    
    private static DateTime openTime;
    private static float lockTime = 10.0f;
    private static float remainingWaitTime;
    private static string message = "No F1 Spaming please. You need to wait for - " + lockTime + " - Seconds";

    private static void Timer() {
        while (locked) {
            lockTime -= Time.deltaTime;
            remainingWaitTime = lockTime;
            if (lockTime <= 0.0f) {
                locked = false;
                lockTime = 10.0f;
            }
        }
    }
    public static void CheckSpam(Action a) {
        if(locked) {
            a.Invoke();
            return;
        }
        if (openTime.AddSeconds(2) > DateTime.Now) {
            locked = true;
            Timer();
            Debug.LogWarning(message);
            a.Invoke();
        }
    }

    public static float LockTime() {
        return remainingWaitTime;
    }
    public static void SetOpenTime() {
        openTime = DateTime.Now;
    }
}

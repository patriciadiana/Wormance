using System;
using System.Collections.Generic;
using UnityEngine;

public class SuspicionManager : Singleton<SuspicionManager>
{
    public const float Min = 0f;
    public const float Max = 100f;

    private Dictionary<string, float> suspicions = new Dictionary<string, float>();
    public static event Action<string, float> OnSuspicionChanged;

    public float GetSuspicionLevel(string npcId)
    {
        if (suspicions.TryGetValue(npcId, out float value))
            return value;

        return 0f;
    }

    public void ModifySuspicion(string npcId, float delta)
    {
        float current = GetSuspicionLevel(npcId);
        float clamped = Mathf.Clamp(current + delta, Min, Max);

        if (Mathf.Approximately(clamped, current)) return;

        suspicions[npcId] = clamped;
        OnSuspicionChanged?.Invoke(npcId, clamped);
    }

    public void ResetSuspicion(string npcId)
    {
        suspicions[npcId] = 0f;
        OnSuspicionChanged?.Invoke(npcId, 0f);
    }
}

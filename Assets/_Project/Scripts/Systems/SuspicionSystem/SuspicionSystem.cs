using System;
using UnityEngine;

public static class SuspicionSystem
{
    public const float Min = 0f;
    public const float Max = 100f;

    private static float _value = 0f;

    public static float Value
    {
        get => _value;
        set
        {
            float clamped = Mathf.Clamp(value, Min, Max);
            if (Mathf.Approximately(clamped, _value)) return;
            _value = clamped;
            OnSuspicionChanged?.Invoke(_value);
        }
    }

    public static event Action<float> OnSuspicionChanged;

    public static void Modify(float delta)
    {
        Value = _value + delta;
    }

    public static void Reset()
    {
        Value = 0f;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    void OnEnable()
    {
        SuspicionSystem.OnSuspicionChanged += HandleSuspicionChanged;
    }

    void OnDisable()
    {
        SuspicionSystem.OnSuspicionChanged -= HandleSuspicionChanged;
    }

    void Start()
    {
        HandleSuspicionChanged(SuspicionSystem.Value);
    }

    void HandleSuspicionChanged(float value)
    {
        float t = Mathf.Clamp01(value / SuspicionSystem.Max);

        if (fillImage) fillImage.fillAmount = t;
    }
}
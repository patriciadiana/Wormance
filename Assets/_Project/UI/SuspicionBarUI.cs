using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text label;

    public string targetNpcId;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        ShowBar(false);
    }

    void OnEnable()
    {
        SuspicionManager.OnSuspicionChanged += HandleSuspicionChanged;
    }

    void OnDisable()
    {
        SuspicionManager.OnSuspicionChanged -= HandleSuspicionChanged;
    }

    void HandleSuspicionChanged(string changedNpcId, float value)
    {
        if (changedNpcId == targetNpcId)
        {
            UpdateBarVisuals(value);
        }
    }

    public void SetTargetNPC(string npcId)
    {
        targetNpcId = npcId;
        float currentValue = SuspicionManager.Instance.GetSuspicionLevel(targetNpcId);
        UpdateBarVisuals(currentValue);
    }

    void UpdateBarVisuals(float value)
    {
        float t = Mathf.Clamp01(value / SuspicionManager.Max);

        if (fillImage)
        {
            fillImage.fillAmount = t;
            fillImage.color = Color.Lerp(Color.green, Color.red, t);
        }

        if (label)
        {
            label.text = $"Suspicion: {Mathf.RoundToInt(value)}";
        }
    }

    public void ShowBar(bool show)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = show ? 1f : 0f;
    }
}
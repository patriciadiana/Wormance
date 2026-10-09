using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayCycleDebugUI : MonoBehaviour
{
    [SerializeField] private DayCycleSystem dayCycle;
    [SerializeField] private TextMeshProUGUI dayLabel;
    [SerializeField] private Image dayProgressFill;
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private TextMeshProUGUI summaryLabel;
    [SerializeField] private Button continueButton;

    void OnEnable()
    {
        GameEvents.OnDayStarted += HandleDayStarted;
        GameEvents.OnDayTimeChanged += HandleTimeChanged;
        GameEvents.OnDayEnded += HandleDayEnded;
        GameEvents.OnGameFinished += HandleGameFinished;

        continueButton.onClick.AddListener(dayCycle.ContinueToNextDay);
        summaryPanel.SetActive(false);
    }

    void OnDisable()
    {
        GameEvents.OnDayStarted -= HandleDayStarted;
        GameEvents.OnDayTimeChanged -= HandleTimeChanged;
        GameEvents.OnDayEnded -= HandleDayEnded;
        GameEvents.OnGameFinished -= HandleGameFinished;
    }

    void HandleDayStarted(int day)
    {
        summaryPanel.SetActive(false);
        dayLabel.text = $"Day {day}";
        dayProgressFill.fillAmount = 0f;
    }

    void HandleTimeChanged(float t)
    {
        dayProgressFill.fillAmount = t;
    }

    void HandleDayEnded(int day)
    {
        summaryPanel.SetActive(true);
        summaryLabel.text = $"Day {day} complete!";
        continueButton.gameObject.SetActive(true);
    }

    void HandleGameFinished(int day)
    {
        summaryLabel.text = $"You survived {day} days with your worm!";
        continueButton.gameObject.SetActive(false);
    }
}
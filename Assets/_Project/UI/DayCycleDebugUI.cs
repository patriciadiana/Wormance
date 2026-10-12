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
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private PlayerEntity player;

    private void Awake()
    {
        if (player == null)
        {
            Debug.LogError($"{nameof(player)} is not assigned on {name}.", this);
        }
    }

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

    void Update()
    {
        RefreshStatsText();
    }

    void HandleDayStarted(int day)
    {
        summaryPanel.SetActive(false);
        dayLabel.text = $"Day {day}";
        RefreshStatsText();
        dayProgressFill.fillAmount = 0f;
    }

    private void RefreshStatsText()
    {
        if (statsText == null) return;

        statsText.text = player != null && player.Stats != null
            ? $"Stats: {player.Stats}"
            : "Stats: unavailable";
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
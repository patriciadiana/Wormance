using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private WormAI wormAI;

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

        if (wormAI == null || wormAI.Stats == null)
        {
            statsText.text = "Stats: unavailable";
            return;
        }

        var s = wormAI.Stats;

        statsText.text =
            $"Stats:\n" +
            $"<color=#4FC3F7>Sleep:</color> {s.Sleep}\n" +
            $"<color=#FFB74D>Hunger:</color> {s.Hunger}\n" +
            $"<color=#F06292>Love:</color> {s.Love}\n" +
            $"<color=#81C784>Happiness:</color> {s.Happiness}";
    }

    void HandleTimeChanged(float t)
    {
        dayProgressFill.fillAmount = 1f - t;
    }

    void HandleDayEnded(int day)
    {
        summaryPanel.SetActive(true);
        summaryLabel.text = BuildEndOfDaySummary(day);
        continueButton.gameObject.SetActive(true);
    }

    private string BuildEndOfDaySummary(int day)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Day {day} complete!");

        var suspicions = SuspicionManager.Instance != null
            ? SuspicionManager.Instance.GetNonZeroSuspicions()
            : null;

        if (suspicions != null && suspicions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Suspicion:");
            foreach (var kvp in suspicions)
                sb.AppendLine($"  {kvp.Key}: {Mathf.RoundToInt(kvp.Value)}");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("Suspicion: none");
        }

        sb.AppendLine();
        if (wormAI != null && wormAI.Stats != null)
        {
            var s = wormAI.Stats;
            sb.AppendLine("Worm:");
            sb.AppendLine($"  Sleep: {s.Sleep}");
            sb.AppendLine($"  Hunger: {s.Hunger}");
            sb.AppendLine($"  Love: {s.Love}");
            sb.AppendLine($"  Happiness: {s.Happiness}");
        }
        else
        {
            sb.AppendLine("Worm: unavailable");
        }

        return sb.ToString().TrimEnd();
    }

    void HandleGameFinished(int day)
    {
        summaryLabel.text = $"You survived {day} days with your worm!";
        continueButton.gameObject.SetActive(false);
    }

}
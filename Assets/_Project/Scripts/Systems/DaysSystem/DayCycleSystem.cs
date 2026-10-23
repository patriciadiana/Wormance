using UnityEngine;

public class DayCycleSystem : MonoBehaviour
{
    [SerializeField] private DayCycleConfig config;
    [SerializeField] private bool autoStart = true;

    public int currentDay { get; private set; }
    public float dayTimer { get; private set; }
    public float dayDuration { get; private set; }
    public float dayProgress => dayDuration <= 0 ? 0 : Mathf.Clamp01(dayTimer / dayDuration);
    public bool isRunning { get; private set; }

    private void Start()
    {
        if (autoStart)
            StartNewGame();
    }

    public void StartNewGame()
    {
        currentDay = config.startingDay - 1;
        GameStateMachine.Instance.SetState(GameState.Playing);
        BeginDay();
    }

    void Update()
    {
        if (!isRunning) return;
        if (!GameStateMachine.Instance.IsPlaying) return;

        dayTimer += Time.deltaTime;
        GameEvents.RaiseDayTimeChanged(dayProgress);

        if (dayTimer >= dayDuration)
            EndDay();
    }

    void BeginDay()
    {
        currentDay++;
        dayDuration = config.dayDurationSeconds;
        dayTimer = 0f;
        isRunning = true;

        GameStateMachine.Instance.SetState(GameState.DayIntro);
        GameEvents.RaiseDayStarted(currentDay);

        Invoke(nameof(EnterGameplay), 1.5f);
    }

    void EnterGameplay()
    {
        GameStateMachine.Instance.SetState(GameState.Playing);
    }

    void EndDay()
    {
        isRunning = false;
        GameEvents.RaiseDayEnded(currentDay);
        GameStateMachine.Instance.SetState(GameState.DaySummary);

        if (currentDay >= config.finalDay)
        {
            GameStateMachine.Instance.SetState(GameState.Victory);
            GameEvents.RaiseGameFinished(currentDay);
        }
    }

    public void ContinueToNextDay()
    {
        if (currentDay >= config.finalDay) return;
        BeginDay();
    }
}

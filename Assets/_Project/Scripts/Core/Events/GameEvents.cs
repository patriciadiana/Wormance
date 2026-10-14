using System;
using UnityEngine;

public enum GameState
{
    Boot,
    MainMenu,
    DayIntro,
    Playing,
    Paused,
    Dialogue,
    DaySummary,
    GameOver,
    Victory
}

public static class GameEvents
{
    public static event Action<int> OnDayStarted;
    public static event Action<int > OnDayEnded;
    public static event Action<float> OnDayTimeChanged;
    public static event Action<int> OnGameFinished; /*the necessary days passed*/

    public static event Action<GameState> OnGameStateChanged;

    public static void RaiseDayStarted(int day) => OnDayStarted?.Invoke(day);
    public static void RaiseDayEnded(int day) => OnDayEnded?.Invoke(day);
    public static void RaiseDayTimeChanged(float t) => OnDayTimeChanged?.Invoke(t);
    public static void RaiseGameFinished(int day) => OnGameFinished?.Invoke(day);
    public static void RaiseGameStateChanged(GameState state)
        => OnGameStateChanged?.Invoke(state);
}

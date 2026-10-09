using UnityEngine;

[CreateAssetMenu(menuName = "Wormance/Day Cycle Config", fileName = "DayCycleConfig")]
public class DayCycleConfig : ScriptableObject
{
    [Header("Timing")]
    [Tooltip("Real-world seconds per in-game day.")]
    public float dayDurationSeconds = 300f; // 5 minutes

    [Header("Progression")]
    public int startingDay = 1;
    public int finalDay = 10;
}
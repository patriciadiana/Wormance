using System.Collections;
using UnityEngine;

public class MomCallSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MomPhoneCall phone;

    [Header("Call Days")]
    [SerializeField] private int minDay = 2;
    [SerializeField] private int maxDay = 8;

    [Header("Dialogues")]
    [SerializeField] private DialogueData[] momDialogues;

    private bool hasCalledToday = false;
    private bool callScheduledForDay = false;
    private int scheduledDay = -1;
    private int currentCallIndex = 0;

    private void OnEnable()
    {
        GameEvents.OnDayStarted += HandleDayStarted;
        GameEvents.OnDayEnded += HandleDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnDayStarted -= HandleDayStarted;
        GameEvents.OnDayEnded -= HandleDayEnded;
    }

    private void HandleDayStarted(int day)
    {
        hasCalledToday = false;

        if (currentCallIndex >= momDialogues.Length)
            return;

        if (!callScheduledForDay)
        {
            int startDay = Mathf.Max(minDay, day);

            if (startDay > maxDay) return;

            scheduledDay = Random.Range(startDay, maxDay + 1);
            callScheduledForDay = true;
            Debug.Log($"[MomCallSpawner] Next mom call scheduled for Day {scheduledDay} (Call #{currentCallIndex + 1})");
        }

        if (day == scheduledDay && !hasCalledToday)
        {
            StartCoroutine(RingPhoneAfterDelay());
        }
    }

    private IEnumerator RingPhoneAfterDelay()
    {
        float delay = Random.Range(5f, 10f);
        yield return new WaitForSeconds(delay);

        if (GameStateMachine.Instance != null && !GameStateMachine.Instance.IsPlaying)
            yield break;

        if (phone != null)
        {
            DialogueData nextDialogue = momDialogues[currentCallIndex];

            phone.StartRinging(nextDialogue);
            hasCalledToday = true;

            currentCallIndex++;
        }
    }

    private void HandleDayEnded(int day)
    {
        if (day == scheduledDay)
        {
            callScheduledForDay = false;
        }
    }
}
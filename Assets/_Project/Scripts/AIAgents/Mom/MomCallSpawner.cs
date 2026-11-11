
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomCallSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MomPhoneCall phone;

    [Header("Call Days")]
    [SerializeField] private int minDay = 1;
    [SerializeField] private int maxDay = 7;

    [Header("Regular Dialogues")]
    [SerializeField] private DialogueData[] momDialogues;

    [Header("Conditioned Dialogues")]
    [SerializeField] private DialogueData[] suspicionDialogues;
    [SerializeField] private float suspicionThreshold = 30f;

    private bool hasCalledToday = false;
    private bool isRingingScheduled = false;

    private int currentCallIndex = 0;
    private int currentSuspicionCallIndex = 0;

    private int[] regularCallDays;
    private bool regularDaysAssigned = false;

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

        if (!regularDaysAssigned)
        {
            ScheduleRegularCalls(day);
        }

        if (currentCallIndex < momDialogues.Length &&
            day >= regularCallDays[currentCallIndex])
        {
            StartCoroutine(RingPhoneAfterDelay(false));
        }

        CheckSuspicionCalls();
    }

    private void ScheduleRegularCalls(int currentDay)
    {
        int startDay = Mathf.Max(minDay, currentDay);

        int availableDays = maxDay - startDay + 1;
        int numberOfCalls = momDialogues.Length;

        if (numberOfCalls == 0)
        {
            regularDaysAssigned = true;
            return;
        }

        if (availableDays < numberOfCalls)
        {
            Debug.LogWarning(
                "[MomCallSpawner] Not enough days for all regular calls. " +
                "Increase maxDay or reduce the number of calls."
            );

            return;
        }

        List<int> available = new List<int>();

        for (int day = startDay; day <= maxDay; day++)
        {
            available.Add(day);
        }

        for (int i = 0; i < available.Count; i++)
        {
            int randomIndex = Random.Range(i, available.Count);

            int temp = available[i];
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        regularCallDays = new int[numberOfCalls];

        for (int i = 0; i < numberOfCalls; i++)
        {
            regularCallDays[i] = available[i];
        }

        System.Array.Sort(regularCallDays);

        regularDaysAssigned = true;
    }

    private void CheckSuspicionCalls()
    {
        if (currentSuspicionCallIndex >= suspicionDialogues.Length)
            return;

        if (SuspicionManager.Instance == null)
            return;

        float suspicion = SuspicionManager.Instance.GetSuspicionLevel(
            suspicionDialogues[currentSuspicionCallIndex].npcId
        );

        if (suspicion > suspicionThreshold)
        {
            if (!isRingingScheduled)
                StartCoroutine(RingPhoneAfterDelay(true));
        }
    }

    private IEnumerator RingPhoneAfterDelay(bool isSuspicionCall)
    {
        if (isRingingScheduled)
            yield break;

        isRingingScheduled = true;

        float delay = Random.Range(5f, 10f);
        yield return new WaitForSeconds(delay);

        if (GameStateMachine.Instance != null &&
            !GameStateMachine.Instance.IsPlaying)
        {
            isRingingScheduled = false;
            yield break;
        }

        if (phone != null && !hasCalledToday)
        {
            DialogueData nextDialogue;

            if (isSuspicionCall)
            {
                nextDialogue = suspicionDialogues[currentSuspicionCallIndex];
                currentSuspicionCallIndex++;
            }
            else
            {
                nextDialogue = momDialogues[currentCallIndex];
                currentCallIndex++;
            }

            phone.StartRinging(nextDialogue);
            hasCalledToday = true;
        }

        isRingingScheduled = false;
    }

    private void HandleDayEnded(int day)
    {
    }
}


using System.Collections;
using UnityEngine;

public class MomCallSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MomPhoneCall phone;

    [Header("Call Days")]
    [SerializeField] private int minDay = 2;
    [SerializeField] private int maxDay = 8;

    [Header("Regular Dialogues")]
    [SerializeField] private DialogueData[] momDialogues;

    [Header("Conditioned Dialogues")]
    [SerializeField] private DialogueData[] suspicionDialogues;
    [SerializeField] private float suspicionThreshold = 30f;

    private bool hasCalledToday = false;
    private bool callScheduledForDay = false;
    private int scheduledDay = -1;
    private int currentCallIndex = 0;
    private int currentSuspicionCallIndex = 0;

    private bool isRingingScheduled = false;

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

        if (currentCallIndex < momDialogues.Length)
        {
            if (!callScheduledForDay)
            {
                int startDay = Mathf.Max(minDay, day);

                if (startDay <= maxDay)
                {
                    scheduledDay = Random.Range(startDay, maxDay + 1);
                    callScheduledForDay = true;

                    Debug.Log($"[MomCallSpawner] Next regular call scheduled for Day {scheduledDay} (Call #{currentCallIndex + 1})");
                }
            }

            if (day == scheduledDay && !hasCalledToday)
            {
                StartCoroutine(RingPhoneAfterDelay(false));
            }
        }

        CheckSuspicionCalls();
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
            Debug.Log($"[MomCallSpawner] Mom's suspicion is {suspicion}. Conditioned call #{currentSuspicionCallIndex + 1} is now eligible.");

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

        if (GameStateMachine.Instance != null && !GameStateMachine.Instance.IsPlaying)
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

                Debug.Log("[MomCallSpawner] Starting a conditioned suspicion call.");
            }
            else
            {
                nextDialogue = momDialogues[currentCallIndex];
                currentCallIndex++;

                Debug.Log("[MomCallSpawner] Starting a regular mom call.");
            }

            phone.StartRinging(nextDialogue);
            hasCalledToday = true;
        }

        isRingingScheduled = false;
    }

    private void HandleDayEnded(int day)
    {
        if (day == scheduledDay)
        {
            callScheduledForDay = false;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public enum FreakMode
{
    FirstMeeting, // first ever visit, gives the present
    SecondVisit,  // second visit: own dialogue, goes straight to the worm
    Visit         // every later visit: goes straight to the worm
}

public class FreakSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject freakPrefab;
    /* Where she starts */
    [SerializeField] private Transform freakHousePoint;
    /* Where she stops */
    [SerializeField] private Transform playerDoorPoint;
    [SerializeField] private List<Transform> stalkPoints;

    [Header("First Meeting")]
    [SerializeField] private DialogueData firstMeetingDialogue;
    [SerializeField] private GameObject firstMeetingPresentPrefab;

    [Header("Second Visit")]
    [SerializeField] private DialogueData secondVisitDialogue;

    [Header("Regular Visit")]
    [SerializeField] private DialogueData visitDialogue;

    [Header("Settings")]
    [SerializeField] private int minFirstDay = 2;
    [SerializeField] private int maxFirstDay = 7;
    [SerializeField] private float highSuspicionThreshold = 50f; // "half"

    private int firstMeetingDay = -1;
    private bool hasMetPlayer = false;
    private bool hasHadSecondVisit = false;
    private GameObject activefreak;

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

    void HandleDayStarted(int day)
    {
        /* Set the meeting day random */
        if (firstMeetingDay == -1)
        {
            firstMeetingDay = Random.Range(minFirstDay, maxFirstDay + 1);
        }

        bool shouldSpawn = false;
        FreakMode mode = FreakMode.Visit;
        DialogueData dialogueToUse = null;
        GameObject presentToGive = null;

        if (!hasMetPlayer && day == firstMeetingDay)
        {
            shouldSpawn = true;
            mode = FreakMode.FirstMeeting;
            dialogueToUse = firstMeetingDialogue;
            presentToGive = firstMeetingPresentPrefab;
        }
        /* After the first meeting */
        else if (hasMetPlayer && day > firstMeetingDay)
        {
            float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Jeremy");
            float spawnChance = suspicion >= highSuspicionThreshold ? 0.8f : 0.2f;

            if (Random.value <= spawnChance)
            {
                shouldSpawn = true;

                if (!hasHadSecondVisit)
                {
                    // The next visit after the first meeting is always the second visit
                    mode = FreakMode.SecondVisit;
                    dialogueToUse = secondVisitDialogue;
                }
                else
                {
                    // Every later visit. After the dialogue he stalks if suspicion is over half but not full.
                    mode = FreakMode.Visit;
                    dialogueToUse = visitDialogue;
                }
            }
        }

        if (shouldSpawn)
        {
            Spawnfreak(mode, dialogueToUse, presentToGive);
        }
    }

    void Spawnfreak(FreakMode mode, DialogueData dialogue, GameObject present)
    {
        if (activefreak != null) return;

        activefreak = Instantiate(freakPrefab, freakHousePoint.position, freakHousePoint.rotation);

        var freakScript = activefreak.GetComponent<FreakNPC>();
        if (freakScript != null)
        {
            if (mode == FreakMode.FirstMeeting)
            {
                // Only the first meeting goes to the door, and he never stalks on it
                // (no stalk points configured = no stalking, whatever the suspicion)
                freakScript.Setup(dialogue, present, playerDoorPoint);
            }
            else
            {
                // Stalk points + threshold, used after the dialogue ends
                freakScript.ConfigureStalking(stalkPoints, highSuspicionThreshold);

                // Every other visit skips the door and goes straight to the worm
                freakScript.SetupFollowWorm(dialogue, playerDoorPoint);
            }
        }

        if (mode == FreakMode.SecondVisit)
        {
            hasHadSecondVisit = true;
        }

        Debug.Log($"[Spawner] freak spawned in {mode} mode!");
    }

    void HandleDayEnded(int day)
    {
        if (activefreak != null)
        {
            Destroy(activefreak);
        }
    }

    public void MarkAsMet() => hasMetPlayer = true;
}
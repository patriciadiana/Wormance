using System.Collections.Generic;
using UnityEngine;

public enum FreakMode
{
    FirstMeeting, // first ever visit, gives the present
    SecondVisit,  // second visit (JeremySpotsWorm): goes straight to the worm
    Offer,        // day after picking the 2nd option on SpotsWorm (JeremyOffer)
    Ending,       // day after picking the 2nd option on Offer (JeremyEnding): steals the worm
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

    [Header("Story: Offer and Ending")]
    [SerializeField] private DialogueData offerDialogue;
    [SerializeField] private DialogueData endingDialogue;
    /* 0 = first option, 1 = second option, 2 = third option */
    [SerializeField] private int advanceChoiceIndex = 1;

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

    // Story progression
    private int currentDay;
    private int lastTalkDay = -1; // day the player last finished a dialogue with him
    private FreakMode currentMode;
    private FreakMode? pendingMode = null; // story visit that has to happen on pendingDay
    private int pendingDay;

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
        currentDay = day;

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
        else if (hasMetPlayer && day > firstMeetingDay && pendingMode.HasValue && day >= pendingDay)
        {
            // Story visit (Offer / Ending): always happens the day after the choice.
            // If the player skipped the dialogue, it comes back the next day.
            shouldSpawn = true;
            mode = pendingMode.Value;
            dialogueToUse = mode == FreakMode.Ending ? endingDialogue : offerDialogue;
        }
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

        // Already talked to him today: he doesn't come back until tomorrow
        if (currentDay == lastTalkDay) return;

        activefreak = Instantiate(freakPrefab, freakHousePoint.position, freakHousePoint.rotation);
        currentMode = mode;

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

                // Every other visit skips the door and goes straight to the worm.
                // On the ending he steals the worm after the dialogue, whatever the suspicion.
                freakScript.SetupFollowWorm(dialogue, playerDoorPoint, mode == FreakMode.Ending);
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

            // Destroy() only takes effect at the end of the frame. If the next day starts in the same
            // frame, the old freak would still count as "alive" and block the new spawn. So clear it now.
            activefreak = null;
        }
    }

    public void MarkAsMet() => hasMetPlayer = true;

    // Called by the freak when a dialogue ends. chosenIndex = last choice the player picked (-1 if none).
    public void OnDialogueFinished(int chosenIndex)
    {
        lastTalkDay = currentDay;

        bool advance = chosenIndex == advanceChoiceIndex;

        // The story visit that was waiting has now been talked through
        if (currentMode == FreakMode.Offer || currentMode == FreakMode.Ending)
        {
            pendingMode = null;
        }

        if (!advance) return;

        if (currentMode == FreakMode.SecondVisit)
        {
            // 2nd option on JeremySpotsWorm -> JeremyOffer next day
            pendingMode = FreakMode.Offer;
            pendingDay = currentDay + 1;
            Debug.Log("[Spawner] Offer queued for the next day");
        }
        else if (currentMode == FreakMode.Offer)
        {
            // 2nd option on JeremyOffer -> JeremyEnding next day
            pendingMode = FreakMode.Ending;
            pendingDay = currentDay + 1;
            Debug.Log("[Spawner] Ending queued for the next day");
        }
    }
}
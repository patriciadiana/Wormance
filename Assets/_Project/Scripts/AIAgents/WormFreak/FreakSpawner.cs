using System.Collections.Generic;
using UnityEngine;

public enum FreakMode
{
    FirstMeeting, // first ever visit, gives the present
    SecondVisit,  // second visit (JeremySpotsWorm): goes straight to the worm
    Offer,        // day after picking the 2nd option on SpotsWorm (JeremyOffer)
    Ending,       // day after picking the 2nd option on Offer: FreakEnding takes over
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

    [Header("Story: Offer")]
    [SerializeField] private DialogueData offerDialogue;
    /* 0 = first option, 1 = second option, 2 = third option */
    [SerializeField] private int advanceChoiceIndex = 1;

    [Header("Regular Visit")]
    [SerializeField] private DialogueData visitDialogue;

    [Header("Settings")]
    [SerializeField] private int minFirstDay = 2;
    [SerializeField] private int maxFirstDay = 7;
    [SerializeField] private float highSuspicionThreshold = 50f; // "half"
    /* He visits every this many days (2 = every other day) */
    [SerializeField] private int visitEveryDays = 2;

    private int firstMeetingDay = -1;
    private bool hasMetPlayer = false;
    private bool hasHadSecondVisit = false;
    private GameObject activefreak;

    // Schedule
    private int lastVisitDay = -100;
    private bool visitsStopped; // set when an ending takes over

    // Story progression
    private int currentDay;
    private int lastTalkDay = -1; // day the player last finished a dialogue with him
    private FreakMode currentMode;
    private FreakMode? pendingMode = null; // story visit that has to happen on pendingDay
    private int pendingDay;

    // FreakEnding uses these
    public bool HasActiveFreak => activefreak != null;
    public void StopVisits() => visitsStopped = true;

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

        if (visitsStopped) return; // an ending has taken over

        /* Set the meeting day random */
        if (firstMeetingDay == -1)
        {
            firstMeetingDay = Random.Range(minFirstDay, maxFirstDay + 1);
        }

        bool shouldSpawn = false;
        FreakMode mode = FreakMode.Visit;
        DialogueData dialogueToUse = null;
        GameObject presentToGive = null;

        // Normal visits happen every other day
        bool visitDue = day - lastVisitDay >= visitEveryDays;

        if (!hasMetPlayer)
        {
            // First meeting (tries again every other day if the player never talked to him)
            if (day >= firstMeetingDay && visitDue)
            {
                shouldSpawn = true;
                mode = FreakMode.FirstMeeting;
                dialogueToUse = firstMeetingDialogue;
                presentToGive = firstMeetingPresentPrefab;
            }
        }
        else if (pendingMode.HasValue && day >= pendingDay)
        {
            // Story visit: always the day after the choice, whatever the schedule says
            if (pendingMode.Value == FreakMode.Ending)
            {
                // The ending is a scripted sequence (front door -> steals the worm -> ending screen)
                pendingMode = null;
                FindAnyObjectByType<FreakEnding>()?.StartChoiceEnding();
                return;
            }

            shouldSpawn = true;
            mode = pendingMode.Value; // Offer
            dialogueToUse = offerDialogue;
        }
        else if (visitDue)
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
        lastVisitDay = currentDay;

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
        if (currentMode == FreakMode.Offer)
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
            // 2nd option on JeremyOffer -> the ending next day
            pendingMode = FreakMode.Ending;
            pendingDay = currentDay + 1;
            Debug.Log("[Spawner] Ending queued for the next day");
        }
    }
}
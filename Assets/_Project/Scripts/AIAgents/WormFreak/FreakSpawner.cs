using UnityEngine;

public class FreakSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject freakPrefab;
    /* Where she starts */
    [SerializeField] private Transform freakHousePoint;
    /* Where she stops */
    [SerializeField] private Transform playerDoorPoint;     

    [Header("First Meeting")]
    [SerializeField] private DialogueData firstMeetingDialogue;
    [SerializeField] private GameObject firstMeetingPresentPrefab;

    [Header("Settings")]
    [SerializeField] private int minFirstDay = 2;
    [SerializeField] private int maxFirstDay = 7;
    [SerializeField] private float highSuspicionThreshold = 50f;

    private int firstMeetingDay = -1;
    private bool hasMetPlayer = false;
    private GameObject activefreak;
    private bool freakVisitedToday = false;

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
        freakVisitedToday = false;

        /* Set the meeting day random */
        if (firstMeetingDay == -1)
        {
            firstMeetingDay = Random.Range(minFirstDay, maxFirstDay + 1);
        }

        bool shouldSpawn = false;
        DialogueData dialogueToUse = null;
        GameObject presentToGive = null;

        if (!hasMetPlayer && day == firstMeetingDay)
        {
            shouldSpawn = true;
            dialogueToUse = firstMeetingDialogue;
            presentToGive = firstMeetingPresentPrefab;
        }
        /* Decide what to do based on suspicion */
        else if (hasMetPlayer && day > firstMeetingDay)
        {
            float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Jeremy");
            float spawnChance = suspicion >= highSuspicionThreshold ? 0.8f : 0.2f;

            if (Random.value <= spawnChance)
            {
                shouldSpawn = true;
                if (suspicion >= highSuspicionThreshold)
                {
                    
                }
                else
                {

                }
            }
        }

        if (shouldSpawn)
        {
            Spawnfreak(dialogueToUse, presentToGive);
        }
    }

    void Spawnfreak(DialogueData dialogue, GameObject present)
    {
        if (activefreak != null) return;

        activefreak = Instantiate(freakPrefab, freakHousePoint.position, freakHousePoint.rotation);

        var freakScript = activefreak.GetComponent<FreakNPC>();
        if (freakScript != null)
        {
            freakScript.Setup(dialogue, present, playerDoorPoint);
        }

        freakVisitedToday = true;
        Debug.Log("[Spawner] freak spawned!");
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

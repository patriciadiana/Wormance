using UnityEngine;

public class NeighbourSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject neighbourPrefab;
    /* Where she starts */
    [SerializeField] private Transform neighbourHousePoint;
    /* Where she stops */
    [SerializeField] private Transform playerDoorPoint;
    [SerializeField] private Transform snoopPoint;

    [Header("First Meeting")]
    [SerializeField] private DialogueData firstMeetingDialogue;
    [SerializeField] private GameObject firstMeetingPresentPrefab;

    [Header("Subsequent Visits - High Suspicion")]
    [SerializeField] private DialogueData[] highSuspicionDialogues; 
    [SerializeField] private GameObject[] highSuspicionPresents; 

    [Header("Subsequent Visits - Low Suspicion")]
    [SerializeField] private DialogueData[] lowSuspicionDialogues; 
    [SerializeField] private GameObject[] lowSuspicionPresents;

    [Header("Settings")]
    [SerializeField] private int minFirstDay = 2;
    [SerializeField] private int maxFirstDay = 7;
    [SerializeField] private float highSuspicionThreshold = 50f;

    private int firstMeetingDay = -1;
    private bool hasMetPlayer = false;
    private GameObject activeNeighbour;
    private bool neighbourVisitedToday = false;

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
        neighbourVisitedToday = false;

        /* Set the meeting day random */
        if (firstMeetingDay == -1)
        {
            firstMeetingDay = Random.Range(minFirstDay, maxFirstDay + 1);
        }

        bool shouldSpawn = false;
        DialogueData dialogueToUse = null;
        GameObject presentToGive = null;
        Transform targetDestination = playerDoorPoint;

        /*First meeting logic*/
        if (!hasMetPlayer && day == firstMeetingDay)
        {
            shouldSpawn = true;
            dialogueToUse = firstMeetingDialogue;
            presentToGive = firstMeetingPresentPrefab;
        }
        /* Decide what to do based on suspicion */
        else if (hasMetPlayer && day > firstMeetingDay)
        {
            float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Neighbour");
            float spawnChance = suspicion >= highSuspicionThreshold ? 0.8f : 0.2f;

            if (Random.value <= spawnChance)
            {
                shouldSpawn = true;

                if (suspicion >= highSuspicionThreshold)
                {
                    targetDestination = snoopPoint;

                    if (highSuspicionDialogues.Length > 0)
                    {
                        int index = Random.Range(0, highSuspicionDialogues.Length);
                        dialogueToUse = highSuspicionDialogues[index];
                    }
                    if (highSuspicionPresents.Length > 0)
                    {
                        int index = Random.Range(0, highSuspicionPresents.Length);
                        presentToGive = highSuspicionPresents[index];
                    }
                }
                else
                {
                    targetDestination = playerDoorPoint;

                    if (lowSuspicionDialogues.Length > 0)
                    {
                        int index = Random.Range(0, lowSuspicionDialogues.Length);
                        dialogueToUse = lowSuspicionDialogues[index];
                    }
                    if (lowSuspicionPresents.Length > 0)
                    {
                        int index = Random.Range(0, lowSuspicionPresents.Length);
                        presentToGive = lowSuspicionPresents[index];
                    }
                }
            }
        }

        if (shouldSpawn)
        {
            SpawnNeighbour(dialogueToUse, presentToGive, targetDestination);
        }
    }

    void SpawnNeighbour(DialogueData dialogue, GameObject present, Transform destination)
    {
        if (activeNeighbour != null) return;

        activeNeighbour = Instantiate(neighbourPrefab, neighbourHousePoint.position, neighbourHousePoint.rotation);

        var neighbourScript = activeNeighbour.GetComponent<NeighbourNPC>();
        if (neighbourScript != null)
        {
            neighbourScript.Setup(dialogue, present, destination);
        }

        neighbourVisitedToday = true;
        Debug.Log("[Spawner] Neighbour spawned!");
    }

    void HandleDayEnded(int day)
    {
        if (activeNeighbour != null)
        {
            Destroy(activeNeighbour);
        }
    }

    public void MarkAsMet() => hasMetPlayer = true;
}

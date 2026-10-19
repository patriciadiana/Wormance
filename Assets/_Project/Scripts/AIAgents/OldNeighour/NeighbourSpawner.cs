using UnityEngine;

public class NeighbourSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject neighbourPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("First Meeting")]
    [SerializeField] private DialogueData firstMeetingDialogue;
    [SerializeField] private GameObject firstMeetingPresentPrefab;

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

        if (!hasMetPlayer && day == firstMeetingDay)
        {
            shouldSpawn = true;
            dialogueToUse = firstMeetingDialogue;
            presentToGive = firstMeetingPresentPrefab;
        }
        /* Decide what to do based on suspicion */
        else if (hasMetPlayer && day > firstMeetingDay)
        {
            float suspicion = SuspicionSystem.Value;
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
            SpawnNeighbour(dialogueToUse, presentToGive);
        }
    }

    void SpawnNeighbour(DialogueData dialogue, GameObject present)
    {
        if (activeNeighbour != null) return;

        activeNeighbour = Instantiate(neighbourPrefab, spawnPoint.position, spawnPoint.rotation);

        var neighbourScript = activeNeighbour.GetComponent<NeighbourNPC>();
        if (neighbourScript != null)
        {
            neighbourScript.Setup(dialogue, present);
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
}

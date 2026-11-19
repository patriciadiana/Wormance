using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FreakEnding : MonoBehaviour
{
    private enum EndingType { Suspicion, Choice }

    [Header("Suspicion")]
    [SerializeField] private string freakNpcId = "Jeremy";
    [SerializeField] private float suspicionLimit = 100f;

    [Header("Freak")]
    [SerializeField] private GameObject freakPrefab;
    /* Place this just outside the front door */
    [SerializeField] private Transform spawnPoint;
    /* Optional, found automatically if empty */
    [SerializeField] private FreakSpawner spawner;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float dialogueDistance = 3f;

    [Header("Main Door")]
    [SerializeField] private DoorInteraction mainDoor;

    [Header("Stealing the worm")]
    [SerializeField] private float stealDistance = 1.2f;
    /* If he can't reach the worm in this time, the worm is taken anyway so the game can't get stuck */
    [SerializeField] private float stealTimeout = 30f;

    [Header("Timing")]
    [SerializeField] private float arrivalDelay = 1f;
    [SerializeField] private float delayBeforeEndingScreen = 1.5f;

    [Header("Ending 1: full suspicion")]
    /* Optional: leave empty for no dialogue at the door */
    [SerializeField] private DialogueData suspicionDialogue;
    [SerializeField] private string suspicionTitle = "Too Suspicious";
    [TextArea(3, 6)]
    [SerializeField] private string suspicionDescription = "Jeremy saw right through you. He took the worm and was gone before you could stop him.";

    [Header("Ending 2: the player's choices")]
    /* JeremyEnding */
    [SerializeField] private DialogueData choiceDialogue;
    [SerializeField] private string choiceTitle = "A Fair Deal";
    [TextArea(3, 6)]
    [SerializeField] private string choiceDescription = "You made your choices, and Jeremy kept his end of the deal. He left with the worm.";

    private bool endingStarted;

    private void Awake()
    {
        if (spawner == null)
        {
            spawner = FindAnyObjectByType<FreakSpawner>();
        }
    }

    private void OnEnable()
    {
        SuspicionManager.OnSuspicionChanged += HandleSuspicionChanged;
    }

    private void OnDisable()
    {
        SuspicionManager.OnSuspicionChanged -= HandleSuspicionChanged;
    }

    private void HandleSuspicionChanged(string npcId, float suspicion)
    {
        if (npcId != freakNpcId || suspicion < suspicionLimit || endingStarted)
            return;

        StartCoroutine(PlayEnding(EndingType.Suspicion));
    }

    // Called by the FreakSpawner on the day after the player's last choice
    public void StartChoiceEnding()
    {
        if (endingStarted) return;

        StartCoroutine(PlayEnding(EndingType.Choice));
    }

    private IEnumerator PlayEnding(EndingType type)
    {
        endingStarted = true;

        DialogueData dialogue = type == EndingType.Suspicion ? suspicionDialogue : choiceDialogue;
        string title = type == EndingType.Suspicion ? suspicionTitle : choiceTitle;
        string description = type == EndingType.Suspicion ? suspicionDescription : choiceDescription;

        // If Jeremy is in the middle of a normal visit, let it finish first
        while (spawner != null && spawner.HasActiveFreak)
            yield return null;

        spawner?.StopVisits();

        GameStateMachine.Instance.SetState(GameState.Paused);

        yield return new WaitForSeconds(arrivalDelay);

        if (freakPrefab == null || spawnPoint == null)
        {
            GameStateMachine.Instance.SetState(GameState.Playing);
            yield break;
        }

        // He shows up at the front door
        GameObject freak = Instantiate(freakPrefab, spawnPoint.position, spawnPoint.rotation);

        // The normal visit behaviour (and its interaction) isn't wanted in the ending
        FreakNPC visitScript = freak.GetComponent<FreakNPC>();
        if (visitScript != null)
            Destroy(visitScript);

        DialogueComponent freakDialogue = freak.GetComponent<DialogueComponent>();
        NavMeshAgent agent = freak.GetComponent<NavMeshAgent>();

        if (mainDoor != null && !mainDoor.IsOpen)
        {
            mainDoor.Interact(gameObject);
            yield return new WaitUntil(() => !mainDoor.IsMoving);
        }

        GameStateMachine.Instance.SetState(GameState.Playing);

        // Optional dialogue at the door, once the player comes close
        if (dialogue != null && freakDialogue != null)
        {
            freakDialogue.dialogueData = dialogue;

            while (player != null &&
                   Vector3.Distance(player.position, freak.transform.position) > dialogueDistance)
            {
                yield return null;
            }

            bool dialogueDone = false;
            freakDialogue.onDialogueEnded += () => dialogueDone = true;

            DialogueSystem.Instance.HandleInteraction(freakDialogue);

            yield return new WaitUntil(() => dialogueDone);
        }

        // Then he goes to steal the worm
        WormAI worm = FindAnyObjectByType<WormAI>();

        if (worm != null && agent != null && freak != null)
        {
            agent.isStopped = false;

            // Agent may stop at its stoppingDistance, so never require getting closer than that
            float reachDistance = Mathf.Max(stealDistance, agent.stoppingDistance + 0.5f);
            float time = stealTimeout;

            while (time > 0f && worm != null && freak != null)
            {
                time -= Time.deltaTime;

                if (Vector3.Distance(freak.transform.position, worm.transform.position) <= reachDistance)
                    break;

                // The worm moves, so keep updating the target
                if (NavMesh.SamplePosition(worm.transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);

                yield return null;
            }
        }

        if (worm != null)
        {
            Debug.Log("[FreakEnding] Jeremy stole the worm");
            Destroy(worm.gameObject);
        }

        // Then the ending screen pops up
        yield return new WaitForSeconds(delayBeforeEndingScreen);

        GameStateMachine.Instance.SetState(GameState.GameOver);
        GameEvents.RaiseGameEnded(title, description);
    }
}
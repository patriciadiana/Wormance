using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DialogueComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public class NeighbourNPC : MonoBehaviour, IInteractable
{
    private DialogueComponent dialogueComponent;
    private NavMeshAgent agent;

    [Header("Present Settings")]
    [SerializeField] private GameObject presentToGive;
    [SerializeField] private Transform handTransform;

    // This will store the location she came from
    private Transform goHomePoint;
    private bool hasGivenPresent = false;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
        agent = GetComponent<NavMeshAgent>();
    }

    // We add 'spawnPoint' to the Setup method so the Spawner can tell her where home is
    public void Setup(DialogueData data, GameObject present, Transform doorDestination, Transform spawnPoint)
    {
        dialogueComponent.dialogueData = data;
        presentToGive = present;
        dialogueComponent.onDialogueEnded = HandleDialogueEnded;

        goHomePoint = spawnPoint; // Save the home point!

        FindAnyObjectByType<NeighbourSpawner>()?.MarkAsMet();

        agent.SetDestination(doorDestination.position);
    }

    void HandleDialogueEnded()
    {
        if (presentToGive != null && !hasGivenPresent)
        {
            hasGivenPresent = true;

            Vector3 spawnPos = handTransform != null ? handTransform.position : transform.position + transform.forward * 1f;
            spawnPos.y -= 0.5f;

            Instantiate(presentToGive, spawnPos, Quaternion.identity);
            Debug.Log($"[Neighbour] Dropped present: {presentToGive.name}");
        }

        DayCycleSystem dayCycle = FindAnyObjectByType<DayCycleSystem>();

        if (dayCycle != null && dayCycle.currentDay >= 10)
        {
            float finalSuspicion = SuspicionManager.Instance.GetSuspicionLevel("Neighbour");

            if (finalSuspicion >= 60f)
            {
                StartCoroutine(TriggerGameOverEnding(
                    "Ending: Worm Protection Agency",
                    "Your negligence was reported. The worm has been taken away."
                ));
            }
            else
            {
                StartCoroutine(TriggerGameOverEnding(
                    "Ending: The Best Neighbour!",
                    "You raised a wonderful worm and made a true friend."
                ));
            }
            return;
        }

        StartCoroutine(WalkAwayAndLeave());
    }

    IEnumerator TriggerGameOverEnding(string title, string description)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(1.5f);

        Time.timeScale = 0f;

        GameStateMachine.Instance.SetState(GameState.GameOver);

        GameEvents.RaiseGameEnded(title, description);
    }

    IEnumerator WalkAwayAndLeave()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        agent.isStopped = false;

        if (goHomePoint != null)
        {
            agent.SetDestination(goHomePoint.position);
        }
        else
        {
            Vector3 goAwayPoint = transform.position - transform.forward * 20f;
            if (NavMesh.SamplePosition(goAwayPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        float timeout = 10f;
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0) break;
            yield return null;
        }

        Destroy(gameObject);
    }

    public string GetPrompt()
    {
        return "Talk to Neighbour";
    }

    public void Interact(GameObject interactor)
    {
        if (dialogueComponent != null && dialogueComponent.dialogueData != null)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            DialogueSystem.Instance.HandleInteraction(dialogueComponent);
        }
    }
}
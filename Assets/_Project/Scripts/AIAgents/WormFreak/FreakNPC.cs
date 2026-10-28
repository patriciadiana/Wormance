using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DialogueComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public class FreakNPC : MonoBehaviour, IInteractable
{
    [Header("Worm Follow")]
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float followDuration = 4f;
    [SerializeField] private float arriveTimeout = 10f;

    [Header("Worm Destroy")]
    [SerializeField] private float destroyDistance = 1.2f;
    [SerializeField] private float destroyTimeout = 15f;

    private DialogueComponent dialogueComponent;
    private NavMeshAgent agent;
    private GameObject presentToGive;
    private Transform goHomePoint;
    private WormAI worm;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(DialogueData data, GameObject present, Transform doorDestination)
    {
        dialogueComponent.dialogueData = data;
        presentToGive = present;

        dialogueComponent.onDialogueEnded = HandleDialogueEnded;

        agent.SetDestination(doorDestination.position);
    }

    void HandleDialogueEnded()
    {// da tips despre worm caring
        if (presentToGive != null)
        {
            Debug.Log($"[Freak] Gave present: {presentToGive.name}");
        }

        FindAnyObjectByType<FreakSpawner>()?.MarkAsMet();

        StartCoroutine(WalkAwayAndLeave());
    }

    IEnumerator WalkAwayAndLeave()
    {
        GetComponent<Collider>().enabled = false;

        // Same leave direction as the neighbour, captured before any worm interaction
        Vector3 homeDirection = -transform.forward;

        float suspicion = SuspicionSystem.Value;

        if (suspicion >= SuspicionSystem.Max)
        {
            // Suspicion is full: go to the worm and destroy it
            yield return DestroyWorm();
        }
        else if (Mathf.Approximately(suspicion, 0f))
        {
            // No suspicion: go next to the worm and follow it
            yield return FollowWorm();
        }

        // Identical to the neighbour's leave logic
        Vector3 goAwayPoint = transform.position + (homeDirection * 20f);

        agent.isStopped = false;
        if (NavMesh.SamplePosition(goAwayPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float timeout = 5f;
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0) break;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator DestroyWorm()
    {
        if (worm == null)
            worm = FindAnyObjectByType<WormAI>();

        if (worm == null)
            yield break;

        agent.isStopped = false;

        // Agent may stop at its stoppingDistance, so never require getting closer than that
        float reachDistance = Mathf.Max(destroyDistance, agent.stoppingDistance + 0.5f);

        float time = destroyTimeout;
        while (time > 0f && worm != null)
        {
            time -= Time.deltaTime;

            if (Vector3.Distance(transform.position, worm.transform.position) <= reachDistance)
            {
                Debug.Log("[Freak] Destroyed the worm");
                Destroy(worm.gameObject);
                worm = null;
                yield break;
            }

            // The worm moves, so keep updating the target
            Vector3 target = worm.transform.position;
            if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
                agent.SetDestination(navHit.position);

            yield return null;
        }
    }

    IEnumerator FollowWorm()
    {
        if (worm == null)
            worm = FindAnyObjectByType<WormAI>();

        if (worm == null)
            yield break;

        agent.isStopped = false;

        // Phase 1: walk up next to the worm (it moves, so keep updating the target)
        float arriveTime = arriveTimeout;
        while (arriveTime > 0f && worm != null)
        {
            arriveTime -= Time.deltaTime;

            if (Vector3.Distance(transform.position, worm.transform.position) <= followDistance + 0.5f)
                break;

            SetDestinationNextToWorm();
            yield return null;
        }

        // Phase 2: stay next to the worm and follow it
        float followTime = followDuration;
        while (followTime > 0f && worm != null)
        {
            followTime -= Time.deltaTime;

            SetDestinationNextToWorm();

            Vector3 lookDir = worm.transform.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    5f * Time.deltaTime);
            }

            yield return null;
        }
    }

    void SetDestinationNextToWorm()
    {
        Vector3 dirFromWorm = transform.position - worm.transform.position;
        dirFromWorm.y = 0f;
        dirFromWorm = dirFromWorm.sqrMagnitude > 0.001f ? dirFromWorm.normalized : Vector3.forward;

        Vector3 target = worm.transform.position + dirFromWorm * followDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            agent.SetDestination(navHit.position);
    }

    public string GetPrompt()
    {
        return "Talk to freak";
    }

    public void Interact(GameObject interactor)
    {
        if (dialogueComponent != null && dialogueComponent.dialogueData != null)
        {
            DialogueSystem.Instance.HandleInteraction(dialogueComponent);
        }
    }
}
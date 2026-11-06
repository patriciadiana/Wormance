using System.Collections;
using System.Collections.Generic;
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

    [Header("Stalking")]
    [SerializeField] private float pointArriveDistance = 0.6f;
    [SerializeField] private float pointTimeout = 20f;

    private DialogueComponent dialogueComponent;
    private NavMeshAgent agent;
    private GameObject presentToGive;
    private bool isFollowingWorm;
    private Transform goHomePoint;
    private WormAI worm;

    // Stalking state
    private List<Transform> unvisitedStalkPoints;
    private bool isStalking;
    private bool isWalkingToPoint;
    private Vector3 homePosition;
    private List<Transform> stalkPoints;
    private float stalkThreshold = 50f;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position; // where he spawned (his house)
    }

    // ------------------------------------------------------------
    // Normal visit setup
    // ------------------------------------------------------------
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

        // Suspicion is checked now, so the dialogue can raise or lower it.
        // Stalk only between the threshold and full: at full suspicion he skips
        // stalking, destroys the worm and leaves (handled in WalkAwayAndLeave).
        float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Jeremy");
        bool hasStalkPoints = stalkPoints != null && stalkPoints.Count > 0;

        if (suspicion >= stalkThreshold && suspicion < SuspicionManager.Max && hasStalkPoints)
        {
            BeginStalking();
            return;
        }

        StartCoroutine(WalkAwayAndLeave());
    }

    // ------------------------------------------------------------
    // Second visit setup: skips the door, goes straight to the worm
    // ------------------------------------------------------------
    public void SetupFollowWorm(DialogueData data, Transform doorFallback)
    {
        dialogueComponent.dialogueData = data;
        dialogueComponent.onDialogueEnded = HandleDialogueEnded;

        StartCoroutine(FollowWormUntilInteracted(doorFallback));
    }

    IEnumerator FollowWormUntilInteracted(Transform doorFallback)
    {
        isFollowingWorm = true;
        worm = FindAnyObjectByType<WormAI>();

        // No worm in the scene: fall back to the normal door visit
        if (worm == null)
        {
            isFollowingWorm = false;
            if (doorFallback != null)
                agent.SetDestination(doorFallback.position);
            yield break;
        }

        // Walk up to the worm and keep following it until the player talks to him
        yield return FollowWorm(true);
    }

    // ------------------------------------------------------------
    // Stalking setup
    // ------------------------------------------------------------
    // Called by the spawner for every visit
    public void ConfigureStalking(List<Transform> points, float threshold)
    {
        stalkPoints = points;
        stalkThreshold = threshold;
    }

    // Starts after the dialogue, before he goes home
    void BeginStalking()
    {
        isStalking = true;
        unvisitedStalkPoints = new List<Transform>(stalkPoints); // copy, so the spawner's list isn't modified

        GoToNextStalkPoint();
    }

    void GoToNextStalkPoint()
    {
        // Visited every point -> leave
        if (unvisitedStalkPoints == null || unvisitedStalkPoints.Count == 0)
        {
            isStalking = false;
            StartCoroutine(WalkAwayAndLeave(true));
            return;
        }

        int index = Random.Range(0, unvisitedStalkPoints.Count);
        Transform next = unvisitedStalkPoints[index];
        unvisitedStalkPoints.RemoveAt(index);

        StartCoroutine(WalkToStalkPoint(next));
    }

    IEnumerator WalkToStalkPoint(Transform point)
    {
        isWalkingToPoint = true;
        agent.isStopped = false;

        if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        // Wait until he arrives (with a safety timeout)
        float timeout = pointTimeout;
        while (timeout > 0f && (agent.pathPending || agent.remainingDistance > pointArriveDistance))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Face the same way as the stalk point
        transform.rotation = point.rotation;

        isWalkingToPoint = false;
    }

    // ------------------------------------------------------------
    // Leaving
    // ------------------------------------------------------------
    IEnumerator WalkAwayAndLeave(bool fromStalking = false)
    {
        GetComponent<Collider>().enabled = false;

        Vector3 homeDirection = -transform.forward;

        float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Jeremy");

        if (suspicion >= SuspicionManager.Max)
        {
            // Suspicion is full: go to the worm and destroy it
            // (for a stalker this happens after the last stalk point)
            yield return DestroyWorm();
        }

        // Stalkers walk back home; everyone else walks "behind" them like before
        Vector3 goAwayPoint = fromStalking
            ? homePosition
            : transform.position + (homeDirection * 20f);

        agent.isStopped = false;
        if (NavMesh.SamplePosition(goAwayPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float timeout = fromStalking ? 20f : 5f;
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0) break;
            yield return null;
        }

        Destroy(gameObject);
    }

    // ------------------------------------------------------------
    // Worm behaviours
    // ------------------------------------------------------------
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

    IEnumerator FollowWorm(bool untilInteracted = false)
    {
        if (worm == null)
            worm = FindAnyObjectByType<WormAI>();

        if (worm == null)
            yield break;

        agent.isStopped = false;

        // Phase 1: walk up next to the worm (it moves, so keep updating the target)
        float arriveTime = arriveTimeout;
        while (arriveTime > 0f && worm != null && (!untilInteracted || isFollowingWorm))
        {
            arriveTime -= Time.deltaTime;

            if (Vector3.Distance(transform.position, worm.transform.position) <= followDistance + 0.5f)
                break;

            SetDestinationNextToWorm();
            yield return null;
        }

        // Phase 2: stay next to the worm and follow it
        float followTime = followDuration;
        while ((untilInteracted ? isFollowingWorm : followTime > 0f) && worm != null)
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

    // ------------------------------------------------------------
    // Interaction
    // ------------------------------------------------------------
    public string GetPrompt()
    {
        return isStalking ? "Confront freak" : "Talk to freak";
    }

    public void Interact(GameObject interactor)
    {
        // Player talked to him while he was following the worm: stop following
        isFollowingWorm = false;

        // Stalking: no dialogue, interacting makes him move on to the next point
        if (isStalking)
        {
            if (isWalkingToPoint) return; // ignore interaction until he has arrived
            GoToNextStalkPoint();
            return;
        }

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
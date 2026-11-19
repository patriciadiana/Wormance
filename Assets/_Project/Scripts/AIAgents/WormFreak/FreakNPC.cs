
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

    [Header("Stalking")]
    [SerializeField] private float pointArriveDistance = 0.6f;
    [SerializeField] private float pointTimeout = 20f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Door Movement Detection")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private float stoppedDuration = 0.5f;
    [SerializeField] private float doorMovementTimeout = 30f;

    private DialogueComponent dialogueComponent;
    private NavMeshAgent agent;
    private GameObject presentToGive;
    private bool isFollowingWorm;
    private bool hasTalked;
    private Transform goHomePoint;
    private WormAI worm;

    // Stalking state
    private List<Transform> unvisitedStalkPoints;
    private bool isStalking;
    private bool isWalkingToPoint;
    private Vector3 homePosition;
    private List<Transform> stalkPoints;
    private float stalkThreshold = 50f;

    // Door waiting state
    private bool isWaitingAtDoor;
    private bool walkingWarningLogged;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Monitor animation while waiting at the door.
        if (!isWaitingAtDoor || animator == null || agent == null)
            return;

        bool isWalkingParameter = animator.GetBool("IsWalking");
        bool isWalkingState = animator.GetCurrentAnimatorStateInfo(0).IsName("Walking");

        if (isWalkingParameter || isWalkingState)
        {
            if (!walkingWarningLogged)
            {
                Debug.LogWarning(
                    "[Freak DEBUG] WALKING DETECTED WHILE WAITING AT DOOR! " +
                    "IsWalking: " + isWalkingParameter +
                    " | Walking State: " + isWalkingState +
                    " | Agent stopped: " + agent.isStopped
                );

                walkingWarningLogged = true;
            }
        }
        else
        {
            walkingWarningLogged = false;
        }
    }

    // ------------------------------------------------------------
    // Normal visit setup
    // ------------------------------------------------------------

    public void Setup(DialogueData data, GameObject present, Transform doorDestination)
    {
        dialogueComponent.dialogueData = data;
        presentToGive = present;

        dialogueComponent.onDialogueEnded = HandleDialogueEnded;

        if (doorDestination != null)
        {
            StartCoroutine(WalkToDoor(doorDestination));
        }
        else
        {
            Debug.LogError("[Freak DEBUG] Door destination is NULL!");
        }
    }

    // ------------------------------------------------------------
    // Walk to door using actual position detection
    // ------------------------------------------------------------

    private IEnumerator WalkToDoor(Transform doorDestination)
    {
        isWaitingAtDoor = false;
        walkingWarningLogged = false;

        agent.isStopped = false;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            animator.Play("Walking");
        }

        Debug.Log("[Freak DEBUG] Starting walk to door.");

        // Set destination.
        if (NavMesh.SamplePosition(
            doorDestination.position,
            out NavMeshHit hit,
            5f,
            NavMesh.AllAreas))
        {
            bool destinationSet = agent.SetDestination(hit.position);

            Debug.Log("[Freak DEBUG] Destination set: " + destinationSet);
        }
        else
        {
            Debug.LogWarning(
                "[Freak DEBUG] Could not sample door position. Trying original position."
            );

            agent.SetDestination(doorDestination.position);
        }

        // Position-based movement detection.
        Vector3 previousPosition = transform.position;

        bool hasStartedMoving = false;

        float stoppedTimer = 0f;
        float totalTimeout = doorMovementTimeout;

        while (totalTimeout > 0f)
        {
            totalTimeout -= Time.deltaTime;

            float distanceMoved = Vector3.Distance(
                transform.position,
                previousPosition
            );

            // Confirm that the NPC has actually started moving.
            if (distanceMoved >= movementThreshold)
            {
                hasStartedMoving = true;
                stoppedTimer = 0f;
            }
            else if (hasStartedMoving)
            {
                stoppedTimer += Time.deltaTime;
            }

            previousPosition = transform.position;

            Debug.DrawLine(
                transform.position,
                doorDestination.position,
                Color.yellow
            );

            // Only count arrival after movement has started
            // and the NPC has physically stopped.
            if (hasStartedMoving && stoppedTimer >= stoppedDuration)
            {
                Debug.Log(
                    "[Freak DEBUG] NPC stopped moving! " +
                    "Position: " + transform.position
                );

                break;
            }

            yield return null;
        }

        if (totalTimeout <= 0f)
        {
            Debug.LogWarning(
                "[Freak DEBUG] Door movement detection timed out!"
            );
        }

        // Completely stop the agent.
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Debug.Log("[Freak DEBUG] Agent stopped: " + agent.isStopped);
        Debug.Log("[Freak DEBUG] Final position: " + transform.position);

        // Force Idle animation.
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);

            Debug.Log(
                "[Freak DEBUG] IsWalking parameter: " +
                animator.GetBool("IsWalking")
            );

            Debug.Log(
                "[Freak DEBUG] Idle playing: " +
                animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")
            );

            Debug.Log(
                "[Freak DEBUG] Walking playing: " +
                animator.GetCurrentAnimatorStateInfo(0).IsName("Walking")
            );
        }

        isWaitingAtDoor = true;

        Debug.Log("[Freak DEBUG] NPC waiting at door in IDLE.");
    }

    // ------------------------------------------------------------
    // Dialogue ended
    // ------------------------------------------------------------

    void HandleDialogueEnded()
    {
        if (presentToGive != null)
        {
            Debug.Log($"[Freak] Gave present: {presentToGive.name}");
        }

        FreakSpawner spawner = FindAnyObjectByType<FreakSpawner>();
        spawner?.MarkAsMet();
        spawner?.OnDialogueFinished(GetChosenOption());

        float suspicion = SuspicionManager.Instance.GetSuspicionLevel("Jeremy");
        bool hasStalkPoints = stalkPoints != null && stalkPoints.Count > 0;

        if (suspicion >= stalkThreshold &&
            suspicion < SuspicionManager.Max &&
            hasStalkPoints)
        {
            BeginStalking();
            return;
        }

        StartCoroutine(WalkAwayAndLeave());
    }

    // ------------------------------------------------------------
    // Dialogue choice
    // ------------------------------------------------------------

    int GetChosenOption()
    {
        DialogueData data = dialogueComponent.dialogueData;

        if (data == null || data.choices == null)
            return -1;

        int endedOnLine = dialogueComponent.dialogueIndex;
        int chosen = -1;

        foreach (DialogueChoice choice in data.choices)
        {
            if (choice.nextDialogueIndex == null)
                continue;

            for (int i = 0; i < choice.nextDialogueIndex.Length; i++)
            {
                if (choice.nextDialogueIndex[i] == endedOnLine)
                    chosen = i;
            }
        }

        Debug.Log(
            $"[Freak] Dialogue ended on line {endedOnLine}, chosen option: {chosen}"
        );

        return chosen;
    }

    // ------------------------------------------------------------
    // Second visit setup: follows worm
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

        // No worm: fall back to normal door visit.
        if (worm == null)
        {
            isFollowingWorm = false;

            if (doorFallback != null)
                yield return WalkToDoor(doorFallback);

            yield break;
        }

        yield return FollowWorm(true);
    }

    // ------------------------------------------------------------
    // Stalking setup
    // ------------------------------------------------------------

    public void ConfigureStalking(List<Transform> points, float threshold)
    {
        stalkPoints = points;
        stalkThreshold = threshold;
    }

    void BeginStalking()
    {
        isWaitingAtDoor = false;

        isStalking = true;
        unvisitedStalkPoints = new List<Transform>(stalkPoints);

        GoToNextStalkPoint();
    }

    void GoToNextStalkPoint()
    {
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

        if (animator != null)
            animator.SetBool("IsWalking", true);

        if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        float timeout = pointTimeout;

        while (timeout > 0f &&
               (agent.pathPending || agent.remainingDistance > pointArriveDistance))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
            animator.SetBool("IsWalking", false);

        transform.rotation = point.rotation;

        isWalkingToPoint = false;
    }

    // ------------------------------------------------------------
    // Leaving
    // ------------------------------------------------------------

    IEnumerator WalkAwayAndLeave(bool fromStalking = false)
    {
        isWaitingAtDoor = false;

        GetComponent<Collider>().enabled = false;

        Vector3 homeDirection = -transform.forward;

        Vector3 goAwayPoint = fromStalking
            ? homePosition
            : transform.position + (homeDirection * 20f);

        agent.isStopped = false;

        if (animator != null)
            animator.SetBool("IsWalking", true);

        if (NavMesh.SamplePosition(goAwayPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float timeout = fromStalking ? 20f : 5f;

        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timeout -= Time.deltaTime;

            if (timeout <= 0)
                break;

            yield return null;
        }

        Destroy(gameObject);
    }

    // ------------------------------------------------------------
    // Worm behaviours
    // ------------------------------------------------------------

    IEnumerator FollowWorm(bool untilInteracted = false)
    {
        if (worm == null)
            worm = FindAnyObjectByType<WormAI>();

        if (worm == null)
            yield break;

        agent.isStopped = false;

        if (animator != null)
            animator.SetBool("IsWalking", true);

        // Phase 1: walk next to the worm.
        float arriveTime = arriveTimeout;

        while (arriveTime > 0f &&
               worm != null &&
               (!untilInteracted || isFollowingWorm))
        {
            arriveTime -= Time.deltaTime;

            if (Vector3.Distance(transform.position, worm.transform.position)
                <= followDistance + 0.5f)
                break;

            SetDestinationNextToWorm();

            yield return null;
        }

        // Phase 2: follow the worm.
        float followTime = followDuration;

        while ((untilInteracted ? isFollowingWorm : followTime > 0f) &&
               worm != null)
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
                    5f * Time.deltaTime
                );
            }

            yield return null;
        }
    }

    void SetDestinationNextToWorm()
    {
        Vector3 dirFromWorm = transform.position - worm.transform.position;
        dirFromWorm.y = 0f;

        dirFromWorm = dirFromWorm.sqrMagnitude > 0.001f
            ? dirFromWorm.normalized
            : Vector3.forward;

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
        // Player talked while NPC was following the worm.
        isFollowingWorm = false;

        // Stalking interaction.
        if (isStalking)
        {
            if (isWalkingToPoint)
                return;

            GoToNextStalkPoint();
            return;
        }

        // Already talked today.
        if (hasTalked)
            return;

        if (dialogueComponent != null &&
            dialogueComponent.dialogueData != null)
        {
            hasTalked = true;
            isWaitingAtDoor = false;

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            if (animator != null)
                animator.SetBool("IsWalking", false);

            DialogueSystem.Instance.HandleInteraction(dialogueComponent);
        }
    }
}

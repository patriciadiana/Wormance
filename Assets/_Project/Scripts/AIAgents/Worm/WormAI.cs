using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class WormAI : MonoBehaviour, IVisitable, IInteractable
{
    private NavMeshAgent _agent;
    private DoorInteraction _doorBeingOpened;
    private DoorInteraction _doorToReach;
    private Transform _roamTarget;
    private float _roamTargetExpiresAt;
    private Collider[] _indoorAreaColliders;
    private Coroutine _sleepCoroutine;
    private DayCycleSystem _dayCycle;

    public GameObject boyfriend;

    // Kept for scene compatibility. The value is refreshed while the AI is running.
    public bool isBoyfriendInside;
    public MeshRenderer floorMeshRenderer;

    public List<GameObject> getModifierPickups = new List<GameObject>();

    [SerializeField] private BaseStats baseStats;
    [Header("Needs destinations")]
    [SerializeField] private Transform bed;
    [SerializeField] private Transform kitchenPoint;
    [SerializeField] private DoorInteraction frontDoor;
    [SerializeField] private Transform[] indoorRoamPoints;
    [SerializeField] private Transform favouriteSpot;
    [SerializeField] private Transform[] outdoorRoamPoints;

    [Header("Decision thresholds")]
    [SerializeField] private int sleepThreshold = 30;
    [SerializeField] private int hungerThreshold = 70;
    [SerializeField] private int veryLowSocialThreshold = 30;
    [SerializeField] private int upsetSocialThreshold = 60;
    [SerializeField] private int approachSocialThreshold = 80;
    [SerializeField] private float roamTargetDuration = 8f;
    [SerializeField] private int sleepRecoveryStepsPerDay = 10;
    [SerializeField] private int sleepRecoveryAmount = 10;

    public float distancerun = 4.0f;
    [SerializeField] private string interactPrompt = "Give item";

    // Set by other scripts (e.g. WormSad) to send the worm somewhere. Null = normal behaviour.
    public Transform OverrideDestination { get; set; }

    public Stats Stats { get; private set; }

    protected virtual void Awake()
    {
        if (baseStats == null)
        {
            Debug.LogError($"{nameof(baseStats)} is not assigned on {name}.", this);
            return;
        }
        ResolveSceneReferences();
        _dayCycle = FindAnyObjectByType<DayCycleSystem>();
        if (_dayCycle == null)
        {
            Debug.LogError($"{nameof(WormAI)} requires an active {nameof(DayCycleSystem)} for sleep timing.", this);
        }
        RefreshPlayerHouseState();
        Stats = new Stats(new StatsMediator(), baseStats);
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);

    public string GetPrompt()
    {
        return interactPrompt;
    }

    public void Interact(GameObject interactor)
    {
        // E interaction is intentionally separate from giving held item.
    }

    public void TryReceiveHeldItem(GameObject interactor)
    {
        Debug.Log("Trying to receive");
        if (interactor == null)
        {
            return;
        }

        PlayerHandInventory handInventory = interactor.GetComponent<PlayerHandInventory>();
        if (handInventory == null)
        {
            handInventory = interactor.GetComponentInParent<PlayerHandInventory>();
        }

        if (handInventory == null)
        {
            Debug.LogWarning($"[{nameof(WormAI)}] No {nameof(PlayerHandInventory)} found on interactor {interactor.name}.", interactor);
            return;
        }

        GameObject objReceived = handInventory.HeldPickup;
        StatModifierPickup pickup = objReceived.GetComponent<StatModifierPickup>();
        Debug.Log(pickup);
        if (pickup)
            pickup.TryCollect(this);



        //return handInventory.TryGiveHeldPickupTo(this);
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError($"{nameof(WormAI)} requires a {nameof(NavMeshAgent)} on {name}.", this);
            return;
        }

        // Door links in the house use dedicated NavMesh areas.
        _agent.areaMask = NavMesh.AllAreas;
    }

    private static void AddIndoorCollider(List<Collider> colliders, string objectName)
    {
        GameObject area = GameObject.Find(objectName);
        Collider collider = area != null ? area.GetComponent<Collider>() : null;
        if (collider != null)
        {
            colliders.Add(collider);
        }
    }

    void Update()
    {

        Stats?.mediator.Update(Time.deltaTime);

        if (_agent == null || Stats == null)
        {
            return;
        }

        CollectNearbyModifierPickup();
        if (TryOpenNearbyDoor())
        {
            return;
        }

        if (boyfriend == null)
        {
            MoveToClosestModifierPickup();
            return;
        }

        RefreshPlayerHouseState();
        UpdateSleepingState();

        // Needs have priority over social behaviour.
        if (Stats.Sleep < sleepThreshold && bed != null)
        {
            MoveToIndoorDestination(bed.position);
            return;
        }

        if (Stats.Hunger >= hungerThreshold && kitchenPoint != null)
        {
            MoveToIndoorDestination(kitchenPoint.position);
            return;
        }

        // Sent somewhere by another script (e.g. WormSad)
        if (OverrideDestination != null)
        {
            TrySetDestination(OverrideDestination.position);
            return;
        }

        int lowestSocialStat = Mathf.Min(Stats.Love, Stats.Happiness);
        if (lowestSocialStat < veryLowSocialThreshold)
        {
            MoveAwayFrom(boyfriend.transform.position);
            return;
        }

        int highestSocialStat = Mathf.Max(Stats.Love, Stats.Happiness);
        if (Stats.Love >= approachSocialThreshold &&
            Stats.Happiness >= approachSocialThreshold)
        {
            float distance = Vector3.Distance(transform.position, boyfriend.transform.position);
            if (distance > distancerun)
            {
                MoveTowards(boyfriend.transform.position);
            }

            return;
        }

        if (Stats.Love >= upsetSocialThreshold &&
            Stats.Happiness >= upsetSocialThreshold &&
            highestSocialStat < approachSocialThreshold)
        {
            MoveToRoamPoint();
            return;
        }

        MoveToRoamPoint();
    }

    private void RefreshPlayerHouseState()
    {
        if (boyfriend != null && floorMeshRenderer != null)
        {
            isBoyfriendInside = floorMeshRenderer.bounds.Contains(boyfriend.transform.position);
        }
    }

    private void ResolveSceneReferences()
    {
        List<Collider> indoorColliders = new List<Collider>();
        AddIndoorCollider(indoorColliders, "LivingRoomEnviromentColliders");
        AddIndoorCollider(indoorColliders, "KitchenEnviromentColliders");
        AddIndoorCollider(indoorColliders, "BathroomEnviromentColliders");
        AddIndoorCollider(indoorColliders, "DormEnviromentColliders");
        _indoorAreaColliders = indoorColliders.ToArray();

        if (kitchenPoint == null)
        {
            GameObject kitchen = GameObject.Find("KitchenPoint");
            if (kitchen == null)
            {
                kitchen = GameObject.Find("KitchenEnviromentColliders");
            }

            kitchenPoint = kitchen != null ? kitchen.transform : null;
        }

        if (frontDoor == null)
        {
            foreach (DoorInteraction door in FindObjectsByType<DoorInteraction>())
            {
                string doorName = door.name.ToLowerInvariant();
                if (doorName.Contains("front") || doorName.Contains("housedoor"))
                {
                    frontDoor = door;
                    break;
                }
            }
        }
    }

    private void MoveToRoamPoint()
    {
        if (_roamTarget == null ||
            Time.time >= _roamTargetExpiresAt ||
            Vector3.Distance(transform.position, _roamTarget.position) <= 1f)
        {
            Transform[] candidates = GetRoamPoints();
            if (candidates.Length == 0)
            {
                MoveToClosestModifierPickup();
                return;
            }

            _roamTarget = candidates[Random.Range(0, candidates.Length)];
            _roamTargetExpiresAt = Time.time + Mathf.Max(1f, roamTargetDuration);
        }

        TrySetDestination(_roamTarget.position);
    }

    private Transform[] GetRoamPoints()
    {
        List<Transform> points = new List<Transform>();
        AddValidPoints(points, indoorRoamPoints);
        AddValidPoints(points, outdoorRoamPoints);
        if (favouriteSpot != null && !points.Contains(favouriteSpot))
        {
            points.Add(favouriteSpot);
        }

        return points.ToArray();
    }

    private static void AddValidPoints(List<Transform> points, Transform[] candidates)
    {
        if (candidates == null)
        {
            return;
        }

        foreach (Transform candidate in candidates)
        {
            if (candidate != null && !points.Contains(candidate))
            {
                points.Add(candidate);
            }
        }
    }

    private void MoveTowards(Vector3 target)
    {
        TrySetDestination(target);
    }

    private void MoveToIndoorDestination(Vector3 target)
    {
        TrySetDestination(target, true);
    }

    private void UpdateSleepingState()
    {
        if (Stats.Sleep < sleepThreshold &&
            bed != null &&
            Vector3.Distance(transform.position, bed.position) <= GetBedArrivalDistance())
        {
            if (_sleepCoroutine == null)
            {
                _sleepCoroutine = StartCoroutine(RecoverSleep());
            }

            return;
        }

        StopSleeping();
    }

    private IEnumerator RecoverSleep()
    {
        while (Stats.Sleep < sleepThreshold && IsAtBed())
        {
            yield return new WaitForSeconds(GetSleepRecoveryInterval());

            if (Stats.Sleep < sleepThreshold && IsAtBed())
            {
                Stats.Modify(StatsType.Sleep, sleepRecoveryAmount);
            }
        }

        _sleepCoroutine = null;
    }

    private float GetSleepRecoveryInterval()
    {
        if (_dayCycle == null || _dayCycle.dayDuration <= 0f)
        {
            return 1f;
        }

        return _dayCycle.dayDuration / Mathf.Max(1, sleepRecoveryStepsPerDay);
    }

    private bool IsAtBed()
    {
        return bed != null &&
            Vector3.Distance(transform.position, bed.position) <= GetBedArrivalDistance();
    }

    private float GetBedArrivalDistance()
    {
        return Mathf.Max(0.5f, _agent != null ? _agent.stoppingDistance + 0.35f : 0.5f);
    }

    private void StopSleeping()
    {
        if (_sleepCoroutine != null)
        {
            StopCoroutine(_sleepCoroutine);
            _sleepCoroutine = null;
        }
    }

    private void MoveAwayFrom(Vector3 target)
    {
        Vector3 direction = transform.position - target;
        if (direction.sqrMagnitude > 0.001f)
        {
            TrySetDestination(transform.position + direction.normalized * distancerun * 2f);
        }
    }

    private void MoveToClosestModifierPickup()
    {
        GameObject closestPickup = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject pickup in getModifierPickups)
        {
            if (pickup == null)
            {
                continue;
            }

            Pickup pickupComponent = pickup.GetComponent<Pickup>();
            if (pickupComponent == null || !pickupComponent.AllowAIAutoCollect)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, pickup.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPickup = pickup;
            }
        }

        if (closestPickup != null)
        {
            TrySetDestination(closestPickup.transform.position);
        }
    }

    private void CollectNearbyModifierPickup()
    {
        foreach (GameObject pickupObject in getModifierPickups)
        {
            if (pickupObject == null)
            {
                continue;
            }

            Pickup pickup = pickupObject.GetComponent<Pickup>();
            if (pickup != null &&
                pickup.AllowAIAutoCollect &&
                Vector3.Distance(transform.position, pickupObject.transform.position) <= 1f)
            {
                pickup.TryCollect(this);
            }
        }
    }

    private bool TrySetDestination(Vector3 destination, bool mustStayIndoors = false)
    {
        if (_agent == null || !_agent.isActiveAndEnabled)
        {
            return false;
        }

        if (!_agent.isOnNavMesh)
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit agentPosition, 5f, NavMesh.AllAreas) ||
                !_agent.Warp(agentPosition.position))
            {
                MoveDirectly(destination);
                return true;
            }
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit destinationPosition, 5f, NavMesh.AllAreas))
        {
            MoveDirectly(destination);
            return true;
        }

        NavMeshPath targetPath = new NavMeshPath();
        if (NavMesh.CalculatePath(
                transform.position,
                destinationPosition.position,
                NavMesh.AllAreas,
                targetPath) &&
            targetPath.status == NavMeshPathStatus.PathComplete &&
                    (!mustStayIndoors ||
                     IsPathInsideHouse(targetPath) ||
                     (!IsInsideHouse(transform.position) && IsInsideHouse(destinationPosition.position))))
        {
            _doorToReach = null;
            _agent.isStopped = false;

            if (_agent.hasPath &&
                Vector3.Distance(_agent.destination, destinationPosition.position) < 0.1f)
            {
                return true;
            }

            if (_agent.SetPath(targetPath))
            {
                return true;
            }

            MoveDirectly(destinationPosition.position);
            return true;
        }

        if (TryRouteToDoor(destinationPosition.position, mustStayIndoors))
        {
            return true;
        }

        _agent.isStopped = false;
        if (_agent.SetDestination(destinationPosition.position))
        {
            return true;
        }

        MoveDirectly(destinationPosition.position);
        return true;
    }

    private bool TryRouteToDoor(Vector3 destination, bool mustStayIndoors)
    {
        DoorInteraction bestDoor = null;
        Vector3 bestApproachPosition = default;
        float bestPathLength = float.PositiveInfinity;
        bool crossingHouseBoundary = mustStayIndoors
            ? !IsInsideHouse(transform.position)
            : floorMeshRenderer != null && IsInsideHouse(transform.position) != IsInsideHouse(destination);
        DoorInteraction[] doors = crossingHouseBoundary && frontDoor != null
            ? new[] { frontDoor }
            : FindObjectsByType<DoorInteraction>();

        foreach (DoorInteraction door in doors)
        {
            if (door == null || door.IsOpen || door.IsMoving ||
                !door.TryGetApproachPosition(transform.position, 0.8f, out Vector3 approachPosition))
            {
                continue;
            }

            if (!NavMesh.SamplePosition(approachPosition, out NavMeshHit approachHit, 1.5f, NavMesh.AllAreas))
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(
                    _agent.nextPosition,
                    approachHit.position,
                    NavMesh.AllAreas,
                    path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            if (mustStayIndoors &&
                !crossingHouseBoundary &&
                !IsInsideHouse(approachHit.position))
            {
                continue;
            }

            float pathLength = GetPathLength(path);
            float estimatedTotalLength = pathLength +
                Vector3.Distance(approachHit.position, destination);
            if (estimatedTotalLength < bestPathLength)
            {
                bestDoor = door;
                bestApproachPosition = approachHit.position;
                bestPathLength = estimatedTotalLength;
            }
        }

        if (bestDoor == null)
        {
            return false;
        }

        _doorToReach = bestDoor;
        _agent.isStopped = false;

        if (_agent.hasPath &&
            Vector3.Distance(_agent.destination, bestApproachPosition) < 0.1f)
        {
            return true;
        }

        return _agent.SetDestination(bestApproachPosition);
    }

    private bool IsPathInsideHouse(NavMeshPath path)
    {
        if (_indoorAreaColliders == null || _indoorAreaColliders.Length == 0)
        {
            return true;
        }

        foreach (Vector3 corner in path.corners)
        {
            if (!IsInsideHouse(corner))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInsideHouse(Vector3 position)
    {
        if (_indoorAreaColliders != null && _indoorAreaColliders.Length > 0)
        {
            foreach (Collider collider in _indoorAreaColliders)
            {
                if (collider.bounds.Contains(position))
                {
                    return true;
                }
            }

            return false;
        }

        return floorMeshRenderer != null && floorMeshRenderer.bounds.Contains(position);
    }

    private static float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }

    private bool TryOpenNearbyDoor()
    {
        if (_doorBeingOpened != null)
        {
            if (_doorBeingOpened.IsMoving)
            {
                _agent.isStopped = true;
                return true;
            }

            _doorBeingOpened = null;
            _doorToReach = null;
            _agent.isStopped = false;
            _agent.ResetPath();
            return false;
        }

        const float doorDetectionRadius = 1.5f;
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            doorDetectionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        DoorInteraction nearestDoor = null;
        float nearestDistance = float.PositiveInfinity;

        if (_doorToReach != null &&
            !_doorToReach.IsOpen &&
            !_doorToReach.IsMoving &&
            IsDoorNearby(_doorToReach, doorDetectionRadius))
        {
            nearestDoor = _doorToReach;
        }

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            DoorInteraction door = nearbyCollider.GetComponentInParent<DoorInteraction>();
            if (door == null || (door.IsOpen && !door.IsMoving))
            {
                continue;
            }

            if (nearestDoor != null && door != nearestDoor)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, nearbyCollider.ClosestPoint(transform.position));
            if (distance < nearestDistance)
            {
                nearestDoor = door;
                nearestDistance = distance;
            }
        }

        if (nearestDoor != null)
        {
            _doorBeingOpened = nearestDoor;
            nearestDoor.Interact(gameObject);
            _agent.isStopped = true;
            _agent.ResetPath();
            return true;
        }

        return false;
    }

    private bool IsDoorNearby(DoorInteraction door, float radius)
    {
        Collider doorCollider = door.GetComponentInChildren<Collider>();
        return doorCollider != null &&
            Vector3.Distance(transform.position, doorCollider.ClosestPoint(transform.position)) <= radius;
    }

    private void MoveDirectly(Vector3 destination)
    {
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            destination,
            _agent.speed * Time.deltaTime);

        transform.position = nextPosition;
    }

    private void OnDisable()
    {
        StopSleeping();
    }

}
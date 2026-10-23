using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class WormAI : MonoBehaviour, IVisitable
{
    private NavMeshAgent _agent;
    private DoorInteraction _doorBeingOpened;

    public GameObject boyfriend;

    //is inside the house
    public bool isBoyfriendInside;
    public MeshRenderer floorMeshRenderer;

    public List<GameObject> getModifierPickups= new List<GameObject>();

    [SerializeField] private BaseStats baseStats;

    public float distancerun=4.0f;

    public Stats Stats { get; private set; }

    protected virtual void Awake()
    {
        if (baseStats == null)
        {
            Debug.LogError($"{nameof(baseStats)} is not assigned on {name}.", this);
            return;
        }
        if(floorMeshRenderer == null)
        {
            Debug.LogError($"{nameof(floorMeshRenderer)} is not assigned on {name}.", this);
            return;
        }
        else
        {
            isBoyfriendInside= floorMeshRenderer.bounds.Contains(boyfriend.transform.position);
        }
       
        Stats = new Stats(new StatsMediator(), baseStats);
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
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
            isBoyfriendInside= floorMeshRenderer.bounds.Contains(boyfriend.transform.position);
            MoveToClosestModifierPickup();
            return;
        }

        float distance = Vector3.Distance(transform.position, boyfriend.transform.position);
        bool hasGoodStats = Stats.Happiness > 20 && Stats.Love > 40;

        if (hasGoodStats || isBoyfriendInside)
        {
            if (distance > distancerun)
            {
                MoveTowards(boyfriend.transform.position);
            }

            return;
        }

        if (distance <= distancerun)
        {
            MoveAwayFrom(boyfriend.transform.position);
            return;
        }

        MoveToClosestModifierPickup();
    }

    private void MoveTowards(Vector3 target)
    {
        TrySetDestination(target);
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
                Vector3.Distance(transform.position, pickupObject.transform.position) <= 1f)
            {
                pickup.TryCollect(this);
            }
        }
    }

    private bool TrySetDestination(Vector3 destination)
    {
        if (!_agent.isActiveAndEnabled)
        {
            return false;
        }

        if (!_agent.isOnNavMesh)
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit agentPosition, 2f, NavMesh.AllAreas) ||
                !_agent.Warp(agentPosition.position))
            {
                MoveDirectly(destination);
                return true;
            }
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit destinationPosition, 2f, NavMesh.AllAreas))
        {
            MoveDirectly(destination);
            return true;
        }

        _agent.isStopped = false;
        return _agent.SetDestination(destinationPosition.position);
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

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            DoorInteraction door = nearbyCollider.GetComponentInParent<DoorInteraction>();
            if (door == null || (door.IsOpen && !door.IsMoving))
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

    private void MoveDirectly(Vector3 destination)
    {
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            destination,
            _agent.speed * Time.deltaTime);

        transform.position = nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_agent == null)
        {
            return;
        }

        DoorInteraction door = other.GetComponentInParent<DoorInteraction>();
        if (door != null)
        {
            door.Interact(gameObject);
        }
    }
}
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class WormAI : MonoBehaviour, IVisitable
{
    private NavMeshAgent _agent;

    public GameObject boyfriend;

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

        if (boyfriend == null)
        {
            MoveToClosestModifierPickup();
            return;
        }

        float distance = Vector3.Distance(transform.position, boyfriend.transform.position);
        bool hasGoodStats = Stats.Happiness > 20 && Stats.Love > 40;

        if (hasGoodStats)
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

    private void MoveDirectly(Vector3 destination)
    {
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            destination,
            _agent.speed * Time.deltaTime);

        transform.position = nextPosition;
    }
}
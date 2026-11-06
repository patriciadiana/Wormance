using UnityEngine;
using UnityEngine.AI;

public class PlayerHandInventory : MonoBehaviour
{
    [SerializeField] private GameObject heldPickup;
    [SerializeField] private float throwForce = 5f;

    public GameObject HeldPickup => heldPickup;
    public bool HasHeldPickup => heldPickup != null;

    public void SetHeldPickup(GameObject pickup)
    {
        heldPickup = pickup;
    }

    public bool TryHoldPickup(GameObject pickup)
    {
        if (pickup == null || heldPickup != null)
        {
            return false;
        }

        heldPickup = pickup;
        return true;
    }

    public bool TryGiveHeldPickupTo(IVisitable target)
    {
        if (heldPickup == null || target == null)
        {
            return false;
        }

        Pickup pickupToGive = heldPickup.GetComponent<StatModifierPickup>();
        bool collected = pickupToGive.TryCollect(target);
        if (collected)
        {
            heldPickup = null;

            // Extra safety: make sure the consumed pickup object is removed.
            if (pickupToGive != null)
            {
                Destroy(pickupToGive.gameObject);
            }
        }

        return collected;
    }

    public bool TryDropHeldPickup(Vector3 worldPosition, Vector3 throwDirection)
    {
        if (heldPickup == null)
        {
            return false;
        }

        Pickup pickupToDrop = heldPickup.GetComponent<StatModifierPickup>();
        Vector3 safeDropPosition = GetSafeDropPosition(worldPosition);
        if (!pickupToDrop.ReleaseFromPlayerHand(safeDropPosition))
        {
            return false;
        }

        heldPickup = null;
        Rigidbody body = pickupToDrop.GetComponent<Rigidbody>();
        if (body != null)
        {
            Vector3 flatThrowDirection = Vector3.ProjectOnPlane(throwDirection, Vector3.up).normalized;
            body.AddForce(flatThrowDirection * throwForce, ForceMode.VelocityChange);
        }

        return true;
    }

    private Vector3 GetSafeDropPosition(Vector3 requestedPosition)
    {
        if (NavMesh.SamplePosition(requestedPosition, out NavMeshHit navMeshHit, 2f, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        }

        if (Physics.Raycast(
                requestedPosition + Vector3.up * 2f,
                Vector3.down,
                out RaycastHit groundHit,
                5f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return groundHit.point;
        }

        return transform.position;
    }

    public bool TryReleaseHeldPickup(Vector3 worldPosition)
    {
        if (heldPickup == null)
        {
            return false;
        }

        Pickup pickupToRelease = heldPickup.GetComponent<StatModifierPickup>();
        if (!pickupToRelease.ReleaseFromPlayerHand(worldPosition))
        {
            return false;
        }

        heldPickup = null;
        return true;
    }
}

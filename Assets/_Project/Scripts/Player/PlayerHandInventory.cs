using UnityEngine;

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
        if (!pickupToDrop.ReleaseFromPlayerHand(worldPosition))
        {
            return false;
        }

        heldPickup = null;
        Rigidbody body = pickupToDrop.GetComponent<Rigidbody>();
        if (body != null)
        {
            
            body.AddForce(throwDirection.normalized * throwForce, ForceMode.VelocityChange);
        }

        return true;
    }
}

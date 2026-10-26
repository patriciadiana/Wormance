using UnityEngine;

public abstract class Pickup : MonoBehaviour, IVisitor
{
    [SerializeField] private bool allowAIAutoCollect = true;
    [SerializeField] private Transform handTransform;

    private bool _isConsumed;
    private bool _isHeldByPlayer;

    public bool AllowAIAutoCollect => allowAIAutoCollect;

    private void Awake()
    {
        // Ensure trigger callbacks are reliable even when the player uses CharacterController.
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }
    }

    protected abstract void ApplyPickupEffect(Stats stats);

    public void Visit(IVisitable visitable)
    {
        if (visitable is WormAI worm && worm.Stats != null)
        {
            ApplyPickupEffect(worm.Stats);
        }
    }

    public bool TryCollect(IVisitable visitable)
    {
        if (_isConsumed || visitable == null)
        {
            return false;
        }

        if (visitable is PlayerEntity)
        {
            return false;
        }

        if (visitable is not WormAI worm || worm.Stats == null)
        {
            return false;
        }

        _isHeldByPlayer = false;
        _isConsumed = true;
        visitable.Accept(this);
        Destroy(gameObject);
        return true;
    }

    public bool TryHoldInPlayerHand(PlayerHandInventory handInventory)
    {
        if (_isConsumed || _isHeldByPlayer || handInventory == null || handInventory.HasHeldPickup)
        {
            return false;
        }

        if (!handInventory.TryHoldPickup(this.gameObject))
        {
            return false;
        }

        _isHeldByPlayer = true;

        if (handTransform != null)
        {
            transform.SetParent(handTransform);
            transform.localPosition = Vector3.zero;
           
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            Debug.LogWarning("Assign hand in inspector");
        }

        SetWorldPhysicsEnabled(false);

        return true;
    }

    private void SetVisualEnabled(bool enabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer currentRenderer in renderers)
        {
            currentRenderer.enabled = enabled;
        }
    }

    private void SetWorldPhysicsEnabled(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody body in rigidbodies)
        {
            if (!enabled)
            {
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }

            body.detectCollisions = enabled;
            body.isKinematic = !enabled;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        HandleTriggerContact(other);
    }

    public void OnTriggerStay(Collider other)
    {
        // Fallback when enter is skipped due to timing/physics order.
        HandleTriggerContact(other);
    }

    private void HandleTriggerContact(Collider other)
    {
        if (_isConsumed || _isHeldByPlayer)
        {
            return;
        }

        PlayerHandInventory handInventory = ResolveHandInventory(other);
        if (handInventory != null)
        {
            // Just call it normally now, no extra parameters needed
            TryHoldInPlayerHand(handInventory);
            return;
        }

        WormAI worm = other.GetComponentInParent<WormAI>();
        if (worm != null && allowAIAutoCollect)
        {
            TryCollect(worm);
        }
    }

    private PlayerHandInventory ResolveHandInventory(Collider other)
    {
        PlayerHandInventory handInventory = other.GetComponentInParent<PlayerHandInventory>();
        if (handInventory != null)
        {
            return handInventory;
        }

        Transform root = other.transform.root;
        if (root != null)
        {
            handInventory = root.GetComponentInChildren<PlayerHandInventory>(true);
        }

        return handInventory;
    }

    public bool ReleaseFromPlayerHand(Vector3 worldPosition)
    {
        if (_isConsumed || !_isHeldByPlayer)
        {
            return false;
        }

        _isHeldByPlayer = false;
        transform.position = worldPosition;
        SetVisualEnabled(true);
        SetWorldPhysicsEnabled(true);
        return true;
    }
}
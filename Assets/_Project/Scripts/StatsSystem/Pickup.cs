using UnityEngine;

public abstract class Pickup : MonoBehaviour, IVisitor
{
    protected abstract void ApplyPickupEffect(PlayerEntity player);

    public void Visit(IVisitable visitable)
    {
        if (visitable is PlayerEntity player)
        {
            ApplyPickupEffect(player);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        IVisitable visitable = other.GetComponentInParent<IVisitable>();
        if (visitable == null) return;

        visitable.Accept(this);
        Destroy(gameObject);
    }
}
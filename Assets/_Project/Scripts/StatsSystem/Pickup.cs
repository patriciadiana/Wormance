using UnityEngine;

public abstract class Pickup : MonoBehaviour, IVisitor
{
    private bool _collected;

    protected abstract void ApplyPickupEffect(Stats stats);

    public void Visit(IVisitable visitable)
    {
        if (visitable is PlayerEntity player && player.Stats != null)
        {
            ApplyPickupEffect(player.Stats);
        }
        else if (visitable is WormAI worm && worm.Stats != null)
        {
            ApplyPickupEffect(worm.Stats);
        }
    }

    public bool TryCollect(IVisitable visitable)
    {
        if (_collected || visitable == null)
        {
            return false;
        }

        if ((visitable is PlayerEntity player && player.Stats == null) ||
            (visitable is WormAI worm && worm.Stats == null))
        {
            return false;
        }

        _collected = true;
        visitable.Accept(this);
        Destroy(gameObject);
        return true;
    }

    public void OnTriggerEnter(Collider other)
    {
        IVisitable visitable = other.GetComponentInParent<IVisitable>();
        TryCollect(visitable);
    }
}
using UnityEngine;

public abstract class PlayerEntity : MonoBehaviour, IVisitable
{
    [SerializeField] private BaseStats baseStats;

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

  protected virtual void Update()
    {
        Stats?.mediator.Update(Time.deltaTime);
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}
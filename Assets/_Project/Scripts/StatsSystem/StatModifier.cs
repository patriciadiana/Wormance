using System;

public class BasicStatModifier : StatModifier
{
    readonly StatsType type;
    readonly Func<int, int> op;

    public BasicStatModifier(StatsType type, float duration, Func<int, int> op) : base(duration)
    {
        this.type = type;
        this.op = op;
    }

    public override void Handle(object sender, Query query)
    {
        if (query.statType == type)
        {
            query.val = op(query.val);
        }
    }
}

public abstract class StatModifier : IDisposable
{
    public bool MarkedForRemoval { get; private set; }

    public event Action<StatModifier> OnDispose = delegate { };

    readonly CountdownTimer timer;

    protected StatModifier(float duration)
    {
        if (duration <= 0) return;

        timer = new CountdownTimer(duration);
        timer.OnTimerStop += () => MarkedForRemoval = true;
        timer.Start();
    }

    public void Update(float deltaTime)
    {
        timer?.Tick(deltaTime);
    }

    public abstract void Handle(object sender, Query query);

    public void Dispose()
    {
        MarkedForRemoval = true;
        OnDispose.Invoke(this);
    }
}
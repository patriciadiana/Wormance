public enum StatsType {Sleep,Hunger,Love,Happiness}


public class Stats{
    public readonly StatsMediator mediator;
    readonly BaseStats baseStats;

    public int Sleep{
        get{
            var q= new Query(StatsType.Sleep,baseStats.sleep);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public int Hunger{
        get{
            var q= new Query(StatsType.Hunger,baseStats.hunger);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public int Love{
        get{
            var q= new Query(StatsType.Love,baseStats.love);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }
    public int Happiness{
        get{
            var q= new Query(StatsType.Happiness,baseStats.happiness );
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public Stats(StatsMediator mediator,BaseStats baseStats)
    {
        this.mediator=mediator;
        this.baseStats=baseStats;
    }

    public override string ToString() => $"Sleep: {Sleep}, Hunger: {Hunger}, Love: {Love}, Happiness: {Happiness}";
}
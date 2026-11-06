public enum StatsType {Sleep,Hunger,Love,Happiness}


public class Stats{
    public readonly StatsMediator mediator;
    readonly BaseStats baseStats;
    private int sleep;
    private int hunger;
    private int love;
    private int happiness;

    public int Sleep{
        get{
            var q= new Query(StatsType.Sleep,sleep);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public int Hunger{
        get{
            var q= new Query(StatsType.Hunger,hunger);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public int Love{
        get{
            var q= new Query(StatsType.Love,love);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }
    public int Happiness{
        get{
            var q= new Query(StatsType.Happiness,happiness);
            mediator.PerformQuery(this,q);
            return q.val;
        }
    }

    public Stats(StatsMediator mediator,BaseStats baseStats)
    {
        this.mediator=mediator;
        this.baseStats=baseStats;
        sleep = baseStats.sleep;
        hunger = baseStats.hunger;
        love = baseStats.love;
        happiness = baseStats.happiness;
    }

    public void Modify(StatsType statType, int amount)
    {
        switch (statType)
        {
            case StatsType.Sleep:
                sleep = ClampStat(sleep + amount);
                break;
            case StatsType.Hunger:
                hunger = ClampStat(hunger + amount);
                break;
            case StatsType.Love:
                love = ClampStat(love + amount);
                break;
            case StatsType.Happiness:
                happiness = ClampStat(happiness + amount);
                break;
        }
    }

    private static int ClampStat(int value) => System.Math.Clamp(value, 0, 100);

    public override string ToString() => $"Sleep: {Sleep}, Hunger: {Hunger}, Love: {Love}, Happiness: {Happiness}";
}
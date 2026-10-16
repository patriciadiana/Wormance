using System;
using System.Collections.Generic;

public class StatsMediator
{
    readonly LinkedList<StatModifier> mods = new();

    public event EventHandler<Query> queries;

    public void PerformQuery(object sender, Query query) => queries?.Invoke(sender, query);

    public void AddModifier(StatModifier modifier)
    {
        mods.AddLast(modifier);
        queries += modifier.Handle;

        modifier.OnDispose += _ =>
        {
            mods.Remove(modifier);
            queries -= modifier.Handle;
        };
    }

    public void Update(float deltaTime)
    {
        var node = mods.First;
        while (node != null)
        {
            var current = node;
            node = node.Next;
            current.Value.Update(deltaTime);
        }

        node = mods.First;
        while (node != null)
        {
            var nextNode = node.Next;
            if (node.Value.MarkedForRemoval)
            {
                node.Value.Dispose();
            }
            node = nextNode;
        }
    }
}

public class Query
{
    public readonly StatsType statType;
    public int val;

    public Query(StatsType statType, int val)
    {
        this.statType = statType;
        this.val = val;
    }
}


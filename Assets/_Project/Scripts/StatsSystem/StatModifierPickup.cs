using System;
using UnityEngine;

public class StatModifierPickup : Pickup
{
    public enum OperatorType { Add, Multiply }

    [SerializeField] StatsType type = StatsType.Sleep;
    [SerializeField] OperatorType opTYPE = OperatorType.Add;
    [SerializeField] int value = 10;
    [SerializeField] float duration = 5f;

    protected override void ApplyPickupEffect(Stats stats)
    {
        StatModifier modifier = opTYPE switch
        {
            OperatorType.Add => new BasicStatModifier(type, duration, v => v + value),
            OperatorType.Multiply => new BasicStatModifier(type, duration, v => v * value),
            _ => throw new ArgumentOutOfRangeException()
        };

        stats.mediator.AddModifier(modifier);
    }
}

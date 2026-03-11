using System;
using System.Collections.Generic;
using UnityEngine;

public interface TowerAttackEffect
{
    public void Apply(AttackContext context);
}
[Serializable]
public class AttackContext
{
    public Tower Source;
    public Enemy Target;
    public float Damage;

    public readonly List<TowerAttackEffect> Effects = new();
}


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
    public TowerType towerType;
    public EnemyTraverseType AttackableType;
    public Enemy Target;
    public float Damage;
    public AudioClip hitClip;
    public static event Action<Tower,TowerType, float> OnDamage;
    public static event Action<Tower, TowerType> OnKill;
    public void InvokeDamageEvent(float damage) => OnDamage?.Invoke(Source, towerType,damage);
    public void InvokeKillEvent() => OnKill?.Invoke(Source, towerType);
    public readonly List<TowerAttackEffect> Effects = new();
}


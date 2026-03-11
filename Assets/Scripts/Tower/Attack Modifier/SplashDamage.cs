using System;
using UnityEngine;

[Serializable]
public class SplashDamage : TowerAttackEffect
{
    [SerializeField] private float radius = 3.5f;
    public void Apply(AttackContext context)
    {
        var hits = Physics2D.OverlapCircleAll(
            context.Target.transform.position,
            radius
        );

        foreach (var hit in hits)
        {
            if(hit.TryGetComponent(out Enemy enemy) && enemy != context.Target)
            {
                enemy.TakeDamage(context.Damage);
            }
        }
    }
}

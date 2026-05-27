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

        EnemyTraverseType attackable = context.AttackableType;

        foreach (var hit in hits)
        {
            if(hit.TryGetComponent(out Enemy enemy) && enemy != context.Target)
            {
                EnemyTraverseType enemyType = enemy.Type;
                if ((attackable & enemyType) == enemyType)
                {
                    enemy.TakeDamage(context.Damage);
                    
                    var performance = context.Source.performance;
                    performance.damageDealt += context.Damage;
                    performance.currentScore += context.Damage;
                    context.InvokeDamageEvent(context.Damage);
                    if (enemy.isDead)
                    {
                        performance.enemySlain++;
                        context.InvokeKillEvent();
                    }
                }
            }
        }
    }
}

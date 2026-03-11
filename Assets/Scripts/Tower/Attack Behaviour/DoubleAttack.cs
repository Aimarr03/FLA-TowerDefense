using System;
using UnityEngine;

[Serializable]
public class DoubleAttack : TowerAttackBehaviour
{
    public void PlanAttack(Tower tower, AttackPlan attackPlan)
    {
        if(tower.enemiesInRange.Count > 1)
        {
            attackPlan.Targets.Add(tower.enemiesInRange[1]);
        }
    }
}

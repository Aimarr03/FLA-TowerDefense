using System.Collections.Generic;
using UnityEngine;

public interface TowerAttackBehaviour
{
    public void PlanAttack(Tower tower, AttackPlan attackPlan);
}
public class AttackPlan
{
    public readonly List<Enemy> Targets = new();
    public int Shots = 1;
}


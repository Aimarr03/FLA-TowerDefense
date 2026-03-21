using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Tower Data", menuName = "Scriptable Objects/Create Tower Data")]
public class TowerData : ScriptableObject
{
    public string TowerName;
    public string Description;

    [Header("Damage")]
    [SerializeField] protected float _baseDamage = 5;
    [SerializeField, Range(0,1f)] protected float _multiplierDamage = 0.5f;

    [Header("Attack Range")]
    [SerializeField] protected float _baseAttackRange = 4f;
    [SerializeField, Range(0,1f)] protected float _multiplierAttackRange = 0.5f;

    [Header("Attack Rate")]
    [SerializeField] protected float _baseAttackRate = 1f;
    [SerializeField, Range(0, 1f)] protected float _multiplierAttackRate = 0.15f;

    [Header("Cost")]
    [SerializeField] protected int _costBuild = 5;
    [SerializeField, Range(0, 3f)] protected float _multiplierCost = 1.3f;

    [SerializeReference] public List<TowerAttackBehaviour> TowerAttackBehaviours;
    [SerializeReference] public List<TowerAttackEffect> TowerAttackEffects;

    public Sprite[] towerSprites;
    public Sprite[] iconSprites;

    public EnemyTraverseType AttackableType;
    public RuntimeAnimatorController animatorController;
    public float AttackRange(int level)
    {
        float multiplier = _baseAttackRange * _multiplierAttackRange;
        return _baseAttackRange + (multiplier * CalculatedLevel(level));
    }

    public float AttackRate(int level)
    {
        float multiplier = _baseAttackRate * _multiplierAttackRate;
        return _baseAttackRate - (multiplier * CalculatedLevel(level));
    }

    public float Damage(int level)
    {
        float multiplier = _baseDamage * _multiplierDamage;
        float totalDamage = _baseDamage + (multiplier * CalculatedLevel(level));
        return Mathf.Ceil(totalDamage);
    }

    public float SellCost(int level)
    {
        if (level == 1)
        {
            return CostBuild();
        }
        else
        {
            return UpgradeCost(level);
        }
    }

    public float UpgradeCost(int level)
    {
        float multiplier = _costBuild * _multiplierCost;
        float totalCost = _costBuild + (multiplier * CalculatedLevel(level));
        return Mathf.Ceil(totalCost);
    }    
    public Sprite GetIconSprite(int level) => iconSprites[Mathf.Clamp(level, 0, iconSprites.Length - 1)];
    public Sprite GetSprite(int level) => towerSprites[Mathf.Clamp(level, 0, towerSprites.Length - 1)];

    public float CostBuild() => _costBuild;
    int CalculatedLevel(int level) => Mathf.Max(0, level - 1);
}

using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeAction", menuName = "Scriptable Objects/UpgradeAction")]
public class UpgradeAction : TowerActionSO
{
    [SerializeField] private Sprite upgradeIcon;
    public override bool ExecutableConditions(Tower tower)
    {
        bool towerCondition = tower.CurrentState == Tower.State.Built && !tower.IsMax;
        int upgradeCost = (int) tower.TowerData.UpgradeCost(tower.Level + 1);
        bool moneyCondition = GameplayManager.Economy.IsEnough(upgradeCost);
        
        bool fullCondition = towerCondition && moneyCondition;
        //Debug.Log($"Upgrade Action: {fullCondition}");
        return fullCondition;
    }

    public override void Executes(Tower tower)
    {
        tower.Upgrade();
    }

    public override ActionContext GetActionContext(Tower tower)
    {
        var towerData = tower.TowerData;
        var level = tower.Level;
        var actionContext = new ActionContext
        {
            actionName = $"Upgrade Tower",
            actionDescription = new(),
            isExecutable = () => ExecutableConditions(tower),
            clickEvent = () => Executes(tower),
            actionIcon = upgradeIcon,
            useMoney = true,
            actionCost = (int)tower.TowerData.UpgradeCost(tower.Level + 1)
        };

        actionContext.actionDescription.Add($"Upgrade Tower for ${towerData.UpgradeCost(level + 1)}");

        var currentDMG = towerData.Damage(level);
        var nextDMG = towerData.Damage(level + 1);
        actionContext.actionDescription.Add($"Attack Damage: {currentDMG:0.#} \u2192 {nextDMG:0.#}");

        var currentARate = towerData.AttackRate(level);
        var nextARate = towerData.AttackRate(level + 1);
        actionContext.actionDescription.Add($"Attack Damage: {currentARate:0.#} \u2192 {nextARate:0.#}");

        var currentARange = towerData.AttackRange(level);
        var nextARange = towerData.AttackRange(level + 1);
        actionContext.actionDescription.Add($"Attack Damage: {currentARange:0.#} \u2192 {nextARange:0.#}");

        return actionContext;
    }
}

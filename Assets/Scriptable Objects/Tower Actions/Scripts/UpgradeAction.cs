using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeAction", menuName = "Scriptable Objects/UpgradeAction")]
public class UpgradeAction : TowerActionSO
{
    [SerializeField] private Sprite upgradeIcon;
    public override bool ExecutableConditions(Tower tower)
    {
        bool towerCondition = tower.CurrentState == Tower.State.Built && !tower.IsMax;
        int upgradeCost = (int) tower.TowerData.UpgradeCost(tower.Level + 1);
        bool moneyCondition = TD_API.Economy.IsEnough(upgradeCost);
        
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
        var actionContext = new ActionContext();
        actionContext.actionName = $"Upgrade Tower";
        actionContext.actionDescription = $"Upgrade Tower for ${tower.TowerData.UpgradeCost(tower.Level + 1)}";
        actionContext.isExecutable = () => ExecutableConditions(tower);
        actionContext.clickEvent = () => Executes(tower);
        actionContext.actionIcon = upgradeIcon;
        actionContext.useMoney = true;
        actionContext.actionCost = (int)tower.TowerData.UpgradeCost(tower.Level + 1);
        return actionContext;
    }
}

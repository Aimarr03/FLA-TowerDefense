using UnityEngine;

[CreateAssetMenu(fileName = "BuyAction", menuName = "Scriptable Objects/BuyAction")]
public class BuyAction : TowerActionSO
{
    [SerializeField] private TowerData towerData;
    public override bool ExecutableConditions(Tower tower)
    {
        bool towerCondition = tower.CurrentState == Tower.State.None && tower.Level < 0;
        float builtCost = towerData.CostBuild();
        bool moneyCondition = GameplayManager.Economy.IsEnough((int) builtCost);
        
        bool fullCondition = towerCondition && moneyCondition;
        
        //Debug.Log($"Buy Condition :{fullCondition}");
        return fullCondition;
    }

    public override void Executes(Tower tower)
    {
        tower.Build(towerData);
    }

    public override ActionContext GetActionContext(Tower tower)
    {
        var actionContext = new ActionContext
        {
            actionName = $"Buy Tower",
            actionDescription = new(),
            isExecutable = () => ExecutableConditions(tower),
            clickEvent = () => Executes(tower),
            actionIcon = towerData.iconSprites[0],
            actionCost = (int)towerData.CostBuild(),
            actionType = TowerActionType.Buy,
            useMoney = true
        };
        actionContext.actionDescription.Add($"Attack Damage: {towerData.Damage(1):0.#}");
        actionContext.actionDescription.Add($"Attack Range: {towerData.AttackRate(1):0.#}s");
        actionContext.actionDescription.Add($"Attack Rate: {towerData.AttackRange(1):0.#}m");
        return actionContext;
    }
    public int GetBuildCost()
    {
        return (int) towerData.CostBuild();
    }
    public string GetName()
    {
        return towerData.TowerName;
    }
}

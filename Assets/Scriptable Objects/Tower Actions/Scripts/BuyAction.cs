using UnityEngine;

[CreateAssetMenu(fileName = "BuyAction", menuName = "Scriptable Objects/BuyAction")]
public class BuyAction : TowerActionSO
{
    [SerializeField] private TowerData towerData;
    public override bool ExecutableConditions(Tower tower)
    {
        bool towerCondition = tower.CurrentState == Tower.State.None && tower.Level < 0;
        float builtCost = towerData.CostBuild();
        bool moneyCondition = TD_API.Economy.IsEnough((int) builtCost);
        
        bool fullCondition = towerCondition && moneyCondition;
        
        //Debug.Log($"Buy Condition :{fullCondition}");
        return fullCondition;
    }

    public override void Executes(Tower tower)
    {
        ActionInvoke?.Invoke(TowerActionType.Buy);
        tower.Build(towerData);
    }

    public override ActionContext GetActionContext(Tower tower)
    {
        var actionContext = new ActionContext();
        actionContext.actionName = $"Buy Tower";
        actionContext.actionDescription = $"{towerData.Description}\nBuy {towerData.TowerName} for ${towerData.CostBuild()}";
        actionContext.isExecutable = () => ExecutableConditions(tower);
        actionContext.clickEvent = () => Executes(tower);
        actionContext.actionIcon = towerData.iconSprites[0];
        actionContext.actionCost = (int) towerData.CostBuild();
        actionContext.actionType = TowerActionType.Buy;
        return actionContext;
    }
}

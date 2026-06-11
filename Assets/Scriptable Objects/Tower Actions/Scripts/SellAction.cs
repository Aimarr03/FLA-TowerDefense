using UnityEngine;

[CreateAssetMenu(fileName = "SellAction", menuName = "Scriptable Objects/SellAction")]
public class SellAction : TowerActionSO
{
    [SerializeField] private Sprite sellIcon;
    public override bool ExecutableConditions(Tower tower)
    {
        bool condition = tower.CurrentState == Tower.State.Built && tower.Level > 0;
        //Debug.Log("Sell Conditions: " + condition);
        return condition;
    }

    public override void Executes(Tower tower)
    {
        tower.Sell();
    }

    public override ActionContext GetActionContext(Tower tower)
    {
        var towerData = tower.TowerData;
        int sellCost = (int) towerData.SellCost(tower.Level);

        var actionContext = new ActionContext
        {
            actionName = $"Sell Tower",
            actionDescription = new(),
            isExecutable = () => ExecutableConditions(tower),
            clickEvent = () => Executes(tower),
            actionIcon = sellIcon,

            useMoney = false,
            actionCost = sellCost
        };
        actionContext.actionDescription.Add($"Sell {towerData.TowerName}");
        actionContext.actionDescription.Add($"Sell: {sellCost:0} gold");
        return actionContext;
    }
}

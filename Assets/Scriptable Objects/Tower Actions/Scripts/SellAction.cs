using UnityEngine;

[CreateAssetMenu(fileName = "SellAction", menuName = "Scriptable Objects/SellAction")]
public class SellAction : TowerActionSO
{
    public override bool ExecutableConditions(Tower tower)
    {
        bool condition = tower.CurrentState == Tower.State.Built && tower.Level > 0;
        Debug.Log("Sell Conditions: " + condition);
        return condition;
    }

    public override void Executes(Tower tower)
    {
        ActionInvoke?.Invoke(TowerActionType.Sell);
        tower.Sell();
    }

    public override ActionContext GetActionContext(Tower tower)
    {
        var actionContext = new ActionContext();
        actionContext.actionName = $"Sell Tower";
        actionContext.actionDescription = $"Sell Tower for ${tower.TowerData.SellCost(tower.Level)}";
        actionContext.isExecutable = ExecutableConditions(tower);
        actionContext.clickEvent = () => Executes(tower);

        return actionContext;
    }
}

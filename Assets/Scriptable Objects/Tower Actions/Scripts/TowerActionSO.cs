using System;
using UnityEngine;

public abstract class TowerActionSO : ScriptableObject
{
    public string Name;
    public string Description;
    public static Action<TowerActionType> ActionInvoke;
    public abstract bool ExecutableConditions(Tower tower);
    public abstract void Executes(Tower tower);
    public abstract ActionContext GetActionContext(Tower tower);
}
[Serializable]
public class ActionContext
{
    public string actionName;
    public Sprite actionIcon;
    public int actionCost;
    public TowerActionType actionType;
    public Action clickEvent;
    public string actionDescription;
    public Func<bool> isExecutable;
}
public enum TowerActionType
{
    Buy,
    Upgrade,
    Sell
}
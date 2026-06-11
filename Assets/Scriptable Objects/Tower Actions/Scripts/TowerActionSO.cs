using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerActionSO : ScriptableObject
{
    public string Name;
    public string Description;
    
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
    public List<string> actionDescription;
    public Func<bool> isExecutable;
    public bool useMoney = false;
}
public enum TowerActionType
{
    Buy,
    Upgrade,
    Sell,
    Presell,
}
using UnityEngine;
using System;
[Serializable]
public class EconomyManager
{
    public Action<int> OnMoneyChange;

    [SerializeField] int currentMoney;

    public int CurrentMoney => currentMoney;

    public EconomyManager(int startingMoney)
    {
        OnMoneyChange = null;
        currentMoney = startingMoney;
    }
    public void UpdateInfo() => OnMoneyChange?.Invoke(currentMoney);
    public bool IsEnough(int amount)
    {
        bool condition = currentMoney >= amount;
        //Debug.Log($"Required Money: {amount} with money left {currentMoney}");
        return condition;
    }
    public void GainMoney(int money)
    {
        currentMoney += money;
        OnMoneyChange?.Invoke(currentMoney);
    }
    public void UseMoney(int money)
    {
        currentMoney -= money;
        OnMoneyChange?.Invoke(currentMoney);
    }
}

using UnityEngine;
using System;
[Serializable]
public class EconomyManager
{
    public Action<int> OnMoneyChange;

    [SerializeField] int currentMoney;

    public int CurrentMoney => currentMoney;

    public bool IsEnough(int amount) => currentMoney >= amount;
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

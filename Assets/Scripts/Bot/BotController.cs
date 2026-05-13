using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

[Serializable]
public class BotProperty
{
    public BotController.Capability botType;
    [Header("Weighted Property Strategy")]
    [Range(1, 100)] public int RandomWeight = 50;
    [Range(1, 100)] public int RandomBestWeight = 35;
    [Range(1, 100)] public int BestWeight = 10;
}
public enum BotTowerSelectionStrategy
{
    PureRandom,
    RandomBestPreference,
    BestPreference
}
public class BotController : MonoBehaviour
{
    public enum Capability
    {
        Low,
        Medium,
        High
    }
    private List<Tower> allTower;
    private List<BuyAction> buyActions;
    private bool isActive = false;
    [SerializeField] float intervalDecision = 1.5f;
    [SerializeField] private Capability botType = Capability.Low;
    [SerializeField] private BotProperty botProperty;
    float currentInterval = 0f;

    /// <summary>
    /// Build Probability Variable
    /// </summary>
    float buildChance = 0.2f;
    float buildAccumulator = 0f;
    float buildInterval = 0f;
    float buildCooldown = 3f;
    bool canBuild => buildInterval >= buildCooldown;
    public bool IsActive => isActive;
    
    public void Init()
    {
        allTower = FindObjectsByType<Tower>(FindObjectsSortMode.None).ToList();
        isActive = true;
        
        buyActions = new();
        foreach(var buyAct in TD_API.BuildActions)
        {
            buyActions.Add(buyAct as BuyAction);
        }

        currentInterval = 0;
        GameplayManager.instance.onchangedState += Gameplay_ChangeState;
        TD_API.Economy.OnMoneyChange += OnMoneyChange;
    }

    private void Gameplay_ChangeState(GameplayManager.State state)
    {
        if(!isActive) return;
        
        bool gameEnd = state switch
        {
            GameplayManager.State.Win => true,
            GameplayManager.State.GameOver => true,
            _ => false
        };
        
        if (gameEnd)
        {
            GameplayManager.instance.onchangedState -= Gameplay_ChangeState;
            TD_API.Economy.OnMoneyChange -= OnMoneyChange;
            isActive = false;
            return;
        }

    }
    private void OnMoneyChange(int currentMoney)
    {
        
    }
    void Update()
    {
        if (!isActive) return;

        currentInterval += Time.deltaTime;
        buildInterval += Time.deltaTime;
        if(currentInterval > intervalDecision)
        {
            currentInterval = 0;
            Build();
            TryStartingDefend();
        }
    }
    private void TryStartingDefend()
    {
        if(GameplayManager.instance.GameState == GameplayManager.State.Building)
        {
            if (!CheckBuildActionLeft())
            {
                GameplayManager.instance.StartDefending();
            }    
        }
    }
    private bool CheckBuildActionLeft()
    {
        var buildAction = GetRandomSufficientTowerBuild();
        bool canBuildAgain = buildAction != null;
        return canBuildAgain;
    }
    private bool TryBuildTrigger()
    {
        buildAccumulator += buildChance;

        float roll = Random.value;

        if (roll < buildAccumulator)
        {
            buildAccumulator = 0f;
            return true;
        }

        return false;
    }
    
    private void Build()
    {
        if(!canBuild)
            return;
        
        if(!TryBuildTrigger()) 
            return;
        
        buildInterval = 0f;
        BotTowerSelectionStrategy stratType = GetStrategyWeighted();
        Debug.Log($"[BOT] Randomized Strats: {stratType}");

        Tower tower = stratType switch 
        { 
            BotTowerSelectionStrategy.PureRandom => GetRandomUnBuiltTower(),
            BotTowerSelectionStrategy.RandomBestPreference => GetRandomizedPreferenceTower(),
            BotTowerSelectionStrategy.BestPreference => GetBestPreferenceTower(),
            _ => null
        };
        TowerActionSO buildAction = GetRandomSufficientTowerBuild();
        if(tower == null || buildAction == null) return;

        buildAction.Executes(tower);
    }
    private BotTowerSelectionStrategy GetStrategyWeighted()
    {
        float totalWeight = botProperty.RandomWeight +
            botProperty.RandomBestWeight +
            botProperty.BestWeight;
        
        float roll = Random.value * totalWeight;
        Debug.Log($"[Debug] Roll: {roll} with totalWeight of {totalWeight}");
        if(roll < botProperty.RandomWeight)
        {
            return BotTowerSelectionStrategy.PureRandom;
        }
        roll -= botProperty.RandomWeight;

        if(roll < botProperty.RandomBestWeight)
        {
            return BotTowerSelectionStrategy.RandomBestPreference;
        }
        return BotTowerSelectionStrategy.BestPreference;
    }

    private Tower GetRandomizedPreferenceTower(int count = 3)
    {
        List<Tower> emptyTower = allTower.Where(tower => tower.CurrentState == Tower.State.None).ToList();
        if(count >= emptyTower.Count)
        {
            count = emptyTower.Count;
        }
        List<Tower> highestTower = emptyTower
        .OrderByDescending(tower => tower.PreferenceScore)
        .Take(count)
        .ToList();

        int maxIndex = highestTower.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);
        Tower randomizedPreferenceTower = highestTower[randomIndex];

        return randomizedPreferenceTower;
    }
    private Tower GetBestPreferenceTower()
    {
        List<Tower> emptyTower = allTower.Where(tower => tower.CurrentState == Tower.State.None).ToList();
        Tower towerHighestScore = allTower.OrderByDescending(tower => tower.PreferenceScore).First();
        return towerHighestScore;
    }
    private Tower GetRandomUnBuiltTower()
    {
        List<Tower> emptyTower = allTower.Where(tower => tower.CurrentState == Tower.State.None).ToList();
        int maxIndex = emptyTower.Count() - 1;
        if(maxIndex < 0) 
            return null;
        
        int randomIndex = Random.Range(0, maxIndex + 1);
        Tower randomNotBuiltTower = emptyTower[randomIndex];
        return randomNotBuiltTower;
    }
    private TowerActionSO GetRandomSufficientTowerBuild()
    {
        var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();
        return GetRandomBuildTowerAction(actions);
    }
    private TowerActionSO GetRandomBuildTowerAction(List<BuyAction> actions)
    {
        if(actions.Count == 0)
        {
            return null;
        }
        int maxIndex = actions.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        TowerActionSO randomBuildAction = actions[randomIndex];
        return randomBuildAction;
    }
}

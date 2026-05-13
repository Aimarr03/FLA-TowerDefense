using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using UnityEngine.Rendering;

[Serializable]
public class BotProperty
{
    public BotController.Capability botType;
    [Header("Weighted Property Strategy")]
    [Range(1, 100)] public int BuildRandomWeight = 50;
    [Range(1, 100)] public int BuildRandomBestWeight = 35;
    [Range(1, 100)] public int BuildBestWeight = 10;
    
    [Header("Weighted Property Type")]
    [Range(1, 100)] public int TowerTypeRandomWeight = 50;
    [Range(1, 100)] public int TowerTypeObservingWeight = 25;
}
public enum BotTowerSelectionStrat
{
    PureRandom,
    RandomBestPreference,
    BestPreference
}
public enum BotTowerTypeStrat
{
    PureRandom,
    Observation
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
    
    private List<EnemySpawnInfo> enemySpawnInfos;
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
        enemySpawnInfos = new();
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
        else
        {
            if(state == GameplayManager.State.Building)
            {
                if(enemySpawnInfos == null)
                    enemySpawnInfos = new();
                
                enemySpawnInfos.Clear();
                enemySpawnInfos.AddRange(GameplayManager.instance.enemySpawnInfos);
            }
            else if(state == GameplayManager.State.Defending)
            {
                
            }
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
        var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();
        bool canBuildAgain = actions != null && actions.Count > 0;
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
        BotTowerSelectionStrat stratType = GetStrategyWeighted();
        Debug.Log($"[BOT] Randomized Get Unbuild Tower Strats: {stratType}");

        Tower tower = stratType switch 
        { 
            BotTowerSelectionStrat.PureRandom => GetRandomUnBuiltTower(),
            BotTowerSelectionStrat.RandomBestPreference => GetRandomizedPreferenceTower(),
            BotTowerSelectionStrat.BestPreference => GetBestPreferenceTower(),
            _ => null
        };
        TowerActionSO buildAction = GetTowerType();
        if(tower == null || buildAction == null) return;

        buildAction.Executes(tower);
    }
    private BotTowerSelectionStrat GetStrategyWeighted()
    {
        float totalWeight = botProperty.BuildRandomWeight +
            botProperty.BuildRandomBestWeight +
            botProperty.BuildBestWeight;
        
        float roll = Random.value * totalWeight;
        Debug.Log($"[Debug] Strat of Get unbuild Tower");
        Debug.Log($"[Debug] Roll: {roll} with totalWeight of {totalWeight}");
        if(roll < botProperty.BuildRandomWeight)
        {
            return BotTowerSelectionStrat.PureRandom;
        }
        roll -= botProperty.BuildRandomWeight;

        if(roll < botProperty.TowerTypeObservingWeight)
        {
            return BotTowerSelectionStrat.RandomBestPreference;
        }
        return BotTowerSelectionStrat.BestPreference;
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

    /// <summary>
    /// This segment is for function of calling what kind of tower
    /// the bot is going to build
    /// </summary>
    private BotTowerTypeStrat GetTowerTypeStrat()
    {
        float totalWeight = botProperty.TowerTypeRandomWeight
            + botProperty.TowerTypeObservingWeight;
        
        float roll = Random.value * totalWeight;

        Debug.Log($"[Debug] Strat of Get Tower Type");
        Debug.Log($"[Debug] Roll: {roll} with totalWeight of {totalWeight}");

        if(roll < botProperty.TowerTypeRandomWeight)
        {
            return BotTowerTypeStrat.PureRandom;
        }
        return BotTowerTypeStrat.Observation;
    }
    // private TowerActionSO GetRandomSufficientTowerBlueprint()
    // {
    //     var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();

        
    //     BotTowerTypeStrat methodGetTowerType = GetTowerTypeStrat();
    //     Debug.Log($"[BOT] Randomized Get Type Strats: {methodGetTowerType}");
    //     TowerActionSO blueprint = methodGetTowerType switch
    //     {
    //         BotTowerTypeStrat.PureRandom =>  GetRandomTowerBuildBlueprint(actions),
    //         BotTowerTypeStrat.Observation => GetBuildingObservastionBasedTowerBuildBlueprint(actions),
    //         _ => null
    //     };

    //     return GetRandomTowerBuildBlueprint(actions);
    // }
    private TowerActionSO GetTowerType()
    {
        var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();

        BotTowerTypeStrat methodGetTowerType = GetTowerTypeStrat();
        Debug.Log($"[BOT] Randomized Get Type Strats: {methodGetTowerType}");
        TowerActionSO blueprint = methodGetTowerType switch
        {
            BotTowerTypeStrat.PureRandom =>  GetRandomTowerBuildBlueprint(actions),
            BotTowerTypeStrat.Observation => GetObservationBasedTowerBuildBlueprint(actions),
            _ => null
        };

        return GetRandomTowerBuildBlueprint(actions);
    }
    private TowerActionSO GetRandomTowerBuildBlueprint(List<BuyAction> blueprints)
    {
        if(blueprints.Count == 0)
        {
            return null;
        }
        int maxIndex = blueprints.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        TowerActionSO randomBuildAction = blueprints[randomIndex];
        return randomBuildAction;
    }
    private TowerActionSO GetObservationBasedTowerBuildBlueprint(List<BuyAction> blueprints)
    {
        if(GameplayManager.instance.GameState == GameplayManager.State.Defending)
        {
            return null;   
        }
        else if(GameplayManager.instance.GameState == GameplayManager.State.Building)
        {
            return GetBuildingObservastionBasedTowerBuildBlueprint(blueprints);
        }
        else return null;
    }
    private TowerActionSO GetBuildingObservastionBasedTowerBuildBlueprint(List<BuyAction> blueprints)
    {
        if(blueprints.Count == 0)
        {
            return null;
        }

        if(enemySpawnInfos.Count == 0)
            return null;

        var sortedHighestSpawn = enemySpawnInfos.OrderBy(enemy => enemy.amount).ToList();
        var highestSpawn = sortedHighestSpawn[0];

        Dictionary<string, BuyAction> dictionaryTower = blueprints.ToDictionary(towerData => towerData.GetName(), towerData => towerData);

        var towerData = highestSpawn.type switch
        {
            EnemyType.Bee => dictionaryTower["Archer Tower"],
            EnemyType.Goblin => dictionaryTower["Mortar Tower"],
            EnemyType.Wolf => dictionaryTower["Archer Tower"],
            EnemyType.Slime => dictionaryTower["Mage Tower"],
            _ => null
        };

        return towerData;
    }
}

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
    [Header("Weighted Action Property")]
    [Range(1, 100)] public int ActionWeightBuild = 75;
    [Range(1, 100)] public int ActionWeightUpgrade = 25;
    [Range(1, 100)] public int ActionWeightDefend = 15;
    [Range(1, 100)] public int ActionWeightSell = 5;
    
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
    public enum ActionType
    {
        None,
        Build,
        Upgrade,
        Sell,
        Defend
    }
    private List<Tower> allTower;
    private List<BuyAction> buyActions;
    private bool isActive = false;
    private ActionType currentActionType = ActionType.None;
    [SerializeField] float intervalDecision = 1.5f;
    [SerializeField] private Capability botType = Capability.Low;
    [SerializeField] private BotProperty botProperty;
    [SerializeField] private EnemySpawner enemySpawner;


    private List<Tower> upgradableTower = new();
    private List<Tower> buildableTower = new();
    private List<Tower> sellableTower = new();

    private bool canUpgrade = false;
    private bool canBuild = false;
    private bool canSell = false;
    //private bool shouldDefend = false;
    float currentInterval = 0f;

    /// <summary>
    /// Build Probability Variable
    /// </summary>
    float buildChance = 0.2f;
    float buildAccumulator = 0f;
    float buildInterval = 0f;
    float buildCooldown = 3f;
    float upgradeInterval = 0f;
    float upgradeCooldown = 3f;
    float sellInterval = 0f;
    float sellCooldown = 30f;
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
        upgradeInterval += Time.deltaTime;
        sellInterval += Time.deltaTime;
        if(currentInterval > intervalDecision)
        {
            currentInterval = 0;
            UpdateActionCondition();
            DecideAction();

            switch (currentActionType)
            {
                case ActionType.Build:
                    BuildAction();
                    break;
                case ActionType.Upgrade:
                    UpgradeAction();
                    break;
                case ActionType.Sell:
                    SellAction();
                    break;
                case ActionType.None:
                    Debug.Log("[BOT] Decide to do none!");
                    break;
            }
            currentActionType = ActionType.None;
            TryStartingDefend();
        }
    }
    private void UpdateActionCondition()
    {
        if (!canBuild)
        {
            bool buildCooldownCondition = buildInterval >= buildCooldown;
            bool buildRandomCondition = TryBuildChanceTrigger();
            bool buildMoneyCondition = CheckBuildActionLeft();

            var buildableLeft = allTower.Where(tower => tower.CurrentState == Tower.State.None);
            buildableTower.Clear();
            buildableTower.AddRange(buildableLeft.ToList());

            bool buildableTowerCondition = buildableTower.Count > 0;
            
            canBuild = buildCooldownCondition && buildRandomCondition 
            && buildMoneyCondition && buildableTowerCondition;       
        }

        if (!canUpgrade)
        {
            UpdateUpgradableTowerList();
            bool upgradeCooldownCondition = upgradeInterval >= upgradeCooldown;
            bool upgradeTowerConditoin = upgradableTower.Count > 0;
            canUpgrade = upgradeTowerConditoin && upgradeTowerConditoin;    
        }
        if (!canSell)
        {
            UpdateSellableTower();
            bool sellCooldownCondition = sellInterval >= sellCooldown;
            bool sellTowerCondition = sellableTower.Count > 0;
            canSell = sellCooldownCondition && sellTowerCondition;
        }
    }
    private void DecideAction()
    {
        int totalWeight = botProperty.ActionWeightBuild 
        + botProperty.ActionWeightUpgrade;

        float roll = Random.value * totalWeight;
        Debug.Log($"[Debug] Strat of Get Action Tower");
        Debug.Log($"[Debug] Roll: {roll} with totalWeight of {totalWeight}");
        Debug.Log($"[Debug] conditons for buy: {canBuild} || upgrade: {canUpgrade} || sell: {canSell}");
        if(roll < botProperty.ActionWeightBuild && canBuild)
        {
            currentActionType = ActionType.Build;
            Debug.Log($"[Debug] Bot go with Build");
            return;
        }

        roll -= botProperty.ActionWeightBuild;
        if(roll < botProperty.ActionWeightUpgrade && canUpgrade)
        {
            currentActionType = ActionType.Upgrade;
            Debug.Log($"[Debug] Bot go with Upgrade");
            return;
        }
        roll -= botProperty.ActionWeightUpgrade;

        if(roll < botProperty.ActionWeightSell && canSell)
        {
            currentActionType = ActionType.Sell;
            Debug.Log($"[Debug] Bot go with Sell");
            return;
        }
        roll -= botProperty.ActionWeightSell;

        currentActionType = ActionType.None;
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
        var actions = buyActions.Where(buyAction => TD_API.Economy.IsEnough(buyAction.GetBuildCost())).ToList();
        bool canBuildAgain = actions != null && actions.Count > 0;
        return canBuildAgain;
    }
    private bool TryBuildChanceTrigger()
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
    
    private void BuildAction()
    {
        if(!canBuild)
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
        canBuild = false;
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
            return GetDefendingObservationBasedTowerBuildBlueprint(blueprints);
        }
        else if(GameplayManager.instance.GameState == GameplayManager.State.Building)
        {
            return GetBuildingObservastionBasedTowerBuildBlueprint(blueprints);
        }
        else return null;
    }
    private TowerActionSO GetDefendingObservationBasedTowerBuildBlueprint(List<BuyAction> blueprints)
    {
        if(blueprints.Count == 0)
        {
            return null;
        }

        if(enemySpawner == null)
        {
            Debug.LogWarning("Enemy Spawner is not referenced! Try Again");
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if(enemySpawner == null)
            {
                Debug.LogError("Enemy Spawner not found!");
                return null;
            }
        }
        List<EnemySpawnInfo> currentEnemy = enemySpawner.EnemySpawnInfos;
        if(currentEnemy.Count == 0)
        {
            return null;
        }

        var sortedHighestSpawn = currentEnemy.OrderBy(enemy => enemy.amount).ToList();
        var highestSpawn = currentEnemy[0];

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

    private List<Tower> GetBuiltTower() => allTower.Where(tower => tower.CurrentState == Tower.State.Built).ToList();

    
    private void UpdateUpgradableTowerList()
    {
        List<Tower> builtTower = GetBuiltTower();
        if(builtTower == null || builtTower.Count == 0)
            return;

        upgradableTower.Clear();
        foreach(var tower in builtTower)
        {
            TowerData towerData = tower.TowerData;
            int upgradeCost = (int)towerData.UpgradeCost(tower.Level);
            bool isUpgradable = !tower.IsMax && TD_API.Economy.IsEnough(upgradeCost);
            if(isUpgradable)
                upgradableTower.Add(tower);
        }
    }
    private void UpgradeAction()
    {
        if(upgradableTower.Count == 0)
            return;
        
        upgradeInterval = 0;
        UpgradeRandomTower(upgradableTower);

        canUpgrade = false;
    }
    private void UpgradeRandomTower(List<Tower> upgradableTower)
    {
        int maxIndex = upgradableTower.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        Tower randomTower = upgradableTower[randomIndex];
        Debug.Log($"[Bot] Random Upgrade Index of: {randomTower.TowerData.TowerName}");
        randomTower.Upgrade();
    }
    private void UpdateSellableTower()
    {
        List<Tower> builtTower = GetBuiltTower();
        if(builtTower == null || builtTower.Count == 0)
        {
            return;
        }
        sellableTower.Clear();

        foreach(var tower in builtTower)
        {
            bool isSellable = tower.CurrentState == Tower.State.Built && tower.Level > 0;
            if(isSellable)
                upgradableTower.Add(tower);
        }
    }
    private void SellAction()
    {
        if(sellableTower.Count == 0)
            return;
        
        sellInterval = 0;
        RandomIndexSell();
    }
    private void RandomIndexSell()
    {
        int maxIndex = sellableTower.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);
        Tower tower = sellableTower[randomIndex];
        tower.Sell();
    }
}

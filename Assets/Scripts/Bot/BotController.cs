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
    [Range(1, 100)] public int BaseWeightBuildAction = 75;
    [Range(1, 100)] public int BaseWeightUpgradeAction = 25;
    [Range(1, 100)] public int BaseWeightDefendAction = 15;
    [Range(1, 100)] public int BaseWeightSellAction = 5;
    [Range(1, 100)] public int BaseWeightNoneAction = 35;
    
    [Header("Weighted Property Strategy")]
    [Range(1, 100)] public int BuildRandomWeight = 50;
    [Range(1, 100)] public int BuildRandomBestWeight = 35;
    [Range(1, 100)] public int BuildBestWeight = 10;
    
    [Header("Weighted Property Type")]
    [Range(1, 100)] public int TowerTypeRandomWeight = 50;
    [Range(1, 100)] public int TowerTypeObservingWeight = 25;

    [Header("Weighted Upgrade Type")]
    [Range(1, 100)] public int TowerUpgradeRandom = 30;
    [Range(1, 100)] public int TowerUpgradePerformance = 60;
    [Range(0, 1.5f)] public float SellThreshold = 0.75f;
    
    [Header("Weighted Sell Type")]
    [Range(1, 100)] public int TowerSellRandom = 30;
    [Range(1, 100)] public int TowerSellPerformance = 30;
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
    private Dictionary<Capability, BotProperty> ListBot = new();
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
    float currentInterval = 0f;
    //private bool shouldDefend = false;

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
    float sellCooldown = 15f;
    public bool IsActive => isActive;
    /// <summary>
    /// For Making weights more dynamic, using base weight and modify the basis based on current Progress
    /// </summary>
    private int buildWeight = 0;
    private int upgradeWeight = 0;
    private int sellWeight = 0;
    private int noneWeight = 0;
    private float DifferenceHealth = 0;
    private float DifferenceDamageDealt = 0;
    private float DifferentEnemySlain = 0;
    void Awake()
    {
        ListBot = new();   
    }
    private List<EnemySpawnInfo> enemySpawnInfos;
    public void Init(BotUsage botUsage)
    {
        if (!botUsage.useBot)
        {
            isActive = false;
            return;
        }
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
            EvaluatePreviousRound();
            if(GameplayManager.instance.CurrentWave > 1)
            {
                EvaluateDifferenceRound();
            }
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
            UpdateDecisionWeight();
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
                    break;
            }
            canBuild = false;
            canSell = false;
            canUpgrade = false;

            currentActionType = ActionType.None;
            TryStartingDefend();
        }
    }
    private void UpdateActionCondition()
    {
        if (!canBuild)
        {
            //bool buildCooldownCondition = buildInterval >= buildCooldown;
            bool buildRandomCondition = TryBuildChanceTrigger();
            bool buildMoneyCondition = CheckBuildActionLeft();

            var buildableLeft = allTower.Where(tower => tower.CurrentState == Tower.State.None);
            buildableTower.Clear();
            buildableTower.AddRange(buildableLeft.ToList());

            bool buildableTowerCondition = buildableTower.Count > 0;
            
            canBuild = buildRandomCondition 
            && buildMoneyCondition && buildableTowerCondition;       
        }

        if (!canUpgrade)
        {
            UpdateUpgradableTowerList();
            bool upgradeCooldownCondition = upgradeInterval >= upgradeCooldown;
            bool upgradeTowerCondition = upgradableTower.Count > 0;
            canUpgrade = upgradeCooldownCondition && upgradeTowerCondition;    
        }
        if (!canSell)
        {
            UpdateSellableTower();
            bool anyZeroTower = sellableTower.Any(tower => tower.performance.currentScore == 0);
            bool shouldSell = false;
            if (anyZeroTower)
            {
                canSell = false;    
                return;
            }

            if(sellableTower.Count > 0)
            {
                var worstTower = sellableTower.OrderBy(tower => tower.performance.currentScore).First();    
                shouldSell = worstTower.performance.currentScore <= botProperty.SellThreshold;
            }
            
            //bool sellCooldownCondition = sellInterval >= sellCooldown;
            bool sellTowerCondition = sellableTower.Count > 1;
            canSell = sellTowerCondition && shouldSell;
        }
    }
    private void UpdateDecisionWeight()
    {
        buildWeight = botProperty.BaseWeightBuildAction;
        
        int totalUnbuildTower = allTower.Where(tower => tower.CurrentState == Tower.State.None).Count();
        buildWeight += 10 * totalUnbuildTower;
        if(buildInterval >= buildCooldown)
        {
            buildWeight += 25;
        }

        upgradeWeight = botProperty.BaseWeightUpgradeAction;
        int totalBuiltTower = allTower.Where(tower => tower.CurrentState == Tower.State.Built).Count();
        upgradeWeight += 5 * totalBuiltTower;
        if(upgradableTower.Count <= 0)
        {
            upgradeWeight = 0;
        }
        noneWeight = botProperty.BaseWeightNoneAction;
        noneWeight += totalBuiltTower * 15;

        sellWeight = botProperty.BaseWeightSellAction;
        if(DifferenceHealth == 0) 
            sellWeight = 0;
        else if(DifferenceHealth < 5)
        {
            sellWeight += (int)DifferenceHealth;
            if(sellInterval >= sellCooldown)
            {
                sellWeight += 25;
            }
        }
        else
        {
            sellWeight += (int)DifferenceHealth * 2;
            if(sellInterval >= sellCooldown)
            {
                sellWeight += 25;
            }
        }

    }
    private void DecideAction()
    {
        int totalWeight = buildWeight 
        + upgradeWeight
        + sellWeight
        + noneWeight;

        float roll = Random.value * totalWeight;
        Debug.Log($"[Debug] Strat of Get Action Tower");
        Debug.Log($"[Debug] Roll: {roll} with totalWeight of {totalWeight}");
        Debug.Log($"[Debug] conditons for buy: {canBuild} || upgrade: {canUpgrade} || sell: {canSell}");
        
        if(roll < noneWeight)
        {
            Debug.Log($"[Debug] Check rolls: {roll} < {botProperty.BaseWeightNoneAction}");
            currentActionType = ActionType.None;
            Debug.Log($"[Debug] Bot go with none");
            return;
        }
        roll -= noneWeight;
        
        if(roll < buildWeight && canBuild)
        {
            currentActionType = ActionType.Build;
            Debug.Log($"[Debug] Bot go with Build");
            return;
        }

        roll -= buildWeight;
        if(roll < upgradeWeight && canUpgrade)
        {
            currentActionType = ActionType.Upgrade;
            Debug.Log($"[Debug] Bot go with Upgrade");
            return;
        }
        roll -= upgradeWeight;

        if(roll < sellWeight && canSell)
        {
            currentActionType = ActionType.Sell;
            Debug.Log($"[Debug] Bot go with Sell");
            return;
        }
        
        roll -= sellWeight;
        Debug.Log($"[Debug] Bot go with none");
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
        BuyAction buildAction = GetTowerType();
        bool isMoneyEnough = TD_API.Economy.IsEnough(buildAction.GetBuildCost());
        if (!isMoneyEnough)
        {
            return;
        }
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
        Tower towerHighestScore = emptyTower.OrderByDescending(tower => tower.PreferenceScore).First();
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
    private BuyAction GetTowerType()
    {
        //var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();

        BotTowerTypeStrat methodGetTowerType = GetTowerTypeStrat();
        Debug.Log($"[BOT] Randomized Get Type Strats: {methodGetTowerType}");
        BuyAction blueprint = methodGetTowerType switch
        {
            BotTowerTypeStrat.PureRandom =>  GetRandomTowerBuildBlueprint(buyActions),
            BotTowerTypeStrat.Observation => GetObservationBasedTowerBuildBlueprint(buyActions),
            _ => null
        };

        return blueprint;
    }
    private BuyAction GetRandomTowerBuildBlueprint(List<BuyAction> blueprints)
    {
        if(blueprints.Count == 0)
        {
            return null;
        }
        int maxIndex = blueprints.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        BuyAction randomBuildAction = blueprints[randomIndex];
        return randomBuildAction;
    }
    private BuyAction GetObservationBasedTowerBuildBlueprint(List<BuyAction> blueprints)
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
    private BuyAction GetDefendingObservationBasedTowerBuildBlueprint(List<BuyAction> blueprints)
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
        
        var roll = Random.value;
        if(roll > 0.5)
        {
            
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
        else
        {
            if(currentEnemy.Any(enemy => enemy.type == EnemyType.Bee))
            {
                return dictionaryTower["Archer Tower"];
            }
            else if(currentEnemy.Any(enemy => enemy.type == EnemyType.Slime))
            {
                return dictionaryTower["Mage Tower"];
            }
            else if(currentEnemy.Any(enemy => enemy.type == EnemyType.Wolf))
            {
                return dictionaryTower["Archer Tower"];
            }
            else
            {
                return dictionaryTower["Mortar Tower"];
            }
        }
    }
    private BuyAction GetBuildingObservastionBasedTowerBuildBlueprint(List<BuyAction> blueprints)
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
            int upgradeCost = (int)towerData.UpgradeCost(tower.Level + 1);
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

        float totalWeight = botProperty.TowerUpgradeRandom + botProperty.TowerUpgradePerformance;
        float roll = Random.value * totalWeight;

        if(roll < botProperty.TowerUpgradeRandom)
        {
            UpgradeRandomTower(upgradableTower);    
            return;
        }
        roll -= botProperty.TowerUpgradeRandom;
        if(roll < botProperty.TowerUpgradePerformance)
        {
            UpgradeBestTower(upgradableTower);
            return;
        }
        
    }
    private void UpgradeRandomTower(List<Tower> upgradableTower)
    {
        int maxIndex = upgradableTower.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        Tower randomTower = upgradableTower[randomIndex];
        Debug.Log($"[Bot] Random Upgrade Index of: {randomTower.TowerData.TowerName}");
        randomTower.Upgrade();
    }
    private void UpgradeBestTower(List<Tower> upgradableTower)
    {
        List<Tower> bestTowerList = upgradableTower.OrderByDescending(tower => tower.performance.currentScore).ToList();
        Tower bestTower = bestTowerList[0];
        bestTower.Upgrade();
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
                sellableTower.Add(tower);
        }
    }
    private void SellAction()
    {
        if(sellableTower.Count == 0)
            return;
        
        sellInterval = 0;

        float totalWeight = botProperty.TowerSellRandom + botProperty.TowerSellPerformance;
        float roll = Random.value * totalWeight;
        
        if(roll < botProperty.TowerSellRandom)
        {
            RandomIndexSell(sellableTower);
            return;
        }
        roll -= botProperty.TowerSellRandom;
        
        if(roll < botProperty.TowerSellPerformance)
        {
            SellWorstTower(sellableTower);
            return;
        }
    }
    private void RandomIndexSell(List<Tower> towers)
    {
        int maxIndex = towers.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);
        Tower tower = towers[randomIndex];
        tower.Sell();
    }
    private void SellWorstTower(List<Tower> towers)
    {
        List<Tower> worstTowerList = towers.OrderBy(tower => tower.performance.currentScore).ToList();
        var worstTower = worstTowerList[0];
        worstTower.Sell();
    }
    private void EvaluatePreviousRound()
    {
        var builtTower = GetBuiltTower();
        var RoundPerformances = GameplayManager.instance.RoundPerformances;
        
        int currentWaveIndex = GameplayManager.instance.CurrentWaveIndex;
        int previousWaveIndex = currentWaveIndex - 1;
        if(previousWaveIndex < 0)
        {
            return;
        }
        
        var previousRoundPerformance = RoundPerformances[previousWaveIndex];
        float totalEnemy = previousRoundPerformance.TotalEnemy;
        
        foreach(var tower in builtTower)
        {
            if(tower.CurrentState == Tower.State.None) continue;
            
            var currentTowerPerformance = tower.performance;
            float towerDamageDealt = currentTowerPerformance.damageDealt;
            float towerEnemySlain = currentTowerPerformance.enemySlain;
            float rationalizedSlainEnemy = towerEnemySlain / totalEnemy;
            Debug.Log($"Rationalized: {towerEnemySlain}/{totalEnemy}={rationalizedSlainEnemy}");
            float currentTowerScore = towerDamageDealt * rationalizedSlainEnemy;
            currentTowerPerformance.currentScore += currentTowerScore;
            Debug.Log($"Checking Score: tower {tower.TowerData.TowerName} with score of: {currentTowerPerformance.currentScore}");
            
            //Reset Stats:
            currentTowerPerformance.enemySlain = 0;
            currentTowerPerformance.damageDealt = 0;
        }
    }
    private void EvaluateDifferenceRound()
    {
        var RoundPerformances = GameplayManager.instance.RoundPerformances;
        int currentWaveIndex = GameplayManager.instance.CurrentWaveIndex;
        int previousWaveIndex = currentWaveIndex - 1;
        if(previousWaveIndex < 1)
        {
            return;
        }

        var previous1 = RoundPerformances[previousWaveIndex];
        var previous2 = RoundPerformances[previousWaveIndex - 1];

        DifferenceHealth = Mathf.Abs(previous2.RemainingHealth - previous1.RemainingHealth);
        DifferenceDamageDealt = Mathf.Abs((previous2.EnemyTotalHealth - previous2.EnemyRemainingHealth) - (previous1.EnemyTotalHealth - previous1.EnemyRemainingHealth)) ;
        DifferentEnemySlain = Mathf.Abs(previous2.RemainingEnemy- previous1.RemainingEnemy);

    }
}

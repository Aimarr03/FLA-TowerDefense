using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static partial class TD_API
{
    public static EconomyManager Economy { get; internal set; }
    public static List<TowerActionSO> TowerActions { get; internal set; }
    public static List<TowerActionSO> BuildActions{ get; internal set; }
    public static Dictionary<EnemyType,EnemyData> EnemyDatas { get; internal set; }
}
public class GameplayManager : MonoBehaviour
{
    public enum ScenarioMode
    {
        DDA,
        Static
    }
    public static GameplayManager instance;
    [Header("Main Componene")]
    [SerializeField] private BotController bot;
    [SerializeField] private FLA fla;
    [SerializeField] private ConfigLoader configLoader;
    [SerializeField] private SelectionManager selectionManager;

    [Header("Enemy Spawner")]
    [SerializeField] private EnemySpawnLoader enemyLoader;
    [SerializeField] private EnemySpawner enemySpawner; 

    [Space(25)]
    [SerializeField] private List<TowerActionSO> towerActions;
    [SerializeField] private List<BuyAction> buildData;
    [SerializeField] private List<EnemyData> enemyData;

    [Space(25)]
    [SerializeField] private NavMeshSurface Surface2D;
    [SerializeField] private MainBase mainBase;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private float BaseBuildStateDuration = 60;

    [Space(25)]
    [SerializeField] private Canvas MainMenu;
    [SerializeField] private Image readyButton;
    [SerializeField] private bool instantPlay = true;
    [SerializeField] private SpawnType spawnType = SpawnType.Subwave;
    [SerializeField] private ScenarioMode scenarioMode = ScenarioMode.Static;

    [Space(25)]
    [SerializeField] private int initHealth = 30;
    
    private RoundPerformance currentRoundPerformance;
    private float buildPhaseDuration;
    private bool isActive = false;
    private int rewardQuestionAnswered = 0;
    private int maxWave = 0;
    private List<Tower> allTower;

    [Header("Enemy Wave")]
    public List<EnemySpawnInfo> enemySpawnInfos;
    private List<RoundPerformance> roundPerformances;
    public int MaxWave => maxWave;

    public GameConfig GameConfiguration { get; private set;}
    public State GameState { get; private set; }
    public int CurrentWaveIndex { get; private set; }
    public int CurrentWave => CurrentWaveIndex + 1;
    public Vector3 DestinationPos { get; private set; }
    public float CurrentBuildPhaseDuration { get; private set; }
    public MainBase MainBase => mainBase;
    public bool IsActive => isActive;
    public float MultiplierGold { get; private set; }
    public float MultiplierHP {get; private set;}
    public float MultiplierSpawnEnemy {get; private set;}
    public SpawnType SpawnMode => spawnType;
    public List<RoundPerformance> RoundPerformances => roundPerformances;
    public List<FLA_Metrics> FLA_Metrics = new();
    public enum State
    {
        Building,
        Defending,
        Win,
        GameOver,
    }
    public enum SpawnType
    {
        Number,
        Percentage,
        Subwave,
    }

    public Action<State> onchangedState;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            fla = GetComponent<FLA>();
            configLoader = GetComponent<ConfigLoader>();

            MultiplierGold = 1f;
            MultiplierSpawnEnemy = 1f;
            MultiplierHP = 1f;
            
            GameConfiguration = configLoader.gameConfig;
            
            allTower = FindObjectsByType<Tower>(FindObjectsSortMode.None).ToList();
        }
    }
    public void InitializeGame(string fileName)
    {
        InitializedConfig(fileName);
        InitializedAPI();
        InitializedWave();
        InitializedBot();

        mainBase.Setup(initHealth);
        fla.Setup(initHealth);
    }

    private void InitializedBot()
    {
        var botProperty = configLoader.gameConfig.botUsage;
        if (botProperty.useBot)
        {
            bot.Init(botProperty);
        }
        if (bot.IsActive)
        {
            selectionManager.DisableSelectionManager();
        }
    }

    private void InitializedConfig(string filename)
    {
        configLoader.LoadGameConfig(filename);
        GameConfiguration = configLoader.gameConfig;
        
        configLoader.TryParseScenarioMode(GameConfiguration.scenarioMode, out scenarioMode);
        configLoader.TryParseSpawnType(GameConfiguration.spawnMode, out spawnType);
        
        economyManager = new EconomyManager(GameConfiguration.startingMoney);
        BaseBuildStateDuration = GameConfiguration.baseDuration;
    }
    private void OnDestroy()
    {
        Tower.ActionInvoke -= TowerActionInvoke;
    }
    private void Update()
    {
        if (!isActive) return;
        switch (GameState)
        {
            case State.Building:
                CurrentBuildPhaseDuration -= Time.deltaTime;
                if (CurrentBuildPhaseDuration <= 0)
                {
                    StartDefending();
                }
                break;
        }
    }
    public void StartGame()
    {
        if(isActive) return;
        isActive = true;
        CurrentBuildPhaseDuration = BaseBuildStateDuration;
        buildPhaseDuration = CurrentBuildPhaseDuration;

        mainBase.OnDeath += GameOver;
        DestinationPos = mainBase.transform.position;

        Tower.ActionInvoke += TowerActionInvoke;
        MainMenu.gameObject.SetActive(false);
        ChangeState(State.Building);
        
        IEnumerator DelayUpdateEconomy()
        {
            yield return new WaitForSeconds(1f);
            economyManager.GainMoney(0);
        }
        StartCoroutine(DelayUpdateEconomy());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    private void InitializedWave()
    {
        enemyLoader.TryLoadData(configLoader.gameConfig.filePath ,spawnType);
        maxWave = enemyLoader.MaxWaveCount;
        
        roundPerformances = new();
        FLA_Metrics = new();
        for(int i = 0; i < maxWave; i++)
        {
            roundPerformances.Add(new RoundPerformance());
            FLA_Metrics.Add(new FLA_Metrics()
            {
                currentWave = i + 1,
                botType = GameConfiguration.botUsage.capability,
                scenarioMode = this.scenarioMode
            });
        }
        
        CurrentWaveIndex = 0;
        int index = Mathf.Clamp(CurrentWaveIndex, 0, maxWave - 1);
        currentRoundPerformance = roundPerformances[index];
        
        UpdateWave();
    }
    private void UpdateWave()
    {
        enemySpawner.GetEnemyWave(); 
        enemySpawnInfos = enemySpawner.EnemySpawnInfos;
    }
    private void InitializedAPI()
    {
        TD_API.Economy = economyManager;
        TD_API.TowerActions = towerActions;
        TD_API.BuildActions = new();
        foreach (var build in buildData)
        {
            TD_API.BuildActions.Add(build);
        }

        TD_API.EnemyDatas = new();
        foreach (var enemy in enemyData)
        {
            Debug.Log($"Add {enemy.enemyType}");
            TD_API.EnemyDatas.Add(enemy.enemyType, enemy);
        }
    }
    private void ChangeState(State newState)
    {
        GameState = newState;
        readyButton.gameObject.SetActive(GameState == State.Building);
        onchangedState?.Invoke(GameState);
    }
    private void TowerActionInvoke(Tower towerAffected, TowerActionType actionType)
    {
        var actionMetric = currentRoundPerformance.ActionMetric;
        switch (actionType)
        {
            case TowerActionType.Buy:
                if(GameState == State.Building)
                {
                    actionMetric.BuildPhase_BuyAction++;
                }
                else if(GameState == State.Defending)
                {
                    actionMetric.DefendPhase_BuyAction++;
                }
                break;
            case TowerActionType.Sell:
                if (GameState == State.Building)
                {
                    actionMetric.BuildPhase_SellAction++;
                }
                else if (GameState == State.Defending)
                {
                    actionMetric.DefendPhase_SellAction++;
                }
                break;
            case TowerActionType.Upgrade:
                if (GameState == State.Building)
                {
                    actionMetric.BuildPhase_UpgradeAction++;
                }
                else if (GameState == State.Defending)
                {
                    actionMetric.DefendPhase_UpgradeAction++;
                }
                break;
        }
    }
    public void GetActCount(out int totalBuild, out int totalUpgrade, out int totalSell)
    {
        totalSell = 0;
        totalBuild = 0;
        totalUpgrade = 0;
        for(int index = 0; index < CurrentWave; index++)
        {
            var currentRound = roundPerformances[index];
            var actionMetric = currentRound.ActionMetric;

            totalBuild += actionMetric.BuildPhase_BuyAction + actionMetric.DefendPhase_BuyAction;
            totalSell += actionMetric.BuildPhase_SellAction + actionMetric.DefendPhase_SellAction;
            totalUpgrade += actionMetric.BuildPhase_UpgradeAction + actionMetric.DefendPhase_UpgradeAction;
        }
    }
    public void GetTotalEnemy(out int totalEnemyDamaged, out int totalEnemySlained, out int totalEnemyEscape, out int totalEnemyCount)
    {
        totalEnemyDamaged = 0;
        totalEnemySlained = 0;
        totalEnemyEscape = 0;
        totalEnemyCount = 0;
        for(int index = 0; index < CurrentWave; index++)
        {
            var currentRound = roundPerformances[index];
            totalEnemyDamaged += (int)(currentRound.EnemyTotalHealth - currentRound.EnemyRemainingHealth);
            totalEnemySlained += currentRound.EnemyKilledCount;
            totalEnemyEscape += currentRound.EnemyEscapedCount;
            totalEnemyCount += currentRound.EnemyTotalCount;
        }
    }
    public void GetScenarioData(out ScenarioMode scenarioMode, out BotController.Capability botType)
    {
        scenarioMode = this.scenarioMode;
        botType = GameConfiguration.botUsage.capability;
    }
    public void GetRemainingHealth(out int remainingHealth)
    {
        remainingHealth = 0;
        var currentRoundPerformance = roundPerformances[CurrentWaveIndex];
        remainingHealth = currentRoundPerformance.RemainingHealth;
    }
    private void GameOver()
    {
        if(GameState == State.GameOver)
        {
            return;
        }
        IEnumerator GameOverSequence()
        {
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(2);
            Time.timeScale = 1;
            isActive = false;
            Debug.Log("Game Over!");
            isActive = false;
            ChangeState(State.GameOver);
        }
        
        StartCoroutine(GameOverSequence());
    }
    public void DefendsOver()
    {
        bool condition = GameState switch
        {
            State.Defending => true,
            _ => false
        };
        if (!condition) return;
        CalculatePerformance();

        buildPhaseDuration = CurrentBuildPhaseDuration;
        int bufferIndex = CurrentWaveIndex + 1;
        IEnumerator FreezeWin()
        {
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(2);
            Time.timeScale = 1;
            isActive = false;
            ChangeState(State.Win);
        }
        if (bufferIndex >= maxWave)
        {
            StartCoroutine(FreezeWin());
        }
        else
        {
            CurrentWaveIndex++;
            int index = Mathf.Clamp(CurrentWaveIndex, 0, maxWave - 1);
            currentRoundPerformance = roundPerformances[index];
            UpdateWave();
            ChangeState(State.Building);

            /// This is because it use arithmetic or  problem posing for getting currency, 
            /// now it is on hold and need further discussion for whether it is needed to be kept or not.
            /// If so, need to know which approach is better
            //if (currentWaveIndex > 0)
            //{
            //    float randomValue = Random.value;
            //    Debug.Log("Random Value: " + randomValue);
            //    int totalReward = 100 + (50 * currentWaveIndex);
            //    if (randomValue > 0.5f)
            //    {
            //        int totalQuestion = 6 + currentWaveIndex;
            //        rewardQuestionAnswered = totalReward / totalQuestion;
            //        arithmeticGeneration.GenerateProblem(totalQuestion);
            //    }
            //    else
            //    {
            //        rewardQuestionAnswered = totalReward;
            //        problemPosingGenerator.GenerateProblem();
            //    }
            //}
        }
    }
    public void StartDefending()
    {
        bool condition = GameState switch
        {
            State.Building => true,
            _ => false
        };

        if (!condition) return;
        
        currentRoundPerformance.BuildPhaseDuration = buildPhaseDuration;
        currentRoundPerformance.RemainingbuildPhaseDuration = CurrentBuildPhaseDuration;
        
        ChangeState(State.Defending);
        CurrentBuildPhaseDuration = 0;
    }
    private void CalculatePerformance()
    {
        int enemyEscapeCount = (int)enemySpawner.CurrentRoundEnemyEscaped;
        int enemySlain = (int)enemySpawner.CurrentRoundEnemySlained;
        Debug.Log($"[Debug Log Gameplay] escaped ${enemyEscapeCount}, Killed {enemySlain}");
        currentRoundPerformance.EnemyEscapedCount = enemyEscapeCount;
        currentRoundPerformance.EnemyKilledCount = enemySlain;
        currentRoundPerformance.EnemyTotalCount = (int)enemySpawner.CurrentRoundEnemyCount;

        
        currentRoundPerformance.EnemyRemainingHealth = enemySpawner.EnemyRemainingHealth;
        currentRoundPerformance.EnemyTotalHealth = enemySpawner.EnemyTotalHealth;
        currentRoundPerformance.RemainingGold = economyManager.CurrentMoney;
        currentRoundPerformance.RoundIndex = CurrentWaveIndex;
        currentRoundPerformance.RemainingHealth = (int)mainBase.CurrentHealth;
        currentRoundPerformance.AttackNumber = CurrentWave;
        


        #region Obsolete
        // var towerEvaluation = currentRoundPerformance.towerEvaluation;
        // towerEvaluation.currentRound = CurrentWave;
        
        // foreach(var tower in allTower)
        // {
        //     TowerGeneralPerformance towerPerformance = new();
        //     if(tower.CurrentState == Tower.State.None)
        //     {
        //         towerPerformance.towerName = "None";
        //         towerPerformance.damageDealt = -1;
        //         towerPerformance.enemySlain = -1;
        //     }
        //     else if(tower.CurrentState == Tower.State.Built)
        //     {
        //         var towerName = tower.TowerData.TowerName;
        //         towerPerformance.towerName = towerName;
        //         towerPerformance.damageDealt = (int) tower.performance.damageDealt;
        //         towerPerformance.enemySlain = (int) tower.performance.enemySlain;
        //         if(towerName == "Archer Tower")
        //         {
        //             towerEvaluation.archerCount++;   
        //         }
        //         else if(towerName == "Mortar Tower")
        //         {
        //             towerEvaluation.mortarCount++;
        //         }
        //         else if(towerName == "Mage Tower")
        //         {
        //             towerEvaluation.mageCount++;
        //         }
        //     }
        //     towerEvaluation.towerPerformance.Add(towerPerformance);
        // }
        #endregion


        Debug.Log($"No. Current Wave Clear: {CurrentWave}");
        if (CurrentWave > 1)
        {
            RoundPerformance previousRoundPerformance = roundPerformances[Mathf.Clamp(CurrentWaveIndex - 1, 0, maxWave - 1)];
            if (previousRoundPerformance != null)
            {
                if(scenarioMode == ScenarioMode.DDA)
                {
                    fla.UpdateFLA(currentRoundPerformance, previousRoundPerformance);    
                }
            }
        }
        else if (CurrentWave == 1)
        {
            RoundPerformance initPerformance = new RoundPerformance
            {
                RemainingHealth = initHealth,
            };
            if(scenarioMode == ScenarioMode.DDA)
            {
                fla.UpdateFLA(currentRoundPerformance, initPerformance);
            }
            
        }

        if(scenarioMode == ScenarioMode.DDA)
        {
            var fla_metric = FLA_Metrics[CurrentWaveIndex];
            fla_metric.currentWave = CurrentWaveIndex+1;
            
            MultiplierGold = FLA.FinalMultiplierGold;
            MultiplierSpawnEnemy = FLA.FinalMultiplierSpawnEnemy;
            MultiplierHP = FLA.FinalMultiplierHP;    
            
            fla_metric.HPMult = MultiplierHP;
            fla_metric.goldMult = MultiplierGold;
            fla_metric.spawnMult = MultiplierSpawnEnemy;
        }
        else
        {
            var fla_metric = FLA_Metrics[CurrentWaveIndex];
            fla_metric.currentWave = CurrentWaveIndex+1;
            fla_metric.HPMult = MultiplierHP;
            fla_metric.goldMult = MultiplierGold;
            fla_metric.spawnMult = MultiplierSpawnEnemy;
        }
    }
}
[Serializable]
public class RoundPerformance
{
    public int RoundIndex;
    public float EnemyTotalHealth;
    public float EnemyRemainingHealth;

    public float BuildPhaseDuration;
    public float RemainingbuildPhaseDuration;
    public int EnemyTotalCount;
    public int EnemyKilledCount;
    public int EnemyEscapedCount;
    public int RemainingHealth;
    public int AttackNumber;
    public int RemainingGold;
    //public TowerEvaluation towerEvaluation = new();
    public ActionMetrics ActionMetric = new();
    public float normalizedEnemyHP => EnemyRemainingHealth / EnemyTotalHealth;
    public int TotalEnemy => EnemyKilledCount + EnemyEscapedCount;
}
[Serializable]
public class ActionMetrics
{
    public int BuildPhase_SellAction;
    public int BuildPhase_BuyAction;
    public int BuildPhase_UpgradeAction;

    public int DefendPhase_SellAction;
    public int DefendPhase_BuyAction;
    public int DefendPhase_UpgradeAction;
}
[Serializable]
public class FLA_Metrics
{
    public int currentWave = 0;
    public BotController.Capability botType;
    public GameplayManager.ScenarioMode scenarioMode;
    public float HPMult = 0;
    public float spawnMult = 0;
    public float goldMult = 0;
}
// [Serializable]
// public class TowerEvaluation
// {
//     public int mortarCount = 0;
//     public int archerCount = 0;
//     public int mageCount = 0;
//     public int currentRound = 0;
//     public List<TowerGeneralPerformance> towerPerformance = new();
// }

// [Serializable]
// public class TowerGeneralPerformance
// {
//     public string towerName;
    
//     public int damageDealt;
//     public int enemySlain;

// }
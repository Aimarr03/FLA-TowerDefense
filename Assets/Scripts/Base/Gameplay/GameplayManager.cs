using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
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

    [Space(25)]
    [SerializeField] private int initHealth = 30;
    
    private RoundPerformance currentRoundPerformance;
    private float buildPhaseDuration;
    private bool isActive = false;
    private int rewardQuestionAnswered = 0;
    private int maxWave = 0;
    
    [Header("Enemy Wave")]
    public List<EnemySpawnInfo> enemySpawnInfos;
    private List<RoundPerformance> roundPerformances;
    public int MaxWave => maxWave;


    public State GameState { get; private set; }
    public int CurrentWaveIndex { get; private set; }
    public int CurrentWave => CurrentWaveIndex + 1;
    public Vector3 DestinationPos { get; private set; }
    public float CurrentBuildPhaseDuration { get; private set; }
    public MainBase MainBase => mainBase;
    public bool IsActive => isActive;
    public float MultiplierSpeed { get; private set; }
    public float MultiplierGold { get; private set; }
    public float MultiplierDuration { get; private set; }
    public SpawnType SpawnMode => spawnType;
    
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

            MultiplierDuration = 1f;
            MultiplierGold = 1f;
            MultiplierSpeed = 1f;
            
            LoadConfig();
            
            mainBase.Setup(initHealth);
            fla.Setup(initHealth);

            bot.Init();
            if (bot.IsActive)
            {
                selectionManager.DisableSelectionManager();
            }
            if (instantPlay)
            {
                StartGame();
            }
        }
    }
    private void LoadConfig()
    {
        if(configLoader == null)
        {
            configLoader = GetComponent<ConfigLoader>();
            if(configLoader == null)
            {
                Debug.LogError("Config Loader is not found, try again!");
                return;
            }
        }
        configLoader.Init();

        var config = configLoader.gameConfig;
        configLoader.TryParseSpawnType(config.spawnMode, out spawnType);
        
        economyManager = new EconomyManager(config.startingMoney);
        BaseBuildStateDuration = config.baseDuration;

        InitializedAPI();
        InitializedWave();
    }
    private void OnDestroy()
    {
        TowerActionSO.ActionInvoke -= TowerActionInvoke;
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

        TowerActionSO.ActionInvoke += TowerActionInvoke;
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
        for(int i = 0; i < maxWave; i++)
            roundPerformances.Add(new RoundPerformance());
        
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
    private void TowerActionInvoke(TowerActionType actionType)
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
    private void GameOver()
    {
        Debug.Log("Game Over!");
        isActive = false;
        ChangeState(State.GameOver);
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

        CurrentWaveIndex++;
        CurrentBuildPhaseDuration = BaseBuildStateDuration * MultiplierDuration;
        buildPhaseDuration = CurrentBuildPhaseDuration;

        if (CurrentWave > maxWave)
        {
            isActive = false;
            ChangeState(State.Win);
        }
        else
        {
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
        currentRoundPerformance.TotalEnemy = enemySpawner.TotalEnemy;
        currentRoundPerformance.RemainingEnemy = enemySpawner.EnemyReachDestination;
        currentRoundPerformance.EnemyRemainingHealth = enemySpawner.EnemyRemainingHealth;
        currentRoundPerformance.EnemyTotalHealth = enemySpawner.EnemyTotalHealth;

        currentRoundPerformance.RemainingHealth = (int)mainBase.CurrentHealth;
        currentRoundPerformance.AttackNumber = CurrentWave;


        Debug.Log($"No. Current Wave Clear: {CurrentWave}");
        if (CurrentWave > 1)
        {
            RoundPerformance previousRoundPerformance = roundPerformances[Mathf.Clamp(CurrentWaveIndex - 1, 0, maxWave - 1)];
            if (previousRoundPerformance != null)
            {
                fla.UpdateFLA(currentRoundPerformance, previousRoundPerformance);
            }
        }
        else if (CurrentWave == 1)
        {
            RoundPerformance initPerformance = new RoundPerformance
            {
                RemainingHealth = initHealth,
            };
            fla.UpdateFLA(currentRoundPerformance, initPerformance);
        }

        MultiplierGold = fla.FinalMultiplierGold;
        MultiplierSpeed = fla.FinalMultiplierSpeed;
        MultiplierDuration = fla.FinalMultiplierDuration;
    }
}
[Serializable]
public class RoundPerformance
{
    public float EnemyTotalHealth;
    public float EnemyRemainingHealth;

    public float BuildPhaseDuration;
    public float RemainingbuildPhaseDuration;

    public int TotalEnemy;
    public int RemainingEnemy;
    public int RemainingHealth;
    public int AttackNumber;

    public ActionMetrics ActionMetric = new();
    public float normalizedEnemyCount => RemainingEnemy / TotalEnemy;
    public float normalizedDuration => RemainingbuildPhaseDuration / BuildPhaseDuration;
    public float normalizedEnemyHP => EnemyRemainingHealth / EnemyTotalHealth;
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
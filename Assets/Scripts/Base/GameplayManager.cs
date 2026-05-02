using NavMeshPlus.Components;
using System;
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
    [SerializeField] private ArithmeticGeneration arithmeticGeneration;
    [SerializeField] private ProblemPosingGenerator problemPosingGenerator;
    [SerializeField] private EnemySpawnLoader enemyLoader;

    [Space(25)]
    [SerializeField] private List<TowerActionSO> towerActions;
    [SerializeField] private List<BuyAction> buildData;
    [SerializeField] private List<EnemyData> enemyData;

    [Space(25)]
    [SerializeField] private NavMeshSurface Surface2D;
    [SerializeField] private MainBase mainBase;
    [SerializeField] private EnemySpawner spawner; 
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private float BaseBuildStateDuration = 60;

    [Space(25)]
    [SerializeField] private Canvas MainMenu;
    [SerializeField] private Image readyButton;
    [SerializeField] private bool instantPlay = true;
    [SerializeField] private bool numberBase = false;

    private RoundPerformance currentRoundPerformance;
    private float buildPhaseDuration;
    private bool isActive = false;
    private int rewardQuestionAnswered = 0;
    
    [Header("Enemy Wave")]
    public EnemyWave currentEnemyWave;
    private List<EnemyWave> enemyWaves;

    private List<RoundPerformance> roundPerformances;

    public State GameState { get; private set; }
    public int currentWaveIndex { get; private set; }
    public int currentWave => currentWaveIndex + 1;
    public int maxWave => enemyWaves.Count;
    public Vector3 DestinationPos { get; private set; }
    public float currentBuildPhaseDuration { get; private set; }
    public MainBase MainBase => mainBase;
    public bool IsActive => isActive;
    public enum State
    {
        Building,
        Defending,
        Win,
        GameOver,
    }

    public Action<State> onchangedState;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            InitializedAPI();
            InitializedWave();
            if (instantPlay)
            {
                StartGame();
            }
        }
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
                currentBuildPhaseDuration -= Time.deltaTime;
                if (currentBuildPhaseDuration <= 0)
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
        currentBuildPhaseDuration = BaseBuildStateDuration;
        buildPhaseDuration = currentBuildPhaseDuration;

        mainBase.OnDeath += GameOver;
        DestinationPos = mainBase.transform.position;

        TowerActionSO.ActionInvoke += TowerActionInvoke;
        MainMenu.gameObject.SetActive(false);
        ChangeState(State.Building);

        if(problemPosingGenerator != null)
        {
            problemPosingGenerator.OnAnsweredQuestion += (bool isCorrect) =>
            {
                if (isCorrect)
                {
                    economyManager.GainMoney(rewardQuestionAnswered);
                }
            };
        }
        if (arithmeticGeneration != null)
        {
            arithmeticGeneration.OnCorrectAnswer += () =>
            {
                economyManager.GainMoney(rewardQuestionAnswered);
            };
        }
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
        enemyLoader.LoadData();   
        enemyWaves = numberBase ? enemyLoader.enemyWaves : enemyLoader.randomizedEnemyWaves;
        roundPerformances = new();
        foreach (var wave in enemyWaves)
        {
            roundPerformances.Add(new RoundPerformance());
        }
        
        currentWaveIndex = 0;
        int index = Mathf.Clamp(currentWaveIndex, 0, enemyWaves.Count - 1);
        
        currentEnemyWave = enemyWaves[index];
        currentRoundPerformance = roundPerformances[index];
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
        
        currentRoundPerformance.TotalEnemy = spawner.TotalEnemy;
        currentRoundPerformance.RemainingEnemy = spawner.EnemyReachDestination;
        currentRoundPerformance.EnemyRemainingHealth = spawner.EnemyRemainingHealth;
        currentRoundPerformance.EnemyTotalHealth = spawner.EnemyTotalHealth;
        
        currentRoundPerformance.RemainingHealth = (int)mainBase.CurrentHealth;
        currentRoundPerformance.AttackNumber = currentWave;

        currentWaveIndex++;
        currentBuildPhaseDuration = BaseBuildStateDuration;
        buildPhaseDuration = currentBuildPhaseDuration;
        if (currentWave > enemyWaves.Count)
        {
            isActive = false;
            ChangeState(State.Win);
        }
        else
        {
            int index = Mathf.Clamp(currentWaveIndex, 0, enemyWaves.Count - 1);
            currentEnemyWave = enemyWaves[index];
            currentRoundPerformance = roundPerformances[index];
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
        ChangeState(State.Defending);
        currentBuildPhaseDuration = 0;

        
        currentRoundPerformance.BuildPhaseDuration = buildPhaseDuration;
        currentRoundPerformance.RemainingbuildPhaseDuration = currentBuildPhaseDuration;
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
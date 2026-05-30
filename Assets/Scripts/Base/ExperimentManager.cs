using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField] private string rootDirName = "Pilot Test #1";
    [SerializeField] private int IterationPerScenario = 30;
    [SerializeField, Range(5,30)] private float multiplierSpeed = 5;
    private int currentIteration = 0;
    public static ExperimentManager instance;
    private List<MatchLog> generalLogs;
    
    private ExperimentPreset currentPreset;
    List<ExperimentPreset> presets;
    string fileName ="";
    string specificIterationDirPath;
    int iterationPreset;
    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        currentIteration = 0;
        generalLogs = new();
        presets = new()
        {
            new(false, BotController.Capability.Low),
            new(false, BotController.Capability.Medium),
            new(false, BotController.Capability.High),

            new(true, BotController.Capability.Low),
            new(true, BotController.Capability.Medium),
            new(true, BotController.Capability.High),
        };
        currentPreset = presets[0];
        UpdateFileName();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameplayManager.instance.onchangedState += OnGameplayStateChange;
        Time.timeScale = multiplierSpeed;
        GameplayManager.instance.InitializeGame(fileName);
        GameplayManager.instance.StartGame();
    }

    private void OnGameplayStateChange(GameplayManager.State state)
    {
        IEnumerator ReloadScene()
        {
            ExportFLALog();
            yield return new WaitForSeconds(3);
            SceneManager.LoadSceneAsync("Prototipe_main");
        }
        bool condition = state switch
        {
            GameplayManager.State.Win => true,
            GameplayManager.State.GameOver => true,
            _ => false
        };
        if (condition)
        {
            CreateMatchLog();

            currentIteration++;
            if(currentIteration < IterationPerScenario)
            {
                StartCoroutine(ReloadScene());
            }
            else
            {
                iterationPreset++;
                if(iterationPreset < presets.Count)
                {
                    ExportAll();
                    
                    currentPreset = presets[iterationPreset];
                    UpdateFileName();

                    currentIteration = 0;
                    generalLogs.Clear();
                    StartCoroutine(ReloadScene());
                }
                else
                {
                    StartCoroutine(ExitPlayMode());        
                }
                
            }
        }
    }

    private void UpdateFileName()
    {
        StringBuilder sb = new();
        string useDDA = currentPreset.useDDA ? "DDA" : "Static";
        sb.Append(useDDA);
        sb.Append("_");
        sb.Append(currentPreset.botType.ToString().ToLower());
        fileName = sb.ToString();
        Debug.Log($"File Name: {fileName}");
    }
    IEnumerator ExitPlayMode()
    {
        ExportAll();
        yield return new WaitForSeconds(5f);
        EditorApplication.ExitPlaymode();
    }
    private void ExportAll()
    {
        string directoryPath = Application.dataPath + "/Experiment Log";
        specificIterationDirPath = directoryPath + $"/{rootDirName}/{fileName}";
        
        if(!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        if (!Directory.Exists(specificIterationDirPath))
        {
            Directory.CreateDirectory(specificIterationDirPath);
        }

        ExportGeneralCSV();
        ExportEnemyCSV();
        ExportTowerCSV();
        ExportHealthMetric();
        ExportFLAMetric();
        ExportFLALog();
    }

    private void UpdateEnemyLog(MatchLog log)
    {
        var EnemySpawner = FindFirstObjectByType<EnemySpawner>();
        var enemyPerformances = EnemySpawner.enemyPerformances;
        var enemyHealthMetric = EnemySpawner.enemyHealthMetrics;

        foreach(var key in enemyPerformances.Keys)
        {
            var performance = enemyPerformances[key];
            var healthMetric = enemyHealthMetric[key];

            float minHealth = -1;
            float maxHealth = -1;
            float avgHealth = -1;
            if (healthMetric.Any())
            {
                minHealth = healthMetric.Min();
                maxHealth = healthMetric.Max();
                avgHealth = healthMetric.Average();    
            }
            
            performance.avgHealth = avgHealth;
            performance.minHealth = minHealth;
            performance.maxHealth = maxHealth;
            
            performance.enemyType = key;

            log.enemyPerformances.Add(performance);
        }
    }
    private void UpdateTowerLog(MatchLog log)
    {
        var TowerLogController = FindFirstObjectByType<TowerLogController>();
        TowerLogController.FinalisedRawData();

        var towerLogs = TowerLogController.towerLogs;
        foreach(var key in towerLogs.Keys)
        {
            var towerLog = towerLogs[key];
            log.towerLogs.Add(towerLog);
        }
    }
    private void UpdateHealthMetric(MatchLog log)
    {
        var RoundPerformances = GameplayManager.instance.RoundPerformances;
        foreach(var round in RoundPerformances)
        {
            log.healthMetrics.remainingHealth.Add(round.RemainingHealth);
        }
    }
    private void UpdateFLAMetric(MatchLog log)
    {
        foreach(var metric in GameplayManager.instance.FLA_Metrics)
        {
            log.fla_metrics.Add(new FLA_Metrics()
            {
                currentWave = metric.currentWave,
                botType = metric.botType,
                scenarioMode = metric.scenarioMode,
                goldMult = metric.goldMult,
                spawnMult = metric.spawnMult,
                HPMult = metric.HPMult,
            });
        }
    } 
    private void CreateMatchLog()
    {
        var matchLog = new MatchLog();
        
        matchLog.experimentIndex = currentIteration;
        matchLog.win = GameplayManager.instance.GameState switch
        {
            GameplayManager.State.Win => true,
            _  => false
        };
        
        matchLog.waveReached = GameplayManager.instance.CurrentWave;
        
        GameplayManager.instance.GetActCount(out int totalBuild, out int totalUpgrade, out int totalSell);
        matchLog.buildCount = totalBuild;
        matchLog.upgradeCount = totalUpgrade;
        matchLog.sellCount = totalSell;

        GameplayManager.instance.GetTotalEnemy(out int totalEnemyDamaged, out int totalEnemySlained, out int totalEnemyEscaped, out int totalEnemyCount);
        matchLog.totalEnemyEscape = totalEnemyEscaped;
        matchLog.totalEnemySlain = totalEnemySlained;
        matchLog.totalEnemyCount = totalEnemyCount;
        Debug.Log($"[Debug Log Experiment] escaped ${totalEnemyEscaped}, Killed {totalEnemySlained}, Total {totalEnemyCount}");
        
        GameplayManager.instance.GetScenarioData(out GameplayManager.ScenarioMode scenarioMode, out BotController.Capability botType);
        matchLog.botType = botType;
        matchLog.scenarioMode = scenarioMode;
        
        int remainingHealth = 0;
        GameplayManager.instance.GetRemainingHealth(out remainingHealth );
        matchLog.remainingHealth = remainingHealth;

        matchLog.totalDamage = totalEnemyDamaged;
        matchLog.enemyPerformances = new();
        matchLog.towerLogs = new();
        matchLog.healthMetrics = new();
        matchLog.fla_metrics = new();

        UpdateEnemyLog(matchLog);
        UpdateTowerLog(matchLog);
        UpdateHealthMetric(matchLog);
        UpdateFLAMetric(matchLog);
        generalLogs.Add(matchLog);
    }
    public void ExportGeneralCSV()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "Experiment,ScenarioMode,BotType,Win,Wave,RemainingHealth,Build,Upgrade,Sell,Damage,EnemyEscape,EnemySlain,TotalEnemy");

        foreach(var log in generalLogs)
        {
            sb.AppendLine(
                $"{log.experimentIndex}," +
                $"{log.scenarioMode}," +
                $"{log.botType}," +
                $"{log.win}," +
                $"{log.waveReached}," +
                $"{log.remainingHealth}," +
                $"{log.buildCount}," +
                $"{log.upgradeCount}," +
                $"{log.sellCount}," +
                $"{log.totalDamage}," +
                $"{log.totalEnemyEscape}," +
                $"{log.totalEnemySlain}," +
                $"{log.totalEnemyCount}");
        }

        string path =
            specificIterationDirPath + "/match_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    private void ExportEnemyCSV()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(
        "Experiment,ScenarioMode,BotType,EnemyType,Spawned,Killed,Escaped,Average");
        foreach(var match in generalLogs)
        {
            foreach(var enemy in match.enemyPerformances)
            {
                sb.AppendLine(
                $"{match.experimentIndex}," +
                $"{match.scenarioMode}," +
                $"{match.botType}," +
                $"{enemy.enemyType}," +
                $"{enemy.spawnedCount}," +
                $"{enemy.killedCount}," +
                $"{enemy.escapedCount},"+
                $"{enemy.avgHealth:F0}");
            }
        }
        string path =
        specificIterationDirPath + "/enemy_log.csv";
        File.WriteAllText(path, sb.ToString());
    }
    public void ExportTowerCSV()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(
            "Experiment,ScenarioMode,BotType,TowerType,Built,Upgrade,Sold,Damage,EnemySlain,Average Score");

        foreach(var match in generalLogs)
        {
            foreach(var tower in match.towerLogs)
            {
                sb.AppendLine(
                $"{match.experimentIndex}," +
                $"{match.scenarioMode}," +
                $"{match.botType}," +
                $"{tower.towerType}," +
                $"{tower.builtTotal}," +
                $"{tower.upgradeTotal}," +
                $"{tower.sellTotal},"+
                $"{tower.totalDamage},"+
                $"{tower.totalKill},"+
                $"{tower.averageScore:F2},"
                );
            }
        }

        string path =
            specificIterationDirPath + "/tower_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    public void ExportHealthMetric()
    {
        StringBuilder sb = new StringBuilder();
        int maxWave = generalLogs[0].healthMetrics.remainingHealth.Count;        
        sb.Append("Experiment,ScenarioMode,BotType,");
        for(int i = 0; i < maxWave; i++)
        {
            sb.Append($"Wave {i + 1:D2}");

            if(i < maxWave - 1)
                sb.Append(",");
        }
        sb.AppendLine();
        foreach(var match in generalLogs)
        {
            sb.Append
            (
                $"{match.experimentIndex},"+
                $"{match.scenarioMode},"+
                $"{match.botType},"
            );
            var healthMetric = match.healthMetrics.remainingHealth;
            bool flagZero = false;
            for(int i = 0; i < healthMetric.Count; i++)
            {
                int health = healthMetric[i];
                if (flagZero)
                {
                    health = -1;
                }
                if(!flagZero && health <= 0)
                {
                    flagZero = true;
                }
                sb.Append($"{health}");    
                

                if(i < maxWave - 1)
                    sb.Append(",");
            }
            sb.AppendLine();
        }

        string path =
            specificIterationDirPath + "/health_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    public void ExportFLAMetric()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(
            "Experiment,ScenarioMode,BotType,Current Wave,HPMult,SpawnMult,GoldMult");

        foreach(var match in generalLogs)
        {
            foreach(var fla in match.fla_metrics)
            {
                sb.AppendLine(
                $"{match.experimentIndex}," +
                $"{match.scenarioMode}," +
                $"{match.botType}," +
                $"{fla.currentWave}," +
                $"{fla.HPMult},"+
                $"{fla.spawnMult}," +
                $"{fla.goldMult},"
                );
            }
        }

        string path =
            specificIterationDirPath + "/fla_metric.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    private void ExportFLALog()
    {
        StringBuilder sb = FindFirstObjectByType<FLA>().DebugFLA;
        string directoryPath = specificIterationDirPath + "/Fla Log/";
        if(!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        
        string fileName = $"FLALog_{generalLogs.Count}.txt";
        string path =
            directoryPath + $"/{fileName}";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }

    [MenuItem("Tools/Start Experiment")]
    public void StartPlayMode()
    {
        EditorApplication.EnterPlaymode();
    }
    
    public void OnGameStart()
    {
        
    }
}
[Serializable]
public class MatchLog
{
    public int experimentIndex;
    public BotController.Capability botType;
    public GameplayManager.ScenarioMode scenarioMode;
    public bool win;
    public int waveReached;
    public int remainingHealth;
    public int buildCount;
    public int upgradeCount;
    public int sellCount;

    public float totalDamage;
    public int totalEnemySlain;
    public int totalEnemyEscape;
    public int totalEnemyCount;
    public List<EnemyPerformance> enemyPerformances;
    public List<TowerLog> towerLogs;
    public HealthMetric healthMetrics;
    public List<FLA_Metrics> fla_metrics;
}
[Serializable]
public class HealthMetric
{
    public List<int> remainingHealth;
    public HealthMetric()
    {
        remainingHealth = new();
    }
}
[Serializable]
public class ExperimentPreset
{
    public bool useDDA;
    public BotController.Capability botType;
    public ExperimentPreset(bool useDDA, BotController.Capability botType)
    {
        this.useDDA = useDDA;
        this.botType = botType;
    }
}

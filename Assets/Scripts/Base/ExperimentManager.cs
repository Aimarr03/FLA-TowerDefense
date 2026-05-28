using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField] private int totalIteration = 10;
    [SerializeField, Range(5,10)] private float multiplierSpeed = 5;
    private int currentIteration = 0;
    public static ExperimentManager instance;
    private List<MatchLog> generalLogs;
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
        EditorApplication.playModeStateChanged += OnPlayStateChange;
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameplayManager.instance.onchangedState += OnGameplayStateChange;
    }

    private void OnGameplayStateChange(GameplayManager.State state)
    {
        IEnumerator ReloadScene()
        {
            ExportFLALog();
            yield return new WaitForSeconds(5);
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
            if(currentIteration < totalIteration)
            {
                StartCoroutine(ReloadScene());
            }
            else
            {
                StartCoroutine(ExitPlayMode());
            }
        }
    }
    IEnumerator OnEnterPlayMode()
    {
        yield return new WaitForSeconds(1f);
        Time.timeScale = multiplierSpeed;
        GameplayManager.instance.StartGame();
    }
    IEnumerator ExitPlayMode()
    {
        ExportGeneralCSV();
        ExportEnemyCSV();
        ExportTowerCSV();
        ExportHealthMetric();
        yield return new WaitForSeconds(5f);
        EditorApplication.ExitPlaymode();
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

        GameplayManager.instance.GetTotalEnemy(out int totalEnemyDamaged, out int totalEnemySlained);
        matchLog.totalEnemySlain = totalEnemySlained;
        matchLog.totalDamage = totalEnemyDamaged;
        matchLog.enemyPerformances = new();
        matchLog.towerLogs = new();
        matchLog.healthMetrics = new();

        UpdateEnemyLog(matchLog);
        UpdateTowerLog(matchLog);
        UpdateHealthMetric(matchLog);
        generalLogs.Add(matchLog);
    }
    public void ExportGeneralCSV()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "Experiment,Win,Wave,Build,Upgrade,Sell,Damage,EnemySlain");

        foreach(var log in generalLogs)
        {
            sb.AppendLine(
                $"{log.experimentIndex}," +
                $"{log.win}," +
                $"{log.waveReached}," +
                $"{log.buildCount}," +
                $"{log.upgradeCount}," +
                $"{log.sellCount}," +
                $"{log.totalDamage}," +
                $"{log.totalEnemySlain}");
        }

        string path =
            Application.dataPath + "/Debug Log/match_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    private void ExportEnemyCSV()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(
        "Experiment,EnemyType,Spawned,Killed,Escaped,Average");
        foreach(var match in generalLogs)
        {
            foreach(var enemy in match.enemyPerformances)
            {
                sb.AppendLine(
                $"{match.experimentIndex}," +
                $"{enemy.enemyType}," +
                $"{enemy.spawnedCount}," +
                $"{enemy.killedCount}," +
                $"{enemy.escapedCount},"+
                $"{enemy.avgHealth:F0}");
            }
        }
        string path =
        Application.dataPath + "/Debug Log/enemy_log.csv";
        File.WriteAllText(path, sb.ToString());
    }
    public void ExportTowerCSV()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(
            "Experiment,TowerType,Built,Upgrade,Sold,Damage,EnemySlain,Average Score");

        foreach(var match in generalLogs)
        {
            foreach(var tower in match.towerLogs)
            {
                sb.AppendLine(
                $"{match.experimentIndex}," +
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
            Application.dataPath + "/Debug Log/tower_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    public void ExportHealthMetric()
    {
        StringBuilder sb = new StringBuilder();
        int maxWave = generalLogs[0].healthMetrics.remainingHealth.Count;        
        sb.Append("Experiment,");
        for(int i = 0; i < maxWave; i++)
        {
            sb.Append($"Wave {i + 1:D2}");

            if(i < maxWave - 1)
                sb.Append(",");
        }
        sb.AppendLine();
        foreach(var match in generalLogs)
        {
            sb.Append($"{match.experimentIndex},");
            var healthMetric = match.healthMetrics.remainingHealth;
            for(int i = 0; i < healthMetric.Count; i++)
            {
                sb.Append($"{healthMetric[i]}");

                if(i < maxWave - 1)
                    sb.Append(",");
            }
            sb.AppendLine();
        }

        string path =
            Application.dataPath + "/Debug Log/health_log.csv";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    private void ExportFLALog()
    {
        StringBuilder sb = FindFirstObjectByType<FLA>().DebugFLA;
        string directoryPath = Application.dataPath + $"/Debug Log/FLA_Log/";
        if(!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        
        string fileName = $"FLALog_{generalLogs.Count}.txt";
        string path =
            directoryPath + $"{fileName}";

        File.WriteAllText(path, sb.ToString());

        Debug.Log("CSV Exported");
    }
    private void OnPlayStateChange(PlayModeStateChange change)
    {
        if(change == PlayModeStateChange.EnteredPlayMode)
        {
            StartCoroutine(OnEnterPlayMode());
        }
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
    public bool win;
    public int waveReached;

    public int buildCount;
    public int upgradeCount;
    public int sellCount;

    public float totalDamage;
    public int totalEnemySlain;
    public List<EnemyPerformance> enemyPerformances;
    public List<TowerLog> towerLogs;
    public HealthMetric healthMetrics;
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

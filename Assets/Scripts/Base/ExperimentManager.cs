using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                SceneManager.LoadSceneAsync("Prototipe_main");
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
        yield return new WaitForSeconds(10f);
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
                $"{enemy.avgHealth:F2}");
            }
        }
        string path =
        Application.dataPath + "/Debug Log/enemy_log.csv";
        File.WriteAllText(path, sb.ToString());
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

        UpdateEnemyLog(matchLog);
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
}

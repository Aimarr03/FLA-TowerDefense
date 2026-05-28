using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float interval = 1f;
    [SerializeField] private int enemyRemaining = 10;
    [SerializeField] private EnemyWave enemyWave;
    [SerializeField] private List<SpawnEvent> spawnEvents;
    [SerializeField] private EnemySpawnLoader enemySpawnLoader;
    [SerializeField] private List<EnemySpawnInfo> enemySpawnInfos = new List<EnemySpawnInfo>();
    public Dictionary<EnemyType, EnemyPerformance> enemyPerformances;
    public Dictionary<EnemyType, List<float>> enemyHealthMetrics;
    public List<EnemySpawnInfo> EnemySpawnInfos => enemySpawnInfos;

    /// <summary>
    /// Obsolete, use spawnEvents instead. 
    /// This is for simple wave that only has one type of enemy and fixed interval.
    /// </summary>
    //private int currentSpawn = 0;
    //private float currentTick = 0;
    //List<Enemy> enemiesSpawned = new();
    //Queue<EnemyData> spawnQueue = new();

    bool isActive = false;
    int currentIndex = 0;
    float timer = 0f;

    
    public int TotalEnemy {get; private set;}
    public float EnemyTotalHealth { get; private set; }
    public float EnemyRemainingHealth { get; private set; }
    public float CurrentRoundEnemyCount { get; private set; }
    public float CurrentRoundEnemySlained {get; private set;}
    public float CurrentRoundEnemyEscaped {get; private set;}
    
    private void OnDestroy()
    {
        GameplayManager.instance.onchangedState -= OnChangeState;
    }
    void Start()
    {
        GameplayManager.instance.onchangedState += OnChangeState;
        enemyPerformances = new Dictionary<EnemyType, EnemyPerformance>()
        {
            { EnemyType.Goblin, new EnemyPerformance() },
            { EnemyType.Bee, new EnemyPerformance() },
            { EnemyType.Wolf, new EnemyPerformance() },
            { EnemyType.Slime, new EnemyPerformance() }
        };
        enemyHealthMetrics = new Dictionary<EnemyType, List<float>>()
        {
            { EnemyType.Goblin, new List<float>() },
            { EnemyType.Bee, new List<float>() },
            { EnemyType.Wolf, new List<float>() },
            { EnemyType.Slime, new List<float>() }
        };
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        while (currentIndex < spawnEvents.Count && timer >= spawnEvents[currentIndex].spawnTime)
        {
            SpawnEnemy(spawnEvents[currentIndex]);
            currentIndex++;
        }
    }
    private void OnChangeState(GameplayManager.State newState)
    {
        isActive = newState switch
        {
            GameplayManager.State.Defending => true,
            _ => false
        };

        switch (newState)
        {
            case GameplayManager.State.Defending:
                currentIndex = 0;
                timer = 0f;
                isActive = true;
                CurrentRoundEnemyCount = 0 ;
                CurrentRoundEnemyEscaped = 0;
                CurrentRoundEnemySlained = 0;
                break;
        }
    }
    public void GetEnemyWave()
    {
        GameplayManager.SpawnType spawnType = GameplayManager.instance.SpawnMode;
        int waveIndex = GameplayManager.instance.CurrentWaveIndex;
        Debug.Log($"[Spawner] Getting Enemy Wave for wave {waveIndex} with spawn type {spawnType}");
        
        switch (GameplayManager.instance.SpawnMode)
        {
            case GameplayManager.SpawnType.Number:
                EnemyWave normalWave = enemySpawnLoader.enemyWaves[waveIndex];
                spawnEvents = EnemyWaveBuilder.NormalBuild(normalWave, 1f);
                
                enemySpawnInfos = normalWave.enemies;
                break;
            case GameplayManager.SpawnType.Subwave:
                PatternEnemyWave subpatternWave = enemySpawnLoader.patternEnemyWaves[waveIndex];
                spawnEvents = EnemyWaveBuilder.SubwaveBuild(subpatternWave);
                
                enemySpawnInfos = subpatternWave.GetTotalEnemySpawn();
                break;
        }
        enemyRemaining = spawnEvents.Count;
        int totalHealth = 0;
        int totalEnemy = 0;
        foreach(var enemyInfo in enemySpawnInfos)
        {
            totalEnemy += enemyInfo.amount;
            totalHealth += (int) TD_API.EnemyDatas[enemyInfo.type].MaxHealth * enemyInfo.amount;
        }
        TotalEnemy = totalEnemy;
        EnemyTotalHealth = totalHealth;
        CurrentRoundEnemyCount = totalEnemy;
    }
    private void SpawnEnemy(SpawnEvent spawnEvent)
    {
        EnemyData enemyData = spawnEvent.enemyData;
        Enemy enemy = Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);
        enemy.Init(enemyData);
        
        
        var enemyPerformance = enemyPerformances[enemy.EnemyType];
        enemyPerformance.spawnedCount++;
        enemy.OnDie += (Enemy enemy, bool reachDestination) =>
        {
            
            var enemyHealthMetric = enemyHealthMetrics[enemy.EnemyType];

            float healthPercentage = Mathf.Clamp(enemy.CurrentHealth / enemy.MaxHealth, 0, 1);
            enemyHealthMetric.Add(healthPercentage);
            if (reachDestination)
            {
                CurrentRoundEnemyEscaped++;
                enemyPerformance.escapedCount++;
            }
            else
            {
                CurrentRoundEnemySlained++;
                enemyPerformance.killedCount++;
            }
            EnemyRemainingHealth += enemy.CurrentHealth;
            OnRemoveEnemy(enemy);
        };
    }
    private void OnRemoveEnemy(Enemy enemy)
    {
        if (GameplayManager.instance.GameState != GameplayManager.State.Defending) return;
        
        enemyRemaining = Mathf.Max(0, enemyRemaining - 1);
        if (enemyRemaining <= 0)
        {
            Debug.Log("All Enemy eradicated, going next round!");
            GameplayManager.instance.DefendsOver();
        }
    }
    #region Obsolete
    [Obsolete("Use EnemyWaveBuilder Instead")]
    private void LoadEnemy()
    {
        //enemyWave = GameplayManager.instance.currentEnemyWave;
        //currentSpawn = 0;

        //EnemyRemainingHealth = 0;
        //EnemyTotalHealth = 0;
        //EnemyReachDestination = 0;
        //EnemyDied = 0;

        //List<EnemyData> List_EnemyData = new();
        //foreach (var enemy in enemyWave.enemies)
        //{
        //    if (!TD_API.EnemyDatas.TryGetValue(enemy.type, out var enemyData))
        //    {
        //        Debug.LogWarning($"Data {enemy.type} is not found!");
        //        continue;
        //    }
        //    for (int i = 0; i < enemy.amount; i++)
        //    {
        //        List_EnemyData.Add(enemyData);
        //        EnemyTotalHealth += enemyData.MaxHealth;
        //    }
        //}
        //List_EnemyData.Shuffle();

        //spawnQueue.Clear();
        //foreach (var data in List_EnemyData)
        //    spawnQueue.Enqueue(data);

        //totalEnemySpawn = spawnQueue.Count;
    }
    [Obsolete("Using different approach on spawning Enemy")]
    private void SpawnEnemy()
    {
        //if (spawnQueue.Count == 0) return;

        //EnemyData enemyData = spawnQueue.Dequeue();
        
        //Enemy newEnemy = Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);
        //newEnemy.Init(enemyData);
        //newEnemy.OnDie += (bool reachDestination) =>
        //{
        //    if (reachDestination)
        //    {
        //        EnemyReachDestination++;
        //    }
        //    else
        //    {
        //        EnemyDied++;
        //    }
        //    EnemyRemainingHealth += newEnemy.CurrentHealth;
        //    OnRemoveEnemy(newEnemy);
            
        //};
        
        //if (!enemiesSpawned.Contains(newEnemy))
        //{
        //    enemiesSpawned.Add(newEnemy);
        //}
        //newEnemy.gameObject.SetActive(true);
        //currentSpawn++;
    }
    [Obsolete("Using different approach on tracking Enemy")]
    private void UpdateTimer()
    {
        //if (currentSpawn >= totalEnemySpawn) return;
        //currentTick += Time.deltaTime;

        //if (currentTick >= interval)
        //{
        //    currentTick = 0;
        //    SpawnEnemy();
        //}
    }
    #endregion
}

public static class PseudoRandom
{
    public static T GetWeightedRandom<T>(List<(T item, float weight)> items)
    {
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.weight;
        }
        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;
        foreach (var item in items)
        {
            cumulativeWeight += item.weight;
            if (randomValue < cumulativeWeight)
            {
                return item.item;
            }
        }
        
        return default(T); // Should never reach here if weights are properly defined
    }
}
[Serializable]
public class EnemyPerformance
{
    public EnemyType enemyType;
    public int spawnedCount = 0;
    public int killedCount = 0;
    public int escapedCount = 0;
    public float avgHealth = 0;
    public float minHealth = 0;
    public float maxHealth = 0;
}
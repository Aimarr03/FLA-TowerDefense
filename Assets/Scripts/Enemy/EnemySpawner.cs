using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private float interval = 1f;
    [SerializeField] private int totalEnemySpawn = 10;
    [SerializeField] private EnemyWave enemyWave;

    private int currentSpawn = 0;
    private float currentTick = 0;

    List<Enemy> enemiesSpawned = new();
    Queue<EnemyData> spawnQueue = new();
    bool isActive = false;

    public int TotalEnemy => totalEnemySpawn;
    public int EnemyReachDestination { get; private set; }
    public int EnemyDied { get; private set; }
    public float EnemyTotalHealth { get; private set; }
    public float EnemyRemainingHealth { get; private set; }
    private void Awake()
    {

    }
    private void OnDestroy()
    {
        GameplayManager.instance.onchangedState -= OnChangeState;
    }
    void Start()
    {
        GameplayManager.instance.onchangedState += OnChangeState;
    }

    void Update()
    {
        if (!isActive) return;
        
        if (currentSpawn >= totalEnemySpawn) return;
        currentTick += Time.deltaTime;

        if (currentTick >= interval)
        {
            currentTick = 0;
            SpawnEnemy();
        }
    }
    private void OnRemoveEnemy(Enemy enemy)
    {
        if (GameplayManager.instance.GameState != GameplayManager.State.Defending) return;
        if (enemiesSpawned.Contains(enemy))
        {
            enemiesSpawned.Remove(enemy);
        }

        bool conditionSpawned = currentSpawn >= totalEnemySpawn;
        bool conditionEmpty = enemiesSpawned.Count == 0;

        if (conditionSpawned && conditionEmpty)
        {
            Debug.Log("All Enemy eradicated, going next round!");
            GameplayManager.instance.DefendsOver();
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
                enemyWave = GameplayManager.instance.currentEnemyWave;
                currentSpawn = 0;
                
                EnemyRemainingHealth = 0;
                EnemyTotalHealth = 0;
                EnemyReachDestination = 0;
                EnemyDied = 0;

                List<EnemyData> List_EnemyData = new();
                foreach (var enemy in enemyWave.enemies)
                {
                    if (!TD_API.EnemyDatas.TryGetValue(enemy.type, out var enemyData))
                    {
                        Debug.LogWarning($"Data {enemy.type} is not found!");
                        continue;
                    }
                    for (int i = 0; i < enemy.amount; i++)
                    {
                        List_EnemyData.Add(enemyData);
                        EnemyTotalHealth += enemyData.MaxHealth;
                    }
                }
                List_EnemyData.Shuffle();

                spawnQueue.Clear();
                foreach (var data in List_EnemyData)
                    spawnQueue.Enqueue(data);
                
                totalEnemySpawn = spawnQueue.Count;
                break;
        }
    }
    private void SpawnEnemy()
    {
        if (spawnQueue.Count == 0) return;

        EnemyData enemyData = spawnQueue.Dequeue();
        
        Enemy newEnemy = Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);
        newEnemy.Init(enemyData);
        newEnemy.OnDie += (bool reachDestination) =>
        {
            if (reachDestination)
            {
                EnemyReachDestination++;
            }
            else
            {
                EnemyDied++;
            }
            EnemyRemainingHealth += newEnemy.CurrentHealth;
            OnRemoveEnemy(newEnemy);
            
        };
        
        if (!enemiesSpawned.Contains(newEnemy))
        {
            enemiesSpawned.Add(newEnemy);
        }
        newEnemy.gameObject.SetActive(true);
        currentSpawn++;
    }
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
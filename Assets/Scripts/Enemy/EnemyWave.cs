using System.Collections.Generic;
using System;

[Serializable]
public struct EnemyWave
{
    public List<EnemySpawnInfo> enemies;

    public int TotalSpawn
    {
        get
        {
            int total = 0;
            foreach (var e in enemies)
                total += e.amount;
            return total;
        }
    }
}

[Serializable]
public struct EnemySpawnInfo
{
    public EnemyType type;
    public int amount;
}

[Serializable]
public struct EnemySubWave
{
    public bool useDefaultInterval;
    public float interval;

    public float duration;

    public List<EnemySpawnInfo> enemies;
}

[Serializable]
public struct PatternEnemyWave
{
    public int index;
    public float defaultInterval;

    public List<EnemySubWave> subWaves;

    private List<EnemySpawnInfo> totalEnemiesSpawn;

    public List<EnemySpawnInfo> GetTotalEnemySpawn()
    {
        if (totalEnemiesSpawn != null) return totalEnemiesSpawn;
        
        totalEnemiesSpawn = new List<EnemySpawnInfo>();
        Dictionary<EnemyType, int> enemyCount = new Dictionary<EnemyType, int>();
        foreach (var subWave in subWaves)
        {
            foreach (var spawnInfo in subWave.enemies)
            {
                if (enemyCount.ContainsKey(spawnInfo.type))
                {
                    enemyCount[spawnInfo.type] += spawnInfo.amount;
                }
                else
                {
                    enemyCount[spawnInfo.type] = spawnInfo.amount;
                }
            }
        }
        foreach (var kvp in enemyCount)
        {
            totalEnemiesSpawn.Add(new EnemySpawnInfo { type = kvp.Key, amount = kvp.Value });
        }
        
        return totalEnemiesSpawn;
    }
}

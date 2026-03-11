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
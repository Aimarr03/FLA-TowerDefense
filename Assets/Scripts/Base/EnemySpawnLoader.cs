using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnLoader : MonoBehaviour
{
    [SerializeField] private string PathNumberBase = "Enemy Waves/Number Base";
    [SerializeField] private string PathPercentageBase = "Enemy Waves/Percentage Base";
    [SerializeField] private string PathSubWaveBase = "Enemy Waves/Subwave Base";

    public List<EnemyWave> enemyWaves = new List<EnemyWave>();
    public List<EnemyWave> randomizedEnemyWaves = new List<EnemyWave>();
    public List<PatternEnemyWave> patternEnemyWaves = new List<PatternEnemyWave>();

    public int GetMaxInfoWave(GameplayManager.SpawnType spawnType)
    {
        switch (spawnType)
        {
            case GameplayManager.SpawnType.NumberBase:
                return enemyWaves.Count;
            case GameplayManager.SpawnType.PercentageBase:
                return randomizedEnemyWaves.Count;
            case GameplayManager.SpawnType.SubWaveBase:
                return patternEnemyWaves.Count;
            default:
                throw new ArgumentOutOfRangeException(nameof(spawnType), spawnType, null);
        }
    }
    public void LoadData()
    {
        LoadSubWaveData();
        var waves = Resources.LoadAll<TextAsset>(PathNumberBase);
        var percentageWaves = Resources.LoadAll<TextAsset>(PathPercentageBase);
        enemyWaves = new();
        foreach (var wave in waves)
        {
            string waveString = wave.text;
            EnemyWave enemyWave = JsonUtility.FromJson<EnemyWave>(waveString);
            enemyWaves.Add(enemyWave);
        }
        
        for(int index = 0; index < percentageWaves.Length; index++)
        {
            string percentageWaveString = percentageWaves[index].text;
            PercentageEnemySpawn percentageEnemyWave = JsonUtility.FromJson<PercentageEnemySpawn>(percentageWaveString);
            int amount = enemyWaves[index].TotalSpawn;

            Dictionary<EnemyType, int> enemyTypeAmount = new Dictionary<EnemyType, int>();
            ConvertPercentageAndUpdateDic(enemyTypeAmount, percentageEnemyWave, amount);
            
            EnemyWave enemyWave = new EnemyWave();
            enemyWave.enemies = new List<EnemySpawnInfo>();
            
            foreach (var key in enemyTypeAmount.Keys)
            {
                var value = enemyTypeAmount[key];
                enemyWave.enemies.Add(new EnemySpawnInfo()
                {
                    type = key,
                    amount = value
                });
            }
            randomizedEnemyWaves.Add(enemyWave);
        }
    }
    private void LoadSubWaveData()
    {
        var subWaveBases = Resources.LoadAll<TextAsset>(PathSubWaveBase);
        patternEnemyWaves = new();
        foreach (var subWaveBase in subWaveBases)
        {
            string subWaveBaseString = subWaveBase.text;
            PatternEnemyWave patternEnemyWave = JsonUtility.FromJson<PatternEnemyWave>(subWaveBaseString);
            patternEnemyWaves.Add(patternEnemyWave);
        }
    }
    public void ConvertPercentageAndUpdateDic(Dictionary<EnemyType, int> container, PercentageEnemySpawn percentageEnemyWave, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            float totalWeight = 0;
            foreach (var info in percentageEnemyWave.EnemyWaves)
            {
                totalWeight += info.percentage;
            }

            float randomValue = Random.Range(0, totalWeight);
            float cumulative = 0;
            foreach (var info in percentageEnemyWave.EnemyWaves)
            {
                cumulative += info.percentage;
                if (randomValue < cumulative)
                {
                    if (container.ContainsKey(info.type))
                    {
                        container[info.type]++;
                    }
                    else
                    {
                        container[info.type] = 1;
                    }
                    break;
                }
            }
        }
    }
}
[Serializable]
public struct PercentageEnemySpawn
{
    public List<PercentageEnemySpawnInfo> EnemyWaves;
}
[Serializable]
public struct PercentageEnemySpawnInfo
{
    public EnemyType type;
    public float percentage;
    public PercentageEnemySpawnInfo(EnemyType type, float percentage)
    {
        this.type = type;
        this.percentage = percentage;
    }
}
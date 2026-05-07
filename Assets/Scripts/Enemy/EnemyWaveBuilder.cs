using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

[Serializable]
public struct SpawnEvent
{
    public float spawnTime;
    public EnemyData enemyData;
}
public static class EnemyWaveBuilder
{
    public static List<SpawnEvent> NormalBuild(EnemyWave wave, float interval)
    {
        List<SpawnEvent> spawnEvents = new List<SpawnEvent>();
        float currentTime = 0f;
        foreach (var spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.amount; i++)
            {
                TD_API.EnemyDatas.TryGetValue(spawnInfo.type, out var enemyData);
                SpawnEvent spawnEvent = new SpawnEvent
                {
                    spawnTime = currentTime,
                    enemyData = enemyData
                };
                spawnEvents.Add(spawnEvent);
                currentTime += interval;
            }
        }
        // Shuffle the spawn events to add randomness
        for (int i = spawnEvents.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = spawnEvents[i];
            spawnEvents[i] = spawnEvents[j];
            spawnEvents[j] = temp;
        }
        return spawnEvents;
    }
    public static List<SpawnEvent> SubwaveBuild(PatternEnemyWave patternWave, bool randomize = false)
    {
        List<SpawnEvent> spawnEvents = new List<SpawnEvent>();
        float currentTime = 0f;
        foreach (var subWave in patternWave.subWaves)
        {
            float interval = subWave.useDefaultInterval ? patternWave.defaultInterval : subWave.interval;
            foreach (var spawnInfo in subWave.enemies)
            {
                for (int i = 0; i < spawnInfo.amount; i++)
                {
                    TD_API.EnemyDatas.TryGetValue(spawnInfo.type, out var enemyData);
                    SpawnEvent spawnEvent = new SpawnEvent
                    {
                        spawnTime = currentTime,
                        enemyData = enemyData
                    };
                    spawnEvents.Add(spawnEvent);
                    currentTime += interval;
                    
                    //Debug.Log($"[EnemyWaveBuilder] Scheduled spawn of {enemyData.EnemyName} at time {currentTime}");
                }
            }
            currentTime += subWave.duration;
            //Debug.Log($"[EnemyWaveBuilder] Subwave completed. Current time: {currentTime}");
        }
        
        // Shuffle the spawn events to add randomness
        if (randomize)
        {
            for (int i = spawnEvents.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = spawnEvents[i];
                spawnEvents[i] = spawnEvents[j];
                spawnEvents[j] = temp;
            }
        }
        return spawnEvents;
    }
}

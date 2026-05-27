
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerLogController : MonoBehaviour
{
    public Dictionary<TowerType, TowerLog> towerLogs;
    public Dictionary<TowerType, List<Tower.Performance>> towerPerformances = new();
    void Start()
    {
        towerLogs = new()
        {
            { TowerType.Archer, new() { towerType = TowerType.Archer }},
            { TowerType.Mage, new() { towerType = TowerType.Mage }},
            { TowerType.Mortar, new() { towerType = TowerType.Mortar }},
        };
        towerPerformances = new()
        {
            { TowerType.Archer, new()},
            { TowerType.Mage, new()},
            { TowerType.Mortar, new()},
        };
        Tower.ActionInvoke += OnTowerActionInvoke;
        AttackContext.OnDamage += OnDamage;
        AttackContext.OnKill += OnKill;
    }
    void OnDestroy()
    {
        Tower.ActionInvoke -= OnTowerActionInvoke;
        AttackContext.OnDamage -= OnDamage;
        AttackContext.OnKill -= OnKill;
    }
    public void FinalisedRawData()
    {
        Dictionary<TowerType, List<float>> TowerScoreTypes = new();
        foreach(var key in towerPerformances.Keys)
        {
            var listPerformance = towerPerformances[key];
            foreach(var performance in listPerformance)
            {
                if(!TowerScoreTypes.ContainsKey(key))
                    TowerScoreTypes.Add(key, new());
                
                List<float> listOfScore = TowerScoreTypes[key];
                listOfScore.Add(performance.currentScore);
            }
        }
        var allBuiltTower = FindObjectsByType<Tower>(FindObjectsSortMode.None).Where(tower => tower.CurrentState == Tower.State.Built);
        foreach(var tower in allBuiltTower)
        {
            var performance = tower.performance;
            var towerType = tower.TowerData.Type;
            if (!TowerScoreTypes.ContainsKey(towerType))
            {
                TowerScoreTypes.Add(towerType, new());
            }
            List<float> listOfScore = TowerScoreTypes[towerType];
            listOfScore.Add(performance.currentScore);
        }
        Dictionary<TowerType, float> averageTowerScore = new();
        foreach(var key in TowerScoreTypes.Keys)
        {
            List<float> listOfScore =TowerScoreTypes[key];
            float totalScore = 0;
            foreach(var score in listOfScore)
                totalScore += score;
            totalScore = totalScore / listOfScore.Count;

            averageTowerScore.Add(key, totalScore);
        }
        foreach(var key in towerLogs.Keys)
        {
            var towerLog = towerLogs[key];
            if (averageTowerScore.ContainsKey(key))
            {
                towerLog.averageScore = averageTowerScore[key];
            }
            else
            {
                towerLog.averageScore = -1;
            }
        }
    }
    private void OnTowerActionInvoke(Tower tower, TowerActionType type)
    {
        if(tower.CurrentState == Tower.State.None) return;
        
        TowerType towerType = tower.TowerData.Type;
        var towerLog = towerLogs[towerType];
        switch (type)
        {
            case TowerActionType.Presell:
                var listPerformance = towerPerformances[towerType];
                var performance = tower.performance;
                listPerformance.Add( 
                    new() 
                    {
                        currentScore = performance.currentScore,
                        damageDealt = performance.damageDealt,
                        enemySlain = performance.enemySlain
                    });
                towerLog.sellTotal++;
                break;
            case TowerActionType.Buy:
                towerLog.builtTotal++;
                break;
            case TowerActionType.Upgrade:
                towerLog.upgradeTotal++;
                break;
        }
    }
    private void OnDamage(Tower tower, TowerType towerType, float damage)
    {
        var towerLog = towerLogs[towerType];
        towerLog.totalDamage += damage;
    }

    private void OnKill(Tower tower, TowerType towerType)
    {
        var towerLog = towerLogs[towerType];
        towerLog.totalKill++;
    }
}
public enum TowerType
{
    Archer,
    Mage,
    Mortar
}
[Serializable]
public class TowerLog
{
    public TowerType towerType;
    public int builtTotal;
    public int upgradeTotal;
    public int sellTotal;
    public float totalDamage;
    public int totalKill;
    public float averageScore;
}


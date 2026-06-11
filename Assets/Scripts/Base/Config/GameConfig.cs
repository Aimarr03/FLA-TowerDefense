using UnityEngine;
using System;
using System.IO;

[Serializable]
public class GameConfig
{
    public string filePath;
    public string scenarioMode;
    public string spawnMode;
    public string cycleMode;
    public int startingMoney;
    public float baseDuration;
    public bool useExperiment;
    public BotUsage botUsage = new();
}
public class GameConfigBuilder
{
    public string filePath = "Enemy Waves/Subwave Base/Case 01";
    public string scenarioMode = "static";
    public string spawnMode = "subwave";
    public string cycleMode = "normal";
    public int startingMoney = 450;
    public float baseDuration = 20;
    public bool useExperiment = false;
    public BotUsage botUsage = new();
    public GameConfig Build()
    {
        var config = new GameConfig();
        config.filePath = this.filePath;
        config.scenarioMode = this.scenarioMode;
        config.spawnMode = this.spawnMode;
        config.cycleMode = this.cycleMode;
        config.startingMoney = this.startingMoney;
        config.baseDuration = this.baseDuration;
        config.useExperiment = this.useExperiment;
        config.botUsage = this.botUsage;
        Debug.Log(config.filePath);
        return config;
    }
    public void SetScenarioMode(GameplayManager.ScenarioMode scenarioMode)
    {
        this.scenarioMode = scenarioMode.ToString().ToLower();
    }
}
[Serializable]
public class BotUsage
{
    public bool useBot = false;
    public BotController.Capability capability = BotController.Capability.Low;
}

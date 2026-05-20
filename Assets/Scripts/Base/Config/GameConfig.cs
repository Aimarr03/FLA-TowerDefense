using UnityEngine;
using System;

[Serializable]
public class GameConfig
{
    public string filePath;
    public string scenarioMode;
    public string spawnMode;
    public string cycleMode;
    public int startingMoney;
    public float baseDuration;
    public BotUsage botUsage = new();
}
[Serializable]
public class BotUsage
{
    public bool useBot = false;
    public BotController.Capability capability = BotController.Capability.Low;
}

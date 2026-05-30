using UnityEngine;
using System.IO;
using YamlDotNet.Serialization;
using System;
using Mono.Cecil;
using System.Text;
public class ConfigLoader : MonoBehaviour
{
    string defaultPath = "Enemy Waves/Subwave Base";
    string defaultScenario = "dda";
    string defaultSpawn = "subwave";
    string defaultCycle = "normal";
    int defaultMoney = 300;
    float defaultBaseDuration = 30;
    string defaultNameFile = "gameconfig.yaml";

    public GameConfig gameConfig;
    public string directoryPath = "Game Config";

    public GameConfig LoadGameConfig(string fileName)
    {
        StringBuilder sb = new();
        sb.Append($"{directoryPath}/");
        sb.Append(fileName);
        var textConfig = Resources.Load<TextAsset>(sb.ToString());
        if(textConfig == null)
        {
            Debug.LogError("No File found, please try different methods");
            return null;
        }
        var deserializer = new DeserializerBuilder().Build();
        string yamlText = textConfig.text;
        gameConfig = deserializer.Deserialize<GameConfig>(yamlText);

        return gameConfig;
    }
    public void Init()
    {
        string folderPath = directoryPath;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("StreamingAssets folder created");
        }
        
        string path = Path.Combine(Application.streamingAssetsPath, defaultNameFile);
        if (!File.Exists(path))
        {
            CreateNewDefaultConfig();
        }
        
        string yamlText = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();

        gameConfig = deserializer.Deserialize<GameConfig>(yamlText);
        
    }
    private void CreateNewDefaultConfig()
    {
        GameConfig config = new()
        {
            filePath = defaultPath,
            cycleMode = defaultCycle,
            scenarioMode = defaultScenario,
            spawnMode = defaultSpawn,
            startingMoney = defaultMoney,
            baseDuration = defaultBaseDuration
        };

        SaveConfig(config);
    }
    private void SaveConfig(GameConfig config)
    {
        var serializer = new SerializerBuilder().Build();

        string yaml = serializer.Serialize(config);

        string path = Path.Combine(Application.streamingAssetsPath, defaultNameFile);

        File.WriteAllText(path, yaml);
    }
    public void TryParseSpawnType(string spawnType, out GameplayManager.SpawnType parsedType)
    {
        spawnType = spawnType.ToLower();
        switch (spawnType)
        {
            case "number":
                parsedType = GameplayManager.SpawnType.Number;
                break;
            case "percentage":
                parsedType = GameplayManager.SpawnType.Percentage;
                break;
            case "subwave":
                parsedType = GameplayManager.SpawnType.Subwave;
                break;
            default:
                Debug.LogWarning($"{spawnType} is not found, please check again! using normal type");
                parsedType = GameplayManager.SpawnType.Number;
                break;
        }
    }
    public void TryParseScenarioMode(string spawnType, out GameplayManager.ScenarioMode parsedType)
    {
        spawnType = spawnType.ToLower();
        switch (spawnType)
        {
            case "dda":
                parsedType = GameplayManager.ScenarioMode.DDA;
                break;
            case "static":
                parsedType = GameplayManager.ScenarioMode.Static;
                break;
            default:
                Debug.LogWarning($"{spawnType} is not found, please check again! using normal type");
                parsedType = GameplayManager.ScenarioMode.Static;
                break;
        }
    }
}

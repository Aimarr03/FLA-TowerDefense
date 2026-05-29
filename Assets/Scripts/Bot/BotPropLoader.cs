using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

public class BotPropLoader : MonoBehaviour
{
    public Dictionary<BotController.Capability, BotProperty> BotProperties = new();
    public void Init()
    {
        // Load semua TextAsset dalam Resources/Bot
        TextAsset[] botFiles = Resources.LoadAll<TextAsset>("Bot");

        var deserializer = new DeserializerBuilder().Build();

        foreach (var file in botFiles)
        {
            string yamlText = file.text;

            BotProperty property = deserializer.Deserialize<BotProperty>(yamlText);

            BotProperties[property.BotType] = property;

            Debug.Log($"Loaded bot: {property.BotType}");
        }
    }
}

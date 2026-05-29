using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class FLA : MonoBehaviour
{
    [SerializeField] private float scalingHP = 0.3f;
    [SerializeField] private float baseMultplierHP = 1f;
    [SerializeField] private static float totalMultiplierHP = 1f;
    [SerializeField] private float scalingGold = 0.45f;
    [SerializeField] private float baseMultplierGold = 2.2f;
    [SerializeField] private static float totalMultiplierGold = 1.0f;
    [SerializeField] private float scalingSpawnEnemy = 0.12f;
    [SerializeField] private float baseMultiplierSpawnEnemy = 1f;
    [SerializeField] private static float totalMultiplierSpawnEnemy = 1f;

    public static float FinalMultiplierHP => totalMultiplierHP;
    public static float FinalMultiplierGold => totalMultiplierGold;
    public static float FinalMultiplierSpawnEnemy => totalMultiplierSpawnEnemy;    
    public StringBuilder DebugFLA;
 
    FibonacciSequence fibonacciSequence;
    Classifier classifier;
    private void Awake()
    {
        DebugFLA = new();
        DebugFLA.AppendLine("FLA PERFORMANCE LOG\n\n");
    }
    public void Setup(int baseHealth)
    {
        fibonacciSequence = new FibonacciSequence();
        classifier = new Classifier(baseHealth);
    }
    public void UpdateFLA(RoundPerformance currentRound, RoundPerformance previousRound)
    {
        var delta = classifier.GetPerformance(currentRound);
        DebugFLA.AppendLine($"Current Round: {currentRound.RoundIndex}");
        DebugFLA.AppendLine($"performance: {classifier.Performance}");
        DebugFLA.AppendLine($"Player Ratio: {classifier.HealthRatio}");
        DebugFLA.AppendLine($"Enemy Ratio: {classifier.EnemyRatio}\n");

        int currentIndex = fibonacciSequence.currentSequence;
        int targetIndex = currentIndex + delta;
        
        targetIndex = Mathf.Clamp(targetIndex, 1, 8);
        currentIndex = (int)Mathf.MoveTowards(currentIndex, targetIndex, 1);
        fibonacciSequence.currentSequence = currentIndex;

        DebugFLA.AppendLine("Fibonacci Result");
        DebugFLA.AppendLine($"Sequenced: {fibonacciSequence.currentSequence}\n");
        
        // float decay = 1f / (1f + (currentIndex * 0.19f));
        // totalMultiplierHP = 1 + (multiplierHP * fibonacciSequence.Value * decay);
        totalMultiplierHP = baseMultplierHP + Mathf.Log(fibonacciSequence.Value) * scalingHP;
        totalMultiplierGold = baseMultplierGold - Mathf.Log(fibonacciSequence.Value) * scalingGold;
        totalMultiplierSpawnEnemy = baseMultiplierSpawnEnemy + Mathf.Log(fibonacciSequence.Value) * scalingSpawnEnemy;

        DebugFLA.AppendLine($"HP Multiplier: {totalMultiplierHP}\n");
        DebugFLA.AppendLine($"Gold Multiplier: {totalMultiplierGold}\n");
        DebugFLA.AppendLine($"Spawn Enemy Multiplier: {totalMultiplierSpawnEnemy}\n\n\n");
    }
}

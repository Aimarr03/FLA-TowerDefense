using System.Text;
using UnityEngine;

public class FLA : MonoBehaviour
{
    [SerializeField] private float multiplierHP = 0.1f;
    [SerializeField] private static float totalMultiplierHP = 1f;
    [SerializeField] private float baseMultiplierGold = 1.0f;
    [SerializeField] private float baseMultiplierSpeed = 1.0f;

    [SerializeField] private float goldScale = 0.05f;
    [SerializeField] private float speedScale = 0.05f;

    private float finalMultiplierGold = 1.0f;
    private float finalMultiplierSpeed = 1.0f;

    public static float FinalMultiplierHP => totalMultiplierHP;
    public float FinalMultiplierGold => finalMultiplierGold;
    public float FinalMultiplierSpeed => finalMultiplierSpeed;
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

        totalMultiplierHP = 1 + (multiplierHP * fibonacciSequence.Value);

        DebugFLA.AppendLine($"HP Multiplier: {totalMultiplierHP}\n");
    }
}

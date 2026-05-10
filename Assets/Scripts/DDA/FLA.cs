using UnityEngine;

public class FLA : MonoBehaviour
{
    [SerializeField] private float baseMultiplierGold = 1.0f;
    [SerializeField] private float baseMultiplierDuration = 1.0f;
    [SerializeField] private float baseMultiplierSpeed = 1.0f;

    [SerializeField] private float goldScale = 0.05f;
    [SerializeField] private float durationScale = 0.05f;
    [SerializeField] private float speedScale = 0.05f;

    private float finalMultiplierGold = 1.0f;
    private float finalMultiplierDuration = 1.0f;
    private float finalMultiplierSpeed = 1.0f;

    public float FinalMultiplierGold => finalMultiplierGold;
    public float FinalMultiplierDuration => finalMultiplierDuration;
    public float FinalMultiplierSpeed => finalMultiplierSpeed;
 
    FibonacciSequence fibonacciSequence;
    Classifier classifier;
    private void Awake()
    {
        
    }
    public void Setup(int baseHealth)
    {
        fibonacciSequence = new FibonacciSequence();
        classifier = new Classifier(baseHealth);
    }
    public void UpdateFLA(RoundPerformance currentRound, RoundPerformance previousRound)
    {
        Classifier.ClassificationResult result = classifier.Classify(currentRound, previousRound);
        Classifier.PlayerClassify playerClassify = result.HealthClassification;
        int difficultyDelta = playerClassify switch
        {
            Classifier.PlayerClassify.High => +2,
            Classifier.PlayerClassify.Medium => +1,
            Classifier.PlayerClassify.Low => -1,
            _ => 0
        };
        fibonacciSequence.currentSequence += difficultyDelta;
        int currentFibonacciValue = fibonacciSequence.Value;

        finalMultiplierGold = baseMultiplierGold - (currentFibonacciValue * goldScale);
        finalMultiplierDuration = baseMultiplierDuration - (currentFibonacciValue * durationScale);
        finalMultiplierSpeed = baseMultiplierSpeed + (currentFibonacciValue * speedScale);

        Debug.Log($"[FLA] Classification Result: {playerClassify}");
        Debug.Log($"[FLA] Final Multipliers: Gold: {finalMultiplierGold}, Duration: {finalMultiplierDuration}, Speed: {finalMultiplierSpeed}");
    }
}

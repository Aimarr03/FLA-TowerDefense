using UnityEngine;


public class Classifier
{
    int baseHealth = 30;
    public Classifier(int baseHealth)
    {
        this.baseHealth = baseHealth;
    }
    public enum PlayerClassify
    {
        High,
        Medium,
        Low
    }
    public ClassificationResult Classify(RoundPerformance currentRound, RoundPerformance previousRound)
    {
        int currentRoundHealth = currentRound.RemainingHealth;
        int previousRoundHealth = previousRound.RemainingHealth;

        int healthDifference = previousRoundHealth - currentRoundHealth;
        PlayerClassify classifcationHealth = ClassifyBasedOnHealth(healthDifference);
        Debug.Log($"[Classifier] Health Difference: {healthDifference} (Previous: {previousRoundHealth}, Current: {currentRoundHealth})");
        
        
        PlayerClassify classificationEnemy = ClassifyBasedOnEnemyRemaining(currentRound);
        return new ClassificationResult
        {
            HealthClassification = classifcationHealth,
            EnemyClassification = classificationEnemy
        };
    }
    private PlayerClassify ClassifyBasedOnEnemyRemaining(RoundPerformance roundPerformance)
    {
        int remainingEnemies = roundPerformance.RemainingEnemy;
        int totalEnemies = roundPerformance.TotalEnemy;
        float delta = ((float)remainingEnemies) / totalEnemies;
        
        PlayerClassify classification = PlayerClassify.Medium;
        if (delta >= 0.7f)
        {
            classification = PlayerClassify.Low;
        }
        else if (delta >= 0.3f)
        {
            classification = PlayerClassify.Medium;
        }
        else
        {
            classification = PlayerClassify.High;
        }
        Debug.Log($"[Classifier] Enemy Remaining: {remainingEnemies}/{totalEnemies} (Delta: {delta:F2}) => Classification: {classification}");
        return classification;
    }
    private PlayerClassify ClassifyBasedOnHealth (int difference)
    {
        PlayerClassify classification = difference switch
        {
            >= 7 => PlayerClassify.Low,
            >= 3 => PlayerClassify.Medium,
            _ => PlayerClassify.High
        };
        return classification;
    }
    public struct ClassificationResult
    {
        public PlayerClassify HealthClassification;
        public PlayerClassify EnemyClassification;
    }
}

using UnityEngine;


public class Classifier
{
    int baseHealth = 30;
    public float Performance;
    public float HealthRatio;
    public float EnemyRatio;
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
    public int GetPerformance(RoundPerformance currentRound)
    {
        float performance = 0;
        float maxHealth = GameplayManager.instance.MainBase.MaxHealth;
        float healthRatio = ((float)currentRound.RemainingHealth / maxHealth);
        Debug.Log($"[Debug] Enemy, total: {currentRound.TotalEnemy} & remain: {currentRound.RemainingEnemy}");
        float enemyRatio = 1 - currentRound.normalizedEnemyHP;

        performance = (healthRatio * 0.6f)+(enemyRatio * 0.4f);
        HealthRatio = healthRatio;
        EnemyRatio = enemyRatio;
        Performance = performance;

        if(performance >= 0.8f)
            return +2;
        else if(performance >= 0.6f)
            return +1;
        else if(performance >= 0.4f)
            return 0;
        else
            return -1;
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

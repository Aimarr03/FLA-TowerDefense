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
    public void Classify(RoundPerformance currentRound, RoundPerformance previousRound)
    {
        int currentRoundHealth = currentRound.RemainingHealth;
        int previousRoundHealth = previousRound.RemainingHealth;

        int healthDifference = previousRoundHealth - currentRoundHealth;
        PlayerClassify classifcationHealth = ClassifyBasedOnHealth(healthDifference);
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
}

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArithmeticGeneration : MonoBehaviour
{
    [SerializeField] private QuestionPopUpHandler questionPopUpHandler;
    [SerializeField] private float questionDuration = 12f;
    [SerializeField] private float intervalSpawn = 3f;

    private float currentTick = 0f;
    private int maxQuestion = 0;
    private int currentQuestionCount = 0;
    private bool isActive = false;

    public Action OnCorrectAnswer;
    private ArithMeticDifficultyTier[] tiers = new ArithMeticDifficultyTier[]
    {
        new ArithMeticDifficultyTier(
            1,
            1,
            10,
            new OperatorType[]
            {
                OperatorType.Addition,
                OperatorType.Subtraction
            }),
        new ArithMeticDifficultyTier(
            1,
            1,
            20,
            new OperatorType[]
            {
                OperatorType.Addition,
                OperatorType.Subtraction,
                OperatorType.Multiplication
            }),
        new ArithMeticDifficultyTier(
            1,
            10,
            50,
            new OperatorType[]
            {
                OperatorType.Addition,
                OperatorType.Subtraction,
                OperatorType.Multiplication,
                OperatorType.Division
            })
    };
    private ArithMeticDifficultyTier currentTier = null;
    private void Awake()
    {
        currentTier = tiers[0];
    }
    private void Update()
    {
        if (!isActive) return;
        currentTick += Time.deltaTime;
        if(currentTick > intervalSpawn && currentQuestionCount < maxQuestion)
        {
            currentTick = 0f;
            OnCreateArithmeticQuestion();
        }
    }
    public void GenerateProblem(int maxQuestion)
    {
        currentTick = 0f;
        currentQuestionCount = 0;
        this.maxQuestion = maxQuestion;
        isActive = true;
        OnCreateArithmeticQuestion();
    }
    public void SetTier(int tier)
    {
        int indexTier = tier - 1;
        indexTier = Mathf.Clamp(indexTier, 0, tiers.Length - 1);
        
        currentTier = tiers[indexTier];
    }
    private void OnCreateArithmeticQuestion()
    {
        currentQuestionCount++;
        var arithmeticQuestion = CreateArithmeticQuestion();
        arithmeticQuestion.OnAnswered += (resultType) =>
        {
            if (resultType == ResultType.Correct)
            {
                OnCorrectAnswer?.Invoke();
            }
        };
        questionPopUpHandler.SpawnPopUp(arithmeticQuestion, questionDuration);

        if (currentQuestionCount >= maxQuestion)
        {
            isActive = false;
        }
    }
    public ArithmeticQuestion CreateArithmeticQuestion()
    {
        if (currentTier == null)
            currentTier = tiers[0];

        int a = Random.Range(currentTier.minRange, currentTier.maxRange);
        int b = Random.Range(currentTier.minRange, currentTier.maxRange);
        OperatorType operatorType = GetOperatorType();

        if(b > a)
        {
            int c = a;
            a = b;
            b = c;
        }
        return new ArithmeticQuestion(a, b, operatorType);
    }
    
    private OperatorType GetOperatorType()
    {
        int maxIndex = currentTier.operatorTypes.Length - 1;
        int index = Random.Range(0, maxIndex);
        return currentTier.operatorTypes[index];
    }
}
public class ArithMeticDifficultyTier
{
    public int tier;
    public int minRange;
    public int maxRange;
    public OperatorType[] operatorTypes;

    public ArithMeticDifficultyTier(int tier, int minRange, int maxRange,OperatorType[] operatorTypes)
    {
        this.tier = tier;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.operatorTypes = operatorTypes;
    }
}
public enum OperatorType
{
    Addition,
    Subtraction,
    Multiplication,
    Division
}
public enum ResultType
{
    None,
    Correct,
    Incorrect,
    OutOfTime
}
public class ArithmeticQuestion
{
    public int a;
    public int b;
    public OperatorType operatorType;
    public ResultType resultType;
    public int result { get; private set; }
    public Action<ResultType> OnAnswered;
    public void AnsweredQuestion(int answered)
    {
        if(resultType != ResultType.None) return;
        resultType = answered == result ? ResultType.Correct : ResultType.Incorrect;
        OnAnswered?.Invoke(resultType);
    }
    public void Timeout()
    {
        if (resultType != ResultType.None) return;
        resultType = ResultType.OutOfTime;
        OnAnswered?.Invoke(resultType);
    }
    public ArithmeticQuestion(int a, int b, OperatorType operatorType)
    {
        this.a = a;
        this.b = b;
        this.operatorType = operatorType;
        this.resultType = ResultType.None;
        result = operatorType switch
        {
            OperatorType.Addition => a + b,
            OperatorType.Subtraction => a - b,
            OperatorType.Multiplication => a * b,
            OperatorType.Division => a / b,
            _ => a
        };
    }
}

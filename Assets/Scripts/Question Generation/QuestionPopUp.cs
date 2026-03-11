using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPopUp : MonoBehaviour
{
    [SerializeField] private Image durationIndication;
    [SerializeField] private TextMeshProUGUI inputText;

    private float maxDuration;
    private float currentDuration;
    private bool isFinished = false;
    private bool isActive = false;
    private string currentInput = "";
    
    private ArithmeticQuestion question;
    private ArithmeticInputHandler inputHandler;
    private Button button;

    public ArithmeticQuestion Question => question;
    public Action<QuestionPopUp> OnFinishedQuestion;
    
    public static QuestionPopUp activePopUp { get; private set; }
    private void Awake()
    {
        button = GetComponent<Button>();
        inputHandler = FindAnyObjectByType<ArithmeticInputHandler>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SetNewActivePopUp());
    }

    public void Setup(ArithmeticQuestion question, float duration)
    {
        this.question = question;
        currentDuration = duration;
        maxDuration = duration;
        isFinished = false;
        currentInput = "0";
        inputText.text = $"{question.a} + {question.b} = {currentInput}";
    }

    private void Update()
    {
        if (isFinished && question.resultType == ResultType.None) return;

        currentDuration -= Time.deltaTime;
        durationIndication.fillAmount = currentDuration / maxDuration;
        if(currentDuration <= 0)
        {
            currentDuration = 0;
            question.resultType = ResultType.OutOfTime;

            Finish();
        }
    }

    private void SetNewActivePopUp()
    {
        if (activePopUp != null && activePopUp == this) return;

        //Make sure active pop up before change becomes inactive
        if(activePopUp != null && activePopUp != this)
        {
            activePopUp.DeactivatePopUp();
        }

        activePopUp = this;
        ActivatePopUp();
    }
    private void ActivatePopUp()
    {
        isActive = true;
        inputHandler.InputDigit += InputDigit;
        inputHandler.InputEnter += InputEnter;
        inputHandler.InputBackspace += InputBackspace;
    }
    private void DeactivatePopUp()
    {
        isActive = false;
        inputHandler.InputDigit -= InputDigit;
        inputHandler.InputEnter -= InputEnter;
        inputHandler.InputBackspace -= InputBackspace;
    }
    private void InputDigit(int digit)
    {
        if(!isActive) return;
        
        if (currentInput == "0") currentInput = "";
        currentInput += digit;
        inputText.text = $"{question.a} + {question.b} = {currentInput}";
    }
    private void InputBackspace()
    {
        if (!isActive) return;
        if (currentInput.Length == 0) return;
        
        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        if(currentInput.Length == 0)
        {
            currentInput = "0";
        }

        inputText.text = $"{question.a} + {question.b} = {currentInput}";
    }
    private void InputEnter()
    {
        if (!isActive) return;

        SubmitAnswer();
    }
    private void SubmitAnswer()
    {
        if(int.TryParse(currentInput, out int answer))
        {
            question.AnsweredQuestion(answer);
        }
        else
        {
            question.resultType = ResultType.Incorrect;
        }

        Finish();
    }
    private void Finish()
    {
        if (activePopUp != null && activePopUp == this)
        {
            activePopUp = null;
        }
        DeactivatePopUp();

        isFinished = true;
        OnFinishedQuestion?.Invoke(this);
    }
}

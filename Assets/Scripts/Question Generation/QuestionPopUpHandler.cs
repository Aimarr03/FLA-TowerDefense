using System.Collections.Generic;
using UnityEngine;

public class QuestionPopUpHandler : MonoBehaviour
{
    private ArithmeticGeneration arithmeticGeneration;

    private QuestionPopUp prefabPopUp;
    private float intervalToSpawn = 2f;
    private float currentTick = 0f;

    private List<QuestionPopUp> questionsSpawned;
    private void Awake()
    {
        prefabPopUp = GetComponentInChildren<QuestionPopUp>();
        arithmeticGeneration = GetComponent<ArithmeticGeneration>();

        questionsSpawned = new List<QuestionPopUp>();
        prefabPopUp.gameObject.SetActive(false);
    }

    private void Update()
    {
        currentTick += Time.deltaTime;
        if (currentTick > intervalToSpawn)
        {
            currentTick = 0f;
            SpawnPopUp();
        }
    }
    private void SpawnPopUp()
    {
        var activePopUp = Instantiate(prefabPopUp, transform);
        var arithmeticQuestion = arithmeticGeneration.CreateArithmeticQuestion();

        activePopUp.OnFinishedQuestion += OnPopUpAnswered;
        activePopUp.Setup(arithmeticQuestion, 12f);

        activePopUp.gameObject.SetActive(true);
        questionsSpawned.Add(activePopUp);
    }
    private void OnPopUpAnswered(QuestionPopUp questionPopUp)
    {
        var question = questionPopUp.Question;
        if (questionsSpawned.Contains(questionPopUp))
        {
            questionsSpawned.Remove(questionPopUp);
        }
        questionPopUp.gameObject.SetActive(false);
        Destroy(questionPopUp.gameObject, 1f);

        if(question.resultType == ResultType.Correct)
        {
            int result = Random.Range(6, 9);
            TD_API.Economy.GainMoney(result);
        }
    }
}

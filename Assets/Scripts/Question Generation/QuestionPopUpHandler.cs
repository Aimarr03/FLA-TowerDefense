using System.Collections.Generic;
using UnityEngine;

public class QuestionPopUpHandler : MonoBehaviour
{
    private QuestionPopUp prefabPopUp;
    private float intervalToSpawn = 2f;
    private float currentTick = 0f;

    private List<QuestionPopUp> questionsSpawned;
    private void Awake()
    {
        prefabPopUp = GetComponentInChildren<QuestionPopUp>();
        questionsSpawned = new List<QuestionPopUp>();
        prefabPopUp.gameObject.SetActive(false);
    }

    private void Update()
    {
        currentTick += Time.deltaTime;
        if (currentTick > intervalToSpawn)
        {
            currentTick = 0f;
            //SpawnPopUp();
        }
    }
    public void SpawnPopUp(ArithmeticQuestion question, float duration)
    {
        var activePopUp = Instantiate(prefabPopUp, transform);
        var arithmeticQuestion = question;

        activePopUp.OnFinishedQuestion += OnPopUpAnswered;
        activePopUp.Setup(arithmeticQuestion, duration);

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
    }
}

using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProblemPosingGenerator : MonoBehaviour
{
    [SerializeField] List<ProblemPosingQuestion> questions;

    private bool isActive = false;
    private ProblemPosingQuestion currentQuestion;

    [SerializeField] private TextMeshProUGUI questionTMP;
    private void Awake()
    {
        questions = Resources.LoadAll<ProblemPosingQuestion>("Problem Posing").ToList();
    }
    private void Update()
    {
        /// For testing purposes, we can toggle the problem posing generator with the space key.
        
        if (Input.GetKeyDown(KeyCode.Space) && !isActive)
        {
            isActive = true;
            if (isActive)
            {
                GenerateProblemPosingQuestion();
            }
        }
    }
    private void GenerateProblemPosingQuestion()
    {
        currentQuestion = questions[UnityEngine.Random.Range(0, questions.Count - 1)];
        questionTMP.text = currentQuestion.questionText;
    }
}

using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
public class ProblemPosingGenerator : MonoBehaviour
{
    [SerializeField] List<ProblemPosingQuestion> questions;

    private bool isActive = false;
    public bool IsActive => isActive;
    private ProblemPosingQuestion currentQuestion;

    [SerializeField] private TextMeshProUGUI questionTMP;
    [SerializeField] private List<TextMeshProUGUI> chosenSenteceTMP;

    List<ProblemPosingSentence> sentences;
    ProblemPosingSentence[] chosenSentence;
    private void Awake()
    {
        questions = Resources.LoadAll<ProblemPosingQuestion>("Problem Posing").ToList();
        sentences = GetComponentsInChildren<ProblemPosingSentence>().ToList();
        
    }
    private void Update()
    {
        /// For testing purposes, we can toggle the problem posing generator with the space key.
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isActive)
            {
                Debug.Log("Question is already on!");
                return;
            }
            isActive = true;
            GenerateProblemPosingQuestion();
        }
    }
    private void GenerateProblemPosingQuestion()
    {
        currentQuestion = questions[Random.Range(0, questions.Count - 1)];
        questionTMP.text = currentQuestion.questionText;
        chosenSentence = new ProblemPosingSentence?[3];

        List<(int index, string sentence)> sentences = new List<(int, string)>();
        int index = 0;
        foreach(var sentence in currentQuestion.possibleSentences)
        {
            sentences.Add((index, sentence));
            index++;
        }

        int iteration = Random.Range(1, 3);
        for(int x= 0; x < iteration; x++)
        {
            sentences.Shuffle();
        }
        for(int i = 0; i < sentences.Count; i++)
        {
            (int index, string sentence) sentence = sentences[i];
            this.sentences[i].SetSentence(sentence.sentence, sentence.index);
        }
    }
    public void SetSentence(ProblemPosingSentence posingSentence)
    {
        int index = Array.IndexOf(chosenSentence, null);
        if(index != -1)
        {
            chosenSentence[index] = posingSentence;
            chosenSenteceTMP[index].text = posingSentence.Sentence;
            
            /// Check if there are any sentences left that isn't null
            index = Array.IndexOf(chosenSentence, null);
            if (index == -1)
            {
                foreach (var sentence in sentences)
                {
                    if (!chosenSentence.Contains(sentence))
                    {
                        sentence.ToggleInterractability(false);
                    }
                }
            }
        }
    }
    public void RemoveSentece(ProblemPosingSentence posingSentence)
    {
        int index = Array.IndexOf(chosenSentence, posingSentence);
        if(index != -1)
        {
            chosenSenteceTMP[index].text = "";
            chosenSentence[index] = null;

            /// Check if there are any sentences left that isn't null
            index = Array.IndexOf(chosenSentence, null);
            if (index != -1)
            {
                foreach (var sentence in sentences)
                {
                    sentence.ToggleInterractability(true);
                }
            }
        }
    }
    public void SubmitAnswer()
    {
        bool isCorrect = true;
        for(int i = 0; i < currentQuestion.correctOrder.Count; i++)
        {
            int correctIndex = currentQuestion.correctOrder[i];
            Debug.Log($"chosen index: {chosenSentence[i]?.IndexSentence}, correct index: {correctIndex}");
            if (chosenSentence[i] == null || chosenSentence[i].IndexSentence != correctIndex)
            {
                isCorrect = false;
                break;
            }
        }

        for(int index = 0; index < chosenSentence.Length; index++)
        {
            var sentence = chosenSentence[index];
            if (sentence != null)
            {
                RemoveSentece(sentence);
            }
        }
        isActive = false;
        currentQuestion = null;
        foreach (var sentence in sentences)
        {
            sentence.SetEmpty();
            sentence.ToggleInterractability(false);
        }

        Debug.Log($"Is Correct: {isCorrect}");
    }
}

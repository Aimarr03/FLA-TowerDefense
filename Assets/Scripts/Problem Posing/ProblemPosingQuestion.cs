using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProblemPosingQuestion", menuName = "Scriptable Objects/ProblemPosingQuestion")]
public class ProblemPosingQuestion : ScriptableObject
{
    [TextArea(3, 10)]
    public string questionText;

    public List<string> possibleSentences;
    public List<int> correctOrder;
}

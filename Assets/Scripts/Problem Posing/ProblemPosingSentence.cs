using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProblemPosingSentence : MonoBehaviour
{
    private Image image;
    private Toggle toggle;
    private TextMeshProUGUI textTMP;

    private int indexSentence = -1;
    private int indexChosen =  -1;
    private string sentence;
    ProblemPosingGenerator generator;

    public int IndexSentence => indexSentence;
    public int IndexChosen => indexChosen;
    public string Sentence => sentence;
    private void Awake()
    {
        image = GetComponent<Image>();
        toggle = GetComponent<Toggle>();
        textTMP = GetComponentInChildren<TextMeshProUGUI>();
        generator = FindFirstObjectByType<ProblemPosingGenerator>();
        
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(ToggleBool);
        toggle.interactable = false;
    }

    public void SetSentence(string sentence, int index)
    {
        this.sentence = sentence;
        this.indexSentence = index;
        textTMP.text = sentence;
        toggle.interactable = true;
    }

    public void ToggleBool(bool value)
    {
        if (!generator.IsActive) return;
        image.color = value ? Color.green : Color.white;
        
        if (value)
        {
            generator.SetSentence(this);
        }
        else
        {
            generator.RemoveSentece(this);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProblemPosingSentence : MonoBehaviour
{
    private Image image;
    private Toggle toggle;
    private TextMeshProUGUI textTMP;

    private int indexSentence = -1;
    private string sentence;
    private bool isActive;
    ProblemPosingGenerator generator;

    public int IndexSentence => indexSentence;
    public string Sentence => sentence;
    private void Awake()
    {
        image = GetComponent<Image>();
        toggle = GetComponent<Toggle>();
        textTMP = GetComponentInChildren<TextMeshProUGUI>();
        generator = FindFirstObjectByType<ProblemPosingGenerator>();
        
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(ToggleBool);
        ToggleInterractability(false);
    }

    public void SetSentence(string sentence, int index)
    {
        this.sentence = sentence;
        this.indexSentence = index;
        textTMP.text = sentence;
        ToggleInterractability(true);
    }
    public void SetEmpty()
    {
        sentence = "";
        indexSentence = -1;
        textTMP.text = "";
        ToggleInterractability(false);
        ToggleVisual(false);
    }

    public void ToggleBool(bool value)
    {
        if (!generator.IsActive) return;
        ToggleVisual(value);

        if (value)
        {
            generator.SetSentence(this);
        }
        else
        {
            generator.RemoveSentece(this);
        }
    }
    private void ToggleVisual(bool value)
    {
        image.color = value ? Color.green : Color.white;
    }
    public void ToggleInterractability(bool value) => toggle.interactable = value;
}

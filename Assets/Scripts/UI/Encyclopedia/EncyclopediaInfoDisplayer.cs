using System.Collections.Generic;
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaInfoDisplayer : MonoBehaviour

{
    [Header("Layout")]
    [SerializeField] private RectTransform profileLayout;
    [SerializeField] private RectTransform descriptionLayout;
    [Header("Panel")]
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private Image Image;
    [SerializeField] private TextMeshProUGUI HP_UI;
    [SerializeField] private TextMeshProUGUI MS_UI;
    [SerializeField] private TextMeshProUGUI Description;
    [SerializeField] private TextMeshProUGUI Deskripsi;
    [SerializeField] private TextMeshProUGUI vocabTemplate;
    private List<TextMeshProUGUI> VocabsUI = new();
    
    public void UpdateInfo(EncyclopediaInfo info)
    {
        Name.text = info.Name;
        Image.sprite = info.Profile;

        HP_UI.text = info.HP.ToString();
        MS_UI.text = info.MS.ToString();

        Description.text = info.Description;
        Deskripsi.text = info.Deskripsi;

        if(VocabsUI.Count < info.Vocabularies.Count)
        {
            foreach(var vocab in info.Vocabularies)
            {
                var newVocabUI = Instantiate(vocabTemplate, vocabTemplate.transform.parent);
                newVocabUI.gameObject.SetActive(false);
                VocabsUI.Add(newVocabUI);
            }
        }
        foreach(var vocab in VocabsUI)
            vocab.gameObject.SetActive(false);

        for(int index = 0; index < info.Vocabularies.Count; index++)
        {
            var vocab = info.Vocabularies[index];
            var ui = VocabsUI[index];
            ui.text = vocab.ToString();
            ui.gameObject.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(profileLayout);
        LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionLayout);
    }
}

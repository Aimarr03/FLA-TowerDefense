using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class EncyclopediaManager : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform optionPanel;
    [SerializeField] private RectTransform unitPanel;
    [SerializeField] private List<EncyclopediaInfo> EncyInfos;
    [SerializeField] private Button templateButton;
    private List<Button> EnemyUnits;
    private List<Button> TowerUnits;
    private EncyclopediaInfoDisplayer infoDisplayer;
    void Awake()
    {
        EncyInfos = new();
        EncyInfos = Resources.LoadAll<EncyclopediaInfo>("Encyclopedia/").ToList();
        EnemyUnits = new();
        TowerUnits = new();
        
        infoDisplayer = FindFirstObjectByType<EncyclopediaInfoDisplayer>(FindObjectsInactive.Include);
        foreach(var ency in EncyInfos)
        {
            var newButton = Instantiate(templateButton, templateButton.transform.parent);
            newButton.gameObject.SetActive(false);
            
            var tmp = newButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var image = newButton.transform.GetChild(0).GetComponent<Image>();
            tmp.text = ency.Name;
            image.sprite = ency.Profile;
            
            if(ency.EncyType == EncyclopediaType.Enemy)
                EnemyUnits.Add(newButton);
            else if(ency.EncyType == EncyclopediaType.Tower)
            {
                TowerUnits.Add(newButton);
                image.transform.localScale = Vector3.one * 2;
            }
                

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() => infoDisplayer.UpdateInfo(ency));
            newButton.onClick.AddListener(OpenUnitPanel);
        }
        CloseMainPanel();
    }
    public void OpenTowerUnits()
    {
        foreach(var tower in TowerUnits)
        {
            tower.gameObject.SetActive(true);
        }
        foreach(var enemy in EnemyUnits)
        {
            enemy.gameObject.SetActive(false);
        }
    }
    public void OpenEnemyUnits()
    {
        foreach(var tower in TowerUnits)
        {
            tower.gameObject.SetActive(false);
        }
        foreach(var enemy in EnemyUnits)
        {
            enemy.gameObject.SetActive(true);
        }
    }

    public void OpenMainPanel()
    {
        background.gameObject.SetActive(true);
        panel.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(true);
        optionPanel.gameObject.SetActive(false);
        unitPanel.gameObject.SetActive(false);
    }

    public void CloseMainPanel()
    {
        background.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(false);
        optionPanel.gameObject.SetActive(false);
        unitPanel.gameObject.SetActive(false);
    }
    public void OpenOptionPanel()
    {
        background.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(false);
        optionPanel.gameObject.SetActive(true);
        unitPanel.gameObject.SetActive(false);
    }
    public void OpenUnitPanel()
    {
        background.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(false);
        optionPanel.gameObject.SetActive(false);
        unitPanel.gameObject.SetActive(true);
    }

}

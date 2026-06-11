using UnityEngine;
using System;
public class EncyclopediaManager : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform optionPanel;
    [SerializeField] private RectTransform unitPanel;
    void Awake()
    {
        CloseMainPanel();
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

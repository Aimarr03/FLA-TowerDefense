using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerActionDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Header;
    [SerializeField] private List<TextMeshProUGUI> Descriptions;

    void Awake()
    {
        HideActionContext();
    }
    public void ShowActionContext(ActionContext context)
    {
        gameObject.SetActive(true);
        Header.text = context.actionName;

        var actionDescriber = context.actionDescription;
        
        foreach(var desc in Descriptions)
        {
            desc.gameObject.SetActive(false);
        }
        for(int index = 0; index < Descriptions.Count; index++)
        {
            var desc = Descriptions[index];

            if(index < actionDescriber.Count)
            {
                desc.gameObject.SetActive(true);
                desc.text = actionDescriber[index];
            }
        }
        var parentRect = Descriptions[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }
    public void HideActionContext()
    {
        foreach(var desc in Descriptions)
        {
            desc.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}

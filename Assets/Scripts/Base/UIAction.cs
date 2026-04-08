using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class UIAction : MonoBehaviour
{
    [SerializeField] private TowerActionButton uiButton;
    [SerializeField] private float distance = 180f;
    List<TowerActionButton> buttons;

    private int startingButtons = 5;
    ActionContext selectedContext;
    TowerActionButton selectedButton;
    private void Awake()
    {
        buttons = new List<TowerActionButton>();
        CreateButton(startingButtons);
    }
    private void CreateButton(int count)
    {
        for (int index = 0; index < count; index++)
        {
            TowerActionButton newButton = Instantiate(uiButton, transform);
            newButton.gameObject.SetActive(false);
            buttons.Add(newButton);
        }
    }
    public void OnDisplayAction(List<ActionContext> clickAction)
    {
        int clickCounts = clickAction.Count;
        Debug.Log($"Click counts: {clickCounts}");
        if (clickCounts > buttons.Count)
        {
            int difference = clickCounts - buttons.Count;
            CreateButton(difference);
        }
        
        for(int index = 0; index < buttons.Count; index++)
        {
            TowerActionButton button = buttons[index];
            RectTransform rectButton = button.GetComponent<RectTransform>();
            
            button.gameObject.SetActive(false);
            rectButton.anchoredPosition = Vector3.zero;
        }

        
        float stepAngle = (clickCounts == 1) ? 0f : 360f / clickCounts;        
        for (int index = 0; index < clickAction.Count; index++) 
        {
            TowerActionButton button = buttons[index];
            RectTransform rectButton = button.GetComponent<RectTransform>();

            button.gameObject.SetActive(true);
            button.Button.onClick.RemoveAllListeners();

            var context = clickAction[index];
            button.iconAction = context.actionIcon;
            button.Button.onClick.AddListener(() => OnActionClicked(context, button));
            button.SetActionIcon();
            button.isExecutable = context.isExecutable;

            if (context.useMoney)
            {
                button.EnableMoneyText();
                button.SetMoneyText(context.actionCost);
            }
            else
            {
                button.DisableMoneyText();
            }

            float angle = stepAngle * index;
            float radians = (angle + 90f) * Mathf.Deg2Rad;

            Vector2 buttonNewPos = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;
            rectButton.anchoredPosition = buttonNewPos;
        }
    }
    private void OnActionClicked(ActionContext context, TowerActionButton button)
    {
        if(selectedContext == context)
        {
            if (selectedContext.isExecutable())
            {
                context.clickEvent?.Invoke();
                CloseAction();
            }
            else
            {
                Debug.Log("It's not Executable");
            }
            return;
        }

        SelectAction(context, button);
    }
    private void SelectAction(ActionContext context, TowerActionButton button)
    {
        bool condition = selectedButton != null && selectedButton != button;
        Debug.Log($"Condition: {condition}");
        if (selectedButton != null && selectedButton != button)
        {
            //Deselectbutton
            selectedButton.SetActionIcon();
        }
        selectedContext = context;
        selectedButton = button;
        button.SetConfirmIcon();
        Debug.Log($"Action: {context.actionName}");
        Debug.Log($"Desc: {context.actionDescription}");
        //Select Button and Show Tooltip
    }
    public void CloseAction()
    {
        if(selectedButton != null)
        {
            //Deselectbutton
            selectedButton.SetActionIcon();
        }

        selectedContext = null;
        selectedButton = null;
        foreach (TowerActionButton button in buttons)
        {
            button.gameObject.SetActive(false);
            RectTransform buttonRect = button.GetComponent<RectTransform>();

            buttonRect.anchoredPosition = Vector2.zero;
        }
    }
}



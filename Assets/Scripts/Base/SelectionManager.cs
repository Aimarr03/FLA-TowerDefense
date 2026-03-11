using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public static partial class TD_API
{
    static internal SelectionManager selectionManager;
    public static I_MouseInteractable HighlightedObject => selectionManager.HighlightedObject;
    public static I_MouseInteractable SelectedObject => selectionManager.SelectedObject;
}
public class SelectionManager : MonoBehaviour
{
    [SerializeField] LayerMask clickableLayer;

    [Header("Debug")]
    [SerializeField] Vector2 worldPoint;
    [SerializeField] bool isOnUI;


    [Header("Selection")]
    [SerializeReference] I_MouseInteractable highlightedObject;
    [SerializeReference] I_MouseInteractable selectedObject;
    
    internal I_MouseInteractable HighlightedObject => highlightedObject;
    internal I_MouseInteractable SelectedObject => selectedObject;
    private void Awake()
    {
        TD_API.selectionManager = this;
    }
    void Update()
    {
        HoveringLogic();
        LeftClickAction();
    }
    private void HoveringLogic()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            isOnUI = true;
            return;
        }
        isOnUI = false;

        Vector3 mousePos = Mouse.current.position.ReadValue();
        worldPoint = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, clickableLayer);
        if (hit.collider != null)
        {
            hit.collider.TryGetComponent(out I_MouseInteractable newInterractable);
            if (newInterractable == null) return;


            if (highlightedObject != newInterractable)
            {
                newInterractable.OnHighlighted();
                var mb = highlightedObject as MonoBehaviour;
                if (mb != null) // Unity destroyed check works here
                {
                    highlightedObject.OnDehighlighted();
                }

                highlightedObject = newInterractable;
                Debug.Log($"Highlighted Object: {highlightedObject.GetType()}");
            }
        }
        else
        {
            if (highlightedObject != null)
            {
                var mb = highlightedObject as MonoBehaviour;
                if (mb != null) // Unity destroyed check works here
                {
                    highlightedObject.OnDehighlighted();
                }
                highlightedObject = null;
            }
        }
    }
    private void LeftClickAction()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (highlightedObject != null)
            {
                if (selectedObject != null) selectedObject.OnMouseDeselect();

                selectedObject = highlightedObject;
                selectedObject.OnMouseSelect();
                selectedObject.OnMouseLeftDown();
                Debug.Log($"Selected Object: {selectedObject.GetType()}");
            }
            else
            {
                Debug.Log("Reset Selection");
                ResetSelection();
            }
        }
    }
    public void ResetSelection()
    {
        if (selectedObject != null) selectedObject.OnMouseDeselect();
        selectedObject = null;
    }
    private void CheckHover()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            Debug.Log("UI blocking pointer:");

            foreach (var result in results)
            {
                Debug.Log($"• {result.gameObject.name} | Canvas: {result.gameObject.GetComponentInParent<Canvas>()?.name}");
            }

            Debug.Log($"TOP UI: {results[0].gameObject.name}");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class TowerActionButton : MonoBehaviour
{
    [SerializeField] private Image iconRenderer;
    private Button button;
    private Image backgroundRenderer;
    
    public Sprite iconConfirm;
    public Sprite iconAction;
    public Button Button => button;
    public Image IconRenderer => iconRenderer;
    private void Awake()
    {
        button = GetComponent<Button>();
        backgroundRenderer = GetComponent<Image>();
    }
    public void SetConfirmIcon() => iconRenderer.sprite = iconConfirm;
    public void SetActionIcon() => iconRenderer.sprite = iconAction;
}

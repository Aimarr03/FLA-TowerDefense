using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerActionButton : MonoBehaviour
{
    [SerializeField] private Image iconRenderer;
    [SerializeField] private Material material;

    public Func<bool> isExecutable;

    private Button button;
    private Image backgroundRenderer;
    private Material instanceMaterial;

    public Sprite iconConfirm;
    public Sprite iconAction;

    public Button Button => button;
    public Image IconRenderer => iconRenderer;
    private void Awake()
    {
        button = GetComponent<Button>();
        backgroundRenderer = GetComponent<Image>();

        var mat = Instantiate(material);
        iconRenderer.material = mat;
        backgroundRenderer.material = mat;
        instanceMaterial = mat;
    }
    private void Update()
    {
        if(isExecutable == null)
        {
            return;
        }
        if (isExecutable())
        {
            SetInterractableVisual();
        }
        else
        {
            SetUninterractableVisual();
        }
    }
    public void SetConfirmIcon() => iconRenderer.sprite = iconConfirm;
    public void SetActionIcon() => iconRenderer.sprite = iconAction;

    public void SetUninterractableVisual()
    {
        instanceMaterial.SetColor("_Color", new Color(0.6f, 0.6f, 0.6f));
        instanceMaterial.SetFloat("_Alpha", 0.8f);
    }
    public void SetInterractableVisual()
    {
        instanceMaterial.SetColor("_Color", new Color(1f, 1f, 1f));
        instanceMaterial.SetFloat("_Alpha", 1f);
    }
}

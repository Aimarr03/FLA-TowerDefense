using System;
using TMPro;
using UnityEngine;

public class UI_PlayersHealth : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private MainBase mainBase;

    private void Awake()
    {
        mainBase.OnUpdatedHealth += OnUpdatedHealth;
    }

    private void OnUpdatedHealth((int maxHealth, int currentHealth) healthData)
    {
        healthText.text = $"{healthData.currentHealth}/{healthData.maxHealth}";
    }
}

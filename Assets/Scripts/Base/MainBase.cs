using System;
using UnityEngine;

public class MainBase : MonoBehaviour, I_MouseInteractable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI")]
    [SerializeField] HealthBar healthBar;
    [Space]
    public Action OnDeath;
    public Action<(int maxHealth, int currentHealth)> OnUpdatedHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public void Setup(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        healthBar.UpdateHealth(1f);
        OnUpdatedHealth?.Invoke(((int)maxHealth, (int)currentHealth));
    }
    private void Start()
    {
        GameplayManager.instance.onchangedState += OnGameStateChanged;
    }
    private void OnDestroy()
    {
        GameplayManager.instance.onchangedState -= OnGameStateChanged;
    }
    public void OnGameStateChanged(GameplayManager.State state)
    {
        if(state == GameplayManager.State.Building)
        {
            healthBar.Hide();
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        healthBar.UpdateHealth(currentHealth / maxHealth);
        OnUpdatedHealth?.Invoke(((int)maxHealth, (int)currentHealth));
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            OnDeath = null;
        }
    }

    public void OnHighlighted()
    {
        
    }

    public void OnDehighlighted()
    {
        
    }

    public void OnMouseLeftDown()
    {
        
    }

    public void OnMouseSelect()
    {
        
    }

    public void OnMouseDeselect()
    {
        
    }
}

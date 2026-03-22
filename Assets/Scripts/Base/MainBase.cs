using System;
using UnityEngine;

public class MainBase : MonoBehaviour, I_MouseInteractable
{
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] private float CurrentHealth;

    HealthBar healthBar;
    public Action OnDeath;
    public Action<(int maxHealth, int currentHealth)> OnUpdatedHealth;
    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>(true);
        CurrentHealth = MaxHealth;
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
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);
        healthBar.UpdateHealth(CurrentHealth / MaxHealth);
        OnUpdatedHealth?.Invoke(((int)MaxHealth, (int)CurrentHealth));
        if (CurrentHealth <= 0)
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

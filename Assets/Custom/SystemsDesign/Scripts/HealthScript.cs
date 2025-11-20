using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthScript : MonoBehaviour
{
    public static event Action<int> OnHealthChanged;

    //private UiManager uiManager;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        //uiManager = GetComponent<UiManager>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify listeners that health changed
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GainHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        //uiManager.GreenFlash();
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }
}

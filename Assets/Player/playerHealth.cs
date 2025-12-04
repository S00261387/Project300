using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    [Header("HUD health")]
    public Image healthMask; 

    [HideInInspector] public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth < 0f)
            currentHealth = 0f;

        UpdateHealthUI(); 
    }

    void UpdateHealthUI()
    {
        if (healthMask != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthMask.fillAmount = 1f - healthPercent;
            Debug.Log("HP = " + currentHealth + "  fillAmount = " + healthMask.fillAmount);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    [Header("HUD здоровья")]
    public Image healthMask; // чёрная полоска поверх красной

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

        UpdateHealthUI(); // ВАЖНО: обновляем полосу каждый раз, когда хп меняется
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

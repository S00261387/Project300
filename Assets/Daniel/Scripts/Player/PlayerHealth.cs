using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health;
    public float currentHealth;
    public float invincibleCd;
    public float invincibleCdTimer;
    public bool invincible;

    private void Awake()
    {
        currentHealth = health;
    }
    public void OnHit(float damage)
    {
        currentHealth -= damage;
        invincible = true;
        invincibleCdTimer = invincibleCd;
    }
    void Update()
    {
        if (invincible)
        {
            invincibleCdTimer -= Time.deltaTime;
            if (invincibleCdTimer <= 0)
            {
                invincible = false;
            }
        }
    }
}

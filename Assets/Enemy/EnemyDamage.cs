using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damagePerSecond = 2f; // 2 хп в секунду

    private void OnTriggerStay(Collider other)
    {
        // Проверяем, есть ли у вошедшего объекта компонент PlayerHealth
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.currentHealth > 0f)
        {
            // наносим урон каждый кадр, считая по времени
            float damageThisFrame = damagePerSecond * Time.deltaTime;
            playerHealth.TakeDamage(damageThisFrame);
        }
    }
}

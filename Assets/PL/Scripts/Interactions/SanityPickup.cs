using UnityEngine;

public class SanityPickup : MonoBehaviour
{
    public int maxSanityIncrease = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {;
            SanityManager.Instance.IncreaseMaxSanity(maxSanityIncrease);

            Destroy(gameObject);
        }
    }
}
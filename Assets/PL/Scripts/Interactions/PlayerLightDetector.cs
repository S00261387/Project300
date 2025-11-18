using UnityEngine;

public class PlayerLightDetector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightZone"))
            SanityManager.Instance.isInDarkness = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LightZone"))
            SanityManager.Instance.isInDarkness = true;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SanityBar : MonoBehaviour
{
    public Image fillImage;
    public float sanity = 100f;

    public void SetSanity(float value)
    {
        fillImage.fillAmount = value;
    }
}

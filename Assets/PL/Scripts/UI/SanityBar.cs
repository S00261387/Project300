using UnityEngine;
using UnityEngine.UI;

public class SanityBar : MonoBehaviour
{
    public Image fillImage;
    public float sanity = 100f;

    public void SetSanity(float value)
    {
        fillImage.color = Color.Lerp(Color.red, Color.green, sanity / 100f);
        sanity = Mathf.Clamp(value, 0f, 100f);
        fillImage.fillAmount = sanity / 100f;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Brainzone : MonoBehaviour
{
    public Image zoneImage; // какой Image красим
    public Color filledColor = Color.red;
    public Color defaultColor = Color.white;
    bool isFilled = false;

    public void OnZoneClick()
    {
        if (zoneImage == null) return;

        isFilled = !isFilled;
        zoneImage.color = isFilled ? filledColor : defaultColor;
    }
}
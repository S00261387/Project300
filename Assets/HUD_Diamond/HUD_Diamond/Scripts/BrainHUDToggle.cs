using UnityEngine;
using UnityEngine.UI;

// Скрипт для включения/выключения отдельного Canvas с мозгом
public class BrainHUDToggle : MonoBehaviour
{
    // Canvas или корневой объект с мозгом (например, BrainCanvas)
    public GameObject brainCanvas;
    // Клавиша для открытия/закрытия
    public KeyCode toggleKey = KeyCode.Tab;
    public static bool IsBrainOpen = false;

    void Start()
    {
        if (brainCanvas != null)
            brainCanvas.SetActive(false);
        IsBrainOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && brainCanvas != null)
        {
            bool newState = !brainCanvas.activeSelf;
            brainCanvas.SetActive(newState);
            IsBrainOpen = newState;
        }
    }
}

    // ------------------------------------------------------------
    // Скрипт для закрашивания части мозга по клику по зоне

    public class BrainZone : MonoBehaviour
{
    // Какой Image красим (часть мозга: top1, left, back и т.д.)
    public Image zoneImage;
    // Цвет закрашенной зоны
    public Color filledColor = Color.red;

    // Цвет по умолчанию (изначальный цвет спрайта)
    public Color defaultColor = Color.white;

    // Флаг: закрашена ли сейчас зона
    bool isFilled = false;

    // Вызывать из OnClick у кнопки-зоны
    public void OnZoneClick()
    {
        if (zoneImage == null)
            return;

        isFilled = !isFilled;
        zoneImage.color = isFilled ? filledColor : defaultColor;
    }
}
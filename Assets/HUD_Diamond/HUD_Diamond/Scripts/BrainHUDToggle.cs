using UnityEngine;
using UnityEngine.UI;


public class BrainHUDToggle : MonoBehaviour
{
    
    public GameObject brainCanvas;
    
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


    public class BrainZone : MonoBehaviour
{
    
    public Image zoneImage;
    
    public Color filledColor = Color.red;

    
    public Color defaultColor = Color.white;

    
    bool isFilled = false;

    
    public void OnZoneClick()
    {
        if (zoneImage == null)
            return;

        isFilled = !isFilled;
        zoneImage.color = isFilled ? filledColor : defaultColor;
    }
}
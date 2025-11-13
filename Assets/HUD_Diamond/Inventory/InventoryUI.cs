using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;  
    public KeyCode openKey = KeyCode.Tab;

    private bool isOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(openKey))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;

        inventoryPanel.SetActive(isOpen);

        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

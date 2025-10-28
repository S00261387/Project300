using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    private Vector2 lookInput;
    private float mouseX;
    private float mouseY;
    private float xRotation;

    [Header("Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] protected float HorizontalSensitivity = 1.0f;
    [SerializeField] protected float VerticalSensitivity = 1.0f;
    [SerializeField] private bool IsCursorVisible = true;

    private void Awake()
    {
        Cursor.visible = IsCursorVisible;
    }
    public void OnLook(InputAction.CallbackContext obj)
    {
        lookInput = obj.ReadValue<Vector2>();
        UpdateLook();
    }

    protected virtual void UpdateLook()
    {
        mouseX = lookInput.x * HorizontalSensitivity * Time.deltaTime;
        mouseY = lookInput.y * VerticalSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);   
    }
}

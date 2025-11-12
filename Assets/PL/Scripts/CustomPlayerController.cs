using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CustomPlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Vector3 topDownOffset = new Vector3(0f, 10f, 0f);
    public float topDownAngle = 90f;
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isFirstPerson = false;
    private float verticalLookRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        SetCameraTopDown();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleModeSwitch();

        if (isFirstPerson)
            HandleFirstPerson();
        else
            HandleTopDown();
    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                SetCameraFirstPerson();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                SetCameraTopDown();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void HandleTopDown()
    {
        // --- ZQSD movement ---
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Rotate toward mouse
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }

        // Camera follow (fixed)
        playerCamera.transform.parent = null;
        Vector3 targetPos = transform.position + topDownOffset;
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPos, Time.deltaTime * 10f);
        playerCamera.transform.rotation = Quaternion.Euler(topDownAngle, 0f, 0f);
    }

    void HandleFirstPerson()
    {
        // --- Mouse look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (local)
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        // --- ZQSD movement ---
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        Vector3 move = (transform.forward * moveZ + transform.right * moveX).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void SetCameraTopDown()
    {
        playerCamera.transform.parent = null;
        playerCamera.transform.position = transform.position + topDownOffset;
        playerCamera.transform.rotation = Quaternion.Euler(topDownAngle, 0f, 0f);
    }

    void SetCameraFirstPerson()
    {
        playerCamera.transform.SetParent(transform);
        playerCamera.transform.localPosition = firstPersonOffset;
        playerCamera.transform.localRotation = Quaternion.identity;
        verticalLookRotation = 0f;
    }
}

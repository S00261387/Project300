using UnityEngine;

public class CustomPlayerController : MonoBehaviour
{
    [Header("General Settings")]
    public Camera playerCamera;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("View Modes")]
    public bool isFirstPerson = false;
    public KeyCode switchKey = KeyCode.R;

    [Header("First Person Settings")]
    public Transform firstPersonCameraPos;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private Renderer[] playerRenderers;

    private string currentAnimation = "";

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (!isFirstPerson && playerCamera != null)
        {
            playerCamera.transform.SetParent(null);
            playerCamera.transform.position = transform.position + Vector3.up * 10f;
            playerCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        animator = GetComponent<Animator>();
        playerRenderers = GetComponentsInChildren<Renderer>(true);
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
        if (Input.GetKeyDown(switchKey))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                playerCamera.transform.SetParent(firstPersonCameraPos);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
                Cursor.lockState = CursorLockMode.Locked;

                foreach (var r in playerRenderers)
                    r.enabled = false;
            }
            else
            {
                playerCamera.transform.SetParent(null);
                playerCamera.transform.position = transform.position + Vector3.up * 10f;
                playerCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                Cursor.lockState = CursorLockMode.None;

                foreach (var r in playerRenderers)
                    r.enabled = true;
            }
        }
    }

    void HandleTopDown()
    {
        Vector3 move = Vector3.zero;
        move.z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        move.x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        move = move.normalized;

        controller.SimpleMove(move * moveSpeed);

        // Look at mouse
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDir = (hitPoint - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }

        // Animation control
        string newAnim = "Idle";

        if (move.sqrMagnitude >= 0.01f)
        {
            Vector3 localMove = transform.InverseTransformDirection(move);

            if (Mathf.Abs(localMove.z) > Mathf.Abs(localMove.x))
            {
                newAnim = (localMove.z > 0f) ? "WalkForward" : "WalkBackward";
            }
            else
            {
                newAnim = (localMove.x > 0f) ? "WalkRight" : "WalkLeft";
            }
        }

        // Only change animation if it's different
        if (newAnim != currentAnimation)
        {
            currentAnimation = newAnim;
            animator.CrossFade(currentAnimation, 0.15f);
        }

        // Keep camera following player
        if (playerCamera != null)
        {
            Vector3 camPos = playerCamera.transform.position;
            camPos.x = transform.position.x;
            camPos.z = transform.position.z;
            playerCamera.transform.position = camPos;
        }
    }

    void HandleFirstPerson()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -85f, 85f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        Vector3 move = Vector3.zero;
        move += transform.forward * ((Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f));
        move += transform.right * ((Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f));

        controller.SimpleMove(move.normalized * moveSpeed);
    }
}

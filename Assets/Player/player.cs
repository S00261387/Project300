using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;  
    public float staminaRegenPerSecond = 10f;  
    public float minStaminaToRun = 20f;        
    public Image staminaMask;                  

    private float currentStamina;
    private bool isTired = false;              

    void Start()
    {
        currentStamina = maxStamina;

        if (staminaMask != null)
            staminaMask.fillAmount = 0f;       
    }

    void Update()
    {
        if (BrainHUDToggle.IsBrainOpen)
            return;
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

        moveDirection = moveDirection.normalized;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isMoving = moveDirection.sqrMagnitude > 0.01f;

        
        bool canRunNow = !isTired && currentStamina > 0f;
        bool isRunning = wantsToRun && isMoving && canRunNow;

        float speed = isRunning ? runSpeed : walkSpeed;
        transform.position += moveDirection * speed * Time.deltaTime;

       
        if (isRunning)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;

            
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isTired = true;
            }
        }
        else
        {
            
            currentStamina += staminaRegenPerSecond * Time.deltaTime;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            
            if (isTired && currentStamina >= minStaminaToRun)
            {
                isTired = false;
            }
        }

       
        float staminaPercent = currentStamina / maxStamina;

        if (staminaMask != null)
            staminaMask.fillAmount = 1f - staminaPercent;
    }
}

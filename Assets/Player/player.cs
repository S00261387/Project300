using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Энергия")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;  // расход при беге
    public float staminaRegenPerSecond = 10f;  // регенерация при ходьбе/стоянии
    public float minStaminaToRun = 20f;        // сколько нужно, чтобы снова разрешить бег
    public Image staminaMask;                  // чёрная полоса поверх синей

    private float currentStamina;
    private bool isTired = false;              // флаг: выдохся и временно бег запрещён

    void Start()
    {
        currentStamina = maxStamina;

        if (staminaMask != null)
            staminaMask.fillAmount = 0f;       // в начале ничего не закрыто
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

        // Бежать можно только если НЕ устал и есть хоть немного стамины
        bool canRunNow = !isTired && currentStamina > 0f;
        bool isRunning = wantsToRun && isMoving && canRunNow;

        float speed = isRunning ? runSpeed : walkSpeed;
        transform.position += moveDirection * speed * Time.deltaTime;

        // ---------- ЛОГИКА СТАМИНЫ ----------
        if (isRunning)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;

            // Если полностью выдохся – обнуляем и ставим флаг "устал"
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isTired = true;
            }
        }
        else
        {
            // Восстанавливаем стамину
            currentStamina += staminaRegenPerSecond * Time.deltaTime;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            // Если был устал и стамина восстановилась хотя бы до порога – снова разрешаем бег
            if (isTired && currentStamina >= minStaminaToRun)
            {
                isTired = false;
            }
        }

        // Обновляем чёрную маску (столько потрачено)
        float staminaPercent = currentStamina / maxStamina;

        if (staminaMask != null)
            staminaMask.fillAmount = 1f - staminaPercent;
    }
}

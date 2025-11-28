using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvisible : MonoBehaviour
{
    public bool Invisible;
    MeshRenderer mesh;
    Material material;
    public Material Translucent;
    [SerializeField] float invisCd;
    float invisCdTimer;

    [SerializeField] float Cooldown;
    float CooldownTimer;
    bool cooldownActive;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
    }

    private void Update()
    {
        if (Invisible)
        {
            mesh.material = Translucent;
            invisCdTimer -= Time.deltaTime;
            if (invisCdTimer <= 0)
            {
                CooldownTimer = Cooldown;
                cooldownActive = true;
                Invisible = false;
            }
        }
        else
        {
            mesh.material = material;
        }


        if (cooldownActive)
        {
            CooldownTimer -= Time.deltaTime;
            if (CooldownTimer <= 0)
            {
                cooldownActive = false;
            }
        }

      
    }

    public void OnInvisible(InputAction.CallbackContext obj)
    {
        if (CooldownTimer <= 0 && invisCdTimer <= 0)
        {
            invisCdTimer = invisCd;
            Invisible = true;

        }
        else
        {
            return;
        }
    }
}

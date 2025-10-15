using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
//[RequireComponent(typeof(PlayerCamera))]
//[RequireComponent(typeof(PlayerAttack))]
//[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
   private PlayerInput playerInput;
   //private PlayerCamera PlayerCamera;
   private PlayerMovement playerMovement;
   //private PlayerInteraction playerInteraction;
    //private PlayerAttack playerAttack;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        //playerInteraction = GetComponent<PlayerInteraction>();
        //PlayerCamera = GetComponent<PlayerCamera>();
        //playerAttack = GetComponent<PlayerAttack>();
    }

    private void OnEnable()
    {
        BindInputs();
    }

    private void OnDisable()
    {
        UnBindInputs();
    }

    protected virtual void BindInputs()
    {
        //playerInput.actions["OpenMenu"].performed += OnOpenMenu;
        //playerInput.actions["CloseMenu"].performed += OnCloseMenu;

        playerInput.actions["Move"].performed += playerMovement.OnMove;
        playerInput.actions["Move"].canceled += playerMovement.OnMove;
        playerInput.actions["Jump"].performed += playerMovement.OnJump;
        //playerInput.actions["Crouch"].performed += playerMovement.OnCrouch;
        //playerInput.actions["Sprint"].performed += playerMovement.OnSprint;
        playerInput.actions["Dash"].performed += playerMovement.OnDash;

        //playerInput.actions["Look"].performed += PlayerCamera.OnLook;
        //playerInput.actions["Interact"].performed += playerInteraction.OnInteract;
        //playerInput.actions["Attack"].performed += playerAttack.OnAttack;
    }

    protected virtual void UnBindInputs()
    {
        //playerInput.actions["OpenMenu"].performed -= OnOpenMenu;
        //playerInput.actions["CloseMenu"].performed -= OnCloseMenu;

        playerInput.actions["Move"].performed -= playerMovement.OnMove;
        playerInput.actions["Move"].canceled -= playerMovement.OnMove;
        playerInput.actions["Jump"].performed -= playerMovement.OnJump;
        //playerInput.actions["Crouch"].performed -= playerMovement.OnCrouch;
        //playerInput.actions["Sprint"].performed -= playerMovement.OnSprint;
        playerInput.actions["Dash"].performed -= playerMovement.OnDash;

        //playerInput.actions["Look"].performed -= PlayerCamera.OnLook;
        //playerInput.actions["Interact"].performed -= playerInteraction.OnInteract;
        //playerInput.actions["Attack"].performed -= playerAttack.OnAttack;
    }

    //private void OnCloseMenu(InputAction.CallbackContext obj)
    //{
    //    GameMode.Instance.HideInventory();
    //    playerInput.SwitchCurrentActionMap("Game");
    //}

    //private void OnOpenMenu(InputAction.CallbackContext obj)
    //{
    //    GameMode.Instance.ShowInventory(GetComponent<Inventory>());
    //    playerInput.SwitchCurrentActionMap("Menu");
    //}
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerRotation))]
//[RequireComponent(typeof(PlayerAttack))]
//[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerThrow))]
public class PlayerController : MonoBehaviour
{
   private PlayerInput playerInput;
    private PlayerRotation playerRotation;
    private PlayerMovement playerMovement; 
    private PlayerThrow playerThrow;
   //private PlayerInteraction playerInteraction;
    //private PlayerAttack playerAttack;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerThrow = GetComponent<PlayerThrow>();
        //playerInteraction = GetComponent<PlayerInteraction>();
        playerRotation = GetComponent<PlayerRotation>();
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
        playerInput.actions["Throw"].performed += playerThrow.OnThrow;
        ////playerInput.actions["Look"].performed += PlayerRotation.OnLook;
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
        playerInput.actions["Throw"].performed -= playerThrow.OnThrow;

        playerInput.actions["Look"].performed -= playerRotation.OnLook;
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

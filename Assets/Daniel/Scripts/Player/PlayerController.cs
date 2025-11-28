using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
//[RequireComponent(typeof(PlayerAttack))]
//[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerThrow))]
[RequireComponent(typeof(PlayerInvisible))]
public class PlayerController : MonoBehaviour
{
   private PlayerInput playerInput;
    private PlayerMovement playerMovement; 
    private PlayerThrow playerThrow;
    private PlayerInvisible playerInvisible;
   // private PlayerCamera playerCamera;
   //private PlayerInteraction playerInteraction;
    //private PlayerAttack playerAttack;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerThrow = GetComponent<PlayerThrow>();
        playerInvisible = GetComponent<PlayerInvisible>();
      //  playerCamera = GetComponent<PlayerCamera>();
        //playerInteraction = GetComponent<PlayerInteraction>();

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
        playerInput.actions["Invisible"].performed += playerInvisible.OnInvisible;
        //playerInput.actions["Sprint"].performed += playerMovement.OnSprint;
        playerInput.actions["Dash"].performed += playerMovement.OnDash;
        playerInput.actions["Throw"].performed += playerThrow.OnThrow;
      //  playerInput.actions["Look"].performed += playerCamera.OnLook;
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
        playerInput.actions["Invisible"].performed -= playerInvisible.OnInvisible;
        //playerInput.actions["Sprint"].performed -= playerMovement.OnSprint;
        playerInput.actions["Dash"].performed -= playerMovement.OnDash;
        playerInput.actions["Throw"].performed -= playerThrow.OnThrow;
       // playerInput.actions["Look"].performed -= playerCamera.OnLook;
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

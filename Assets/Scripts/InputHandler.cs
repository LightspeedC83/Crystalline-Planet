using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;
    [SerializeField] private PlayerInput playerInput;

    private InputAction moveAction, lookAction, jumpAction, pauseAction;
    private bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookAction = playerInput.actions.FindAction("Look");
        pauseAction = playerInput.actions.FindAction("Pause");

        pauseAction.performed += OnPausePerformed;

        Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        playerController.updatePlayerState();


        // This is independent of the state machine, as look direction shouldn't affect state
        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        playerController.Rotate(lookVector);
    }

    private void FixedUpdate()
    {
        playerController.updatePlayerPosition();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        playerController.Jump();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Pause();
    }

    /** Pauses or unpauses the game based on its current state
     */
    private void Pause()
    {
        if (isPaused)
        {
            // TODO: unpause the game, deactivate UI action map, and freeze cursor
        }
        else
        {
            // TODO: pause the game, activate UI action map, and unfreeze cursor
        }
    }
}

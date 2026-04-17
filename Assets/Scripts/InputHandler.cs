using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;

    private InputAction moveAction, lookAction, jumpAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");

        jumpAction.performed += OnJumpPerformed;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Calls Move and Rotate functions, even if no input is detected
        Vector2 movementVector = moveAction.ReadValue<Vector2>();
        playerController.Move(movementVector);

        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        playerController.Rotate(lookVector);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        playerController.Jump();
    }
}

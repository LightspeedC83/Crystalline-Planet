using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject pauseMenu;

    private InputAction lookAction, pauseAction, unpauseAction, mineAction;
    private bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookAction = playerInput.actions.FindAction("Look");
        pauseAction = playerInput.actions.FindAction("Pause");
        unpauseAction = playerInput.actions.FindAction("Unpause");
        mineAction = playerInput.actions.FindAction("Mine");


        pauseAction.performed += OnPausePerformed;
        unpauseAction.performed += OnPausePerformed;
        mineAction.performed += playerController.Mine;
        mineAction.canceled += playerController.StopMining;

        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        isPaused = false;
        playerInput.defaultActionMap = "Player";
        playerInput.SwitchCurrentActionMap("Player");

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

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /** Pauses or unpauses the game based on its current state
     */
    public void TogglePause()
    {
        if (isPaused)
        {
            // TODO: unpause the game, deactivate UI action map, and freeze cursor
            playerInput.SwitchCurrentActionMap("Player");
            pauseMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("Unpaused");
            Time.timeScale = 1;
        }
        else
        {
            // TODO: pause the game, activate UI action map, and unfreeze cursor
            playerInput.SwitchCurrentActionMap("UI");
            pauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("Paused");
            Time.timeScale = 0;
        }
        isPaused = !isPaused;
    }

    // Gets rid of action callbacks when the player can't input things
    public void OnDestroy()
    {
        pauseAction.performed -= OnPausePerformed;
        unpauseAction.performed -= OnPausePerformed;
        mineAction.performed -= playerController.Mine;
        mineAction.canceled -= playerController.StopMining;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

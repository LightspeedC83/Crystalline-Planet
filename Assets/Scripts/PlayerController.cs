using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    private PlayerInput playerInput;
    [SerializeField] public CinemachinePanTilt cineCamera;
    private float verticalVelocity;


    public StateMachine movementSM;
    public StandingState standing;
    public MovingState moving;
    public LandingState landing;
    public JumpingState jumping;

    [Header("Controller Parameters")]
    [SerializeField] public float movementSpeed = 11f, jumpForce = 10f, gravity = -30f;
    [SerializeField][Tooltip("Aerial control multiplier, range 0-1")] public float airControl = 0.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        moving = new MovingState(this, movementSM);
        landing = new LandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);

        movementSM.Initialize(standing);
    }

    /** Updates velocity according to input and gravity
     * @param movementVector The movement input Vector2.
     */
    public void Move(Vector2 movementVector)
    {
        Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        move *= movementSpeed * Time.deltaTime;
        characterController.Move(move);

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    public void Rotate(Vector2 rotationVector)
    {
        // Sets the player y axis rotation equal to the camera rotation.
        float panAngle = cineCamera.PanAxis.Value;
        Quaternion panRotation = Quaternion.Euler(0, panAngle, 0);
        transform.localRotation = panRotation;
    }

    public void Jump()
    {
            verticalVelocity = jumpForce;
    }

    public void updatePlayerState()
    {
        State activeState = movementSM.GetActiveState();
        //should call current state's HandleInput
        activeState.HandleInput();
        activeState.LogicUpdate();
    }

    public void updatePlayerPosition()
    {
        State activeState = movementSM.GetActiveState();
        // Calls Move function, even if no input is detected.
        //Vector2 movementVector = moveAction.ReadValue<Vector2>();
        //Move(movementVector);

        activeState.PhysicsUpdate();
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

}

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    private PlayerInput playerInput;
    [SerializeField] public CinemachinePanTilt cineCamera;
    private float rotationY;
    private float rotationX;
    private float verticalVelocity;

    public StateMachine movementSM;
    public StandingState standing;
    public MovingState moving;
    public LandingState landing;
    public JumpingState jumping;

    [SerializeField] public float movementSpeed = 11f, jumpForce = 10f, gravity = -30f;
    
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

    public void Move(Vector2 movementVector)
    {
        Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        move = move * movementSpeed * Time.deltaTime;
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
        if (characterController.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    public void updatePlayerState()
    {
        //should call current state's HandleInput
    }

    public void updatePlayerPosition()
    {
        // Calls Move function, even if no input is detected.
        //Vector2 movementVector = moveAction.ReadValue<Vector2>();
        //Move(movementVector);

        // should call current state's PhysicsUpdate
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

}

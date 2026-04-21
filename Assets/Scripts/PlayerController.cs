using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    private PlayerInput playerInput;
    [SerializeField] public CinemachinePanTilt cineCamera;
    public float verticalVelocity;
    public Vector3 characterVelocity;


    public StateMachine movementSM;
    public StandingState standing;
    public MovingState moving;
    public LandingState landing;
    public JumpingState jumping;
    public FallingState falling;

    private State activeState;

    [Header("Controller Parameters")]
    [SerializeField] public float movementSpeed = 10f, jumpForce = 10f, gravity = -30f, frictionConstant = 0.2f, aerialFrictionConstant = 0.1f;
    [SerializeField][Tooltip("Aerial control multiplier, range 0-1")] public float airControl = 0.75f;


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
        falling = new FallingState(this, movementSM);

        activeState = standing;
        movementSM.Initialize(standing);
    }

    /** Updates velocity according to input and gravity
     * @param movementVector The movement input Vector2.
     */
    public void Move(Vector2 movementVector)
    {
        if (movementVector != Vector2.zero)
        {
            Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
            move *= movementSpeed * Time.deltaTime;

            characterVelocity = move;
        } else
        {
            if (characterController.isGrounded)
            {
                characterVelocity -= characterVelocity * frictionConstant;
            } else
            {
                characterVelocity -= characterVelocity * frictionConstant * aerialFrictionConstant; // If the player is in the air, drag is reduced
            }
            //Debug.Log("friction applied");
        }

        if (!characterController.isGrounded) //This is bad code, doesn't account for States where I don't want the player to fall but they aren't grounded.
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        characterVelocity.y = verticalVelocity * Time.deltaTime;

        characterController.Move(characterVelocity);
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
        activeState = movementSM.GetActiveState();
        activeState.HandleInput();
        activeState.LogicUpdate();
    }

    public void updatePlayerPosition()
    {
        activeState = movementSM.GetActiveState();
        activeState.PhysicsUpdate();
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

}

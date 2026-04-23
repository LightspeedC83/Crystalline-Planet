using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    private PlayerInput playerInput;
    [SerializeField] public CinemachinePanTilt cineCamera;
    [SerializeField] public GameObject miningRay;
    public float verticalVelocity;
    public Vector3 characterVelocity;
    public bool hasDoubleJump;

    //Mining variables
    private bool mineKeyDown;
    private bool mining;
    private float mineTimer;
    private RaycastHit hitInfo;


    public StateMachine movementSM;
    public StandingState standing;
    public MovingState moving;
    public LandingState landing;
    public JumpingState jumping;
    public FallingState falling;
    public DoubleJumpingState doubleJumping;

    private State activeState;

    [Header("Controller Parameters")] [SerializeField]
    public float movementSpeed = 10f;
    public float jumpForce = 10f;
    public float gravity = -30f;
    public float frictionConstant = 0.2f;
    public float aerialFrictionConstant = 0.1f;
    public float coyoteTime = 0.1f;
    public float miningDestroyTime = 1f;
    public float miningRange = 5f;
    public float doubleJumpForce = 10f;
    public float doubleJumpForwardBoost = 5f;
    public float doubleJumpAdjustmentConstant = 0.1f;
    [SerializeField] [Tooltip("Aerial control multiplier, range 0-1")] public float airControl = 0.75f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        mineKeyDown = false;
        mineTimer = 0f;
        mining = false;

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        moving = new MovingState(this, movementSM);
        landing = new LandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);
        falling = new FallingState(this, movementSM);
        doubleJumping = new DoubleJumpingState(this, movementSM);
        miningRay.SetActive(false);

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
        UpdateMiningLogic();
        
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

    public void Mine(InputAction.CallbackContext context)
    {
        mineKeyDown = true;
        miningRay.SetActive(true);
    }

    public void StopMining(InputAction.CallbackContext context)
    {
        mineKeyDown = false;
        mining = false;
        miningRay.SetActive(false);
    }

    private void UpdateMiningLogic()
    {
        bool breakable;
        if (mineKeyDown)
        {
            Debug.DrawRay(cineCamera.transform.position, cineCamera.transform.forward * miningRange, Color.black, miningDestroyTime, true);
            mining = Physics.Raycast(cineCamera.transform.position, cineCamera.transform.forward, out hitInfo, miningRange, 11111111, QueryTriggerInteraction.Ignore);

            //If there is a collider, check if it is breakable
            if (hitInfo.collider != null)
            {
                breakable = hitInfo.collider.gameObject.CompareTag("Breakable");
            } else { breakable = false; }

            // If we're mining something, add to the timer
            if (mining && breakable)
            {
                mineTimer += Time.deltaTime;
            } else { mineTimer = 0; }
        }
        else
        {
            mineTimer = 0;
            breakable = false;
        }

        // If we've been mining long enough, destroy the object we're mining.
        if (mineTimer >= miningDestroyTime && breakable)
        {
            Destroy(hitInfo.collider.gameObject); 
            mineTimer = 0;
        }
    }
}

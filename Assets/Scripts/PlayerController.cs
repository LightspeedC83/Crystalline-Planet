using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public CharacterController characterController;
    private PlayerInput playerInput;
    [HideInInspector] public AudioSource audioSource;
    [SerializeField] public CinemachinePanTilt cineCamera;
    [SerializeField] public GameObject miningRay;
    [SerializeField] public RectTransform jumpChargeDisplay;
    [SerializeField] public GameObject particleGenerator;

    [SerializeField] public TextMeshProUGUI oreTrackerText;
    private int oreMined;
    [SerializeField] public int oreToWin = 15;

    public float verticalVelocity;
    public Vector3 characterVelocity;
    public bool hasDoubleJump;
    public bool hasDive;

    //Mining variables
    private bool mineKeyDown;
    private bool mining;
    private float mineTimer;
    private RaycastHit hitInfo;

    // State Machine
    public StateMachine movementSM;
    public StandingState standing;
    public MovingState moving;
    public LandingState landing;
    public JumpingState jumping;
    public FallingState falling;
    public DoubleJumpingState doubleJumping;
    public DivingState diving;
    public HardLandingState hardLanding;
    public SuperJumpingState superJumping;
    private State activeState; // The current state

    public Vector3 respawnPoint;
    private int gameOverScene = 1;

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
    public float doubleJumpSteeringConstant = 0.1f;
    public float diveForce = 30f;
    public float hardLandStunTime = 0.5f;
    public float superJumpMinCharge = 0.2f;
    public float superJumpMaxCharge = 0.5f;
    [SerializeField] [Tooltip("Aerial control multiplier, range 0-1")] public float airControl = 0.75f;

    [Header("Audio Files")] [SerializeField]
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip landingSound;
    public AudioClip hardLandingSound;
    public AudioClip miningBreakSound;
    public AudioClip minChargeSound;
    public AudioClip maxChargeSound;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();

        mineKeyDown = false;
        mineTimer = 0f;
        mining = false;

        jumpChargeDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        jumpChargeDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        hasDoubleJump = true;
        hasDive = true;

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        moving = new MovingState(this, movementSM);
        landing = new LandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);
        falling = new FallingState(this, movementSM);
        doubleJumping = new DoubleJumpingState(this, movementSM);
        diving = new DivingState(this, movementSM);
        hardLanding = new HardLandingState(this, movementSM);
        superJumping = new SuperJumpingState(this, movementSM);
        
        miningRay.SetActive(false);
        oreMined = 0;
        oreTrackerText.SetText("Ore: " + oreMined + "/" + oreToWin);

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

        // If the player has fallen too far, kill them.
        if (transform.position.y <= -1000)
        {
            OnDeath();
        }
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
        particleGenerator.SetActive(false);
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
                particleGenerator.SetActive(true);
                Vector3 particlePosition = new Vector3(transform.position.x, transform.position.y - 0.48f, transform.position.z);
                particlePosition += hitInfo.distance * cineCamera.transform.forward;
                particleGenerator.transform.position = particlePosition;
            } else 
            {
                particleGenerator.SetActive(false);
                mineTimer = 0; 
            }
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
            audioSource.PlayOneShot(miningBreakSound);
            mineTimer = 0;
            oreMined++;
            oreTrackerText.SetText("Ore: " + oreMined + "/" + oreToWin);
            
            if (oreMined >= oreToWin){
                SceneManager.LoadScene("WinScene");
            }

        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //On a collision while diving, land the dive on non-flat surfaces
        if (activeState == diving && hit.normal != Vector3.up)
        {
            diving.LandDive();
        }

        //In any state, if you hit the bottom of a thing, set vertical velocity to zero so you start falling immediately.
        if (Vector3.Dot(hit.normal, Vector3.down) > 0.5f)
        {
            Debug.Log("headbonk!");
            characterController.Move(Vector3.down * 0.3f); //Shove down so we don't get caught in the ceiling
            verticalVelocity = 0;
        }
    }

    //Incomplete method to call when the player dies
    public void OnDeath()
    {
        /*Vector3 previousPosition = transform.position;
        characterController.enabled = false;
        transform.position = respawnPoint;
        Debug.Log("death! Respawned at: " + respawnPoint + " from point: " + previousPosition);
        characterController.enabled = true;*/

        SceneManager.LoadScene(gameOverScene);
    }
    
        
}

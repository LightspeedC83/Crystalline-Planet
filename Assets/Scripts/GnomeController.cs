using UnityEngine;
using System.Collections;

public enum GnomeState
{
    Walking,
    Attacking,
    Recovery,
    Idle
}

// Sub-states for Attacking
enum AttackPhase { Windup, Flying }

// Sub-states for Recovery
enum RecoveryPhase { Searching, Traveling, Aligning }

public class GnomeController : MonoBehaviour
{
    public static int gnomeCount = 0;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;
    public float surfaceAlignSpeed = 12f;
    public float groundOffset = 0.5f;

    [Header("Behavior")]
    public float visualRadius = 60f;
    public float attackRadius = 30f;
    public float idleRadius = 100f;
    public float attackSpeed = 20f;
    public Vector2 attackFlyDurationRange = new Vector2(0.6f, 1.5f);
    public float randomTurnInterval = 2f;
    public float randomTurnAngle = 60f;

    [Header("Surface Detection")]
    public float probeLength = 1.5f;
    public float probeRadius = 0.25f;
    public float edgeProbeDistance = 0.6f;
    public LayerMask environmentMask;

    [Header("Recovery")]
    public float gravity = 9.81f;
    public float recoveryProbeLength = 200f;
    public float recoveryFlySpeed = 15f;
    public float recoveryAlignSpeed = 6f;
    public int recoveryRayCount = 24;
    public float recoveryArrivalDistance = 0.1f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    public AudioSource audioSource;
    public AudioClip launchSound;
    public AudioClip drillSound;
    
    

    // Animator parameter names
    const string ANIM_LAUNCH = "Launch";
    const string ANIM_FLY    = "Fly";
    const string ANIM_WALK   = "Walk";

    // State
    public GnomeState currentState;
    public bool playerVisible;
    public bool playerAttackable;

    private Vector3 currentNormal = Vector3.up;
    private Vector3 heading = Vector3.forward;
    private Vector3 fallVelocity;
    private bool grounded;

    private float nextRandomTurnTime;

    // Attack sub-state
    private AttackPhase attackPhase;
    private Vector3 attackDirection;     // locked at launch time
    private float   attackFlyEndTime;
    private bool    windupAnimDone;

    // Recovery sub-state
    private RecoveryPhase recoveryPhase;
    private Vector3 recoveryTargetPoint;
    private Vector3 recoveryTargetNormal;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        heading = transform.forward;
        currentNormal = transform.up;
        currentState = GnomeState.Walking;
        SetAnimTrigger(ANIM_WALK);
        gnomeCount++;
    }

    void Update()
    {
        EvaluatePlayer();
        UpdateState();

        switch (currentState)
        {
            case GnomeState.Walking:   TickWalking();   break;
            case GnomeState.Attacking: TickAttacking(); break;
            case GnomeState.Recovery:  TickRecovery();  break;
            case GnomeState.Idle:      /* stand still */ break;
        }
    }

    // ---------- Sensing ----------

    void EvaluatePlayer()
    {
        if (player == null) { playerVisible = playerAttackable = false; return; }
        float d = Vector3.Distance(player.position, transform.position);
        playerVisible = d <= visualRadius;
        playerAttackable = d <= attackRadius;

        if (d < 0.25 && currentState == GnomeState.Attacking){
            player.GetComponent<PlayerController>().OnDeath();
        }
    }

    void UpdateState()
    {
        // Don't interrupt an active attack or recovery.
        if (currentState == GnomeState.Attacking) return;
        if (currentState == GnomeState.Recovery)  return;

        if (player == null || Vector3.Distance(player.position, transform.position) >= idleRadius)
        {
            currentState = GnomeState.Idle;
            return;
        }

        if (playerAttackable && grounded)
        {
            BeginAttack();
            return;
        }

        if (!grounded)
        {
            BeginRecovery();
            return;
        }

        currentState = GnomeState.Walking;
    }

    // ---------- Walking ----------

    void TickWalking()
    {
        ChooseHeading();
        StepAlongSurface();
        ApplyRotation();
    }

    void ChooseHeading()
    {
        Vector3 desired = heading;

        if (playerVisible && player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            Vector3 projected = Vector3.ProjectOnPlane(toPlayer, currentNormal);
            if (projected.sqrMagnitude > 0.0001f)
                desired = projected.normalized;
        }
        else if (Time.time >= nextRandomTurnTime)
        {
            nextRandomTurnTime = Time.time + randomTurnInterval;
            float angle = Random.Range(-randomTurnAngle, randomTurnAngle);
            desired = Quaternion.AngleAxis(angle, currentNormal) * heading;
        }

        desired = Vector3.ProjectOnPlane(desired, currentNormal);
        if (desired.sqrMagnitude > 0.0001f)
            heading = desired.normalized;
    }

    void StepAlongSurface()
    {
        Vector3 forward = Vector3.ProjectOnPlane(heading, currentNormal).normalized;
        if (forward.sqrMagnitude < 0.0001f) { grounded = ProbeBelow(out _); return; }

        Vector3 step = forward * moveSpeed * Time.deltaTime;
        Vector3 candidatePos = transform.position + step;

        if (Physics.Raycast(transform.position, forward, out RaycastHit wallHit,
                            step.magnitude + probeRadius, environmentMask))
        {
            currentNormal = wallHit.normal;
            transform.position = wallHit.point + currentNormal * groundOffset;
            grounded = true;
            return;
        }

        if (ProbeBelowFrom(candidatePos, currentNormal, out RaycastHit downHit))
        {
            currentNormal = Vector3.Slerp(currentNormal, downHit.normal,
                                          surfaceAlignSpeed * Time.deltaTime);
            Vector3 targetPos = downHit.point + downHit.normal * groundOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos,
                                              surfaceAlignSpeed * Time.deltaTime);
            grounded = true;
            return;
        }

        Vector3 probeStart = candidatePos - currentNormal * 0.05f + forward * edgeProbeDistance;
        if (Physics.Raycast(probeStart, -forward, out RaycastHit edgeHit,
                            edgeProbeDistance + groundOffset * 2f, environmentMask))
        {
            currentNormal = edgeHit.normal;
            transform.position = edgeHit.point + currentNormal * groundOffset;
            heading = Vector3.ProjectOnPlane(forward, currentNormal).normalized;
            grounded = true;
            return;
        }

        transform.position = candidatePos;
        grounded = false;
    }

    bool ProbeBelow(out RaycastHit hit) =>
        ProbeBelowFrom(transform.position, currentNormal, out hit);

    bool ProbeBelowFrom(Vector3 pos, Vector3 up, out RaycastHit hit)
    {
        Vector3 origin = pos + up * 0.2f;
        return Physics.SphereCast(origin, probeRadius, -up, out hit,
                                  probeLength, environmentMask);
    }

    void ApplyRotation()
    {
        Vector3 fwd = Vector3.ProjectOnPlane(heading, currentNormal);
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
        Quaternion targetRot = Quaternion.LookRotation(fwd.normalized, currentNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                              rotationSpeed * Time.deltaTime);
    }

    // ---------- Attacking ----------

    void BeginAttack()
    {
        currentState = GnomeState.Attacking;
        attackPhase  = AttackPhase.Windup;
        windupAnimDone = false;

        // Lock attack vector at the moment of launch.
        attackDirection = (player.position - transform.position).normalized;
        grounded = false;

        SetAnimTrigger(ANIM_LAUNCH);
        audioSource.PlayOneShot(launchSound);
        StartCoroutine(SetReadyAfterDelay(2)); //launch is a 2 second animation clip
    }

    // /// <summary>
    // /// Hook this up as an Animation Event at the END of the Launch clip.
    // /// </summary>
    // public void OnAttackWindupComplete()
    // {
    //     windupAnimDone = true;
    // }
    
    private IEnumerator SetReadyAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        windupAnimDone = true;
    }

    void TickAttacking()
    {
        if (attackPhase == AttackPhase.Windup)
        {
            // Face the player while winding up; do not move.
            if (attackDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(attackDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, target,
                                                      rotationSpeed * Time.deltaTime);
            }

            if (windupAnimDone)
            {
                attackPhase = AttackPhase.Flying;
                attackFlyEndTime = Time.time + Random.Range(attackFlyDurationRange.x,
                                                            attackFlyDurationRange.y);
                SetAnimTrigger(ANIM_FLY);
                audioSource.PlayOneShot(drillSound);
            }
            return;
        }

        // ---- Flying phase ----
        // Move along the ORIGINAL launch vector (no homing).
        transform.position += attackDirection * attackSpeed * Time.deltaTime;

        if (attackDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(attackDirection),
                rotationSpeed * Time.deltaTime);
        }

        // Hit something? Stick and walk.
        if (Physics.SphereCast(transform.position, probeRadius, attackDirection,
                               out RaycastHit hit, attackSpeed * Time.deltaTime + 0.1f,
                               environmentMask))
        {
            currentNormal = hit.normal;
            transform.position = hit.point + currentNormal * groundOffset;
            heading = Vector3.ProjectOnPlane(transform.forward, currentNormal).normalized;
            if (heading.sqrMagnitude < 0.0001f)
                heading = Vector3.Cross(currentNormal, Vector3.right).normalized;
            grounded = true;
            currentState = GnomeState.Walking;
            SetAnimTrigger(ANIM_WALK);
            return;
        }

        // Time out → recovery.
        if (Time.time >= attackFlyEndTime)
            BeginRecovery();
    }

    // ---------- Recovery ----------

    void BeginRecovery()
    {
        currentState = GnomeState.Recovery;
        recoveryPhase = RecoveryPhase.Searching;
        fallVelocity = Vector3.zero;
        grounded = false;
        SetAnimTrigger(ANIM_FLY);
    }

    void TickRecovery()
    {
        switch (recoveryPhase)
        {
            case RecoveryPhase.Searching:  TickRecoverySearching();  break;
            case RecoveryPhase.Traveling:  TickRecoveryTraveling();  break;
            case RecoveryPhase.Aligning:   TickRecoveryAligning();   break;
        }
    }

    void TickRecoverySearching()
    {
        // Cast rays in random directions until one hits the environment.
        // We try several per frame so it doesn't take forever in open space.
        for (int i = 0; i < recoveryRayCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit,
                                recoveryProbeLength, environmentMask))
            {
                recoveryTargetPoint  = hit.point + hit.normal * groundOffset;
                recoveryTargetNormal = hit.normal;
                recoveryPhase = RecoveryPhase.Traveling;
                return;
            }
        }
        // Nothing found this frame; try again next frame (gnome just floats).
    }

    void TickRecoveryTraveling()
    {
        Vector3 toTarget = recoveryTargetPoint - transform.position;
        float dist = toTarget.magnitude;

        if (dist <= recoveryArrivalDistance)
        {
            transform.position = recoveryTargetPoint;
            currentNormal = recoveryTargetNormal;
            recoveryPhase = RecoveryPhase.Aligning;
            return;
        }

        Vector3 dir = toTarget / Mathf.Max(dist, 0.0001f);
        transform.position += dir * recoveryFlySpeed * Time.deltaTime;

        // Face direction of travel.
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir),
                rotationSpeed * Time.deltaTime);
        }
    }

    void TickRecoveryAligning()
    {
        // Snap rotation toward the surface normal + a tangent forward.
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, currentNormal);
        if (fwd.sqrMagnitude < 0.0001f)
            fwd = Vector3.Cross(currentNormal, Vector3.right);
        if (fwd.sqrMagnitude < 0.0001f)
            fwd = Vector3.Cross(currentNormal, Vector3.forward);
        fwd.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(fwd, currentNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                              recoveryAlignSpeed * Time.deltaTime);

        // Once aligned closely enough, enter walking.
        if (Quaternion.Angle(transform.rotation, targetRot) < 5f)
        {
            transform.rotation = targetRot;
            heading = fwd;
            grounded = true;
            currentState = GnomeState.Walking;
            SetAnimTrigger(ANIM_WALK);
        }
    }

    // ---------- Animator helper ----------

    void SetAnimTrigger(string name)
    {
        if (animator == null) return;
        // Reset other triggers so they don't stack.
        animator.ResetTrigger(ANIM_LAUNCH);
        animator.ResetTrigger(ANIM_FLY);
        animator.ResetTrigger(ANIM_WALK);
        animator.SetTrigger(name);
    }

    // ---------- Lifecycle ----------

    void OnDestroy() => gnomeCount--;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 0.5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, heading * 1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        if (currentState == GnomeState.Recovery && recoveryPhase != RecoveryPhase.Searching)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, recoveryTargetPoint);
            Gizmos.DrawWireSphere(recoveryTargetPoint, 0.2f);
        }
    }
}
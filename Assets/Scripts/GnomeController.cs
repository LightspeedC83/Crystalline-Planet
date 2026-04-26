using UnityEngine;
using System.Collections.Generic;

public enum GnomeState
{
    walking,
    attacking,
    recovery,
    idle
}

public class GnomeController : MonoBehaviour
{   
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 20f;
    public float glueToSurfaceSpeed= 0.1f;
    public float randomInfluence = 5f;
    public float randomInfluenceProbability = 0.1f;

    [Header("Behavior")]
    public float visualRadius = 60f;
    public float attackRadius = 30f;
    public float idleRadius = 500f;
    

    [Header("Raycasts")]
    public float rayLength = 1.2f;          // downward ray length
    public float cornerRayLength = 0.8f;    // shorter diagonal ray
    public float groundOffset = 0.5f;       // how far to hover above surface
    public LayerMask environmentMask;

    [Header("Plugins")]
    public GameObject player;


    private Vector3 currentNormal = Vector3.up;
    private bool forwardPresence;
    private bool downwardPresence;
    private Vector3 heading;

    [System.NonSerialized] public bool playerVisible;
    [System.NonSerialized] public bool playerAttackable;

    [System.NonSerialized] public GnomeState currentState;
    [System.NonSerialized] public static int gnomeCount = 0;


    void Start()
    {
        heading = transform.forward; // start facing forward
        currentState = GnomeState.walking;
        gnomeCount++;
    }
    void OnDestroy(){
        gnomeCount--;
    }

    void Update()
    {
        // basically if we are super far from the player, we don't walk around
        if ((player.transform.position - this.transform.position).magnitude >= idleRadius){ 
            currentState = GnomeState.idle;
        } else{
            currentState = GnomeState.walking;
        }

    

        if (currentState == GnomeState.walking){
            CastRays();
            MoveForward();
        }
        else if (currentState == GnomeState.attacking){

        }
        else if (currentState == GnomeState.recovery){
            
        }
    }

    void CastRays()
    {
        // --- DOWNWARD RAY (stay glued to surface) ---
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, -transform.up, out groundHit, rayLength, environmentMask))
        {
            downwardPresence = true;
            // Smoothly align normal to surface
            currentNormal = Vector3.Lerp(currentNormal, groundHit.normal, rotationSpeed * Time.deltaTime);

            // Glue to surface
            Vector3 targetPos = groundHit.point + groundHit.normal * groundOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, glueToSurfaceSpeed * Time.deltaTime);

            // Align up direction to surface normal
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, currentNormal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            
            if (groundHit.distance < groundOffset){
                transform.position += transform.up * glueToSurfaceSpeed * Time.deltaTime; 
            }

        }else{
            downwardPresence = false;
        }
        

        // --- DIAGONAL RAY (detect walls / corners ahead) ---
        Vector3 diagDirection = (transform.forward + (-transform.up)).normalized;
        RaycastHit cornerHit;

        Debug.DrawRay(transform.position, diagDirection * cornerRayLength, Color.red);
        Debug.DrawRay(transform.position, -transform.up * rayLength, Color.green);

        
        if (Physics.Raycast(transform.position, diagDirection, out cornerHit, cornerRayLength, environmentMask))//if there is a forward hit
        {
            forwardPresence = true;
            // Hit a wall ahead - rotate UPWARD (backward tilt) to climb it
            Quaternion climbRotation = Quaternion.FromToRotation(transform.up, cornerHit.normal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, climbRotation, rotationSpeed * Time.deltaTime);
        }
        else // if there is no forward presence
        {
            forwardPresence = false;
            // No wall ahead - rotate DOWNWARD to walk off edge
            // nudge forward to check if there's ground below-forward
            
            if (!downwardPresence)
            {
                // nothing below us at all, rotate forward (fall off edge onto next face)
                Quaternion fallRotation = Quaternion.Euler(rotationSpeed*2 * Time.deltaTime, 0, 0);
                transform.rotation = transform.rotation * fallRotation;
            }
        }
    }

    void MoveForward()
    {
        if (!downwardPresence){
            transform.position += -transform.up * glueToSurfaceSpeed * Time.deltaTime;
        }
        if (!forwardPresence && !downwardPresence){
            transform.position += transform.forward * moveSpeed/10 * Time.deltaTime;
            return;
        }
        
        //adding random movement
        if (downwardPresence && Random.value <= randomInfluenceProbability){
            Vector3 randomVector = new Vector3(Random.Range(-randomInfluence, randomInfluence), Random.Range(-randomInfluence, randomInfluence), Random.Range(-randomInfluence, randomInfluence));
            randomVector = Vector3.ProjectOnPlane(randomVector, transform.up);

            heading = randomVector + transform.forward;
            RotateToHeading();
        }
        
        //if the player is visible, we update the heading towards it
        if (playerVisible){
            Vector3 toPlayer = player.transform.position - this.transform.position;
            toPlayer = Vector3.ProjectOnPlane(toPlayer, transform.up);

            heading = toPlayer.normalized + transform.forward.normalized;
            RotateToHeading();
        }
        
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
     
    }

    void RotateToHeading()
    {
        if (heading == Vector3.zero) return;

        // Build target rotation: forward = heading, up = currentNormal
        Quaternion targetRot = Quaternion.LookRotation(heading, currentNormal);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        // Down ray
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.up * rayLength);

        // Diagonal ray
        Gizmos.color = Color.red;
        Vector3 diagDirection = (transform.forward + (-transform.up)).normalized;
        Gizmos.DrawRay(transform.position, diagDirection * cornerRayLength);
    }
}

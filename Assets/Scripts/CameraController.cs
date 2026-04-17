using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTarget, lookTarget;
    public float followSpeed = 10f;
    
    private void LateUpdate()
    {
        /* Third Person Camera Code
        Vector3 targetPosition = followTarget.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        */

        //First Person Camera Code
        transform.position = followTarget.position;

        transform.LookAt(lookTarget);
    }
}

using UnityEngine;

public class GnomeFarCollider : MonoBehaviour
{
    public GnomeController master;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        master = transform.parent.GetComponent<GnomeController>();
    
        SphereCollider colliderOnThis = this.GetComponent<SphereCollider>();
        colliderOnThis.radius = master.visualRadius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other){
        if (other.CompareTag("Player")){
            master.playerVisible = true;
        }
    }
}

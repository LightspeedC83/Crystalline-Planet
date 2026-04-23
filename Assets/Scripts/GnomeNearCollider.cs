using UnityEngine;

public class GnomeNearCollider : MonoBehaviour
{
    public GnomeController master;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        master = transform.parent.GetComponent<GnomeController>();
    
        SphereCollider colliderOnThis = this.GetComponent<SphereCollider>();
        colliderOnThis.radius = master.attackRadius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player")){
            master.playerAttackable = true;
        }
    }
    
    void OnTriggerExit(Collider other){
        if (other.CompareTag("Player")){
            master.playerAttackable = false;
        }
    }
}

using UnityEngine;

public class GnomeSpawner : MonoBehaviour
{
    public GameObject gnome;
    public int maxGnomes = 500;
    public float gnomeSpawnProbability = 0.025f;
    public bool startWithGnomeSpawned = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        if (startWithGnomeSpawned){
            SpawnGnome();
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if (GnomeController.gnomeCount < maxGnomes  && Random.value < gnomeSpawnProbability){
            SpawnGnome();
        }
    }

    public GameObject SpawnGnome(){
        GameObject spawnedGnome =  Instantiate(gnome, this.transform.position, this.transform.localRotation);
        spawnedGnome.SetActive(true);
        return spawnedGnome;
    }

    // void OntriggerStay(Collider other){
    //     if (other.CompareTag("Environment")){ //if the gnome spawner is inside a block
    //         this.gameObject.SetActive(false);
    //     }
    // }
}

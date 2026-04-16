using UnityEngine;
using System.Collections.Generic;

public class TerrainMaster : MonoBehaviour
{
    public float targetDensity;
    public int bailOutLimit; 
    public int x_lim;
    public int y_lim; 
    public int z_lim;
    public GameObject crystallineComponent;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DLA masterDLA = new DLA(x_lim, y_lim, z_lim);

        masterDLA.GenerateDLA(targetDensity, bailOutLimit);

        List<(int,int, int)> locations = masterDLA.getLocations();
        for (int i=0; i<locations.Count; i++){
            Vector3 targetPos = new Vector3((float)locations[i].Item1, (float)locations[i].Item2, (float)locations[i].Item3);
            Instantiate(crystallineComponent, targetPos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections; //needed for the co-routine used in StartSimulation()
using TMPro;

public class TerrainGenerator : MonoBehaviour
{

    public int mapX;
    public int mapZ;
    public float mapScale;
    public int expansionAmount;

    public float targetDensity;
    public int bailOutLimit;

    public GameObject terrainBlock;
    public GameObject token;

    public AudioSource ding;
    public TMP_Text hudTokens;
    private int tokensCollected = 0;

    private DLA2D mapGenerator;
    private int[,] terrainMap;
    private List<(int,int)> terrainLoactions;
    private List<(int,int)> recentTerrainLoactions;
    

    void Awake(){
        //creating a DLA map generator object
        mapGenerator = new DLA2D(mapZ, mapX);
        terrainMap = mapGenerator.GetMap(); //setting an empty map
        recentTerrainLoactions = mapGenerator.GenerateDLA(targetDensity, bailOutLimit);
        GameObject requiredBlock = Instantiate(terrainBlock, new Vector3(0f,0f,0f), Quaternion.identity); //making sure there's a block under where the player spawns/respawns
        requiredBlock.SetActive(true);
        Animator animator = requiredBlock.GetComponent<Animator>(); //making sure that the spawn animation doens't play for the initial blocks
        animator.enabled = false;
        
        terrainMap = mapGenerator.GetMap(); //generating the initial map

        int tokenIndex = UnityEngine.Random.Range(0, recentTerrainLoactions.Count);
        int i=0;
        foreach ((int,int) point in recentTerrainLoactions){
            int x = point.Item1;
            int z = point.Item2;
            GameObject newTerrainBlock = Instantiate(terrainBlock, new Vector3(mapScale*(x-mapX/2)/1f,0f,mapScale*(z-mapZ/2)/1f), Quaternion.identity);
            newTerrainBlock.SetActive(true);
            Animator anim = newTerrainBlock.GetComponent<Animator>(); //making sure that the spawn animation doens't play for the initial blocks
            anim.enabled = false;

            if (i == tokenIndex){
                GameObject newToken =  Instantiate(token, new Vector3(mapScale*(x-mapX/2)/1f, 1.5f, mapScale*(z-mapZ/2)/1f), Quaternion.identity);
                newToken.SetActive(true);
            }
            i++;
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            UpdateMapSize(expansionAmount);
        }
    }

    public void ExpandMap(int padding){
        recentTerrainLoactions = new List<(int,int)>();
        terrainMap = mapGenerator.GetMap(); //getting a map of the points before expansion
        UpdateMapSize(padding);
        ding.Play();
        tokensCollected++;
        hudTokens.text = "Tokens: " + tokensCollected.ToString();
        
    }

    private void UpdateMapSize(int padding){
        this.mapX = mapX +2*padding;
        this.mapZ = mapZ +2*padding;
        mapGenerator.EnlargeMap(padding);
        recentTerrainLoactions = mapGenerator.GenerateDLA(targetDensity,bailOutLimit);
        
        StartCoroutine(LoadNewCubes(recentTerrainLoactions));


    }

    private IEnumerator LoadNewCubes(List<(int,int)> terrainUnitList){

        int tokenIndex = UnityEngine.Random.Range(0, terrainUnitList.Count);
        int i=0;
        foreach ((int,int) point in terrainUnitList){
            int x = point.Item1;
            int z = point.Item2;

            GameObject newTerrainBlock = Instantiate(terrainBlock, new Vector3(mapScale*(x-mapX/2)/1f, 0f, mapScale*(z-mapZ/2)/1f), Quaternion.identity);
            newTerrainBlock.SetActive(true);

            if (i == tokenIndex){ //instantiating the expansion token at a random (valid) location above a newly spawned block
                GameObject newToken = Instantiate(token, new Vector3(mapScale*(x-mapX/2)/1f, 1.5f, mapScale*(z-mapZ/2)/1f), Quaternion.identity);
                newToken.SetActive(true);
                }
            i++;
            yield return new WaitForSeconds(0.05f);
               

        }
        

    }

}

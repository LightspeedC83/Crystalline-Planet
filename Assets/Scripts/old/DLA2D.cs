using UnityEngine;
using System.Collections.Generic;

public class DLA2D
{

    public int[,] DLA_map;
    private int height;
    private int width;
    private int numberCellsON;

    private List<(int,int)> pointsLocations;

    /*Constructor*/
    public DLA2D(int height, int width){
        this.height = height;
        this.width = width; 

        pointsLocations = new List<(int,int)>();
        pointsLocations.Add((width/2,height/2));

        //creating the empty DLA map
        DLA_map = new int[height, width]; //creating grid size rectangular 2d array; all the values start as 0 by default
        DLA_map[height/2,width/2] = 1; //creating a seed point
        numberCellsON = 1;
    }

    public int[,] GetMap(){
        return DLA_map;
    }

    public List<(int,int)> getLocations(){
        return pointsLocations;
    }

    /*
    Expands the DLA_map 2d rectangular array, keeping the center in the center (ie, adds padding to all edges of 2d array)
    */
    public void EnlargeMap(int padding){
        int newHeight = height + 2*padding;
        int newWidth = width + 2*padding;

        int[,] enlargedMap = new int[newHeight, newWidth]; //instantiate

        //padding the top
        for (int y=0; y<padding; y++){
            for (int x=0; x<newWidth; x++){
                enlargedMap[y,x] = 0;
            }
        }

        //padding the sides and copying data over
        for (int y=padding; y<height+padding; y++){
            for (int x=0; x<padding; x++){//padding the left
                enlargedMap[y,x] = 0;
            }
            for (int x=padding; x<padding+width; x++){ //copying the data from the middle
                enlargedMap[y,x] = DLA_map[y-padding, x-padding];
            }
            for (int x=padding+width; x<newWidth; x++){ //padding the right
                enlargedMap[y,x] = 0;
            }
        }

        //padding the bottom
        for (int y=height+padding; y<newHeight; y++){
            for (int x=0; x<newWidth; x++){
                enlargedMap[y,x] = 0;
            }
        }

        //updating the instance variables with enlarged map data
        DLA_map = enlargedMap;
        height = newHeight;
        width = newWidth;
    }


    /*
    Generates the DLA map to a target density, using bailout optimization
    */
    public List<(int,int)> GenerateDLA(float targetDensity, int bailOutLimit){
        List<(int,int)> addedPointsLocations = new List<(int,int)>();

        DLA_map[height/2, width/2] = 1; //making sure that there is indeed a seed point
        numberCellsON++;
 
        //neighbors to check list; in this satte, it will only 
        (int, int)[] neighbors = new (int, int)[] 
                    {
                                 (0, -1), 
                        (-1, 0),           (1, 0),
                                  (0, 1)
                    };
    
        float density = numberCellsON/((float)width*height);
        while (density<targetDensity){
            
            int bailOut = bailOutLimit;
            (int,int) candidatePoint = (UnityEngine.Random.Range(0,width), UnityEngine.Random.Range(0,height)); //NB coordinates are in (x,y) form, but when entered into the 2d array, they are entered using array[y,x]=value
            
            if (DLA_map[candidatePoint.Item2,candidatePoint.Item1]==1){continue;} //if the initial candidate point is already on, we get a new one.

            while (bailOut>0) { //random walk the candidate point until it collides with existing structure
                
                int x = candidatePoint.Item1;
                int y = candidatePoint.Item2;

                //check to see if there is  a collision
                foreach ((int,int) neighbor in neighbors){
                    //validity check
                    if (neighbor.Item1 + x < 0 || neighbor.Item1 + x >= width || neighbor.Item2 + y < 0 || neighbor.Item2 + y >= height) { continue; }
                    //if neighbor of candidate point is ON, we stop the walk and turn on the candidate point in its position
                    if(DLA_map[neighbor.Item2 + y, neighbor.Item1 + x] == 1){ //if neighbor is on
                        DLA_map[y,x] = 1; //turn on the point we're at
                        addedPointsLocations.Add((x,y));
                        numberCellsON++;
                        bailOut = 0;
                        break;
                    }
                }

                if (bailOut==0){continue;} //if the point has been found we don't care about walking the candidate because the candidate is good

                //walking the candidate randomly
                candidatePoint = (candidatePoint.Item1 + UnityEngine.Random.Range(-1, 2), candidatePoint.Item2 + UnityEngine.Random.Range(-1, 2));
                //if the new candidate point is outside the bounds, we bail out
                if (candidatePoint.Item1 < 0 || candidatePoint.Item1 >= width || candidatePoint.Item2< 0 || candidatePoint.Item2 >= height) { bailOut=0; }

                bailOut--;
            }

            density = numberCellsON/((float)width*height); //calculating density
        }
        pointsLocations.AddRange(addedPointsLocations);
        return addedPointsLocations;


    }

}

using UnityEngine;
using System.Collections.Generic;

public class DLA
{
    /// in unity the y axis is vertical

    public int[,,] DLA_map;
    private int x_lim;
    private int y_lim;
    private int z_lim;
    private int numberCellsON;

    private List<(int,int,int)> pointsLocations;

    /*Constructor*/
    public DLA(int x_lim, int y_lim, int z_lim ){
        this.x_lim = x_lim;
        this.y_lim = y_lim;
        this.z_lim = z_lim;

        pointsLocations = new List<(int,int,int)>();
        pointsLocations.Add( ((int)x_lim/2, (int)y_lim/2, (int)z_lim/2) );

        //creating the empty DLA map
        DLA_map = new int[x_lim, y_lim, z_lim]; //creating grid size rectangular 3d array; all the values start as 0 by default
        DLA_map[(int)x_lim/2, (int)y_lim/2, (int)z_lim/2] = 1; //creating a seed point
        numberCellsON = 1;
    }

    public int[,,] GetMap(){
        return DLA_map;
    }

    public List<(int,int, int)> getLocations(){
        return pointsLocations;
    }

    /*
    Expands the DLA_map 2d rectangular array, keeping the center in the center (ie, adds padding to all edges of 2d array)
    */
    public void EnlargeMap(int padding){
        int newXLim = x_lim + 2*padding;
        int newYLim = y_lim + 2*padding;
        int newZLim = z_lim + 2*padding;

        int[,,] enlargedMap = new int[newXLim, newYLim, newZLim]; //instantiate (0 by default)

        //padding the top
        // --->actually don't need to pad the top because it's starts as 0
        // for (int y=0; y<padding; y++){
        //     for (int x=0; x<newXLim; x++){
        //         for (int z=0; z<newZLim; z++){
        //             enlargedMap[x,y,z] = 0;
        //         }
        //     }
        // }

        //copying data over
        for (int i=0; i<pointsLocations.Count; i++){
            (int, int, int) point = pointsLocations[i];

            enlargedMap[point.Item1+padding, point.Item2+padding, point.Item3+padding] = 1;
        }

        //updating the instance variables with enlarged map data
        DLA_map = enlargedMap;
        x_lim = newXLim;
        y_lim = newYLim;
        z_lim = newZLim;

        for (int i = 0; i < pointsLocations.Count; i++){
            var point = pointsLocations[i];
            pointsLocations[i] = (
                point.Item1 + padding,
                point.Item2 + padding,
                point.Item3 + padding
            );
        }
    }


    /*
    Generates the DLA map to a target density, using bailout optimization
    */
    public List<(int,int,int)> GenerateDLA(float targetDensity, int bailOutLimit){
        List<(int,int,int)> addedPointsLocations = new List<(int,int,int)>();

        DLA_map[x_lim/2, y_lim/2, z_lim/2] = 1; //making sure that there is indeed a seed point
        numberCellsON++;
 
        //neighbors to check list; in this satte, it will only 
        (int, int, int)[] neighbors = new (int, int, int)[] {
            (-1,0,0), (1,0,0),   // x-axis
            (0,-1,0), (0,1,0),   // y-axis
            (0,0,-1), (0,0,1)    // z-axis
        };
        float density = numberCellsON/((float)x_lim*y_lim*z_lim);
        while (density<targetDensity){
            
            int bailOut = bailOutLimit;
            (int,int,int) candidatePoint = (UnityEngine.Random.Range(0,x_lim), UnityEngine.Random.Range(0,y_lim), UnityEngine.Random.Range(0,z_lim)); //NB coordinates are in (x,y) form, but when entered into the 2d array, they are entered using array[y,x]=value
            
            if (DLA_map[candidatePoint.Item1, candidatePoint.Item2, candidatePoint.Item3]==1){continue;} //if the initial candidate point is already on, we get a new one.

            while (bailOut>0) { //random walk the candidate point until it collides with existing structure
                
                int x = candidatePoint.Item1;
                int y = candidatePoint.Item2;
                int z = candidatePoint.Item3;

                //check to see if there is  a collision
                foreach ((int,int,int) neighbor in neighbors){
                    //validity check
                    if (neighbor.Item1 + x < 0 || neighbor.Item1 + x >= x_lim || neighbor.Item2 + y < 0 || neighbor.Item2 + y >= y_lim || neighbor.Item3 + z < 0 || neighbor.Item3 + z >= z_lim) { 
                        continue; 
                    }

                    //if neighbor of candidate point is ON, we stop the walk and turn on the candidate point in its position
                    if(DLA_map[neighbor.Item1 + x, neighbor.Item2 + y, neighbor.Item3 + z] == 1){ //if neighbor is on
                        DLA_map[x,y,z] = 1; //turn on the point we're at
                        addedPointsLocations.Add((x,y,z));
                        numberCellsON++;
                        bailOut = 0;
                        break;
                    }
                }

                if (bailOut==0){continue;} //if the point has been found we don't care about walking the candidate because the candidate is good

                //walking the candidate randomly
                int axis = UnityEngine.Random.Range(0, 3);
                int step = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                candidatePoint = (
                    candidatePoint.Item1 + (axis == 0 ? step : 0),
                    candidatePoint.Item2 + (axis == 1 ? step : 0),
                    candidatePoint.Item3 + (axis == 2 ? step : 0)
                );
                
                //if the new candidate point is outside the bounds, we bail out
                if (candidatePoint.Item1 < 0 || candidatePoint.Item1 >= x_lim || candidatePoint.Item2 < 0 || candidatePoint.Item2 >= y_lim || candidatePoint.Item3 < 0 || candidatePoint.Item3 >= z_lim) { bailOut=0; }

                bailOut--;
            }

            density = numberCellsON/((float)x_lim*y_lim*z_lim); //calculating density
        }
        pointsLocations.AddRange(addedPointsLocations);
        return addedPointsLocations;


    }

}

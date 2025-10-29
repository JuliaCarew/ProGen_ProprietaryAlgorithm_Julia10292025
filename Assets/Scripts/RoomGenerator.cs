using UnityEngine;

// public class for Tile
// tile is a 1x1 space in a grid.
public class Tile
{

}

// public class for Grid
// grid is made up of tiles (x amount by z amount)

// public class for Room
// has floor and walls, holds current position, door tile positions
public class Room 
{

}

// public class for Wall

// public class for Door
// needs to attach to connection
// one tile in a wall, store 'facing' direction

public class RoomGenerator : MonoBehaviour
{
    public int roomsToGenerate;
    public int[,] gridSize;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject obstaclePrefab;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}

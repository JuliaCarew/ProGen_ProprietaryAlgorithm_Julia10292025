using UnityEngine;
using System.Collections.Generic;

// one tile in a wall, store 'facing' direction
public enum DoorDirection
{
    North, // +Z
    South, // -Z
    East,  // +X
    West   // -X
}

public class Door
{
    public int gridX;
    public int gridZ;
    public DoorDirection direction;
    public bool isConnected; // for connection logic in Step 2

    public Door(int x, int z, DoorDirection dir)
    {
        gridX = x;
        gridZ = z;
        direction = dir;
        isConnected = false;
    }
}

public class WallGenerator
{
    private Grid grid;

    public WallGenerator(Grid grid)
    {
        this.grid = grid;
    }

    public void GenerateDoorsForRooms(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            GenerateDoorsForRoom(room);
        }

        // mark door tiles
        MarkDoors(rooms);
    }

    void GenerateDoorsForRoom(Room room)
    {
        // randomly determine number of doors (1 to 4)
        int numDoors = Random.Range(1, 5);
        
        // create list of all possible door directions
        List<DoorDirection> availableDirections = new List<DoorDirection>
        {
            DoorDirection.North,
            DoorDirection.South,
            DoorDirection.East,
            DoorDirection.West
        };
        
        // randomly select which walls get doors
        List<DoorDirection> selectedDirections = new List<DoorDirection>();
        for (int i = 0; i < numDoors; i++)
        {
            int randomIndex = Random.Range(0, availableDirections.Count);
            selectedDirections.Add(availableDirections[randomIndex]);
            availableDirections.RemoveAt(randomIndex); // remove to avoid duplicates
        }
        
        // calculate door positions for selected walls
        foreach (DoorDirection direction in selectedDirections)
        {
            int doorX, doorZ;
            
            switch (direction)
            {
                case DoorDirection.North:
                    doorX = room.gridX + room.width / 2;
                    doorZ = room.gridZ + room.depth - 1;
                    break;
                case DoorDirection.South:
                    doorX = room.gridX + room.width / 2;
                    doorZ = room.gridZ;
                    break;
                case DoorDirection.East:
                    doorX = room.gridX + room.width - 1;
                    doorZ = room.gridZ + room.depth / 2;
                    break;
                case DoorDirection.West:
                    doorX = room.gridX;
                    doorZ = room.gridZ + room.depth / 2;
                    break;
                default:
                    continue; 
            }
            
            room.doors.Add(new Door(doorX, doorZ, direction));
        }
    }

    void MarkDoors(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            foreach (Door door in room.doors)
            {
                Tile doorTile = grid.GetTile(door.gridX, door.gridZ);
                if (doorTile != null && doorTile.type == TileType.Floor)
                {
                    // mark as door - doors are on floor edge tiles
                    doorTile.type = TileType.Door;
                }
            }
        }
    }

    public void CreateWallsForRooms(List<Room> rooms, GameObject wallPrefab, Transform roomsParent)
    {
        foreach (Room room in rooms)
        {
            CreateWalls(room, wallPrefab, roomsParent);
        }
    }

    void CreateWalls(Room room, GameObject wallPrefab, Transform roomsParent)
    {
        float wallHeight = 2f / 3f; 
        HashSet<string> wallsPlaced = new HashSet<string>(); // track tiles that already have walls to avoid duplicates on corners
        HashSet<string> doorPositions = new HashSet<string>(); // track door positions to skip them

        // build set of door positions for this room
        foreach (Door door in room.doors)
        {
            doorPositions.Add($"{door.gridX},{door.gridZ}");
        }

        // place walls on the edge tiles of the floor
        // North edge (top row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ + room.depth - 1; // top edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, doorPositions, roomsParent);
        }

        // South edge (bottom row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ; // bottom edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, doorPositions, roomsParent);
        }

        // East edge (right column)
        for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
        {
            int x = room.gridX + room.width - 1; // right edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, doorPositions, roomsParent);
        }

        // West edge (left column)
        for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
        {
            int x = room.gridX; // left edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, doorPositions, roomsParent);
        }
    }

    void PlaceWallOnTile(int x, int z, float wallHeight, GameObject wallPrefab, HashSet<string> wallsPlaced, HashSet<string> doorPositions, Transform roomsParent)
    {
        string key = $"{x},{z}";
        
        // skip if already placed a wall on this tile
        if (wallsPlaced.Contains(key))
            return;
        
        // skip door positions
        if (doorPositions.Contains(key))
            return;

        Tile tile = grid.GetTile(x, z);
        if (tile != null && tile.type == TileType.Floor)
        {
            // place wall on the same tile as the floor - use the exact same X/Z as floor, just different Y (offset of 0.8f)
            Vector3 floorPosition = GridToWorldPosition(x, z, 0f);
            Vector3 wallPosition = new Vector3(floorPosition.x, 0.8f, floorPosition.z);
            
            GameObject wall = Object.Instantiate(wallPrefab, wallPosition, Quaternion.identity, roomsParent);
            wall.transform.localScale = new Vector3(1f, wallHeight, 1f);
            wallsPlaced.Add(key); // mark this tile as having a wall
            
            // Debug: verify wall placement
            if (wallPrefab != null)
            {
                Debug.Log($"Placed wall at grid ({x}, {z}) -> world ({wallPosition.x}, {wallPosition.y}, {wallPosition.z})");
            }
        }
    }

    Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        // convert grid coordinates to world position
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }
}


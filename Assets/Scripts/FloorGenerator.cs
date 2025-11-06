using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a room in the floor layout
/// </summary>
public class Room
{
    public int gridX;
    public int gridZ;
    public int width;
    public int depth;
    public List<Door> doors;

    public Room(int x, int z, int w, int d)
    {
        gridX = x;
        gridZ = z;
        width = w;
        depth = d;
        doors = new List<Door>();
    }
}

/// <summary>
/// Generates floor layouts with rooms on a grid
/// </summary>
public class FloorGenerator
{
    private Grid grid;
    private List<Room> generatedRooms;
    private int minRoomWidth;
    private int maxRoomWidth;
    private int minRoomDepth;
    private int maxRoomDepth;
    private int minDistanceBetweenRooms;
    private int maxPlacementAttempts;

    private bool debug = false;

    public FloorGenerator(Grid grid, int minRoomWidth, int maxRoomWidth, int minRoomDepth, int maxRoomDepth,
                         int minDistanceBetweenRooms, int maxPlacementAttempts)
    {
        this.grid = grid;
        this.minRoomWidth = minRoomWidth;
        this.maxRoomWidth = maxRoomWidth;
        this.minRoomDepth = minRoomDepth;
        this.maxRoomDepth = maxRoomDepth;
        this.minDistanceBetweenRooms = minDistanceBetweenRooms;
        this.maxPlacementAttempts = maxPlacementAttempts;
        generatedRooms = new List<Room>();
    }

    public List<Room> GenerateFloors(int roomsToGenerate)
    {
        generatedRooms.Clear();

        // generate rooms
        for (int i = 0; i < roomsToGenerate; i++)
        {
            TryPlaceRoom();
        }

        return generatedRooms;
    }

    /// <summary>
    /// Attempts to place a room on the grid
    /// </summary>
    void TryPlaceRoom()
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            // random room size
            int roomWidth = Random.Range(minRoomWidth, maxRoomWidth + 1);
            int roomDepth = Random.Range(minRoomDepth, maxRoomDepth + 1);

            // random position
            int startX = Random.Range(0, grid.width - roomWidth + 1);
            int startZ = Random.Range(0, grid.depth - roomDepth + 1);

            // check if the room can be placed at this position
            if (grid.CanPlaceRoom(startX, startZ, roomWidth, roomDepth, minDistanceBetweenRooms))
            {
                // create room
                Room room = new Room(startX, startZ, roomWidth, roomDepth);
                generatedRooms.Add(room);

                // mark floor tiles in grid
                for (int x = startX; x < startX + roomWidth; x++)
                {
                    for (int z = startZ; z < startZ + roomDepth; z++)
                    {
                        grid.GetTile(x, z).type = TileType.Floor;
                        if(debug) Debug.Log($"Marked grid tile ({x}, {z}) as Floor");
                    }
                }

                return; // placed room
            }
        }

        if(debug) Debug.LogWarning($"Could not place room after {maxPlacementAttempts} attempts");
    }

    /// <summary>
    /// Creates visual representations of the floor tiles for a given room
    /// </summary>
    /// <param name="room"></param>
    /// <param name="floorPrefab"></param>
    /// <param name="roomsParent"></param>
    public void CreateFloorVisuals(Room room, GameObject floorPrefab, Transform roomsParent)
    {
        // create floor for each tile in the room
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
            {
                Vector3 position = GridToWorldPosition(x, z, 0f);
                GameObject floorTile = Object.Instantiate(floorPrefab, position, Quaternion.identity, roomsParent);
                grid.GetTile(x, z).tilePrefab = floorTile;
                if(debug) Debug.Log($"Created floor tile at grid ({x}, {z}) world position {position}");
            }
        }
    }

    Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        // convert grid coordinates to world position
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }
}
using UnityEngine;
using System.Collections.Generic;

public class WallGenerator
{
    private Grid grid;

    public WallGenerator(Grid grid)
    {
        this.grid = grid;
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

        // place walls on the edge tiles of the floor
        // North edge (top row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ + room.depth - 1; // top edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, roomsParent);
        }

        // South edge (bottom row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ; // bottom edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, roomsParent);
        }

        // East edge (right column)
        for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
        {
            int x = room.gridX + room.width - 1; // right edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, roomsParent);
        }

        // West edge (left column)
        for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
        {
            int x = room.gridX; // left edge of floor
            PlaceWallOnTile(x, z, wallHeight, wallPrefab, wallsPlaced, roomsParent);
        }
    }

    void PlaceWallOnTile(int x, int z, float wallHeight, GameObject wallPrefab, HashSet<string> wallsPlaced, Transform roomsParent)
    {
        string key = $"{x},{z}";
        
        // skip if already placed a wall on this tile
        if (wallsPlaced.Contains(key))
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
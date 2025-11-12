using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Responsible for creating visual representations of corridors.
/// Handles instantiating floor tiles for corridor paths.
/// </summary>
public class CorridorVisualizer
{
    private Grid grid;

    public CorridorVisualizer(Grid grid)
    {
        this.grid = grid;
    }

    /// <summary>
    /// Create visual floor tiles for corridors
    /// </summary>
    /// <param name="floorPrefab">Prefab to use for floor tiles</param>
    /// <param name="roomsParent">Parent transform for organizing hierarchy</param>
    /// <param name="rooms">List of rooms to check (corridor tiles are not in rooms)</param>
    public void CreateCorridorVisuals(GameObject floorPrefab, Transform roomsParent, List<Room> rooms)
    {
        if (floorPrefab == null || roomsParent == null)
            return;

        // Create a set of all room tile positions for quick lookup
        HashSet<string> roomTilePositions = new HashSet<string>();
        foreach (Room room in rooms)
        {
            for (int x = room.gridX; x < room.gridX + room.width; x++)
            {
                for (int z = room.gridZ; z < room.gridZ + room.depth; z++)
                {
                    roomTilePositions.Add($"{x},{z}");
                }
            }
        }

        // Iterate through all tiles in the grid
        for (int x = 0; x < grid.width; x++)
        {
            for (int z = 0; z < grid.depth; z++)
            {
                Tile tile = grid.GetTile(x, z);
                if (tile != null && tile.type == TileType.Floor)
                {
                    // Check if this tile is not part of any room (it's a corridor)
                    string key = $"{x},{z}";
                    if (!roomTilePositions.Contains(key))
                    {
                        // Check if we haven't already created a visual for this tile
                        if (tile.tilePrefab == null)
                        {
                            Vector3 position = GridToWorldPosition(x, z, 0f);
                            GameObject floorTile = Object.Instantiate(floorPrefab, position, Quaternion.identity, roomsParent);
                            tile.tilePrefab = floorTile;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Convert grid coordinates to world position
    /// </summary>
    private Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }
}


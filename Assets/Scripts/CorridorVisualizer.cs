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

    // create visual floor tiles for corridors
    public void CreateCorridorVisuals(GameObject floorPrefab, Transform roomsParent, List<Room> rooms)
    {
        if (floorPrefab == null || roomsParent == null)
            return;

        // create a set of all room tile positions for quick lookup
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

        // iterate through all tiles in the grid
        for (int x = 0; x < grid.width; x++)
        {
            for (int z = 0; z < grid.depth; z++)
            {
                Tile tile = grid.GetTile(x, z);
                if (tile != null && tile.type == TileType.Floor)
                {
                    // check if this tile is not part of any room 
                    string key = $"{x},{z}";
                    if (!roomTilePositions.Contains(key))
                    {
                        // check if theres not a visual for this tile
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

    private Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }
}
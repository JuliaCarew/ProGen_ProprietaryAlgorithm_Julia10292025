using UnityEngine;

/// <summary>
/// handles visualization logic for instantiating water prefabs
/// </summary>
public class WaterVisualizer
{
    private Grid grid;
    private Transform roomsParent;
    private GameObject waterPrefab;
    private bool debug;

    public WaterVisualizer(Grid grid, Transform roomsParent, GameObject waterPrefab, bool debug)
    {
        this.grid = grid;
        this.roomsParent = roomsParent;
        this.waterPrefab = waterPrefab;
        this.debug = debug;
    }

    // instantiates water prefabs on water tiles
    public void CreateWaterVisuals()
    {
        if (waterPrefab == null)
        {
            Debug.LogError("Water prefab not assigned in WaterVisualizer!");
            return;
        }

        if (roomsParent == null)
        {
            Debug.LogError("Rooms parent transform is null!");
            return;
        }

        int waterTileCount = 0;

        for (int x = 0; x < grid.width; x++)
        {
            for (int z = 0; z < grid.depth; z++)
            {
                Tile tile = grid.GetTile(x, z);
                if (tile != null && tile.type == TileType.Water)
                {
                    Vector3 position = GridToWorldPosition(x, z, 0f);
                    GameObject waterObject = Object.Instantiate(waterPrefab, position, Quaternion.identity, roomsParent);
                    
                    // destroy the existing floor prefab if it exists
                    if (tile.tilePrefab != null)
                    {
                        Object.DestroyImmediate(tile.tilePrefab);
                    }
                    
                    tile.tilePrefab = waterObject;
                    waterTileCount++;
                }
            }
        }

        if (waterTileCount == 0)
        {
            Debug.LogWarning("No water tiles were created! Check that water paths were generated.");
        }
        else if (debug)
        {
            Debug.Log($"Created {waterTileCount} water tiles");
        }
    }

    private Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }
}


using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// handles pond generation logic for creating ponds at water points surrounded by enough floor tiles
/// </summary>
public class PondGenerator
{
    private Grid grid;
    private bool debug;
    private int minPondSize;
    private int maxPondSize;
    private int minFloorTilesForPond;
    private int pondRadius;

    public PondGenerator(Grid grid, bool debug, int minPondSize, int maxPondSize, 
                        int minFloorTilesForPond, int pondRadius)
    {
        this.grid = grid;
        this.debug = debug;
        this.minPondSize = minPondSize;
        this.maxPondSize = maxPondSize;
        this.minFloorTilesForPond = minFloorTilesForPond;
        this.pondRadius = pondRadius;
    }

    // create ponds at water points that are in the middle of rooms (surrounded by enough floor tiles)
    public void CreatePonds(List<Vector2Int> waterPoints)
    {
        if (waterPoints == null || waterPoints.Count == 0)
            return;

        List<Vector2Int> pondPoints = new List<Vector2Int>();

        // check each water point to see if it's suitable for a pond
        foreach (Vector2Int waterPoint in waterPoints)
        {
            // count floor tiles in the surrounding area
            int floorTileCount = CountFloorTilesInRadius(waterPoint, pondRadius);

            if (floorTileCount >= minFloorTilesForPond)
            {
                pondPoints.Add(waterPoint);
                if (debug)
                {
                    Debug.Log($"Water point at ({waterPoint.x}, {waterPoint.y}) has {floorTileCount} floor tiles nearby - creating pond");
                }
            }
        }

        // create ponds at suitable points
        foreach (Vector2Int pondPoint in pondPoints)
        {
            CreatePondAtPoint(pondPoint);
        }

        if (debug && pondPoints.Count > 0)
        {
            Debug.Log($"Created {pondPoints.Count} ponds");
        }
    }

    // counts floor tiles within a radius of a given point
    private int CountFloorTilesInRadius(Vector2Int center, int radius)
    {
        int count = 0;

        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int z = center.y - radius; z <= center.y + radius; z++)
            {
                if (!grid.IsValidPosition(x, z))
                    continue;

                Tile tile = grid.GetTile(x, z);
                if (tile != null && tile.type == TileType.Floor)
                {
                    // check if within circular radius 
                    int dx = Mathf.Abs(x - center.x);
                    int dz = Mathf.Abs(z - center.y);
                    if (dx + dz <= radius)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    // creates a pond (group of water tiles)
    private void CreatePondAtPoint(Vector2Int center)
    {
        int pondSize = Random.Range(minPondSize, maxPondSize + 1);
        HashSet<Vector2Int> pondTiles = new HashSet<Vector2Int>();
        List<Vector2Int> candidates = new List<Vector2Int> { center };

        // start from center and expand outward
        pondTiles.Add(center);

        // create pond by adding tiles around the center
        while (pondTiles.Count < pondSize && candidates.Count > 0)
        {
            // pick a random candidate tile
            int randomIndex = Random.Range(0, candidates.Count);
            Vector2Int current = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);

            // try to add neighbors
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(current.x + 1, current.y),
                new Vector2Int(current.x - 1, current.y),
                new Vector2Int(current.x, current.y + 1),
                new Vector2Int(current.x, current.y - 1),
                // add diagonal neighbors 
                new Vector2Int(current.x + 1, current.y + 1),
                new Vector2Int(current.x - 1, current.y + 1),
                new Vector2Int(current.x + 1, current.y - 1),
                new Vector2Int(current.x - 1, current.y - 1)
            };

            foreach (Vector2Int neighbor in neighbors)
            {
                if (!grid.IsValidPosition(neighbor.x, neighbor.y))
                    continue;

                // check if within pond radius
                int dx = Mathf.Abs(neighbor.x - center.x);
                int dz = Mathf.Abs(neighbor.y - center.y);
                if (dx + dz > pondRadius)
                    continue;

                Tile tile = grid.GetTile(neighbor.x, neighbor.y);
                if (tile != null && (tile.type == TileType.Floor || tile.type == TileType.Door) && !pondTiles.Contains(neighbor))
                {
                    pondTiles.Add(neighbor);
                    candidates.Add(neighbor);

                    if (pondTiles.Count >= pondSize)
                        break;
                }
            }
        }

        // mark all pond tiles as water
        foreach (Vector2Int pondTile in pondTiles)
        {
            Tile tile = grid.GetTile(pondTile.x, pondTile.y);
            if (tile != null && (tile.type == TileType.Floor || tile.type == TileType.Door))
            {
                tile.type = TileType.Water;
            }
        }

        if (debug)
        {
            Debug.Log($"Created pond at ({center.x}, {center.y}) with {pondTiles.Count} water tiles");
        }
    }
}


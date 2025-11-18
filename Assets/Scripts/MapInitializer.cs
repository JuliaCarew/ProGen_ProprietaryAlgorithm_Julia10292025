using UnityEngine;

/// <summary>
/// Types of tiles in the grid
/// </summary>
public enum TileType
{
    Air,
    Floor,
    Wall,
    Door,
    Water
}

/// <summary>
/// Represents a tile in the grid
/// </summary>
public class Tile
{
    public TileType type;
    public int gridX;
    public int gridZ;
    public GameObject tilePrefab;

    public Tile(int x, int z)
    {
        gridX = x;
        gridZ = z;
        type = TileType.Air;
        tilePrefab = null;
    }
}

/// <summary>
/// Represents a grid of tiles for floor layout
/// </summary>
public class Grid
{
    public Tile[,] tiles;
    public int width;
    public int depth;

    public bool debug = false;

    public Grid(int width, int depth)
    {
        this.width = width;
        this.depth = depth;
        tiles = new Tile[width, depth];

        // initialize all tiles as Air
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                tiles[x, z] = new Tile(x, z);
                if(debug) Debug.Log($"Initialized tile at ({x}, {z}) as Air");
            }
        }
    }

    public bool IsValidPosition(int x, int z)
    {
        bool isValid = x >= 0 && x < width && z >= 0 && z < depth;
        if(debug) Debug.Log($"Checked valid position for ({x}, {z}): {isValid}");
        return isValid;
    }

    public Tile GetTile(int x, int z)
    {
        if (IsValidPosition(x, z))
            return tiles[x, z];
        return null;
    }

    /// <summary>
    /// Checks if a room can be placed at the specified position 
    /// with given dimensions and minimum distance from other rooms
    /// </summary>
    /// <param name="startX"></param>
    /// <param name="startZ"></param>
    /// <param name="roomWidth"></param>
    /// <param name="roomDepth"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    public bool CanPlaceRoom(int startX, int startZ, int roomWidth, int roomDepth, int minDistance)
    {
        // check if room fits within grid bounds
        if (startX < 0 || startZ < 0 ||
            startX + roomWidth > width ||
            startZ + roomDepth > depth)
        {
            if(debug) Debug.Log($"Room does not fit within grid bounds at ({startX}, {startZ}) with size ({roomWidth}, {roomDepth})");
            return false;
        }

        // check minimum distance from existing rooms (non-air tiles)
        int checkStartX = Mathf.Max(0, startX - minDistance);
        int checkEndX = Mathf.Min(width, startX + roomWidth + minDistance);
        int checkStartZ = Mathf.Max(0, startZ - minDistance);
        int checkEndZ = Mathf.Min(depth, startZ + roomDepth + minDistance);

        for (int x = checkStartX; x < checkEndX; x++)
        {
            for (int z = checkStartZ; z < checkEndZ; z++)
            {
                if (tiles[x, z].type != TileType.Air)
                {
                    // found a non-air tile, check if it's too close to avoid room overlap
                    bool tooClose = !(
                           x < startX - minDistance
                        || x >= startX + roomWidth + minDistance
                        || z < startZ - minDistance
                        || z >= startZ + roomDepth + minDistance
                    );

                    if (tooClose)
                    {
                        if(debug) Debug.Log($"Cannot place room at ({startX}, {startZ}) , too close to existing room at ({x}, {z})");
                        return false;
                    }
                }
            }
        }

        if(debug) Debug.Log($"Can place room at ({startX}, {startZ}) with size ({roomWidth}, {roomDepth})");
        return true;
    }
}
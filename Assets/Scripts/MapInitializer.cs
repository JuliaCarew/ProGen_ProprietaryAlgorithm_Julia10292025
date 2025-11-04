using UnityEngine;

// tile = 1x1 space in a grid
public enum TileType
{
    Air,
    Floor,
    Wall,
    Door
}

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

// grid is made up of tiles (x amount by z amount of tiles)
public class Grid
{
    public Tile[,] tiles;
    public int width;
    public int depth;

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
            }
        }
    }

    public bool IsValidPosition(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < depth;
    }

    public Tile GetTile(int x, int z)
    {
        if (IsValidPosition(x, z))
            return tiles[x, z];
        return null;
    }

    public bool CanPlaceRoom(int startX, int startZ, int roomWidth, int roomDepth, int minDistance)
    {
        // check if room fits within grid bounds
        if (startX < 0 || startZ < 0 || 
            startX + roomWidth > width || 
            startZ + roomDepth > depth)
        {
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
                    bool tooClose = !(x < startX - minDistance || x >= startX + roomWidth + minDistance ||
                                     z < startZ - minDistance || z >= startZ + roomDepth + minDistance);
                    
                    if (tooClose)
                        return false;
                }
            }
        }

        return true;
    }
}


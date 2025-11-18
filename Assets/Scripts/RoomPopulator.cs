using UnityEngine;
using System.Collections.Generic;

public class RoomPopulator : MonoBehaviour
{
    [Header("Water Generation Settings")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private int minWaterPoints = 3;
    [SerializeField] private int maxWaterPoints = 8;
    [SerializeField] private int minDistanceBetweenPoints = 5;
    [SerializeField] private int maxDistanceBetweenPoints = 20;

    [Header("Pond Settings")]
    [SerializeField] private bool enablePonds = true;
    [SerializeField] private int minPondSize = 4;
    [SerializeField] private int maxPondSize = 12;
    [SerializeField] private int minFloorTilesForPond = 20;
    [SerializeField] private int pondRadius = 3;

    private Grid grid;
    private List<Room> rooms;
    private Transform roomsParent;
    [SerializeField] private bool debug = true;

    // specialized generators and visualizer
    private RiverGenerator riverGenerator;
    private PondGenerator pondGenerator;
    private WaterVisualizer waterVisualizer;

    // properties for UI 
    public GameObject WaterPrefab
    {
        get { return waterPrefab; }
        set { waterPrefab = value; }
    }

    public int MinWaterPoints
    {
        get { return minWaterPoints; }
        set { minWaterPoints = value; }
    }

    public int MaxWaterPoints
    {
        get { return maxWaterPoints; }
        set { maxWaterPoints = value; }
    }

    public int MinDistanceBetweenPoints
    {
        get { return minDistanceBetweenPoints; }
        set { minDistanceBetweenPoints = value; }
    }

    public int MaxDistanceBetweenPoints
    {
        get { return maxDistanceBetweenPoints; }
        set { maxDistanceBetweenPoints = value; }
    }

    public bool EnablePonds
    {
        get { return enablePonds; }
        set { enablePonds = value; }
    }

    public int MinPondSize
    {
        get { return minPondSize; }
        set { minPondSize = value; }
    }

    public int MaxPondSize
    {
        get { return maxPondSize; }
        set { maxPondSize = value; }
    }

    public int MinFloorTilesForPond
    {
        get { return minFloorTilesForPond; }
        set { minFloorTilesForPond = value; }
    }

    public int PondRadius
    {
        get { return pondRadius; }
        set { pondRadius = value; }
    }

    /// <summary>
    /// populates the dungeon with water rivers connecting random floor tiles
    /// </summary>
    /// <param name="grid">the grid containing all dungeon tiles</param>
    /// <param name="rooms">list of all generated rooms</param>
    /// <param name="roomsParent">parent transform for instantiating water prefabs</param>
    public void PopulateWater(Grid grid, List<Room> rooms, Transform roomsParent)
    {
        if (debug) Debug.Log("RoomPopulator.PopulateWater called");

        this.grid = grid;
        this.rooms = rooms;
        this.roomsParent = roomsParent;

        if (waterPrefab == null)
        {
            Debug.LogError("RoomPopulator: Water Prefab is not assigned! Please assign it in the Inspector.");
            return;
        }

        if (grid == null)
        {
            Debug.LogError("RoomPopulator: Grid is null!");
            return;
        }

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("RoomPopulator: No rooms provided");
            return;
        }

        // get all floor tiles from the dungeon
        List<Vector2Int> floorTiles = GetAllFloorTiles();

        if (debug) Debug.Log($"Found {floorTiles.Count} floor tiles");

        if (floorTiles.Count == 0)
        {
            Debug.LogWarning("No floor tiles found for water generation");
            return;
        }

        // initialize generators
        riverGenerator = new RiverGenerator(grid, debug, minWaterPoints, maxWaterPoints, 
                                           minDistanceBetweenPoints, maxDistanceBetweenPoints);
        pondGenerator = new PondGenerator(grid, debug, minPondSize, maxPondSize, 
                                         minFloorTilesForPond, pondRadius);
        waterVisualizer = new WaterVisualizer(grid, roomsParent, waterPrefab, debug);

        // generate water points
        List<Vector2Int> waterPoints = riverGenerator.GenerateWaterPoints(floorTiles);

        if (debug) Debug.Log($"Generated {waterPoints.Count} water points");

        if (waterPoints.Count == 0)
        {
            Debug.LogWarning("No valid water points generated");
            return;
        }

        // check for ponds and create them before marking points as water
        if (enablePonds)
        {
            pondGenerator.CreatePonds(waterPoints);
        }

        // mark all water points as water tiles
        foreach (Vector2Int waterPoint in waterPoints)
        {
            Tile tile = grid.GetTile(waterPoint.x, waterPoint.y);
            if (tile != null && tile.type != TileType.Water)
            {
                tile.type = TileType.Water;
            }
        }

        // create river paths connecting water points
        riverGenerator.CreateRiverPaths();

        // instantiate water prefabs
        waterVisualizer.CreateWaterVisuals();
    }

    // gets all floor tiles from the map grid
    private List<Vector2Int> GetAllFloorTiles()
    {
        List<Vector2Int> floorTiles = new List<Vector2Int>();

        for (int x = 0; x < grid.width; x++)
        {
            for (int z = 0; z < grid.depth; z++)
            {
                Tile tile = grid.GetTile(x, z);
                if (tile != null && tile.type == TileType.Floor)
                {
                    floorTiles.Add(new Vector2Int(x, z));
                }
            }
        }

        return floorTiles;
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates rooms on a grid-based floor layout
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    #region Generation Settings

    [Header("Generation Settings")]
    [SerializeField] private int roomsToGenerate = 5;

    public int RoomsToGenerate
    {
        get { return roomsToGenerate; }
        set { roomsToGenerate = value; }
    }

    [SerializeField] private int gridWidth = 50;
    public int GridWidth
    {
        get { return gridWidth; }
        set { gridWidth = value; }
    }

    [SerializeField] private int gridDepth = 50;
    public int GridDepth
    {
        get { return gridDepth; }
        set { gridDepth = value; }
    }

    [SerializeField] private int minRoomWidth = 3;
    public int MinRoomWidth
    {
        get { return minRoomWidth; }
        set { minRoomWidth = value; }
    }

    [SerializeField] private int maxRoomWidth = 8;
    public int MaxRoomWidth
    {
        get { return maxRoomWidth; }
        set { maxRoomWidth = value; }
    }

    [SerializeField] private int minRoomDepth = 3;
    public int MinRoomDepth
    {
        get { return minRoomDepth; }
        set { minRoomDepth = value; }
    }

    [SerializeField] private int maxRoomDepth = 8;
    public int MaxRoomDepth
    {
        get { return maxRoomDepth; }
        set { maxRoomDepth = value; }
    }

    [SerializeField] private int minDistanceBetweenRooms = 5;
    public int MinDistanceBetweenRooms
    {
        get { return minDistanceBetweenRooms; }
        set { minDistanceBetweenRooms = value; }
    }

    #endregion

    #region Generation Parameters

    [SerializeField] private int maxPlacementAttempts = 100;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    private Grid grid;
    private FloorGenerator floorGenerator;
    private WallGenerator wallGenerator;
    private RoomConnector roomConnector;
    private List<Room> generatedRooms;
    private Transform roomsParent;
    
    #endregion

    void Start()
    {
        // create new parent object
        if (roomsParent == null)
        {
            GameObject parent = new GameObject("GeneratedRooms");
            roomsParent = parent.transform;
        }

        GenerateDungeon(); // initial dungeon generation
    }

    public void GenerateDungeon()
    {
        ClearPreviousGeneration();

        // create new grid
        grid = new Grid(gridWidth, gridDepth);

        // create floor generator
        floorGenerator = new FloorGenerator(grid, minRoomWidth, maxRoomWidth, minRoomDepth, maxRoomDepth, minDistanceBetweenRooms, maxPlacementAttempts);

        // create wall generator
        wallGenerator = new WallGenerator(grid);

        // create room connector
        roomConnector = new RoomConnector(grid);

        // generate floors
        generatedRooms = floorGenerator.GenerateFloors(roomsToGenerate);
        
        // connect rooms with corridors
        roomConnector.ConnectAllRooms(generatedRooms);

        // ensure all rooms are in one connected component (no isolated room groups)
        roomConnector.EnsureAllRoomsConnected(generatedRooms);

        // build room visuals
        BuildRoomVisuals();
    }

    void BuildRoomVisuals()
    {
        foreach (Room room in generatedRooms)
        {
            // create floor visuals
            floorGenerator.CreateFloorVisuals(room, floorPrefab, roomsParent);
        }

        // create wall visuals for all rooms
        if (wallPrefab != null)
        {
            wallGenerator.CreateWallsForRooms(generatedRooms, wallPrefab, roomsParent);
        }

        // create corridor visuals
        if (floorPrefab != null)
        {
            roomConnector.CreateCorridorVisuals(floorPrefab, roomsParent, generatedRooms);
        }
    }

    void ClearPreviousGeneration()
    {
        // destroy all previous room objects
        if (roomsParent != null)
        {
            DestroyImmediate(roomsParent.gameObject);
        }

        // create new parent object
        GameObject parent = new GameObject("GeneratedRooms");
        roomsParent = parent.transform;
    }
}
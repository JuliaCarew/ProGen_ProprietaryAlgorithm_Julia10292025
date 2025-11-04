using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates rooms on a grid-based floor layout
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int roomsToGenerate = 5;

    public int RoomsToGenerate
    {
        get { return roomsToGenerate; }
        set { roomsToGenerate = value; }
    }

    [SerializeField] private int gridWidth = 50;
    [SerializeField] private int gridDepth = 50;
    [SerializeField] private int minRoomWidth = 3;
    [SerializeField] private int maxRoomWidth = 8;
    [SerializeField] private int minRoomDepth = 3;
    [SerializeField] private int maxRoomDepth = 8;
    [SerializeField] private int minDistanceBetweenRooms = 5;
    [SerializeField] private int maxPlacementAttempts = 100;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    private Grid grid;
    private FloorGenerator floorGenerator;
    private List<Room> generatedRooms;
    private Transform roomsParent;

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

        // generate floors
        generatedRooms = floorGenerator.GenerateFloors(roomsToGenerate);

        // build room visuals
        BuildRoomVisuals();
    }

    void BuildRoomVisuals()
    {
        foreach (Room room in generatedRooms)
        {
            // create floor visuals
            floorGenerator.CreateFloorVisuals(room, floorPrefab, roomsParent);

            // create wall visuals
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
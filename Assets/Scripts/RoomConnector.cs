using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ARCHITECTURE:
/// ======================
/// RoomConnector is responsible for connecting rooms together by creating corridors
/// between their doors. It works as a post-processing step after rooms and doors
/// have been generated.
/// 
/// HOW IT WORKS:
/// - Finds closest door pairs between different rooms
/// - Creates L-shaped corridors (shortest Manhattan distance path)
/// 
/// PROCESS:
/// 1. Iterate through all rooms
/// 2. For each unconnected door in a room, find the closest unconnected door in another room
/// 3. Calculate the shortest L-shaped path between the two doors
/// 4. Mark tiles along the path as Floor type
/// 5. Mark both doors as connected
/// 6. Create visual floor tiles for corridors
/// </summary>
public class RoomConnector
{
    private Grid grid;
    //private bool debug = false;
    
    // track corridor tiles for pruning
    //private Dictionary<string, CorridorInfo> corridorTiles;

    public RoomConnector(Grid grid)
    {
        this.grid = grid;
    }

    /// </summary>
    /// <param name="rooms">List of all rooms to connect</param>
    public void ConnectAllRooms(List<Room> rooms)
    {
        
    }

    /// Ensure all rooms are connected in a single connected component
    /// Uses graph connectivity to find disconnected room groups and connects them
    public void EnsureAllRoomsConnected(List<Room> rooms)
    {
       
    }

    /// <summary>
    /// Helper struct to hold a pair of doors
    /// </summary>
    private struct DoorPair
    {
        public Door door1;
        public Door door2;
        public float distance;

        public DoorPair(Door d1, Door d2, float dist)
        {
            door1 = d1;
            door2 = d2;
            distance = dist;
        }
    }

    /// Clean up unconnected doors by filling them with walls and removing connection tiles
    /// </summary>
    /// <param name="rooms">List of all rooms to clean up</param>
    public void CleanupUnconnectedDoors(List<Room> rooms)
    {
        
    }
  
    /// Find the closest unconnected door in a different room
    /// </summary>
    /// <param name="sourceRoom">The room containing the source door</param>
    /// <param name="sourceDoor">The door we want to connect from</param>
    /// <param name="allRooms">List of all rooms to search</param>
    /// <returns>The closest unconnected door, or null if none found</returns>
    private Door FindClosestUnconnectedDoor(Room sourceRoom, Door sourceDoor, List<Room> allRooms)
    {
       return null;
    }

    /// <summary>
    /// Calculate Manhattan distance between two points
    /// Manhattan distance = |x1 - x2| + |z1 - z2|
    /// shortest path length for L-shaped corridors
    /// </summary>
    private float CalculateManhattanDistance(int x1, int z1, int x2, int z2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
    }

    /// Create a corridor between two doors using the shortest L-shaped path
    /// </summary>
    /// <param name="door1">Starting door</param>
    /// <param name="door2">Ending door</param>
    private void CreateCorridorBetweenDoors(Door door1, Door door2)
    {
        
    }

    /// Create an L-shaped path between two points
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">Ending position</param>
    /// <param name="horizontalFirst">If true, move horizontally first, then vertically</param>
    /// <returns>List of positions forming the L-shaped path</returns>
    private List<Vector2Int> CreateLShapedPath(Vector2Int start, Vector2Int end, bool horizontalFirst)
    {
        return null;    
    }

    
    /// Mark all tiles in a path as Floor type
    /// </summary>
    /// <param name="path">List of positions to mark as floor</param>
    private void MarkPathAsFloor(List<Vector2Int> path)
    {
        
    }

    /// Create visual floor tiles for corridors
    /// </summary>
    /// <param name="floorPrefab">Prefab to use for floor tiles</param>
    /// <param name="roomsParent">Parent transform for organizing hierarchy</param>
    /// <param name="rooms">List of rooms to check (corridor tiles are not in rooms)</param>
    public void CreateCorridorVisuals(GameObject floorPrefab, Transform roomsParent, List<Room> rooms)
    {
        
    }

    /// Convert grid coordinates to world position
    private Vector3 GridToWorldPosition(int gridX, int gridZ, float yOffset)
    {
        return new Vector3(gridX - 0.5f, yOffset, gridZ - 0.5f);
    }

    /// Information about a corridor tile for tracking and pruning
    private class CorridorInfo
    {
        public int useCount; // how many corridors use this tile
        public List<CorridorConnection> connections; // which connections use this tile

        public CorridorInfo()
        {
            useCount = 0;
            connections = new List<CorridorConnection>();
        }
    }

    /// Represents a corridor connection between two doors
    private class CorridorConnection
    {
        public Door door1;
        public Door door2;
        public List<Vector2Int> path;
        public Room room1;
        public Room room2;
    }

    /// <summary>
    /// Get string key for a position
    /// </summary>
    private string GetKey(Vector2Int pos)
    {
        return $"{pos.x},{pos.y}";
    }

    /// <summary>
    /// Find the room containing a specific door
    /// </summary>
    private Room FindRoomContainingDoor(Door door, List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (room.doors.Contains(door))
                return room;
        }
        return null;
    }

    /// <summary>
    /// Remove a corridor path (change tiles back to Air if they're not used by other corridors)
    /// </summary>
    private void RemoveCorridorPath(List<Vector2Int> path)
    {
        
    }
}

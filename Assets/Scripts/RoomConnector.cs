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
    private bool debug = false;
    
    // Helper classes following single responsibility principle
    private CorridorPathfinder pathfinder;
    private CorridorConnectionManager connectionManager;
    private CorridorVisualizer visualizer;
    
    // Track corridor connections (using positions instead of doors)
    private Dictionary<Vector2Int, Vector2Int> positionConnections;
    private List<CorridorConnection> corridorConnections;
    
    // Track corridor tiles for pruning
    private Dictionary<string, CorridorInfo> corridorTiles;
    
    // Track used positions to avoid duplicate connections
    private HashSet<string> usedPositions;

    public RoomConnector(Grid grid)
    {
        this.grid = grid;
        this.pathfinder = new CorridorPathfinder(grid);
        this.connectionManager = new CorridorConnectionManager(pathfinder);
        this.visualizer = new CorridorVisualizer(grid);
        this.positionConnections = new Dictionary<Vector2Int, Vector2Int>();
        this.corridorConnections = new List<CorridorConnection>();
        this.corridorTiles = new Dictionary<string, CorridorInfo>();
        this.usedPositions = new HashSet<string>();
    }

    /// <summary>
    /// Connect all rooms by finding edge position pairs and creating corridors between them
    /// </summary>
    /// <param name="rooms">List of all rooms to connect</param>
    public void ConnectAllRooms(List<Room> rooms)
    {
        if (rooms == null || rooms.Count <= 1)
            return;

        positionConnections.Clear();
        corridorConnections.Clear();
        corridorTiles.Clear();
        usedPositions.Clear();

        // Find all potential edge position pairs and sort by distance
        List<CorridorConnectionManager.EdgePair> edgePairs = connectionManager.FindAllEdgePairs(rooms, usedPositions);

        // Connect rooms, starting with the closest pairs
        foreach (var pair in edgePairs)
        {
            // Skip if positions are already used
            string key1 = GetKey(pair.pos1);
            string key2 = GetKey(pair.pos2);
            
            if (usedPositions.Contains(key1) || usedPositions.Contains(key2))
                continue;

            // Create corridor between the two edge positions
            CreateCorridorBetweenPositions(pair.pos1, pair.pos2, pair.room1, pair.room2);
            
            // Mark positions as used
            usedPositions.Add(key1);
            usedPositions.Add(key2);
        }

        if (debug)
        {
            Debug.Log($"Connected {positionConnections.Count} position pairs across {rooms.Count} rooms");
        }
    }

    /// <summary>
    /// Ensure all rooms are connected in a single connected component
    /// Uses graph connectivity to find disconnected room groups and connects them
    /// </summary>
    public void EnsureAllRoomsConnected(List<Room> rooms)
    {
        if (rooms == null || rooms.Count <= 1)
            return;

        // Check if all rooms are already connected
        if (connectionManager.AreAllRoomsConnected(rooms, positionConnections))
        {
            if (debug)
                Debug.Log("All rooms are already connected");
            return;
        }

        // Find disconnected room groups using union-find
        Dictionary<Room, Room> parent = new Dictionary<Room, Room>();
        foreach (Room room in rooms)
        {
            parent[room] = room;
        }

        // Union connected rooms
        foreach (var connection in positionConnections)
        {
            Vector2Int pos1 = connection.Key;
            Vector2Int pos2 = connection.Value;
            
            Room room1 = connectionManager.FindRoomContainingPosition(pos1, rooms);
            Room room2 = connectionManager.FindRoomContainingPosition(pos2, rooms);
            
            if (room1 != null && room2 != null && room1 != room2)
            {
                Union(room1, room2, parent);
            }
        }

        // Group rooms by their root
        Dictionary<Room, List<Room>> roomGroups = new Dictionary<Room, List<Room>>();
        foreach (Room room in rooms)
        {
            Room root = FindRoot(room, parent);
            if (!roomGroups.ContainsKey(root))
            {
                roomGroups[root] = new List<Room>();
            }
            roomGroups[root].Add(room);
        }

        // If we have multiple groups, connect them
        if (roomGroups.Count > 1)
        {
            List<Room> groupList = new List<Room>(roomGroups.Keys);
            
            // Connect each group to the next group
            for (int i = 0; i < groupList.Count - 1; i++)
            {
                Room group1 = groupList[i];
                Room group2 = groupList[i + 1];
                
                // Find closest edge positions between the two groups
                Vector2Int closestPos1 = Vector2Int.zero;
                Vector2Int closestPos2 = Vector2Int.zero;
                float closestDistance = float.MaxValue;
                Room closestRoom1 = null;
                Room closestRoom2 = null;

                foreach (Room room1 in roomGroups[group1])
                {
                    List<Vector2Int> centerTiles1 = connectionManager.GetWallCenterTiles(room1);
                    foreach (Room room2 in roomGroups[group2])
                    {
                        List<Vector2Int> centerTiles2 = connectionManager.GetWallCenterTiles(room2);
                        
                        foreach (Vector2Int pos1 in centerTiles1)
                        {
                            foreach (Vector2Int pos2 in centerTiles2)
                            {
                                float distance = pathfinder.CalculateManhattanDistance(pos1.x, pos1.y, pos2.x, pos2.y);
                                
                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestPos1 = pos1;
                                    closestPos2 = pos2;
                                    closestRoom1 = room1;
                                    closestRoom2 = room2;
                                }
                            }
                        }
                    }
                }

                // Create corridor between the two groups
                if (closestRoom1 != null && closestRoom2 != null)
                {
                    CreateCorridorBetweenPositions(closestPos1, closestPos2, closestRoom1, closestRoom2);
                    
                    if (debug)
                    {
                        Debug.Log($"Connected room groups {i} and {i + 1} with corridor");
                    }
                }
            }
        }
    }

    private Room FindRoot(Room room, Dictionary<Room, Room> parent)
    {
        if (parent[room] != room)
        {
            parent[room] = FindRoot(parent[room], parent); // Path compression
        }
        return parent[room];
    }

    private void Union(Room room1, Room room2, Dictionary<Room, Room> parent)
    {
        Room root1 = FindRoot(room1, parent);
        Room root2 = FindRoot(room2, parent);
        
        if (root1 != root2)
        {
            parent[root2] = root1;
        }
    }

    /// <summary>
    /// Create a corridor between two positions using the shortest L-shaped path
    /// Places door tiles at the start and end positions
    /// The path will align parallel to both door positions' walls
    /// </summary>
    private void CreateCorridorBetweenPositions(Vector2Int pos1, Vector2Int pos2, Room room1, Room room2)
    {
        // Find shortest path using A* pathfinding
        // This allows multiple turns and finds the optimal path avoiding obstacles
        List<Vector2Int> path = pathfinder.FindShortestPath(pos1, pos2, room1, room2);
        
        // If no path found, fall back to a simple straight path (shouldn't happen in normal cases)
        if (path == null || path.Count == 0)
        {
            if (debug)
            {
                Debug.LogWarning($"No path found from ({pos1.x}, {pos1.y}) to ({pos2.x}, {pos2.y})");
            }
            return;
        }

        // Mark path as floor (this will replace walls with floors)
        pathfinder.MarkPathAsFloor(path);

        // Place doors at the start and end of the corridor
        pathfinder.PlaceDoorsAtEndpoints(path);

        // Track corridor tiles
        foreach (Vector2Int pos in path)
        {
            string key = GetKey(pos);
            if (!corridorTiles.ContainsKey(key))
            {
                corridorTiles[key] = new CorridorInfo();
            }
            corridorTiles[key].useCount++;
        }

        // Store connection info
        positionConnections[pos1] = pos2;

        CorridorConnection connection = new CorridorConnection
        {
            pos1 = pos1,
            pos2 = pos2,
            path = path,
            room1 = room1,
            room2 = room2
        };
        corridorConnections.Add(connection);

        // Update corridor tile tracking
        foreach (Vector2Int pos in path)
        {
            string key = GetKey(pos);
            if (corridorTiles.ContainsKey(key))
            {
                corridorTiles[key].connections.Add(connection);
            }
        }

        if (debug)
        {
            Debug.Log($"Created corridor from ({pos1.x}, {pos1.y}) to ({pos2.x}, {pos2.y}) with {path.Count} tiles");
        }
    }

    /// <summary>
    /// Create visual floor tiles for corridors
    /// </summary>
    public void CreateCorridorVisuals(GameObject floorPrefab, Transform roomsParent, List<Room> rooms)
    {
        visualizer.CreateCorridorVisuals(floorPrefab, roomsParent, rooms);
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

    /// Represents a corridor connection between two positions
    private class CorridorConnection
    {
        public Vector2Int pos1;
        public Vector2Int pos2;
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
}

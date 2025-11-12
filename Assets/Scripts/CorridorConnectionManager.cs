using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages corridor connections and pairing between rooms.
/// Handles finding closest edge positions and tracking connection state.
/// </summary>
public class CorridorConnectionManager
{
    private CorridorPathfinder pathfinder;

    public CorridorConnectionManager(CorridorPathfinder pathfinder)
    {
        this.pathfinder = pathfinder;
    }

    /// <summary>
    /// Get all edge positions of a room (positions on the perimeter where walls would be)
    /// </summary>
    public List<Vector2Int> GetRoomEdgePositions(Room room)
    {
        List<Vector2Int> edgePositions = new List<Vector2Int>();

        // North edge (top row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ + room.depth - 1;
            edgePositions.Add(new Vector2Int(x, z));
        }

        // South edge (bottom row)
        for (int x = room.gridX; x < room.gridX + room.width; x++)
        {
            int z = room.gridZ;
            edgePositions.Add(new Vector2Int(x, z));
        }

        // East edge (right column) - skip corners already added
        for (int z = room.gridZ + 1; z < room.gridZ + room.depth - 1; z++)
        {
            int x = room.gridX + room.width - 1;
            edgePositions.Add(new Vector2Int(x, z));
        }

        // West edge (left column) - skip corners already added
        for (int z = room.gridZ + 1; z < room.gridZ + room.depth - 1; z++)
        {
            int x = room.gridX;
            edgePositions.Add(new Vector2Int(x, z));
        }

        return edgePositions;
    }

    /// <summary>
    /// Get the center tile of each wall edge (North, South, East, West)
    /// Returns a list of 4 center positions (one for each wall)
    /// </summary>
    public List<Vector2Int> GetWallCenterTiles(Room room)
    {
        List<Vector2Int> centerTiles = new List<Vector2Int>();

        // North wall center (top edge)
        int northX = room.gridX + room.width / 2;
        int northZ = room.gridZ + room.depth - 1;
        centerTiles.Add(new Vector2Int(northX, northZ));

        // South wall center (bottom edge)
        int southX = room.gridX + room.width / 2;
        int southZ = room.gridZ;
        centerTiles.Add(new Vector2Int(southX, southZ));

        // East wall center (right edge)
        int eastX = room.gridX + room.width - 1;
        int eastZ = room.gridZ + room.depth / 2;
        centerTiles.Add(new Vector2Int(eastX, eastZ));

        // West wall center (left edge)
        int westX = room.gridX;
        int westZ = room.gridZ + room.depth / 2;
        centerTiles.Add(new Vector2Int(westX, westZ));

        return centerTiles;
    }

    /// <summary>
    /// Find the center tile of a specific wall edge
    /// </summary>
    /// <param name="room">The room</param>
    /// <param name="wallEdge">Wall edge: 0=North, 1=South, 2=East, 3=West</param>
    /// <returns>Center tile position of the specified wall</returns>
    public Vector2Int FindCenterTile(Room room, int wallEdge)
    {
        List<Vector2Int> centers = GetWallCenterTiles(room);
        if (wallEdge >= 0 && wallEdge < centers.Count)
        {
            return centers[wallEdge];
        }
        // Default to North if invalid
        return centers[0];
    }

    /// <summary>
    /// Find the closest edge position pair between two rooms using center tiles
    /// </summary>
    public EdgePair FindClosestEdgePair(Room room1, Room room2)
    {
        List<Vector2Int> centerTiles1 = GetWallCenterTiles(room1);
        List<Vector2Int> centerTiles2 = GetWallCenterTiles(room2);

        Vector2Int closestPos1 = centerTiles1[0];
        Vector2Int closestPos2 = centerTiles2[0];
        float closestDistance = float.MaxValue;

        // Compare center tiles of each wall edge
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
                }
            }
        }

        return new EdgePair(closestPos1, closestPos2, closestDistance, room1, room2);
    }

    /// <summary>
    /// Find all valid edge position pairs between rooms
    /// </summary>
    public List<EdgePair> FindAllEdgePairs(List<Room> rooms, HashSet<string> usedPositions)
    {
        List<EdgePair> pairs = new List<EdgePair>();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                EdgePair pair = FindClosestEdgePair(rooms[i], rooms[j]);
                
                // Check if positions are already used
                string key1 = $"{pair.pos1.x},{pair.pos1.y}";
                string key2 = $"{pair.pos2.x},{pair.pos2.y}";
                
                if (!usedPositions.Contains(key1) && !usedPositions.Contains(key2))
                {
                    pairs.Add(pair);
                }
            }
        }

        // Sort by distance (shortest first)
        pairs.Sort((a, b) => a.distance.CompareTo(b.distance));
        return pairs;
    }

    /// <summary>
    /// Helper struct to hold a pair of edge positions
    /// </summary>
    public struct EdgePair
    {
        public Vector2Int pos1;
        public Vector2Int pos2;
        public float distance;
        public Room room1;
        public Room room2;

        public EdgePair(Vector2Int p1, Vector2Int p2, float dist, Room r1, Room r2)
        {
            pos1 = p1;
            pos2 = p2;
            distance = dist;
            room1 = r1;
            room2 = r2;
        }
    }

    /// <summary>
    /// Find the room containing a specific position
    /// </summary>
    public Room FindRoomContainingPosition(Vector2Int pos, List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (pos.x >= room.gridX && pos.x < room.gridX + room.width &&
                pos.y >= room.gridZ && pos.y < room.gridZ + room.depth)
            {
                return room;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if all rooms are connected in a single connected component
    /// Uses union-find (disjoint set) algorithm
    /// </summary>
    public bool AreAllRoomsConnected(List<Room> rooms, Dictionary<Vector2Int, Vector2Int> positionConnections)
    {
        if (rooms.Count <= 1)
            return true;

        // Create a mapping from room to its parent
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
            
            Room room1 = FindRoomContainingPosition(pos1, rooms);
            Room room2 = FindRoomContainingPosition(pos2, rooms);
            
            if (room1 != null && room2 != null && room1 != room2)
            {
                Union(room1, room2, parent);
            }
        }

        // Check if all rooms are in the same set
        Room root = FindRoot(rooms[0], parent);
        foreach (Room room in rooms)
        {
            if (FindRoot(room, parent) != root)
                return false;
        }

        return true;
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

}
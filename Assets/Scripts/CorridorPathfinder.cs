using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Responsible for calculating and creating corridor paths between two points.
/// Uses A* pathfinding to find the shortest path avoiding obstacles.
/// </summary>
public class CorridorPathfinder
{
    private Grid grid;

    public CorridorPathfinder(Grid grid)
    {
        this.grid = grid;
    }

    /// <summary>
    /// Node for A* pathfinding
    /// </summary>
    private class PathNode
    {
        public Vector2Int position;
        public float gCost; // Distance from start
        public float hCost; // Heuristic distance to end
        public float fCost { get { return gCost + hCost; } }
        public PathNode parent;

        public PathNode(Vector2Int pos)
        {
            position = pos;
            gCost = float.MaxValue; // Initialize to infinity
            hCost = 0;
            parent = null;
        }
    }

    /// <summary>
    /// Calculate Manhattan distance between two points
    /// Manhattan distance = |x1 - x2| + |z1 - z2|
    /// This represents the shortest path length for L-shaped corridors
    /// </summary>
    public float CalculateManhattanDistance(int x1, int z1, int x2, int z2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
    }

    /// <summary>
    /// Create an L-shaped path between two points
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">Ending position</param>
    /// <param name="horizontalFirst">If true, move horizontally first, then vertically</param>
    /// <returns>List of positions forming the L-shaped path</returns>
    public List<Vector2Int> CreateLShapedPath(Vector2Int start, Vector2Int end, bool horizontalFirst)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        
        if (horizontalFirst)
        {
            // Move horizontally first, then vertically
            int stepX = (end.x > start.x) ? 1 : -1;
            for (int x = start.x; x != end.x; x += stepX)
            {
                path.Add(new Vector2Int(x, start.y));
            }
            
            int stepZ = (end.y > start.y) ? 1 : -1;
            for (int z = start.y; z != end.y; z += stepZ)
            {
                path.Add(new Vector2Int(end.x, z));
            }
        }
        else
        {
            // Move vertically first, then horizontally
            int stepZ = (end.y > start.y) ? 1 : -1;
            for (int z = start.y; z != end.y; z += stepZ)
            {
                path.Add(new Vector2Int(start.x, z));
            }
            
            int stepX = (end.x > start.x) ? 1 : -1;
            for (int x = start.x; x != end.x; x += stepX)
            {
                path.Add(new Vector2Int(x, end.y));
            }
        }
        
        // Add the end point
        path.Add(end);
        
        return path;
    }

    /// <summary>
    /// Create an L-shaped path between two points, choosing the better direction
    /// </summary>
    public List<Vector2Int> CreateOptimalLShapedPath(Vector2Int start, Vector2Int end)
    {
        // Try both directions and choose the one that avoids obstacles better
        List<Vector2Int> horizontalFirst = CreateLShapedPath(start, end, true);
        List<Vector2Int> verticalFirst = CreateLShapedPath(start, end, false);
        
        // Prefer the path that goes through fewer existing floor tiles (to avoid room overlap)
        int horizontalObstacles = CountObstaclesInPath(horizontalFirst);
        int verticalObstacles = CountObstaclesInPath(verticalFirst);
        
        return (horizontalObstacles <= verticalObstacles) ? horizontalFirst : verticalFirst;
    }

    /// <summary>
    /// Find the shortest path between two points using A* pathfinding
    /// Avoids obstacles (walls, room floors) and finds the optimal path
    /// </summary>
    public List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int end, Room startRoom = null, Room endRoom = null)
    {
        // A* pathfinding algorithm
        Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();
        Dictionary<Vector2Int, PathNode> openSet = new Dictionary<Vector2Int, PathNode>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        // Create start node
        PathNode startNode = new PathNode(start);
        startNode.gCost = 0; // Start node has 0 cost from start
        startNode.hCost = CalculateManhattanDistance(start.x, start.y, end.x, end.y);
        allNodes[start] = startNode;
        openSet[start] = startNode;

        // Directions: up, down, left, right (4-directional movement)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0), // Left
            new Vector2Int(1, 0)   // Right
        };

        while (openSet.Count > 0)
        {
            // Get node with lowest fCost
            PathNode currentNode = openSet.Values.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            openSet.Remove(currentNode.position);
            closedSet.Add(currentNode.position);

            // Check if we reached the goal
            if (currentNode.position.Equals(end))
            {
                // Reconstruct path
                return ReconstructPath(currentNode);
            }

            // Check neighbors
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.position + dir;

                // Skip if out of bounds
                if (!grid.IsValidPosition(neighborPos.x, neighborPos.y))
                    continue;

                // Skip if already in closed set
                if (closedSet.Contains(neighborPos))
                    continue;

                // Check if this tile is traversable
                if (!IsTraversable(neighborPos, start, end))
                    continue;

                // Get or create neighbor node
                PathNode neighborNode;
                if (!allNodes.ContainsKey(neighborPos))
                {
                    neighborNode = new PathNode(neighborPos);
                    allNodes[neighborPos] = neighborNode;
                }
                else
                {
                    neighborNode = allNodes[neighborPos];
                }

                // Calculate new gCost
                float newGCost = currentNode.gCost + 1; // Each step costs 1

                // If we found a better path to this neighbor
                bool isInOpenSet = openSet.ContainsKey(neighborPos);
                if (newGCost < neighborNode.gCost || !isInOpenSet)
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.hCost = CalculateManhattanDistance(neighborPos.x, neighborPos.y, end.x, end.y);
                    neighborNode.parent = currentNode;

                    if (!isInOpenSet)
                    {
                        openSet[neighborPos] = neighborNode;
                    }
                }
            }
        }

        // No path found - return null
        return null;
    }

    /// <summary>
    /// Check if a tile is traversable for corridor pathfinding
    /// </summary>
    private bool IsTraversable(Vector2Int pos, Vector2Int start, Vector2Int end)
    {
        // Start and end positions are always traversable (they're door positions)
        if (pos.Equals(start) || pos.Equals(end))
            return true;

        Tile tile = grid.GetTile(pos.x, pos.y);
        if (tile == null)
            return false;

        // Can traverse: Air, Floor (existing corridors), Door
        // Cannot traverse: Wall (room walls)
        return tile.type == TileType.Air || tile.type == TileType.Floor || tile.type == TileType.Door;
    }

    /// <summary>
    /// Reconstruct path from goal node back to start
    /// </summary>
    private List<Vector2Int> ReconstructPath(PathNode goalNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        PathNode currentNode = goalNode;

        while (currentNode != null)
        {
            path.Insert(0, currentNode.position);
            currentNode = currentNode.parent;
        }

        return path;
    }

    /// <summary>
    /// Determine if a position is on a horizontal wall (North or South) or vertical wall (East or West)
    /// </summary>
    private bool IsPositionOnHorizontalWall(Vector2Int pos, Room room)
    {
        // Check if position is at the center X coordinate (horizontal wall)
        // Note: Vector2Int uses x and y, where y corresponds to gridZ (Z axis in 3D space)
        int centerX = room.gridX + room.width / 2;
        int centerZ = room.gridZ + room.depth / 2;
        
        // Check if it's on North or South edge (horizontal wall)
        bool isOnNorthSouthEdge = (pos.y == room.gridZ || pos.y == room.gridZ + room.depth - 1);
        
        // Check if it's on East or West edge (vertical wall)
        bool isOnEastWestEdge = (pos.x == room.gridX || pos.x == room.gridX + room.width - 1);
        
        // If it's on a horizontal edge and X matches center, it's a horizontal wall
        // If it's on a vertical edge and Z matches center, it's a vertical wall
        if (isOnNorthSouthEdge && pos.x == centerX)
            return true;
        if (isOnEastWestEdge && pos.y == centerZ)
            return false;
        
        // Fallback: check X coordinate (center tiles for North/South have X at center)
        return pos.x == centerX;
    }

    /// <summary>
    /// Count obstacles (non-Air tiles) in a path
    /// </summary>
    private int CountObstaclesInPath(List<Vector2Int> path)
    {
        int count = 0;
        foreach (Vector2Int pos in path)
        {
            Tile tile = grid.GetTile(pos.x, pos.y);
            if (tile != null && tile.type != TileType.Air && tile.type != TileType.Floor)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Mark all tiles in a path as Floor type
    /// </summary>
    /// <param name="path">List of positions to mark as floor</param>
    public void MarkPathAsFloor(List<Vector2Int> path)
    {
        foreach (Vector2Int pos in path)
        {
            Tile tile = grid.GetTile(pos.x, pos.y);
            if (tile != null)
            {
                // Only mark as Floor if it's currently Air or Wall (don't overwrite existing floors/doors)
                if (tile.type == TileType.Air || tile.type == TileType.Wall)
                {
                    tile.type = TileType.Floor;
                }
            }
        }
    }

    /// <summary>
    /// Place door tiles at the start and end of a corridor path
    /// Replaces wall tiles at those positions
    /// </summary>
    /// <param name="path">The corridor path</param>
    public void PlaceDoorsAtEndpoints(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
            return;

        // Place door at start position
        Vector2Int startPos = path[0];
        PlaceDoorAtPosition(startPos);

        // Place door at end position (if different from start)
        if (path.Count > 1)
        {
            Vector2Int endPos = path[path.Count - 1];
            if (endPos != startPos)
            {
                PlaceDoorAtPosition(endPos);
            }
        }
    }

    /// <summary>
    /// Place a door tile at a specific position, replacing wall/floor if present
    /// This is called at corridor endpoints to create doorways in room walls
    /// </summary>
    /// <param name="pos">Position to place door</param>
    private void PlaceDoorAtPosition(Vector2Int pos)
    {
        Tile tile = grid.GetTile(pos.x, pos.y);
        if (tile != null)
        {
            // When a corridor connects, we mark the tile as Door to indicate a doorway
            if (tile.type == TileType.Wall || tile.type == TileType.Floor)
            {
                tile.type = TileType.Door;
                
                // TODO: If wall GameObject is stored in tile, destroy it here
                // For now, wall visuals remain - door visual would be placed separately
            }
        }
    }
}


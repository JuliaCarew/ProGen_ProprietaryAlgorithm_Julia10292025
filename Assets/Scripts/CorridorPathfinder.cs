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
    /// Node for pathfinding
    /// </summary>
    private class PathNode
    {
        public Vector2Int position;
        public float gCost; // distance from start
        public float hCost; // distance to end
        public float fCost { get { return gCost + hCost; } }
        public PathNode parent;

        public PathNode(Vector2Int pos)
        {
            position = pos;
            gCost = float.MaxValue;
            hCost = 0;
            parent = null;
        }
    }

    /// <summary>
    /// Calculate Manhattan distance between two points
    /// Manhattan distance = |x1 - x2| + |z1 - z2|
    /// This becomes the shortest path length for L-shaped corridors
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
            // move horizontally first, then vertically
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
            // move vertically first, then horizontally
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
        
        // add the end point
        path.Add(end);
        
        return path;
    }

    // create an L-shaped path between two points, choosing the better direction
    public List<Vector2Int> CreateOptimalLShapedPath(Vector2Int start, Vector2Int end)
    {
        // try both directions and choose the one that avoids obstacles better
        List<Vector2Int> horizontalFirst = CreateLShapedPath(start, end, true);
        List<Vector2Int> verticalFirst = CreateLShapedPath(start, end, false);
        
        // prefer the path that goes through fewer existing floor tiles to avoid room overlap
        int horizontalObstacles = CountObstaclesInPath(horizontalFirst);
        int verticalObstacles = CountObstaclesInPath(verticalFirst);
        
        return (horizontalObstacles <= verticalObstacles) ? horizontalFirst : verticalFirst;
    }

    // find the shortest path between two points using A* pathfinding
    // avoids obstacles (walls, room floors)
    public List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int end, Room startRoom = null, Room endRoom = null)
    {
        // A* pathfinding algorithm
        Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();
        Dictionary<Vector2Int, PathNode> openSet = new Dictionary<Vector2Int, PathNode>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        // create start node
        PathNode startNode = new PathNode(start);
        startNode.gCost = 0; // start node has 0 cost from start
        startNode.hCost = CalculateManhattanDistance(start.x, start.y, end.x, end.y);
        allNodes[start] = startNode;
        openSet[start] = startNode;

        // directions: up, down, left, right (4-directional movement)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0), // Left
            new Vector2Int(1, 0)   // Right
        };

        while (openSet.Count > 0)
        {
            // get node with lowest fCost
            PathNode currentNode = openSet.Values.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            openSet.Remove(currentNode.position);
            closedSet.Add(currentNode.position);

            // check if reached the end
            if (currentNode.position.Equals(end))
            {
                // reconstruct path
                return ReconstructPath(currentNode);
            }

            // check neighbors
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.position + dir;

                if (!grid.IsValidPosition(neighborPos.x, neighborPos.y))
                    continue;

                if (closedSet.Contains(neighborPos))
                    continue;

                if (!IsTraversable(neighborPos, start, end))
                    continue;

                // get or create neighbor node
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

                // calculate new gCost
                float newGCost = currentNode.gCost + 1;

                // if new path to neighbor is better, update its costs and parent
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

        // no path found
        return null;
    }

    // check if a tile is traversable for corridor pathfinding
    private bool IsTraversable(Vector2Int pos, Vector2Int start, Vector2Int end)
    {
        // Start and end positions are always traversable (they're door positions)
        if (pos.Equals(start) || pos.Equals(end))
            return true;

        Tile tile = grid.GetTile(pos.x, pos.y);
        if (tile == null)
            return false;

        return tile.type == TileType.Air || tile.type == TileType.Floor || tile.type == TileType.Door;
    }

    // reconstruct path from goal node back to start
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

    // count obstacles (non-Air tiles) in a path
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

    // mark all tiles in a path as Floor type
    public void MarkPathAsFloor(List<Vector2Int> path)
    {
        foreach (Vector2Int pos in path)
        {
            Tile tile = grid.GetTile(pos.x, pos.y);
            if (tile != null)
            {
                // only mark as Floor if it's currently Air or Wall (don't overwrite existing floors/doors)
                if (tile.type == TileType.Air || tile.type == TileType.Wall)
                {
                    tile.type = TileType.Floor;
                }
            }
        }
    }

    // place door tiles at the start and end of a corridor path, replaces wall tiles at those positions
    public void PlaceDoorsAtEndpoints(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
            return;

        // place door at start position
        Vector2Int startPos = path[0];
        PlaceDoorAtPosition(startPos);

        // place door at end position (if different from start)
        if (path.Count > 1)
        {
            Vector2Int endPos = path[path.Count - 1];
            if (endPos != startPos)
            {
                PlaceDoorAtPosition(endPos);
            }
        }
    }

    // place a door tile at a specific position, replacing wall/floor if present
    private void PlaceDoorAtPosition(Vector2Int pos)
    {
        Tile tile = grid.GetTile(pos.x, pos.y);
        if (tile != null)
        {
            // when a corridor connects, we mark the tile as Door to indicate a doorway
            if (tile.type == TileType.Wall || tile.type == TileType.Floor)
            {
                tile.type = TileType.Door;
            }
        }
    }
}


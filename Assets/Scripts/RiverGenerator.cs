using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// handles river generation logic including water points, pathfinding, and connections
/// </summary>
public class RiverGenerator
{
    private Grid grid;
    private bool debug;
    private int minWaterPoints;
    private int maxWaterPoints;
    private int minDistanceBetweenPoints;
    private int maxDistanceBetweenPoints;
    private List<Vector2Int> waterPoints;

    public List<Vector2Int> WaterPoints
    {
        get { return waterPoints; }
    }

    public RiverGenerator(Grid grid, bool debug, int minWaterPoints, int maxWaterPoints, 
                         int minDistanceBetweenPoints, int maxDistanceBetweenPoints)
    {
        this.grid = grid;
        this.debug = debug;
        this.minWaterPoints = minWaterPoints;
        this.maxWaterPoints = maxWaterPoints;
        this.minDistanceBetweenPoints = minDistanceBetweenPoints;
        this.maxDistanceBetweenPoints = maxDistanceBetweenPoints;
        this.waterPoints = new List<Vector2Int>();
    }

    /// <summary>
    /// generates water points on floor tiles with distance constraints
    /// </summary>
    public List<Vector2Int> GenerateWaterPoints(List<Vector2Int> floorTiles)
    {
        waterPoints.Clear();

        if (floorTiles.Count == 0)
            return waterPoints;

        int numPoints = Random.Range(minWaterPoints, maxWaterPoints + 1);
        int maxAttempts = 1000;
        int attempts = 0;

        while (waterPoints.Count < numPoints && attempts < maxAttempts)
        {
            attempts++;

            // pick a random floor tile
            Vector2Int candidate = floorTiles[Random.Range(0, floorTiles.Count)];

            // check if this point meets distance constraints
            bool isValid = true;

            // check minimum distance from all existing water points
            foreach (Vector2Int existingPoint in waterPoints)
            {
                float distance = Vector2Int.Distance(candidate, existingPoint);
                if (distance < minDistanceBetweenPoints)
                {
                    isValid = false;
                    break;
                }
            }

            // check maximum distance constraint
            if (isValid && waterPoints.Count > 0)
            {
                bool hasNearbyPoint = false;
                foreach (Vector2Int existingPoint in waterPoints)
                {
                    float distance = Vector2Int.Distance(candidate, existingPoint);
                    if (distance <= maxDistanceBetweenPoints)
                    {
                        hasNearbyPoint = true;
                        break;
                    }
                }

                // ensure its within max distance of at least one other point
                if (!hasNearbyPoint && waterPoints.Count > 0)
                {
                    isValid = false;
                }
            }

            if (isValid)
            {
                waterPoints.Add(candidate);
                if (debug) Debug.Log($"Added water point at ({candidate.x}, {candidate.y})");
            }
        }

        if (debug)
        {
            Debug.Log($"Generated {waterPoints.Count} water points out of {numPoints} requested");
        }

        return waterPoints;
    }

    // creates river paths connecting all water points
    public void CreateRiverPaths()
    {
        if (waterPoints.Count < 2)
            return;
        
        // start from the first point
        HashSet<Vector2Int> connectedPoints = new HashSet<Vector2Int>();
        List<Vector2Int> unconnectedPoints = new List<Vector2Int>(waterPoints);

        Vector2Int currentPoint = unconnectedPoints[0];
        connectedPoints.Add(currentPoint);
        unconnectedPoints.RemoveAt(0);

        // connect remaining points
        while (unconnectedPoints.Count > 0)
        {
            Vector2Int closestPoint = FindClosestPoint(currentPoint, unconnectedPoints);
            
            // create path from current point to closest unconnected point
            CreatePathBetweenPoints(currentPoint, closestPoint);

            // mark as connected
            connectedPoints.Add(closestPoint);
            unconnectedPoints.Remove(closestPoint);

            // move to the newly connected point to continue the river flow
            currentPoint = closestPoint;
        }

        AddAdditionalRiverConnections();
    }

    // finds the closest point from a list of candidate points
    private Vector2Int FindClosestPoint(Vector2Int fromPoint, List<Vector2Int> candidates)
    {
        Vector2Int closest = candidates[0];
        float closestDistance = Vector2Int.Distance(fromPoint, candidates[0]);

        foreach (Vector2Int candidate in candidates)
        {
            float distance = Vector2Int.Distance(fromPoint, candidate);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    // creates a path between two points 
    // marks all tiles in the path as Water
    private void CreatePathBetweenPoints(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = FindPath(start, end);

        foreach (Vector2Int pos in path)
        {
            Tile tile = grid.GetTile(pos.x, pos.y);
            if (tile != null && (tile.type == TileType.Floor || tile.type == TileType.Door || tile.type == TileType.Water))
            {
                // replace floor, door, or existing water tiles, not walls
                tile.type = TileType.Water;
            }
        }

        // also ensure start and end points are marked as water
        Tile startTile = grid.GetTile(start.x, start.y);
        if (startTile != null && (startTile.type == TileType.Floor || startTile.type == TileType.Door))
        {
            startTile.type = TileType.Water;
        }

        Tile endTile = grid.GetTile(end.x, end.y);
        if (endTile != null && (endTile.type == TileType.Floor || endTile.type == TileType.Door))
        {
            endTile.type = TileType.Water;
        }

        if (debug)
        {
            Debug.Log($"Created river path from ({start.x}, {start.y}) to ({end.x}, {end.y}) with {path.Count} tiles");
        }
    }

    //pathfinding between two points
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        // A* pathfinding
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        List<Vector2Int> openSet = new List<Vector2Int> { start };
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            // find node with lowest fScore
            Vector2Int current = openSet[0];
            float lowestF = fScore.ContainsKey(current) ? fScore[current] : float.MaxValue;
            
            foreach (Vector2Int node in openSet)
            {
                float f = fScore.ContainsKey(node) ? fScore[node] : float.MaxValue;
                if (f < lowestF)
                {
                    lowestF = f;
                    current = node;
                }
            }

            if (current == end)
            {
                // reconstruct path
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // check neighbors
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(current.x + 1, current.y),
                new Vector2Int(current.x - 1, current.y),
                new Vector2Int(current.x, current.y + 1),
                new Vector2Int(current.x, current.y - 1)
            };

            foreach (Vector2Int neighbor in neighbors)
            {
                if (!grid.IsValidPosition(neighbor.x, neighbor.y))
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                Tile tile = grid.GetTile(neighbor.x, neighbor.y);
                // can only path through floor, door, or water tiles (rivers can cross)
                if (tile != null && (tile.type == TileType.Floor || tile.type == TileType.Door || tile.type == TileType.Water))
                {
                    float tentativeGScore = (gScore.ContainsKey(current) ? gScore[current] : float.MaxValue) + 1f;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= (gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue))
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);
                }
            }
        }

        // if no path found, return a simple straight path
        return GetStraightPath(start, end);
    }

    // reconstructs the path from the cameFrom dictionary
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private float Heuristic(Vector2Int a, Vector2Int b) // Manhattan distance
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // creates a simple straight path (fallback) between two points
    private List<Vector2Int> GetStraightPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        // L-shaped path
        int x = start.x;
        int y = start.y;

        // X direction
        int xStep = dx > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            x += xStep;
            path.Add(new Vector2Int(x, y));
        }

        // Y direction
        int yStep = dy > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            y += yStep;
            path.Add(new Vector2Int(x, y));
        }

        return path;
    }

    // adds additional river connections between water points
    private void AddAdditionalRiverConnections()
    {
        // randomly add more connections between water points
        int additionalConnections = Random.Range(0, Mathf.Min(waterPoints.Count / 2, 3));

        for (int i = 0; i < additionalConnections; i++)
        {
            if (waterPoints.Count < 2)
                break;

            Vector2Int point1 = waterPoints[Random.Range(0, waterPoints.Count)];
            Vector2Int point2 = waterPoints[Random.Range(0, waterPoints.Count)];

            if (point1 != point2)
            {
                // only create path if distance is possible
                float distance = Vector2Int.Distance(point1, point2);
                if (distance <= maxDistanceBetweenPoints * 1.5f)
                {
                    CreatePathBetweenPoints(point1, point2);
                }
            }
        }
    }
}


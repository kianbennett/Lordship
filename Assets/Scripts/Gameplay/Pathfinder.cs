using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PointData {
    public float distFromStart;
    public float heuristic;
    public GridPoint parent;
    public bool visited;    

    public float Score(float cost) {
        return distFromStart + heuristic + cost; 
    }
}

[ExecuteInEditMode]
public class Pathfinder : Singleton<Pathfinder> {

    public IEnumerator FindPath(GridPoint[,] gridPoints, GridPoint start, GridPoint destination, bool log, System.Action<List<GridPoint>> onComplete) {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        List<GridPoint> path = new List<GridPoint>();
        List<GridPoint> pointsToBeTested = new List<GridPoint>();

        // Make sure the start and end points are valid and can be travelled to
        if (start == null || destination == null || destination.type == GridPoint.Type.Obstacle) {
            Debug.LogWarning("Cannot find path for invalid nodes");
            // return null;
            onComplete(null);
            yield break;
        }
        
        // Nothing to solve if there is a direct connection between these two locations
        GridPoint directConnection = start.connections.Find(c => c == destination);
        if (directConnection != null) {
            path.Add(start);
            path.Add(destination);
            onComplete(path);
            yield break;
        }

        stopwatch.Restart();

        // For each grid point store it's pathfinding data (parent, score, heuristic etc)
        Dictionary<GridPoint, PointData> pointDataDictionary = new Dictionary<GridPoint, PointData>();
        foreach(GridPoint gridPoint in gridPoints) {
            pointDataDictionary.Add(gridPoint, new PointData());
        }

        // Maintain a list of nodes to be tested and begin with the start node, keep going as long as there are nodes to test and destination hasn't been reached
        GridPoint currentNode = start;
        pointsToBeTested.Add(start);

        // Keep track of ticks elapsed - if this gets too large then yield return null to skip to the next frame
        // This avoids stuttering as the function will never spend too long on one frame
        float timer = 0;

		// Keep looping as long as there are still nodes to check and the target hasn't been reached
        while (pointsToBeTested.Count > 0) {
            float time = stopwatch.ElapsedTicks;

            // Begin by sorting the list each time by the heuristic
            pointsToBeTested.Sort((a, b) => (int) (pointDataDictionary[a].Score(a.cost) - pointDataDictionary[b].Score(b.cost)));

            currentNode = pointsToBeTested[0];

            // Reached destination
            if (currentNode == destination) {
                pointDataDictionary[destination].visited = true;
                break;
            }

            // Remove any tiles that have already been visited
            pointsToBeTested.RemoveAll(o => pointDataDictionary[o].visited);

            // Check there are still have locations to visit
            if (pointsToBeTested.Count > 0) {
                // Mark this note visited and then process it
                currentNode = pointsToBeTested[0];
                PointData currentNodeData = pointDataDictionary[currentNode];
                currentNodeData.visited = true;

                // Check each neighbour, if it is accessible and hasn't already been processed then add it to the list to be tested 
                for (int i = 0; i < currentNode.connections.Count; i++) {
                    GridPoint neighbour = currentNode.connections[i];
                    PointData neighbourData = pointDataDictionary[neighbour];

                    if(neighbourData.visited || pointsToBeTested.Contains(neighbour)) continue;

                    neighbourData.distFromStart = currentNodeData.distFromStart + distance(currentNode, neighbour);
                    neighbourData.heuristic = heuristic(neighbour, destination);
                    neighbourData.parent = currentNode;
                    pointsToBeTested.Add(neighbour);
                }
            }

            // If taking too long skip to the next frame to avoid freezing
            timer += stopwatch.ElapsedTicks - time;
            // Debug.Log(timer);
            if(timer > 50000) {
                timer = 0;
                yield return null;
            }
        }

        stopwatch.Stop();

        // Trace the path back through the parents then reverse it to return the correct route
        if (pointDataDictionary[destination].visited) {
            GridPoint routeNode = destination;

            while (pointDataDictionary[routeNode].parent != null) {
                path.Add(routeNode);
                routeNode = pointDataDictionary[routeNode].parent;
                // yield return null;
            }
            path.Add(routeNode);
            path.Reverse();

            onComplete(path);

            float time = (float) stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
            if(log) Debug.LogFormat("Found path with length of {0} in {1}ms", path.Count, time);
        } else {
            if(log) Debug.LogWarning("Path not found");
            onComplete(null);
        }
    }

    // Length of a connection between two nodes
    private float distance(GridPoint a, GridPoint b) {
        return Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
    }

    // Estimate how close two points are, atm this is just line of sight as a quick but slightly innacurate solution
    private float heuristic(GridPoint a, GridPoint b) {
        return Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
    }

    public static List<Vector3> SmoothPath(List<Vector3> path) {
        // Can't smooth a path without enouch points
        if(path == null || path.Count < 2) return path;

        List<Vector3> newPath = new List<Vector3>();

        newPath.Add(path.First());

        float dist = 0.4f;
        int steps = 3;

        for(int i = 1; i < path.Count - 1; i++) {
            Vector3 prev = path[i - 1];
            Vector3 next = path[i + 1];

            Vector3 p0 = Vector3.Lerp(path[i], prev, dist);
            Vector3 p1 = Vector3.Lerp(path[i], next, dist);

            for(int s = 0; s < steps; s++) {
                float t = (float) s / (steps - 1);
                Vector3 p = Vector3.Lerp(Vector3.Lerp(p0, path[i], t), Vector3.Lerp(path[i], p1, t), t);
                newPath.Add(p);
            }
        }

        newPath.Add(path.Last());
        return newPath;
    }

    public static void RemoveRedundantNodes(List<Vector3> path) {
        List<Vector3> nodesToRemove = new List<Vector3>();

        for(int i = 1; i < path.Count - 1; i++) {
            Vector3 dirFromPrevious = (path[i] - path[i - 1]).normalized;
            Vector3 dirToNext = (path[i + 1] - path[i]).normalized;

            if(dirToNext == dirFromPrevious) {
                nodesToRemove.Add(path[i]);
            }
        }

        foreach(Vector3 node in nodesToRemove) {
            path.Remove(node);
        }
    }
}

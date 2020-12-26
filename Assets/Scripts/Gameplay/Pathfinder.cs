using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class Pathfinder : Singleton<Pathfinder> {

    private List<GridPoint> pointsToBeTested = new List<GridPoint>();
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    public List<GridPoint> FindPath(GridPoint[,] gridPoints, GridPoint start, GridPoint destination) {
        List<GridPoint> path = new List<GridPoint>();

        // Make sure the start and end points are valid and can be travelled to
        if (start == null || destination == null || destination.type == GridPoint.Type.Obstacle) {
            Debug.LogWarning("Cannot find path for invalid nodes");
            return null;
        }
        
        // Nothing to solve if there is a direct connection between these two locations
        GridPoint directConnection = start.connections.Find(c => c == destination);
        if (directConnection != null) {
            path.Add(start);
            path.Add(destination);
            return path;
        }

        stopwatch.Restart();

        // Set all the pathfinding parameters to its starting values
        pointsToBeTested.Clear();
        for(int j = 0; j < gridPoints.GetLength(1); j++) {
            for(int i = 0; i < gridPoints.GetLength(0); i++) {
                gridPoints[i, j].parent = null;
                // gridPoints[i, j].global = float.MaxValue;
                gridPoints[i, j].distFromStart = 0;
                gridPoints[i, j].heuristic = 0;
                // gridPoints[i, j].local = float.MaxValue;
                gridPoints[i, j].visited = false;
            }
        }

        // Maintain a list of nodes to be tested and begin with the start node, keep going as long as there are nodes to test and destination hasn't been reached
        GridPoint currentNode = start;
        pointsToBeTested.Add(start);

		// Keep looping as long as there are still nodes to check and the target hasn't been reached
        while (pointsToBeTested.Count > 0) {
            // Begin by sorting the list each time by the heuristic
            pointsToBeTested.Sort((a, b) => (int) (a.score - b.score));

            currentNode = pointsToBeTested[0];

            // Reached destination
            if (currentNode == destination) break;

            // Remove any tiles that have already been visited
            pointsToBeTested.RemoveAll(o => o.visited);

            // Check there are still have locations to visit
            if (pointsToBeTested.Count > 0) {
                // Mark this note visited and then process it
                currentNode = pointsToBeTested[0];
                currentNode.visited = true;

                // Check each neighbour, if it is accessible and hasn't already been processed then add it to the list to be tested 
                for (int i = 0; i < currentNode.connections.Count; i++) {
                    GridPoint neighbour = currentNode.connections[i];

                    if(neighbour.visited || pointsToBeTested.Contains(neighbour)) continue;

                    neighbour.distFromStart = currentNode.distFromStart + distance(currentNode, neighbour);
                    neighbour.heuristic = heuristic(neighbour, destination);
                    neighbour.parent = currentNode;
                    pointsToBeTested.Add(neighbour);
                }
            }
        }

        stopwatch.Stop();

        // Trace the path back through the parents then reverse it to return the correct route
        if (destination.visited) {
            GridPoint routeNode = destination;

            while (routeNode.parent != null) {
                path.Add(routeNode);
                routeNode = routeNode.parent;
            }
            path.Add(routeNode);
            path.Reverse();

            float time = (float) stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
            Debug.LogFormat("Found path with length of {0} in {1}ms", path.Count, time);
        } else {
            Debug.LogWarning("Path not found");
        }

        return path;
    }

    // Length of a connection between two nodes
    private float distance(GridPoint a, GridPoint b) {
        return new Vector2(b.x - a.x, b.y - a.y).magnitude;
    }

    // Estimate how close two points are, atm this is just line of sight as a quick but slightly innacurate solution
    private float heuristic(GridPoint a, GridPoint b) {
        return Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
    }

    public static List<Vector3> SmoothPath(List<Vector3> path) {
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

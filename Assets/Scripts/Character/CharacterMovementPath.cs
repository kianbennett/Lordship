using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class CharacterMovementPath {

    private List<Vector3> nodes;
    private MovementParams movementParams;

    // The distance over which to decelerate to zero
    private float decDist;
    private float totalPathLength;
    private float distTravelled;
    private int targetNode;

    private UnityAction<Vector3> onSetTargetNode;
    private UnityAction onCompletePath;

    public CharacterMovementPath(List<Vector3> nodes, MovementParams movementParams, UnityAction<Vector3> onSetTargetNode, UnityAction onCompletePath) {
        this.nodes = nodes;
        this.movementParams = movementParams;
        this.onSetTargetNode = onSetTargetNode;
        this.onCompletePath = onCompletePath;
        decDist = movementParams.DecCurve.keys.Last().time;
        calculatePathLength();
    }

    public void Update() {

    }

    public float GetCurrentSpeed(float maxSpeed) {
        if (distTravelled < totalPathLength - decDist) {
            return movementParams.AccCurve.Evaluate(distTravelled) * maxSpeed;
        } else {
            return movementParams.DecCurve.Evaluate(distTravelled - (totalPathLength - decDist)) * maxSpeed;
        }
    }

    // Add points onto the end of the path (not using pathfinding)
    public void AddToPath(params Vector3[] nodes) {
        if(nodes != null) {
            this.nodes.AddRange(nodes);
            calculatePathLength();
        }
    }

    public void IncrementDistTravlled(float dist) {
        distTravelled += dist;
    }
    
    public int GetTargetNode() {
        return targetNode;
    }

    public void SetTargetNode(int node) {
        //Debug.LogFormat("Set node: {0}/{1}", node, nodes.Count);
        if (node >= nodes.Count) {
            onCompletePath();
            return;
        }

        targetNode = node;
        onSetTargetNode(nodes[targetNode]);
    }

    public void NextTargetNode() {
        SetTargetNode(targetNode + 1);
    }

    // Distance between player and the most recent node passed 
    public float DistFromLastWaypoint(Vector3 position) {
        if (targetNode > 0) {
            return Vector3.Distance(position, nodes[targetNode - 1]);
        } else {
            return nodes[targetNode].magnitude;
        }
    }

    // Distance between next node and previous one
    public float DistToNextWaypoint() {
        return Vector3.Distance(nodes[targetNode], nodes[targetNode - 1]);
    }

    public float DistToPathEnd() {
        return totalPathLength - distTravelled;
    }

    private void calculatePathLength() {
        totalPathLength = MathHelper.TotalVectorLengths(nodes.ToArray());
    }

    public void DrawDebugLines() {
        for(int i = targetNode - 1; i < nodes.Count - 1; i++) {
            Debug.DrawLine(nodes[i], nodes[i + 1]);
        }
    }

    public void DrawDebugGizmos() {
        for(int i = targetNode - 1; i < nodes.Count; i++) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nodes[i], 0.05f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

// Store the nodes of a character's path with a bunch of helper functions

public class CharacterMovementPath 
{
    private List<Vector3> nodes;

    // The distance over which to decelerate to zero
    private float decDist;
    private float totalPathLength;
    private float distTravelled;
    private int targetNode;

    // Curves to define speed for acceleration and deceleration 
    private AnimationCurve accCurve, decCurve;

    private UnityAction<Vector3> onSetTargetNode;
    private UnityAction onCompletePath;

    public CharacterMovementPath(List<Vector3> nodes, AnimationCurve accCurve, AnimationCurve decCurve, UnityAction<Vector3> onSetTargetNode, UnityAction onCompletePath)
    {
        this.nodes = nodes;
        this.accCurve = accCurve;
        this.decCurve = decCurve;
        this.onSetTargetNode = onSetTargetNode;
        this.onCompletePath = onCompletePath;
        decDist = decCurve.keys.Last().time;
        calculatePathLength();
    }

    public float GetCurrentSpeed(float maxSpeed) 
    {
        if (distTravelled < totalPathLength - decDist) 
        {
            return accCurve.Evaluate(distTravelled) * maxSpeed;
        }
        else
        {
            return decCurve.Evaluate(distTravelled - (totalPathLength - decDist)) * maxSpeed;
        }
    }

    public void IncrementDistTravlled(float dist) 
    {
        distTravelled += dist;
    }
    
    public int GetTargetNode() 
    {
        return targetNode;
    }

    public void SetTargetNode(int node) 
    {
        //Debug.LogFormat("Set node: {0}/{1}", node, nodes.Count);
        if (node >= nodes.Count) 
        {
            onCompletePath();
            return;
        }

        targetNode = node;
        onSetTargetNode(nodes[targetNode]);
    }

    public void NextTargetNode() 
    {
        SetTargetNode(targetNode + 1);
    }

    // Distance between player and the most recent node passed 
    public float DistFromLastWaypoint(Vector3 position) 
    {
        if (targetNode > 0) 
        {
            return Vector3.Distance(position, nodes[targetNode - 1]);
        }
        else
        {
            return nodes[targetNode].magnitude;
        }
    }

    // Distance between next node and previous one
    public float DistToNextWaypoint() 
    {
        return Vector3.Distance(nodes[targetNode], nodes[targetNode - 1]);
    }

    public float DistToPathEnd() 
    {
        return totalPathLength - distTravelled;
    }

    private void calculatePathLength() 
    {
        totalPathLength = MathHelper.TotalVectorLengths(nodes.ToArray());
    }

    public void DrawDebugLines() 
    {
        for(int i = targetNode - 1; i < nodes.Count - 1; i++) 
        {
            Debug.DrawLine(nodes[i], nodes[i + 1]);
        }
    }

    public void DrawDebugGizmos() 
    {
        for(int i = targetNode - 1; i < nodes.Count; i++) 
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nodes[i], 0.05f);
        }
    }
}

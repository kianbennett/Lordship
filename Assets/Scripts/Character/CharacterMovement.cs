using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterMovement : MonoBehaviour {

    [SerializeField] private MovementParams movementParams;

    private Character character;

    private CharacterMovementPath path;
    private bool hasReachedDestination;
    private bool isRunning;
    private float currentSpeed;
    private Vector3 lookDir;
    private bool canMove = true;

    // Used to add nodes once the path is created
    private List<Vector3> additionalPathNodes = new List<Vector3>();

    void Start () {
        character = GetComponent<Character>();
    }

    void Update() {
        if (path != null && !hasReachedDestination) {
            currentSpeed = path.GetCurrentSpeed(GetMaxSpeed());

            if(path.DistFromLastWaypoint(transform.position) > path.DistToNextWaypoint()) {
                path.NextTargetNode();
            }

            Vector3 delta = lookDir.normalized * currentSpeed * Time.deltaTime;
            path.IncrementDistTravlled(delta.magnitude);
            transform.position += delta;
        }
        // Set transform rotation from LookDir
        if (lookDir != Vector3.zero) {
            lookDir.y = 0; // Lock to xz axis
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5);
        }
    }

    public void MoveToPoint(Vector3 target, bool usePathFinding = true, bool ignoreCanMove = false) {
        if (!ignoreCanMove && !canMove) return;

        path = null;
        hasReachedDestination = false;
        additionalPathNodes.Clear();
        if (usePathFinding) {
            // Start finding path
        } else {
            // Set a path on a straight line between current position and target
            createPath(new List<Vector3>() { transform.position, target });
        }
    }

    // Moves to a position a tiny bit ahead of the direction they were moving (to account for deceleration) 
    public void StopMoving() {
        if(!hasReachedDestination) MoveToPoint(transform.position + lookDir * 0.1f, false);
    }

    // public void SetPath(Path p) {
    //     if (p.error) {
    //         Debug.LogError("Could not get path: " + p.errorLog);
    //         return;
    //     }

    //     createPath(p.vectorPath);
    // }

    public void AddToPath(params Vector3[] nodes) {
        if(path != null) path.AddToPath(nodes);
            else additionalPathNodes.AddRange(nodes);
    }

    private void createPath(List<Vector3> nodes) {
        //Debug.LogFormat("Creating path: {0} nodes and {1} additional nodes", nodes.Count, additionalPathNodes.Count);
        nodes.AddRange(additionalPathNodes);
        additionalPathNodes.Clear();
        path = new CharacterMovementPath(nodes, movementParams, delegate (Vector3 node) {
            lookDir = node - transform.position;
        }, delegate {
            currentSpeed = 0;
            hasReachedDestination = true;
        });
        path.SetTargetNode(1);
    }

    public bool HasPath() {
        return path != null;
    }

    public CharacterMovementPath GetPath() {
        return path;
    }

    public float GetMaxSpeed() {
        return isRunning ? movementParams.RunSpeed : movementParams.WalkSpeed;
    }

    public float GetAnimSpeed() {
        return currentSpeed / movementParams.RunSpeed;
    }

    public void SetLookDir(Vector3 lookDir) {
        this.lookDir = lookDir;
    }

    public bool HasReachedDestination() {
        return hasReachedDestination;
    }
}

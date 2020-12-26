using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterMovement : MonoBehaviour {

    [SerializeField] private MovementParams movementParams;
    [SerializeField] private bool drawDebugLines;

    private Character character;

    private CharacterMovementPath path;
    private bool hasReachedDestination;
    private bool isRunning;
    private float currentSpeed;
    private Vector3 lookDir;
    private bool canMove = true;

    // Used to add nodes once the path is created
    // private List<Vector3> additionalPathNodes = new List<Vector3>();

    private Coroutine findPathCoroutine;

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
            if(path != null) path.IncrementDistTravlled(delta.magnitude); // Check path again incase it gets set to null by path.NextTargetNode()
            transform.position += delta;
        }
        // Set transform rotation from LookDir
        if (lookDir != Vector3.zero) {
            lookDir.y = 0; // Lock to xz axis
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5);
        }

        if(path != null && drawDebugLines) {
            path.DrawDebugLines();
        }

        setHeightFromGridPoint();
    }

    public void MoveToPoint(Vector3 target, bool usePathFinding = true, bool ignoreCanMove = false) {
        if (!ignoreCanMove && !canMove) return;

        path = null;
        hasReachedDestination = false;
        // additionalPathNodes.Clear();
        if (usePathFinding) {
            // Start finding path asynchronously, call SetPath when complete
            findPathAsync(TownGenerator.instance.GetGridPoints(), transform.position, target, SetPath);
        } else {
            // Set a path on a straight line between current position and target
            createPath(new List<Vector3>() { transform.position, target });
        }
    }

    // Moves to a position a tiny bit ahead of the direction they were moving (to account for deceleration) 
    public void StopMoving() {
        if(!hasReachedDestination) MoveToPoint(transform.position + lookDir * 0.1f, false);
    }

    public void SetPath(List<Vector3> nodes) {
        if (nodes == null) {
            Debug.Log("Character path is null!");
            return;
        }
        Debug.Log("Character path found!");
        createPath(nodes);
    }

    // public void AddToPath(params Vector3[] nodes) {
    //     if(path != null) path.AddToPath(nodes);
    //         else additionalPathNodes.AddRange(nodes);
    // }

    private void findPathAsync(GridPoint[,] gridPoints, Vector3 start, Vector3 destination, System.Action<List<Vector3>> onPathFound) {
        // If already finding a path then stop it and find a new one to avoid overlapping
        if(findPathCoroutine != null) StopCoroutine(findPathCoroutine);
        findPathCoroutine = StartCoroutine(findPathIEnum(gridPoints, start, destination, onPathFound));
    }

    private IEnumerator findPathIEnum(GridPoint[,] gridPoints, Vector3 start, Vector3 destination, System.Action<List<Vector3>> onPathFound) {
        GridPoint gridPointStart = TownGenerator.instance.GridPointFromWorldPos(start);
        GridPoint gridPointEnd = TownGenerator.instance.GridPointFromWorldPos(destination);
        List<GridPoint> points = Pathfinder.instance.FindPath(gridPoints, gridPointStart, gridPointEnd);

        List<Vector3> path = new List<Vector3>();

        if(points != null) {
            // Remove first and last grid points as they will be replaced by the exact start and end positions
            if(points.Count >= 2) {
                points.Remove(points.First());
                points.Remove(points.Last());
            }

            path.Add(start);
            path.AddRange(points.Select(o => TownGenerator.instance.GridPointToWorldPos(o.x, o.y) + new Vector3(0.5f, 0, 0.5f)));
            path.Add(destination);
        }

        onPathFound(path);

        yield return null;
    }

    private void createPath(List<Vector3> nodes) {
        // nodes.AddRange(additionalPathNodes);
        // additionalPathNodes.Clear();
        path = new CharacterMovementPath(nodes, movementParams, delegate (Vector3 node) {
            lookDir = node - transform.position;
        }, delegate {
            currentSpeed = 0;
            hasReachedDestination = true;
            path = null;
        });
        path.SetTargetNode(1);
    }

    // Raise the height if walking on pavement
    private void setHeightFromGridPoint() {
        Vector3 pos = transform.position;
        bool isOnPavement = TownGenerator.instance.GridPointFromWorldPos(pos).type == GridPoint.Type.Pavement;
        pos.y = Mathf.Lerp(pos.y, isOnPavement ? 0.05f : 0f, Time.deltaTime * 15);
        transform.position = pos;
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

    public void SetRunning(bool running) {
        isRunning = running;
    }

    public bool HasReachedDestination() {
        return hasReachedDestination;
    }
}

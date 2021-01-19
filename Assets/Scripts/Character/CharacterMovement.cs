using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterMovement : MonoBehaviour {

    [SerializeField] private GameObject moveMarkerPrefab;
    [SerializeField] private MovementParams movementParams;
    [SerializeField] private bool drawDebugLines, debugLogMessages;

    private Character character;

    private CharacterMovementPath path;
    private bool searchingForPath;
    private bool hasReachedDestination;
    private bool isRunning;
    private float currentSpeed;
    private Vector3 lookDir;
    private Character characterSpeaking;

    // Follow another character
    private Character followedCharacter;
    // Position of followed character, if this changes by a certain amount then recalculate path
    private Vector3 lastFollowedCharPos;

    private Coroutine findPathCoroutine;

    public CharacterMovementPath Path { get { return path; } }
    public bool HasPath { get { return path != null; } }
    public bool HasTarget { get { return path != null || searchingForPath; } }
    public float MaxSpeed { get { return isRunning ? movementParams.RunSpeed : movementParams.WalkSpeed; } }
    public float AnimSpeed { get { return HasPath ? currentSpeed / movementParams.RunSpeed : 0; } }
    public bool HasReachedDestination { get { return hasReachedDestination; } }
    public bool IsSpeaking { get { return characterSpeaking; } }
    public Vector3 LookDir {
        get { return lookDir; }
        set { lookDir = value; }
    }
    public bool IsRunning { 
        get { return isRunning; } 
        set { isRunning = value; }
    }

    void Start () {
        character = GetComponent<Character>();
    }

    void Update() {
        // If following a character check their current position - if they moved more than a certain distance
        // then move to their new position
        if(followedCharacter != null) {
            float distToLastPos = Vector3.Distance(followedCharacter.transform.position, lastFollowedCharPos);
            if(distToLastPos > 1) {
                MoveToPoint(followedCharacter.transform.position, true, false);
                lastFollowedCharPos = followedCharacter.transform.position;
            }
        }

        if(characterSpeaking != null) {
            lookDir = characterSpeaking.transform.position - transform.position;
        }

        if (path != null && !hasReachedDestination) {
            if(drawDebugLines) path.DrawDebugLines();

            currentSpeed = path.GetCurrentSpeed(MaxSpeed);

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

        setHeightFromGridPoint();
    }

    public void MoveToPoint(Vector3 target, bool run = false, bool showMarker = false, bool usePathFinding = true) {
        isRunning = run;

        path = null;
        hasReachedDestination = false;
        // additionalPathNodes.Clear();
        if (usePathFinding) {
            // Start finding path asynchronously, call SetPath when complete
            findPathAsync(TownGenerator.instance.GridPoints, transform.position, target, showMarker);
        } else {
            // Set a path on a straight line between current position and target
            createPath(new List<Vector3>() { transform.position, target }, showMarker);
        }
    }

    // Moves to a position a tiny bit ahead of the direction they were moving (to account for deceleration) 
    public void StopMoving(Vector3 lookDir = default) {
        if(lookDir == default) lookDir = this.lookDir;
        if(!hasReachedDestination) MoveToPoint(transform.position + lookDir * 0.1f, false, false, false);
        CancelFollowing();
    }

    // private void setPath(List<Vector3> nodes) {
    //     if (nodes == null) {
    //         if(debugLogMessages) Debug.Log("Character path is null!");
    //         return;
    //     }
    //     if(debugLogMessages) Debug.Log("Character path found!");
        
    //     searchingForPath = false;
    //     createPath(nodes);
    // }

    private void findPathAsync(GridPoint[,] gridPoints, Vector3 start, Vector3 destination, bool showMarker) {
        searchingForPath = true;

        GridPoint gridPointStart = TownGenerator.instance.GridPointFromWorldPos(start);
        GridPoint gridPointEnd = TownGenerator.instance.GridPointFromWorldPos(destination);
        // If already finding a path then stop it and find a new one to avoid overlapping
        if(findPathCoroutine != null) StopCoroutine(findPathCoroutine);
        findPathCoroutine = StartCoroutine(
            Pathfinder.instance.FindPath(gridPoints, gridPointStart, gridPointEnd, debugLogMessages, 
            delegate(List<GridPoint> points) {
                onPathComplete(start, destination, points, showMarker);
            })
        );
    }

    private void onPathComplete(Vector3 start, Vector3 destination, List<GridPoint> points, bool showMarker) {
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
        
        // Smooth and clean path
        path = Pathfinder.SmoothPath(path);
        Pathfinder.RemoveRedundantNodes(path);

        searchingForPath = false;
        createPath(path, showMarker);
    }

    private void createPath(List<Vector3> nodes, bool showMarker) {
        if(nodes.Count < 1) return;

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

        if (showMarker) {
            Instantiate(moveMarkerPrefab, nodes.Last(), Quaternion.identity);
        }
    }

    public void SetSpeaking(Character character) {
        characterSpeaking = character;

        if(characterSpeaking != null) {
            // Vector3 dir = characterSpeaking.transform.position - transform.position;
            // if(dir.magnitude < 2) {
            //     MoveToPoint(characterSpeaking.transform.position + dir.normalized * 2, false);
            //     CancelFollowing();
            // } else {
                // StopMoving();
            // }
            StopMoving();
        }
    }

    public void FollowCharacter(Character followedCharacter, bool run = false) {
        this.followedCharacter = followedCharacter;
        lastFollowedCharPos = followedCharacter.transform.position;
        MoveToPoint(followedCharacter.transform.position, run, false);
    }

    public void CancelFollowing() {
        followedCharacter = null;
    }

    public float FollowedCharacterDistance() {
        if(followedCharacter != null) {
            return Vector3.Distance(followedCharacter.transform.position, transform.position);
        }
        return 0;
    }

    // Raise the height if walking on pavement
    private void setHeightFromGridPoint() {
        Vector3 pos = transform.position;
        bool isOnPavement = TownGenerator.instance.GridPointFromWorldPos(pos).type == GridPoint.Type.Pavement;
        pos.y = Mathf.Lerp(pos.y, isOnPavement ? 0.05f : 0f, Time.deltaTime * 15);
        transform.position = pos;
    }

    void OnDrawGizmos() {
        if(HasPath && drawDebugLines) {
            path.DrawDebugGizmos();
        }
    }
}

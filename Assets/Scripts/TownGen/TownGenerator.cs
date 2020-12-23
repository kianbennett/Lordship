using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridPoint {
    public enum Type {
        Path, Grass, Obstacle
    }

    public int x, y;
    public Type type;

    public GridPoint(int x, int y, Type type) {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}

[ExecuteInEditMode]
public class TownGenerator : MonoBehaviour {

    [Header("Debug")]
    public bool debugGrid;
    public bool debugRoadPath;
    public bool debugBuildingBounds;
    public float debugNoiseScale;
    [Range(0f, 1f)]
    public float debugNoiseThreshold1, debugNoiseThreshold2;

    [Header("Parameters")]
    public int width;
    public int height;
    public int borderSize;
    public int minNodeSize, maxNodeSize;
    public int seed;

    [Header("Noise Parameters")]
    public float treeNoiseScale;
    [Range(0f, 1f)]
    public float treeNoiseThreshold;
    public float grassNoiseScale;
    [Range(0f, 1f)]
    public float grassNoiseThresholdSmall, grassNoiseThresholdBig;

    [Header("Objects")]
    public Transform basePlane;
    public Transform objectContainer;
    public GameObject roadPrefab, pavementPrefab;
    public GameObject wallPrefab, wallCornerPrefab, gatePrefab;
    public GameObject lamppostPrefab;
    public GameObject grassPrefab;
    public GameObject[] treePrefabs;
    public Building[] buildingPrefabs;

    private List<BSPNode> nodes;
    private GridPoint[,] gridPoints;
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Update() {
        if(debugRoadPath && nodes != null) {
            foreach(BSPNode node in nodes) {
                float x = node.x - width / 2f + borderSize;
                float y = node.y - height / 2f + borderSize;
                Debug.DrawLine(new Vector3(x, 0, y), new Vector3(x + node.width, 0, y));
                Debug.DrawLine(new Vector3(x + node.width, 0, y), new Vector3(x + node.width, 0, y + node.height));
                Debug.DrawLine(new Vector3(x + node.width, 0, y + node.height), new Vector3(x, 0, y + node.height));
                Debug.DrawLine(new Vector3(x, 0, y + node.height), new Vector3(x, 0, y));
            }
        }

        if(debugGrid && gridPoints != null) {
            float originX = -width / 2f;
            float originZ = -height / 2f;
            for(int i = 0; i < width; i++) Debug.DrawLine(new Vector3(originX + i, 0, originZ), new Vector3(originX + i, 0, originZ + height), new Color(1, 1, 1, 0.2f));
            for(int j = 0; j < height; j++) {
                Debug.DrawLine(new Vector3(originX, 0, originZ + j), new Vector3(originX + width, 0, originZ + j), new Color(1, 1, 1, 0.2f));
                for(int i = 0; i < width; i++) {
                    Color color = Color.green;
                    if(gridPoints[i, j].type == GridPoint.Type.Path) color = Color.blue;
                    if(gridPoints[i, j].type == GridPoint.Type.Obstacle) color = Color.red;
                    if(gridPoints[i, j].type == GridPoint.Type.Grass) continue;
                    Debug.DrawLine(new Vector3(originX + i + 0.5f, 0, originZ + j + 0.25f), new Vector3(originX + i + 0.5f, 0, originZ + j + 0.75f), color);
                    Debug.DrawLine(new Vector3(originX + i + 0.25f, 0, originZ + j + 0.5f), new Vector3(originX + i + 0.75f, 0, originZ + j + 0.5f), color);
                    
                    // float noiseValue = Mathf.PerlinNoise(i * debugNoiseScale, j * debugNoiseScale);
                    // if(noiseValue > debugNoiseThreshold2) {
                    //     Debug.DrawLine(new Vector3(originX + i + 0.5f, 0, originZ + j + 0.25f), new Vector3(originX + i + 0.5f, 0, originZ + j + 0.75f), Color.green);
                    //     Debug.DrawLine(new Vector3(originX + i + 0.25f, 0, originZ + j + 0.5f), new Vector3(originX + i + 0.75f, 0, originZ + j + 0.5f), Color.green);
                    // } else if(noiseValue > debugNoiseThreshold1) {
                    //     Debug.DrawLine(new Vector3(originX + i + 0.5f, 0, originZ + j + 0.25f), new Vector3(originX + i + 0.5f, 0, originZ + j + 0.75f), Color.white);
                    //     Debug.DrawLine(new Vector3(originX + i + 0.25f, 0, originZ + j + 0.5f), new Vector3(originX + i + 0.75f, 0, originZ + j + 0.5f), Color.white);
                    // }
                }
            }
        }

        if(debugBuildingBounds) {
            foreach(Building building in objectContainer.GetComponentsInChildren<Building>()) {
                float w = building.transform.rotation.eulerAngles.y % 180 != 0 ? building.depth : building.width;
                float h = building.transform.rotation.eulerAngles.y % 180 != 0 ? building.width : building.depth;
                float x = building.transform.position.x - w / 2f;
                float y = building.transform.position.z - h / 2f;

                int xMin = Mathf.FloorToInt(x);
                int xMax = Mathf.CeilToInt(x + w);
                int yMin = Mathf.FloorToInt(y);
                int yMax = Mathf.CeilToInt(y + h);

                Debug.DrawLine(new Vector3(xMin, 0, yMin), new Vector3(xMax, 0, yMin), Color.yellow);
                Debug.DrawLine(new Vector3(xMax, 0, yMin), new Vector3(xMax, 0, yMax), Color.yellow);
                Debug.DrawLine(new Vector3(xMax, 0, yMax), new Vector3(xMin, 0, yMax), Color.yellow);
                Debug.DrawLine(new Vector3(xMin, 0, yMax), new Vector3(xMin, 0, yMin), Color.yellow);
            }    
        }
    }

    public void Generate() {
        Random.InitState(seed);

        stopwatch.Restart();
        nodes = new List<BSPNode>();
        BSPNode rootNode = new BSPNode(0, 0, width - borderSize * 2, height - borderSize * 2, minNodeSize);
        splitBSPNode(rootNode);

        gridPoints = new GridPoint[width, height];
        for(int j = 0; j < height; j++) {
            for(int i = 0; i < width; i++) {
                GridPoint.Type type = GridPoint.Type.Grass;
                if(i == 0 || i == width - 1 || j == 0 || j == height - 1) type = GridPoint.Type.Obstacle;
                gridPoints[i, j] = new GridPoint(i, j, type);
            }
        }

        Build();

        stopwatch.Stop();
        Debug.Log("Found " + nodes.Count + " nodes in " + ((float) stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond) + "ms");
    }

    // Place objects from the generated BSP
    public void Build() {
        if(nodes == null) return;

        // Destroy all children in base plane
        for(int i = objectContainer.childCount - 1; i >= 0; i--) {
            DestroyImmediate(objectContainer.GetChild(i).gameObject);
        }

        // Scale base plane correctly
        basePlane.localScale = new Vector3(width / 10f, 1, height / 10f);

        for(int i = 0; i < nodes.Count; i++) {
            BSPNode node = nodes[i];
            int x = (int) (node.x - width / 2f + borderSize);
            int y = (int) (node.y - height / 2f + borderSize);

            // Add roads
            if(node.y != 0) {
                // TODO: Add function that adds a road and sets the grid types all in one
                GameObject roadH = Instantiate(roadPrefab, new Vector3(x + node.width / 2f, 0.01f, y), Quaternion.Euler(90, 0, 0), objectContainer);
                roadH.transform.localScale = new Vector3(node.width, 2 /*Random.Range(1.5f, 2.5f)*/, 1);
                setGridPointsFromRect(node.x + borderSize, node.y + borderSize - 1, node.width, 2, GridPoint.Type.Path);

            }
            if(node.x != 0) {
                GameObject roadV = Instantiate(roadPrefab, new Vector3(x, 0.01f, y + node.height / 2f), Quaternion.Euler(90, 0, 0), objectContainer);
                roadV.transform.localScale = new Vector3(2, node.height, 1);
                setGridPointsFromRect(node.x + borderSize - 1, node.y + borderSize, 2, node.height, GridPoint.Type.Path);
            }

            // Add pavements
            float pavementWidth = 0.8f;
            GameObject pavementL = Instantiate(pavementPrefab, new Vector3(x + 1 + pavementWidth / 2f, 0, y + node.height / 2f), Quaternion.identity, objectContainer);
            GameObject pavementR = Instantiate(pavementPrefab, new Vector3(x + node.width - 1 - pavementWidth / 2f, 0, y + node.height / 2f), Quaternion.identity, objectContainer);
            GameObject pavementT = Instantiate(pavementPrefab, new Vector3(x + node.width / 2f, 0, y + node.height - 1 - pavementWidth / 2f), Quaternion.identity, objectContainer);
            GameObject pavementB = Instantiate(pavementPrefab, new Vector3(x + node.width / 2f, 0, y + 1 + pavementWidth / 2f), Quaternion.identity, objectContainer);
            pavementL.transform.localScale = pavementR.transform.localScale = new Vector3(pavementWidth, pavementL.transform.localScale.y, node.height - 2);
            pavementT.transform.localScale = pavementB.transform.localScale = new Vector3(node.width - 2, pavementL.transform.localScale.y, pavementWidth);

            // Add buildings
            float border = 2.5f;
            float gapMin = 0.5f, gapMax = 2.5f;
            Rect buildingArea = new Rect(x + border, y + border, node.width - border * 2, node.height - border * 2);

            Building buildingPrefabFirst = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            Building buildingNext;

            // Left edge
            buildingNext = placeBuildingsAlongEdge(buildingPrefabFirst, new Vector3(buildingArea.xMin, 0, buildingArea.yMin), new Vector3(buildingArea.x, 0, buildingArea.yMax), gapMin, gapMax);
            // Top edge
            buildingNext = placeBuildingsAlongEdge(buildingNext, new Vector3(buildingArea.x, 0, buildingArea.yMax), new Vector3(buildingArea.xMax, 0, buildingArea.yMax), gapMin, gapMax);
            // Right edge
            buildingNext = placeBuildingsAlongEdge(buildingNext, new Vector3(buildingArea.xMax, 0, buildingArea.yMax), new Vector3(buildingArea.xMax, 0, buildingArea.y), gapMin, gapMax);
            // Bottom edge
            placeBuildingsAlongEdge(buildingNext, new Vector3(buildingArea.xMax, 0, buildingArea.y), new Vector3(buildingArea.xMin, 0, buildingArea.yMin), gapMin, gapMax);

            // Add lampposts
            int corner = Random.Range(0, 4); // 0 is bottom left, increases clockwise
            float indent = 1.8f;
            Vector3 pos = new Vector3(x + indent + (corner > 1 ? node.width - indent * 2 : 0), 0, y + indent + (corner == 1 || corner == 2 ? node.height - indent * 2 : 0));
            GameObject lamppost = Instantiate(lamppostPrefab, pos, Quaternion.Euler(0, -135 + 90 * corner, 0), objectContainer);
            setGridPointFromWorldPos(pos, GridPoint.Type.Obstacle);
        }

        int roadAreaWidth = width - borderSize * 2;
        int roadAreaHeight = height - borderSize * 2;

        // Add border roads
        GameObject roadL = Instantiate(roadPrefab, new Vector3(-roadAreaWidth / 2f, 0.01f, 0), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadR = Instantiate(roadPrefab, new Vector3(roadAreaWidth / 2f, 0.01f, 0), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadT = Instantiate(roadPrefab, new Vector3(0, 0.01f, roadAreaHeight / 2), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadB = Instantiate(roadPrefab, new Vector3(0, 0.01f, -roadAreaHeight / 2), Quaternion.Euler(90, 0, 0), objectContainer);
        roadL.transform.localScale = roadR.transform.localScale = new Vector3(2, roadAreaHeight + 2, 1);
        roadT.transform.localScale = roadB.transform.localScale = new Vector3(roadAreaWidth + 2, 2, 1);
        setGridPointsFromRect(borderSize - 1, borderSize - 1, 2, roadAreaHeight + 2, GridPoint.Type.Path);
        setGridPointsFromRect(width - borderSize - 1, borderSize - 1, 2, roadAreaHeight + 2, GridPoint.Type.Path);
        setGridPointsFromRect(borderSize - 1, borderSize - 1, roadAreaWidth + 2, 2, GridPoint.Type.Path);
        setGridPointsFromRect(borderSize - 1, height - borderSize - 1, roadAreaWidth + 2, 2, GridPoint.Type.Path);

        // Add gates
        bool gate1Left = Random.value > 0.5f; // Gate on left or right wall
        bool gate2Bottom = Random.value > 0.5f; // Gate on top or bottom wall
        int gate1Pos = Random.Range((int) (-roadAreaHeight / 2 * 0.8f), (int) (roadAreaHeight / 2 * 0.8f));
        int gate2Pos = Random.Range((int) (-roadAreaWidth / 2 * 0.8f), (int) (roadAreaWidth / 2 * 0.8f));
        GameObject gate1 = Instantiate(gatePrefab, new Vector3(width / 2f * (gate1Left ? -1 : 1), 0, gate1Pos), Quaternion.Euler(0, 90, 0), objectContainer);
        GameObject gate2 = Instantiate(gatePrefab, new Vector3(gate2Pos, 0, height / 2f * (gate2Bottom ? -1 : 1)), Quaternion.identity, objectContainer);

        // Add border buildings
        Bounds gate1Bounds = new Bounds(gate1.transform.position, MathHelper.AbsVector3(gate1.transform.rotation * new Vector3(8, 1, borderSize + 12)));
        Bounds gate2Bounds = new Bounds(gate2.transform.position, MathHelper.AbsVector3(gate2.transform.rotation * new Vector3(8, 1, borderSize + 12)));
        placeBuildingsAlongEdge(null, new Vector3(-roadAreaWidth / 2, 0, -roadAreaHeight / 2 - 2), new Vector3(roadAreaWidth / 2, 0, -roadAreaHeight / 2 - 2), 0.5f, 2.5f, false, gate2Bounds);
        placeBuildingsAlongEdge(null, new Vector3(roadAreaWidth / 2 + 2, 0, -roadAreaHeight / 2), new Vector3(roadAreaWidth / 2 + 2, 0, roadAreaHeight / 2 - 2), 0.5f, 2.5f, false, gate1Bounds);
        placeBuildingsAlongEdge(null, new Vector3(roadAreaWidth / 2, 0, roadAreaHeight / 2 + 2), new Vector3(-roadAreaWidth / 2, 0, roadAreaHeight / 2 + 2), 0.5f, 2.5f, false, gate2Bounds);
        placeBuildingsAlongEdge(null, new Vector3(-roadAreaWidth / 2 - 2, 0, roadAreaHeight / 2), new Vector3(-roadAreaWidth / 2 - 2, 0, -roadAreaHeight / 2), 0.5f, 2.5f, false, gate1Bounds);

        // Add walls (TODO: Put this into functions to reduce number of lines)
        if(gate1Left) {
            GameObject wallL1 = Instantiate(wallPrefab, new Vector3(-width / 2f, 0, (-height / 2f + gate1Pos) / 2f - 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallL2 = Instantiate(wallPrefab, new Vector3(-width / 2f, 0, (height / 2f + gate1Pos) / 2f + 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR = Instantiate(wallPrefab, new Vector3(width / 2f, 0, 0), Quaternion.Euler(0, 90, 0), objectContainer);
            wallL1.transform.localScale = new Vector3(height / 2f + gate1Pos - 4, 1, 1);
            wallL2.transform.localScale = new Vector3(height / 2f - gate1Pos - 4, 1, 1);
            wallR.transform.localScale = new Vector3(height - 2, 1, 1);
        } else {
            GameObject wallL = Instantiate(wallPrefab, new Vector3(-width / 2f, 0, 0), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR1 = Instantiate(wallPrefab, new Vector3(width / 2f, 0, (-height / 2f + gate1Pos) / 2f - 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR2 = Instantiate(wallPrefab, new Vector3(width / 2f, 0, (height / 2f + gate1Pos) / 2f + 1), Quaternion.Euler(0, 90, 0), objectContainer);
            wallL.transform.localScale = new Vector3(height - 2, 1, 1);
            wallR1.transform.localScale = new Vector3(height / 2f + gate1Pos - 4, 1, 1);
            wallR2.transform.localScale = new Vector3(height / 2f - gate1Pos - 4, 1, 1);
        }
        if(gate2Bottom) {
            GameObject wallB1 = Instantiate(wallPrefab, new Vector3((-width / 2f + gate2Pos) / 2f - 1, 0, -height / 2f), Quaternion.identity, objectContainer);
            GameObject wallB2 = Instantiate(wallPrefab, new Vector3((width / 2f + gate2Pos) / 2f + 1, 0, -height / 2f), Quaternion.identity, objectContainer);
            GameObject wallT = Instantiate(wallPrefab, new Vector3(0, 0, height / 2f), Quaternion.identity, objectContainer);
            wallB1.transform.localScale = new Vector3(height / 2f + gate2Pos - 4, 1, 1);
            wallB2.transform.localScale = new Vector3(height / 2f - gate2Pos - 4, 1, 1);
            wallT.transform.localScale = new Vector3(height - 2, 1, 1);
        } else {
            GameObject wallB = Instantiate(wallPrefab, new Vector3(0, 0, -height / 2f), Quaternion.identity, objectContainer);
            GameObject wallT1 = Instantiate(wallPrefab, new Vector3((-width / 2f + gate2Pos) / 2f - 1, 0, height / 2f), Quaternion.identity, objectContainer);
            GameObject wallT2 = Instantiate(wallPrefab, new Vector3((width / 2f + gate2Pos) / 2f + 1, 0, height / 2f), Quaternion.identity, objectContainer);
            wallB.transform.localScale = new Vector3(height - 2, 1, 1);
            wallT1.transform.localScale = new Vector3(height / 2f + gate2Pos - 4, 1, 1);
            wallT2.transform.localScale = new Vector3(height / 2f - gate2Pos - 4, 1, 1);
        }

        // Add roads from the gates
        GameObject roadGate1 = Instantiate(roadPrefab, new Vector3((width / 2f - borderSize / 2f) * (gate1Left ? -1 : 1), 0.01f, gate1Pos), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadGate2 = Instantiate(roadPrefab, new Vector3(gate2Pos, 0.01f, (height / 2f - borderSize / 2f) * (gate2Bottom ? -1 : 1)), Quaternion.Euler(90, 0, 0), objectContainer);
        roadGate1.transform.localScale = new Vector3(borderSize, 4, 1);
        roadGate2.transform.localScale = new Vector3(4, borderSize, 1);
        setGridPointsFromRect(gate1Left ? 0 : width - borderSize, gate1Pos + height / 2 - 2, borderSize + 1, 4, GridPoint.Type.Path);
        setGridPointsFromRect(gate2Pos + width / 2 - 2, gate2Bottom ? 0 : height - borderSize, 4, borderSize + 1, GridPoint.Type.Path);

        // Add wall corners
        GameObject wallCorner1 = Instantiate(wallCornerPrefab, new Vector3(-width / 2f, 0, -height / 2f), Quaternion.Euler(0, 90, 0), objectContainer);
        GameObject wallCorner2 = Instantiate(wallCornerPrefab, new Vector3(width / 2f, 0, -height / 2f), Quaternion.Euler(0, 0, 0), objectContainer);
        GameObject wallCorner3 = Instantiate(wallCornerPrefab, new Vector3(width / 2f, 0, height / 2f), Quaternion.Euler(0, -90, 0), objectContainer);
        GameObject wallCorner4 = Instantiate(wallCornerPrefab, new Vector3(-width / 2f, 0, height / 2f), Quaternion.Euler(0, 180, 0), objectContainer);

        // Add lampposts at all 4 corners of the map
        for(int i = 0; i < 4; i++) {
            float indent = 1.8f;
            Vector3 pos = new Vector3(-roadAreaWidth / 2f - indent + (i > 1 ? roadAreaWidth + indent * 2 : 0), 0, -roadAreaHeight / 2f - indent + (i == 1 || i == 2 ? roadAreaHeight + indent * 2 : 0));
            GameObject lamppost = Instantiate(lamppostPrefab, pos, Quaternion.Euler(0, 45 + 90 * i, 0), objectContainer);
            setGridPointFromWorldPos(pos, GridPoint.Type.Obstacle);
        }

        // Add trees
        for(int j = 0; j < height; j += 2) {
            for(int i = 0; i < width; i += 2) {
                if(gridPoints[i, j].type != GridPoint.Type.Grass || gridPoints[i + 1, j].type != GridPoint.Type.Grass || gridPoints[i, j + 1].type != GridPoint.Type.Grass ||
                    gridPoints[i + 1, j + 1].type != GridPoint.Type.Grass) continue;

                float noiseValue = Mathf.PerlinNoise(i * treeNoiseScale, j * treeNoiseScale);

                if(noiseValue > treeNoiseThreshold) {
                    Vector3 offset = MathHelper.RandomVector2(0.2f, 0.8f);
                    Vector3 pos = new Vector3(-width / 2f + i + offset.x, 0, -height / 2f + j + offset.y);
                    GameObject treeObject = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), objectContainer);
                    treeObject.transform.localScale = Vector3.one * Random.Range(1f, 3f);
                    gridPoints[i, j].type = GridPoint.Type.Obstacle;
                }
            }
        }

        // Add grass
        for(int j = 0; j < height; j++) {
            for(int i = 0; i < width; i++) {
                if(gridPoints[i, j].type != GridPoint.Type.Grass) continue;
                
                float noiseValue = Mathf.PerlinNoise(i * grassNoiseScale, j * grassNoiseScale);

                int grassNumber = 0;
                if(noiseValue > grassNoiseThresholdSmall) grassNumber = 1;
                if(noiseValue > grassNoiseThresholdBig) grassNumber = 2;

                for(int g = 0; g < grassNumber; g++) {
                    Vector3 offset = MathHelper.RandomVector2(0.2f, 0.8f);
                    Vector3 pos = new Vector3(-width / 2f + i + offset.x, 0, -height / 2f + j + offset.y);
                    GameObject grassObject = Instantiate(grassPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), objectContainer);
                    grassObject.transform.localScale = MathHelper.RandomVector3(2f, 3f);
                }
            }
        }
    }

    private void splitBSPNode(BSPNode node) {
        bool split = node.width > maxNodeSize || node.height > maxNodeSize || Random.value > 0.25f;
        if(split && node.Split()) {
            splitBSPNode(node.leftChild);
            splitBSPNode(node.rightChild);
        } else {
            nodes.Add(node);
        }
    }

    // Returns building that would be placed at the start of the next edge
    private Building placeBuildingsAlongEdge(Building buildingStart, Vector3 start, Vector3 end, float gapMin, float gapMax, bool leaveGapForLastBuilding = true, Bounds ignoreBounds = default) {
        float edgeLength = (end - start).magnitude;
        Vector3 edgeDir = (end - start).normalized;
        if (!buildingStart) buildingStart = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
        Building buildingPrefabNext = buildingStart;
        Building buildingPrefab;

        // Left edge
        float dist = 0;
        while(dist < edgeLength) {
            buildingPrefab = buildingPrefabNext;
            buildingPrefabNext = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

            float length = edgeLength;
            if(leaveGapForLastBuilding) length -= buildingPrefabNext.depth;
            
            if(dist + buildingPrefab.width < length) {
                Quaternion rot = Quaternion.LookRotation(edgeDir, Vector3.up);
                Vector3 pos = start + edgeDir * dist + rot * new Vector3(buildingPrefab.size.y / 2, 0, buildingPrefab.size.x / 2);
                if (!ignoreBounds.Contains(pos)) {
                    Building building = Instantiate(buildingPrefab, pos, rot * Quaternion.Euler(0, 90, 0), objectContainer);
                    building.transform.localScale = MathHelper.RandomVector3(0.9f, 1.1f);    
                    setGridPointsFromBuilding(building);
                }
            }
            dist += buildingPrefab.width + Random.Range(gapMin, gapMax);
        }

        return buildingPrefabNext;
    }

    // Set a rectangle of points within a grid to a specific type
    private void setGridPointsFromRect(int x, int y, int w, int h, GridPoint.Type type) {
        if(gridPoints == null) return;

        for(int j = y; j < y + h; j++) {
            // Check if position is within bounds of array
            if(j < 0 || j >= height) continue;
            for(int i = x; i < x + w; i++) {
                if(i < 0 || i >= width) continue;

                gridPoints[i, j].type = type;
            }
        }
    }

    private void setGridPointsFromBuilding(Building building) {
        float w = building.transform.rotation.eulerAngles.y % 180 != 0 ? building.depth : building.width;
        float h = building.transform.rotation.eulerAngles.y % 180 != 0 ? building.width : building.depth;
        float x = building.transform.position.x - w / 2f;
        float y = building.transform.position.z - h / 2f;
        int w_i = Mathf.CeilToInt(x + w) - Mathf.FloorToInt(x);
        int h_i = Mathf.CeilToInt(y + h) - Mathf.FloorToInt(y);
        setGridPointsFromRectWorldPos(Mathf.FloorToInt(x), Mathf.FloorToInt(y), w_i, h_i, GridPoint.Type.Obstacle);
    }

    private void setGridPointsFromRectWorldPos(int x, int y, int w, int h, GridPoint.Type type) {
        x = (int) (x + width / 2f);
        y = (int) (y + height / 2f);
        setGridPointsFromRect(x, y, w, h, type);
    }

    private void setGridPointFromWorldPos(Vector3 worldPos, GridPoint.Type type) {
        int x = Mathf.FloorToInt(worldPos.x + width / 2f);
        int y = Mathf.FloorToInt(worldPos.z + height / 2f);
        if(x < 0 || x >= width || y < 0 || y >= height) return;
        gridPoints[x, y].type = type;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TownGenerator : MonoBehaviour {

    [Header("Parameters")]
    public int width;
    public int height;
    public int borderSize;
    public int minNodeSize, maxNodeSize;
    public int seed;

    [Header("Objects")]
    public Transform basePlane;
    public Transform objectContainer;
    public GameObject roadPrefab;
    public GameObject wallPrefab, wallCornerPrefab, gatePrefab;
    public Building[] buildingPrefabs;

    private List<BSPNode> nodes;
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Update() {
        if(nodes != null) {
            foreach(BSPNode node in nodes) {
                float x = node.x - width / 2f;
                float y = node.y - height / 2f;
                Debug.DrawLine(new Vector3(x, 0, y), new Vector3(x + node.width, 0, y));
                Debug.DrawLine(new Vector3(x + node.width, 0, y), new Vector3(x + node.width, 0, y + node.height));
                Debug.DrawLine(new Vector3(x + node.width, 0, y + node.height), new Vector3(x, 0, y + node.height));
                Debug.DrawLine(new Vector3(x, 0, y + node.height), new Vector3(x, 0, y));
            }
        }
    }

    public void Generate() {
        Random.InitState(seed);

        stopwatch.Restart();
        nodes = new List<BSPNode>();
        BSPNode rootNode = new BSPNode(0, 0, width, height, minNodeSize);
        splitBSPNode(rootNode);

        Build();

        stopwatch.Stop();
        Debug.Log("Found " + nodes.Count + " nodes in " + ((float) stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond) + "ms");
    }

    public void Build() {
        if(nodes == null) return;

        // Destroy all children in base plane
        for(int i = objectContainer.childCount - 1; i >= 0; i--) {
            DestroyImmediate(objectContainer.GetChild(i).gameObject);
        }

        // Scale base plane correctly
        basePlane.localScale = new Vector3((width + borderSize * 2f) / 10f, 1, (height + borderSize * 2f) / 10f);

        for(int i = 0; i < nodes.Count; i++) {
            BSPNode node = nodes[i];
            float x = node.x - width / 2f;
            float y = node.y - height / 2f;

            // Add roads and buildings
            if(node.y != 0) {
                GameObject roadH = Instantiate(roadPrefab, new Vector3(x + node.width / 2f, 0.01f, y), Quaternion.Euler(90, 0, 0), objectContainer);
                roadH.transform.localScale = new Vector3(node.width, 2, 1);
            }
            if(node.x != 0) {
                GameObject roadV = Instantiate(roadPrefab, new Vector3(x, 0.01f, y + node.height / 2f), Quaternion.Euler(90, 0, 0), objectContainer);
                roadV.transform.localScale = new Vector3(2, node.height, 1);
            }

            // Add buildings
            float dist = 0;
            float perimeter = node.GetBuildingPerimeter();
            float border = 2.5f;
            Rect buildingArea = new Rect(x + border, y + border, node.width - border * 2, node.height - border * 2);

            Building buildingPrefab, buildingPrefabFirst, buildingPrefabNext;
            buildingPrefabFirst = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            buildingPrefabNext = buildingPrefabFirst;

            // Left edge
            while(dist < buildingArea.height) {
                buildingPrefab = buildingPrefabNext;
                buildingPrefabNext = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

                if(dist + buildingPrefab.width < buildingArea.height - buildingPrefabNext.depth) {
                    Vector3 pos = new Vector3(buildingArea.x + buildingPrefab.size.y / 2, 0, buildingArea.y + dist + buildingPrefab.size.x / 2);
                    Building building = Instantiate(buildingPrefab, pos, Quaternion.Euler(0, 90, 0), objectContainer);
                }
                dist += buildingPrefab.width + Random.Range(0.5f, 2.5f);
            }

            // Top edge
            dist = 0;
            while(dist < buildingArea.width) {
                buildingPrefab = buildingPrefabNext;
                buildingPrefabNext = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

                if(dist + buildingPrefab.width < buildingArea.width - buildingPrefabNext.depth) {
                    Vector3 pos = new Vector3(buildingArea.x + dist + buildingPrefab.size.x / 2, 0, buildingArea.yMax - buildingPrefab.size.y / 2);
                    Building building = Instantiate(buildingPrefab, pos, Quaternion.Euler(0, 0, 0), objectContainer);
                }
                dist += buildingPrefab.width + Random.Range(0.5f, 2.5f);
            }

            // Right edge
            dist = 0;
            while(dist < buildingArea.height) {
                buildingPrefab = buildingPrefabNext;
                buildingPrefabNext = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

                if(dist + buildingPrefab.width < buildingArea.height - buildingPrefabNext.depth) {
                    Vector3 pos = new Vector3(buildingArea.xMax - buildingPrefab.size.y / 2, 0, buildingArea.yMax - dist - buildingPrefab.size.x / 2);
                    Building building = Instantiate(buildingPrefab, pos, Quaternion.Euler(0, 90, 0), objectContainer);
                }
                dist += buildingPrefab.width + Random.Range(0.5f, 2.5f);
            }

            // Bottom edge
            dist = 0;
            while(dist < buildingArea.width) {
                buildingPrefab = buildingPrefabNext;
                buildingPrefabNext = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

                if(dist + buildingPrefab.width < buildingArea.width - buildingPrefabFirst.depth) {
                    Vector3 pos = new Vector3(buildingArea.xMax - dist - buildingPrefab.size.x / 2, 0, buildingArea.y + buildingPrefab.size.y / 2);
                    Building building = Instantiate(buildingPrefab, pos, Quaternion.identity, objectContainer);
                }
                dist += buildingPrefab.width + Random.Range(0.5f, 2.5f);
            }

            // while(dist < perimeter) {
            //     Building buildingPrefab = housePrefabs[Random.Range(0, housePrefabs.Length)];
            //     Building building;
            //     float buildingWidth = buildingPrefab.depth + gap;

            //     if(dist + buildingWidth < buildingArea.height) {
            //         Vector3 pos = new Vector3(buildingArea.x + buildingPrefab.size.x / 2, 0, buildingArea.y + dist + buildingPrefab.size.y / 2);
            //         building = Instantiate(buildingPrefab, pos, Quaternion.Euler(0, 90, 0), objectContainer);
            //     } else if(dist + buildingWidth < buildingArea.height + buildingArea.width) {
            //         Vector3 pos = new Vector3(buildingArea.x + dist - buildingArea.height + buildingWidth + buildingPrefab.size.x / 2, 0, buildingArea.y + buildingArea.height - buildingPrefab.size.y / 2);
            //         building = Instantiate(buildingPrefab, pos, Quaternion.identity, objectContainer);
            //         Material mat = building.GetComponentInChildren<MeshRenderer>().material;
            //         mat.color = Color.blue;
            //         building.GetComponentInChildren<MeshRenderer>().material = mat;
            //     } else if(dist + buildingWidth < buildingArea.height * 2 + buildingArea.width) {

            //     } else {

            //     }

            //     dist += buildingWidth;
            // }
        }

        // Add border roads
        GameObject roadL = Instantiate(roadPrefab, new Vector3(-width / 2f, 0.01f, 0), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadR = Instantiate(roadPrefab, new Vector3(width / 2f, 0.01f, 0), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadT = Instantiate(roadPrefab, new Vector3(0, 0.01f, height / 2f), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadB = Instantiate(roadPrefab, new Vector3(0, 0.01f, -height / 2f), Quaternion.Euler(90, 0, 0), objectContainer);
        roadL.transform.localScale = roadR.transform.localScale = new Vector3(2, height + 2, 1);
        roadT.transform.localScale = roadB.transform.localScale = new Vector3(width + 2, 2, 1);

        // Add gates
        bool gate1Left = Random.value > 0.5f; // Gate on left or right wall
        bool gate2Bottom = Random.value > 0.5f; // Gate on top or bottom wall
        int gate1Pos = Random.Range((int) (-height / 2 * 0.8f), (int) (height / 2 * 0.8f));
        int gate2Pos = Random.Range((int) (-width / 2 * 0.8f), (int) (width / 2 * 0.8f));
        GameObject gate1 = Instantiate(gatePrefab, new Vector3((width / 2f + borderSize) * (gate1Left ? -1 : 1), 0, gate1Pos), Quaternion.Euler(0, 90, 0), objectContainer);
        GameObject gate2 = Instantiate(gatePrefab, new Vector3(gate2Pos, 0, (height / 2f + borderSize) * (gate2Bottom ? -1 : 1)), Quaternion.identity, objectContainer);

        // Add walls (TODO: Put this into functions to reduce number of lines)
        if(gate1Left) {
            GameObject wallL1 = Instantiate(wallPrefab, new Vector3(-width / 2f - borderSize, 0, (-height / 2 - borderSize + gate1Pos) / 2 - 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallL2 = Instantiate(wallPrefab, new Vector3(-width / 2f - borderSize, 0, (height / 2 + borderSize + gate1Pos) / 2 + 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR = Instantiate(wallPrefab, new Vector3(width / 2f + borderSize, 0, 0), Quaternion.Euler(0, 90, 0), objectContainer);
            wallL1.transform.localScale = new Vector3(borderSize + height / 2 + gate1Pos - 4, 1, 1);
            wallL2.transform.localScale = new Vector3(borderSize + height / 2 - gate1Pos - 4, 1, 1);
            wallR.transform.localScale = new Vector3(height + borderSize * 2 - 2, 1, 1);
        } else {
            GameObject wallL = Instantiate(wallPrefab, new Vector3(-width / 2f - borderSize, 0, 0), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR1 = Instantiate(wallPrefab, new Vector3(width / 2f + borderSize, 0, (-height / 2 - borderSize + gate1Pos) / 2 - 1), Quaternion.Euler(0, 90, 0), objectContainer);
            GameObject wallR2 = Instantiate(wallPrefab, new Vector3(width / 2f + borderSize, 0, (height / 2 + borderSize + gate1Pos) / 2 + 1), Quaternion.Euler(0, 90, 0), objectContainer);
            wallL.transform.localScale = new Vector3(height + borderSize * 2 - 2, 1, 1);
            wallR1.transform.localScale = new Vector3(borderSize + height / 2 + gate1Pos - 4, 1, 1);
            wallR2.transform.localScale = new Vector3(borderSize + height / 2 - gate1Pos - 4, 1, 1);
        }
        if(gate2Bottom) {
            GameObject wallB1 = Instantiate(wallPrefab, new Vector3((-width / 2 - borderSize + gate2Pos) / 2 - 1, 0, -height / 2f - borderSize), Quaternion.identity, objectContainer);
            GameObject wallB2 = Instantiate(wallPrefab, new Vector3((width / 2 + borderSize + gate2Pos) / 2 + 1, 0, -height / 2f - borderSize), Quaternion.identity, objectContainer);
            GameObject wallT = Instantiate(wallPrefab, new Vector3(0, 0, height / 2f + borderSize), Quaternion.identity, objectContainer);
            wallB1.transform.localScale = new Vector3(borderSize + height / 2 + gate2Pos - 4, 1, 1);
            wallB2.transform.localScale = new Vector3(borderSize + height / 2 - gate2Pos - 4, 1, 1);
            wallT.transform.localScale = new Vector3(height + borderSize * 2 - 2, 1, 1);
        } else {
            GameObject wallB = Instantiate(wallPrefab, new Vector3(0, 0, -height / 2f - borderSize), Quaternion.identity, objectContainer);
            GameObject wallT1 = Instantiate(wallPrefab, new Vector3((-width / 2 - borderSize + gate2Pos) / 2 - 1, 0, height / 2f + borderSize), Quaternion.identity, objectContainer);
            GameObject wallT2 = Instantiate(wallPrefab, new Vector3((width / 2 + borderSize + gate2Pos) / 2 + 1, 0, height / 2f + borderSize), Quaternion.identity, objectContainer);
            wallB.transform.localScale = new Vector3(height + borderSize * 2 - 2, 1, 1);
            wallT1.transform.localScale = new Vector3(borderSize + height / 2 + gate2Pos - 4, 1, 1);
            wallT2.transform.localScale = new Vector3(borderSize + height / 2 - gate2Pos - 4, 1, 1);
        }

        // Add roads from the gates
        GameObject roadGate1 = Instantiate(roadPrefab, new Vector3((width / 2f + borderSize / 2f) * (gate1Left ? -1 : 1), 0.01f, gate1Pos), Quaternion.Euler(90, 0, 0), objectContainer);
        GameObject roadGate2 = Instantiate(roadPrefab, new Vector3(gate2Pos, 0.01f, (height / 2f + borderSize / 2f) * (gate2Bottom ? -1 : 1)), Quaternion.Euler(90, 0, 0), objectContainer);
        roadGate1.transform.localScale = new Vector3(borderSize, 2, 1);
        roadGate2.transform.localScale = new Vector3(2, borderSize, 1);

        // Add wall corners
        GameObject wallCorner1 = Instantiate(wallCornerPrefab, new Vector3(-width / 2f - borderSize, 0, -height / 2f - borderSize), Quaternion.Euler(0, 90, 0), objectContainer);
        GameObject wallCorner2 = Instantiate(wallCornerPrefab, new Vector3(width / 2f + borderSize, 0, -height / 2f - borderSize), Quaternion.Euler(0, 0, 0), objectContainer);
        GameObject wallCorner3 = Instantiate(wallCornerPrefab, new Vector3(width / 2f + borderSize, 0, height / 2f + borderSize), Quaternion.Euler(0, -90, 0), objectContainer);
        GameObject wallCorner4 = Instantiate(wallCornerPrefab, new Vector3(-width / 2f - borderSize, 0, height / 2f + borderSize), Quaternion.Euler(0, 180, 0), objectContainer);
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
}

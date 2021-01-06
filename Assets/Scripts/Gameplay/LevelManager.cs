using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {

    [SerializeField] private int npcsMin, npcsMax;
    [SerializeField] private NPC npcPrefab;
    [SerializeField] private Transform npcContainer;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float dayDuration;

    private List<NPC> npcs;
    // Need a reference to all grid points of type Path or Pavement to spawn NPCs on and to set where they walk to
    private GridPoint[] roadGridPoints; 
    private float timeElapsed;

    public GridPoint[] RoadGridPoints { get { return roadGridPoints; } }

    protected override void Awake() {
        base.Awake();
    }

    // Spawn NPCs in Start after grid points get set in TownGenerator's Awake
    void Start() {
        spawnNpcs();
    }

    void Update() {
        timeElapsed += Time.deltaTime; 
        // For now wrap over to 0, this wouldn't happen in an actual level as the day would end
        if(timeElapsed > dayDuration) timeElapsed = 0;
        dayNightCycle.UpdateDayTime(timeElapsed, dayDuration);
    }

    // Should probably move this to TownGenerator
    private void spawnNpcs() {
        int npcNumber = Random.Range(npcsMax, npcsMax);
        MathHelper.stopwatch.Restart();

        // Destroy any existing NPCs
        if(npcs != null) {
            foreach(NPC character in npcs) Destroy(character.gameObject);
        }
        npcs = new List<NPC>();

        roadGridPoints = TownGenerator.instance.GetGridPoints(GridPoint.Type.Path, GridPoint.Type.Pavement);

        // For each NPC use Mitchell's Best Candidate to find a grid point further away from all others
        for(int i = 0; i < npcNumber; i++) {
            GridPoint bestCandidate = null;
            float bestDistance = 0;
            int sampleCount = 10; // High count yields better distrubition but lower performance
            for(int s = 0; s < sampleCount; s++) {
                GridPoint candidate = roadGridPoints[Random.Range(0, roadGridPoints.Length)];
                float distance = distToClosestNpc(candidate, npcs);
                if(distance > bestDistance || bestDistance == 0) {
                    bestCandidate = candidate;
                    bestDistance = distance;
                }
            }

            NPC npc = Instantiate(npcPrefab, TownGenerator.instance.GridPointToWorldPos(bestCandidate) + new Vector3(0.5f, 0, 0.5f), Quaternion.identity, npcContainer);
            npc.Randomise();
            npcs.Add(npc);
        }

        MathHelper.stopwatch.Stop();
        Debug.LogFormat("{0} NPCs spawned in {1}ms", npcNumber, (float) MathHelper.stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond);
    }

    // Get distance from grid point to the closes NPC
    private float distToClosestNpc(GridPoint gridPoint, List<NPC> characters) {
        float dist = 0;
        foreach(NPC c in characters) {
            float d = Vector3.Distance(TownGenerator.instance.GridPointToWorldPos(gridPoint), c.transform.position);
            if(dist == 0 || d < dist) dist = d;
        }
        return dist;
    }
}

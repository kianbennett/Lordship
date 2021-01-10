using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NpcSpawner : MonoBehaviour {

    [SerializeField] private int npcsMin, npcsMax;
    [SerializeField] private NPC npcPrefab;
    [SerializeField] private Transform npcContainer;

    private List<NPC> npcs;
    
    public List<NPC> NpcList { get { return npcs; } }

    public void SpawnNpcs() {
        MathHelper.stopwatch.Restart();

        // Destroy any existing NPCs
        if(npcs != null) {
            foreach(NPC character in npcs) Destroy(character.gameObject);
        }
        npcs = new List<NPC>();

        int npcNumber = Random.Range(npcsMax, npcsMax);
        // Take away three for the 3 candidates
        npcNumber -= 3;

        // For each NPC use Mitchell's Best Candidate to find a grid point further away from all others
        for(int i = 0; i < npcNumber; i++) {
            SpawnNewNpc();
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
        // Check distance to player as well since we don't want NPCs spawning near the palyer
        if(PlayerController.instance && PlayerController.instance.playerCharacter) {
            float playerDist = Vector3.Distance(TownGenerator.instance.GridPointToWorldPos(gridPoint), PlayerController.instance.playerCharacter.transform.position);
            if(dist == 0 || playerDist < dist) dist = playerDist;
        }
        return dist;
    }

    public NPC SpawnNewNpc() {
        GridPoint bestCandidate = null;
        float bestDistance = 0;
        int sampleCount = 10; // High count yields better distrubition but lower performance
        for(int s = 0; s < sampleCount; s++) {
            GridPoint candidate = TownGenerator.instance.RoadGridPoints[Random.Range(0, TownGenerator.instance.RoadGridPoints.Length)];
            float distance = distToClosestNpc(candidate, npcs);
            if(distance > bestDistance || bestDistance == 0) {
                bestCandidate = candidate;
                bestDistance = distance;
            }
        }

        NPC npc = Instantiate(npcPrefab, TownGenerator.instance.GridPointToWorldPos(bestCandidate) + new Vector3(0.5f, 0, 0.5f), Quaternion.identity, npcContainer);
        npc.Randomise();
        npc.name = "NPC (" + npc.DisplayName + ")";
        npcs.Add(npc);

        return npc;
    }
}

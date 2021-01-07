using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
    private int goldAmount;
    private bool isPaused;

    public GridPoint[] RoadGridPoints { get { return roadGridPoints; } }
    public List<NPC> NpcList { get { return npcs; } }
    public int GoldRemaining { get { return goldAmount; } }
    public bool IsPaused { get { return isPaused; } }

    protected override void Awake() {
        base.Awake();
        AudioManager.instance.musicTown.PlayAsMusic();
    }

    // Spawn NPCs in Start after grid points get set in TownGenerator's Awake
    void Start() {
        spawnNpcs();
        DialogueSystem.instance.InitialiseRumours();
        goldAmount = 1000;
    }

    void Update() {
        timeElapsed += Time.deltaTime; 
        // For now wrap over to 0, this wouldn't happen in an actual level as the day would end
        if(timeElapsed > dayDuration) timeElapsed = 0;
        dayNightCycle.UpdateDayTime(timeElapsed, dayDuration);
    }

    public void SetPaused(bool paused) {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        CameraController.instance.SetPostProcessingEffectEnabled<ColorGrading>(paused);
        HUD.instance.pauseMenu.SetActive(paused);
    }

    public void TogglePaused() {
        SetPaused(!isPaused);
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
            npc.name = "NPC (" + npc.DisplayName + ")";
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

    public void RemoveGold(int amount) {
        goldAmount -= amount;
        if(goldAmount < 0) goldAmount = 0;
    }

    // bribeAmount = 0 (low), 1 (mid), 2 (height), returns
    public int BribeGoldAmount(int bribeAmount) {
        int goldAmount = 50;
        if(bribeAmount == 1) goldAmount = 200;
        if(bribeAmount == 2) goldAmount = 500;
        return goldAmount;
    }

    public void SpendBribe(int bribeAmount) {
        LevelManager.instance.RemoveGold(BribeGoldAmount(bribeAmount));
    }

    public bool CanAffordBribe(int bribeAmount) {
        return GoldRemaining >= BribeGoldAmount(bribeAmount);
    }
}

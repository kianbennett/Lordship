using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum Season { Spring, Summer, Autumn, Winter }

public class LevelManager : Singleton<LevelManager> {

    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float dayDuration;

    private float timeElapsed;
    private int goldAmount;
    private bool isPaused;
    private int currentYear, electionYear;
    private Season currentSeason, electionSeason;

    private NPC[] opponents;

    public int GoldRemaining { get { return goldAmount; } }
    public bool IsPaused { get { return isPaused; } }

    protected override void Awake() {
        base.Awake();
        AudioManager.instance.musicTown.PlayAsMusic();
        HUD.instance.screenFader.SetAlpha(1);
    }

    void Start() {
        TownGenerator.instance.Generate();
        // Spawn 3 opponent politicians
        opponents = new NPC[3];
        for(int i = 0; i < opponents.Length; i++) {
            opponents[i] = TownGenerator.instance.npcSpawner.SpawnNewNpc();
            opponents[i].occupation = CharacterOccupation.Politician;
            opponents[i].wealth = CharacterWealth.Rich;
        }
        // Only fade in once town has been spawned to avoid stuttering while fading
        HUD.instance.screenFader.FadeIn();

        Random.InitState((int) System.DateTime.UtcNow.Ticks);
        DialogueSystem.instance.InitialiseRumours();
        goldAmount = 1000;
        currentYear = Random.Range(1500, 1700);
        electionYear = currentYear + 1;
        currentSeason = Season.Autumn;
        electionSeason = Season.Summer;
    }

    void Update() {
        timeElapsed += Time.deltaTime; 
        // For now wrap over to 0, this wouldn't happen in an actual level as the day would end
        if(timeElapsed > dayDuration) timeElapsed = 0;
        dayNightCycle.UpdateDayTime(timeElapsed, dayDuration);

        if(Input.GetKeyDown(KeyCode.P)) {
            List<CharacterAppearance> characters = new List<CharacterAppearance>();
            List<string> names = new List<string>();
            characters.Add(PlayerController.instance.playerCharacter.appearance);
            names.Add("You");
            foreach(NPC npc in opponents) {
                names.Add(npc.DisplayName);
                characters.Add(npc.appearance);
            }
            int totalVotes = TownGenerator.instance.npcSpawner.NpcList.Count - 3; // -3 for the 3 opponents
            HUD.instance.resultsMenu.ShowResults(currentSeason, currentYear, electionSeason, electionYear, characters.ToArray(), 
                names.ToArray(), calculateVotes(), totalVotes);
        }
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

    private int[] calculateVotes() {
        int[] votes = new int[opponents.Length + 1];

        float[] opponentPopularities = new float[opponents.Length];
        float totalPopularity = 0;
        for(int i = 0; i < opponentPopularities.Length; i++) {
            // Each opponent has a random popularity value between 0.4 and 0.6 which influences which one each npc will vote for
            opponentPopularities[i] = 0.4f + 0.2f * Random.value;
            totalPopularity += opponentPopularities[i];
        }

        foreach(NPC npc in TownGenerator.instance.npcSpawner.NpcList) {
            if(npc.occupation != CharacterOccupation.Politician) {
                int dispositionForVote = Random.Range(50, 70);
                // For for player
                if(npc.disposition >= dispositionForVote) {
                    votes[0]++;
                } else {
                    // Otherwise vote for random opponent based on their popularity
                    float r = Random.Range(0f, totalPopularity);
                    float p = 0;
                    for(int i = 0; i < opponentPopularities.Length; i++) {
                        p += opponentPopularities[i];
                        if(r < p) {
                            votes[i + 1]++;
                            break;
                        }
                    }
                }
            }
        }

        return votes;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Controls the day progress and transitions, pausing and gold

public enum Season { Spring, Summer, Autumn, Winter }

public class LevelManager : Singleton<LevelManager> 
{
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float dayDuration;
    [SerializeField] private bool showIntro;

    private float timeElapsed;
    private int goldAmount;
    private bool isPaused;
    private int currentYear, electionYear;
    private Season currentSeason, electionSeason;
    private int day; // 0 for first day, 1 for second day etc
    private bool isInDayTransition;
    private bool hasShownTutorial;

    private NPC[] opponents;
    private List<CharacterAppearance> candidateAppearances;
    private List<string> candidateNames;

    public int GoldRemaining { get { return goldAmount; } }
    public bool IsPaused { get { return isPaused; } }

    void Start() 
    {
        TownGenerator.instance.Generate(true);
        TownGenerator.instance.npcSpawner.SpawnNpcs();
        // Spawn 3 opponent politicians
        opponents = new NPC[3];
        candidateAppearances = new List<CharacterAppearance>();
        candidateNames = new List<string>();
        candidateAppearances.Add(PlayerController.instance.playerCharacter.Appearance);
        candidateNames.Add("You");

        for(int i = 0; i < opponents.Length; i++) 
        {
            opponents[i] = TownGenerator.instance.npcSpawner.SpawnNewNpc();
            opponents[i].occupation = CharacterOccupation.Politician;
            opponents[i].wealth = CharacterWealth.Rich;
            opponents[i].disposition = Random.Range(10, 30); // Politicians automatically dislike like you for running against then
            opponents[i].Randomise(); // Need to randomise again to account for changes in attributes
            candidateAppearances.Add(opponents[i].Appearance);
            candidateNames.Add(opponents[i].DisplayName);
        }

        Random.InitState((int) System.DateTime.UtcNow.Ticks);
        DialogueSystem.instance.InitialiseRumours();
        goldAmount = 2500;
        currentYear = Random.Range(1500, 1700);
        electionYear = currentYear + 1;
        currentSeason = Season.Autumn;
        electionSeason = Season.Summer;

        if(showIntro) 
        {
            HUD.instance.screenFader.SetAlpha(1);
            StartCoroutine(startLevelIEnum(0.8f));
        } 
        else 
        {
            // If not showing inro then don't show the tutorial next level start
            hasShownTutorial = true;
        }
    }

    void Update() 
    {
        if(!isInDayTransition) 
        {
            timeElapsed += Time.deltaTime; 
            if(timeElapsed > dayDuration) StartCoroutine(endLevelIEnum());
        }
        dayNightCycle.UpdateDayTime(timeElapsed, dayDuration);
        
        if(Input.GetKeyDown(KeyCode.Return)) {
            foreach(NPC npc in TownGenerator.instance.npcSpawner.NpcList) {
                npc.disposition = 70;
            }
            StartCoroutine(endLevelIEnum());
        }
    }

    // The delay before the start of the level is different for the first level after the main menu
    private IEnumerator startLevelIEnum(float delay = 0f) 
    {
        SetPaused(true, false);
        PlayerController.instance.ResetPlayerPosition();
        TownGenerator.instance.npcSpawner.ResetNpcs();
        isInDayTransition = true;
        timeElapsed = 0;

        HUD.instance.textDate.gameObject.SetActive(true);
        HUD.instance.textDate.text = currentSeason + " " + currentYear;
        HUD.instance.textDateCanvasGroup.alpha = 0;

        // Same effect as WaitForSeconds but doesn't need to worry about Time.timeScale
        float start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start + delay) yield return null;

        AudioManager.instance.sfxBell.PlayAsSFX();

        // Fade in text showing season and year
        while(HUD.instance.textDateCanvasGroup.alpha < 1) 
        {
            HUD.instance.textDateCanvasGroup.alpha += Time.unscaledDeltaTime;
            yield return null;
        }

        start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start + 2.0f) yield return null;

        while(HUD.instance.textDateCanvasGroup.alpha > 0) 
        {
            HUD.instance.textDateCanvasGroup.alpha -= Time.unscaledDeltaTime;
            yield return null;
        }

        start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start + 0.5f) yield return null;

        HUD.instance.textDate.gameObject.SetActive(false);
        
        SetPaused(false, false);
        HUD.instance.screenFader.FadeIn();
        AudioManager.instance.musicTown.PlayAsMusic();
        AudioManager.instance.FadeInMusic();

        isInDayTransition = false;

        if(!hasShownTutorial) 
        {
            yield return new WaitForSeconds(1.0f);

            LevelManager.instance.SetPaused(true, true);
            HUD.instance.tutorialMenu.ShowTutorial(currentSeason, currentYear);
            hasShownTutorial = true;
        }
    }

    public IEnumerator endLevelIEnum() 
    {
        isInDayTransition = true;
        HUD.instance.dialogueMenu.HideAll();
        PlayerController.instance.ExitDialogue();
        HUD.instance.screenFader.FadeOut(null, true, 1.2f);
        AudioManager.instance.FadeOutMusic();

        yield return new WaitForSeconds(1.8f);

        SetPaused(true, false);
        // Faded colour gets enabled when pausing, so disable this here so it doesn't affect the heads
        CameraController.instance.SetPostProcessingEffectEnabled<ColorGrading>(false);

        int totalVotes = TownGenerator.instance.npcSpawner.NpcList.Count - 3; // -3 for the 3 opponents
        HUD.instance.resultsMenu.ShowResults(currentSeason, currentYear, electionSeason, electionYear, candidateAppearances.ToArray(), 
            candidateNames.ToArray(), calculateVotes(), totalVotes);
    }

    public void NextLevel() 
    {
        // Increment season and year if necessary
        currentSeason = (Season) (((int) currentSeason + 1) % 4);
        if(currentSeason == Season.Spring) currentYear++;
        StartCoroutine(startLevelIEnum(1.0f));
    }

    public void SetPaused(bool paused, bool showMenu) 
    {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        CameraController.instance.SetPostProcessingEffectEnabled<ColorGrading>(paused);
        HUD.instance.pauseMenu.SetActive(paused && showMenu);
        HUD.instance.darkOverlay.SetActive(paused && showMenu);
    }

    public void TogglePaused() 
    {
        // If is in transition then disable pausing with escape
        if(isInDayTransition && !isPaused) return;
        SetPaused(!isPaused, true);
    }

    public void RemoveGold(int amount) 
    {
        goldAmount -= amount;
        if(goldAmount < 0) goldAmount = 0;
    }

    // bribeAmount = 0 (low), 1 (mid), 2 (height), returns
    public int BribeGoldAmount(int bribeAmount) 
    {
        int goldAmount = 50;
        if(bribeAmount == 1) goldAmount = 200;
        if(bribeAmount == 2) goldAmount = 500;
        return goldAmount;
    }

    public void SpendBribe(int bribeAmount) 
    {
        LevelManager.instance.RemoveGold(BribeGoldAmount(bribeAmount));
    }

    public bool CanAffordBribe(int bribeAmount) 
    {
        return GoldRemaining >= BribeGoldAmount(bribeAmount);
    }

    private int[] calculateVotes() 
    {
        int[] votes = new int[opponents.Length + 1];

        float[] opponentPopularities = new float[opponents.Length];
        float totalPopularity = 0;
        for(int i = 0; i < opponentPopularities.Length; i++) 
        {
            // Each opponent has a random popularity value between 0.4 and 0.6 which influences which one each npc will vote for
            opponentPopularities[i] = 0.4f + 0.2f * Random.value;
            totalPopularity += opponentPopularities[i];
        }

        foreach(NPC npc in TownGenerator.instance.npcSpawner.NpcList) 
        {
            if(npc.occupation != CharacterOccupation.Politician) 
            {
                int dispositionForVote = Random.Range(55, 65);
                // For for player
                if(npc.disposition >= dispositionForVote) 
                {
                    votes[0]++;
                } 
                else 
                {
                    // Otherwise vote for random opponent based on their popularity
                    float r = Random.Range(0f, totalPopularity);
                    float p = 0;
                    for(int i = 0; i < opponentPopularities.Length; i++) 
                    {
                        p += opponentPopularities[i];
                        if(r < p) 
                        {
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

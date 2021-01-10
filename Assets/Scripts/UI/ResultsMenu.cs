using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsMenu : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI textTitle, textElectionInfo;
    [SerializeField] private RectTransform[] bars;
    [SerializeField] private TextMeshProUGUI[] textVotes;
    [SerializeField] private TextMeshProUGUI[] textNames;
    [SerializeField] private TextMeshProUGUI textResult;
    [SerializeField] private Button buttonNext;
    [SerializeField] private RectTransform rectButtonNext;
    [SerializeField] private TextMeshProUGUI textButtonNext;

    [SerializeField] private GameObject headsParent;
    [SerializeField] private Transform[] headContainers;
    [SerializeField] private CharacterAppearance[] characterHeads;

    private bool[] headsLookingUp;
    private float[] headXRotations;
    private float[] barScales;

    // Store these as variables so they can be used by the continue button
    private bool isElection, victory;
    private int playerVotes;

    void Awake() {
        headsLookingUp = new bool[headContainers.Length];
        headXRotations = new float[headContainers.Length];
        barScales = new float[bars.Length];
    }

    void Update() {
        for(int i = 0; i < headContainers.Length; i++) {
            // This menu will be shown while the game is paused so use unscaledDeltaTime
            headXRotations[i] = Mathf.Lerp(headXRotations[i], headsLookingUp[i] ? -25f : 0, Time.unscaledDeltaTime * 5);
            headContainers[i].localRotation = Quaternion.Euler(headXRotations[i], headContainers[i].localEulerAngles.y, 0);

            float scaleY = Mathf.Lerp(bars[i].localScale.y, barScales[i], Time.unscaledDeltaTime * 10);
            bars[i].localScale = new Vector3(1, scaleY, 1);
        }
    }

    public void ShowResults(Season season, int year, Season electionSeason, int electionYear, CharacterAppearance[] characters, string[] names, int[] votes, int totalVotes) {
        gameObject.SetActive(true);
        headsParent.SetActive(true);

        victory = getPosition(votes) == 0;
        playerVotes = votes[0];
        textTitle.text = season + " " + year;
        isElection = season == electionSeason && year == electionYear;
        if(isElection) {
            textTitle.text += " Election";
            textButtonNext.text = "Continue >";
            textElectionInfo.text = "";
        } else {
            textTitle.text += " Opinion Poll";
            textElectionInfo.text = "In preparation for the " + electionSeason + " " + electionYear + " election";
            Season nextSeason = (Season) (((int) season + 1) % 4);
            int nextYear = year;
            if(nextSeason == Season.Spring) nextYear = year + 1;
            textButtonNext.text = "Continue to " + nextSeason + " " + nextYear + " >";
        }

        rectButtonNext.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textButtonNext.preferredWidth + 24);

        for(int i = 0; i < characters.Length; i++) {
            characterHeads[i].CopyAppearanceFromOther(characters[i]);
            characterHeads[i].ApplyAll();
            textNames[i].text = names[i];
            textVotes[i].text = 0.ToString();
        }

        StartCoroutine(showResultsIEnum(votes, totalVotes));
    }

    public void Hide() {
        gameObject.SetActive(false);
        headsParent.SetActive(false);
    }

    private IEnumerator showResultsIEnum(int[] votes, int totalVotes) {
        int[] currentVotes = new int[votes.Length];

        bool hasFinished = false;

        headsParent.SetActive(true);

        for(int i = 0; i < headContainers.Length; i++) {
            barScales[i] = 0;
            bars[i].localScale = new Vector3(1, 0, 1);
            headXRotations[i] = 0;
            // headContainers[i].localEulerAngles = new Vector3(0, headContainers[i].localEulerAngles.y, 0);
        }

        textResult.text = "";
        buttonNext.interactable = false;
        textButtonNext.color = new Color(1, 1, 1, 0.5f);

        // Same effect as WaitForSeconds but doesn't need to worry about Time.timeScale
        float start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start + 1.5f) yield return null;

        for(int i = 0; i < headsLookingUp.Length; i++) {
            headsLookingUp[i] = true;
        }

        float pitch = 1;

        while(!hasFinished) {
            hasFinished = true;
            for(int i = 0; i < currentVotes.Length; i++) {
                if(currentVotes[i] < votes[i]) {
                    currentVotes[i]++;
                    hasFinished = false;
                    barScales[i] = (float) currentVotes[i] / totalVotes;
                    textVotes[i].text = currentVotes[i].ToString();
                } else {
                    headsLookingUp[i] = false;
                }
            }

            AudioManager.instance.sfxBlip.PlayAsSFX(pitch);
            pitch += 0.03f;

            start = Time.realtimeSinceStartup;
            float delay = isElection ? 0.25f : 0.15f; // Actual election is slower and more tense
            while(Time.realtimeSinceStartup < start + delay) yield return null;
        }

        start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start + 0.6f) yield return null;

        int position = getPosition(votes);
        switch(position) {
            case 1:
                textResult.text = isElection ? "You gained the most votes!" : "You are ahead in the polls!";
                break;
            case 2:
                textResult.text = isElection ? "You gained the second most votes." : "You are second in the polls.";
                break;
            case 3:
                textResult.text = isElection ? "You gained the third most votes." : "You are third in the polls.";
                break;
            case 4:
                textResult.text = isElection ? "You gained the fewest votes." : "You are last in the polls.";
                break;
        }

        if(position == 0) {
            AudioManager.instance.sfxLeading.PlayAsSFX();
        } else {
            AudioManager.instance.sfxLosing.PlayAsSFX();
        }

        buttonNext.interactable = true;
        textButtonNext.color = Color.white;
    }

    // Returns 1 for first place, 2 for second etc
    private int getPosition(int[] votes) {
        int playerVotes = votes[0];
        int position = 1;
        for(int i = 1; i < votes.Length; i++) {
            if(votes[i] > playerVotes) {
                position++;
            }
        }
        return position;
    }

    public void Continue() {
        AudioManager.instance.PlayButtonClick();
        Hide();
        if(isElection) {
            HUD.instance.endingMenu.Show(victory, playerVotes);
        } else {
            LevelManager.instance.NextLevel();
        }
    }
}

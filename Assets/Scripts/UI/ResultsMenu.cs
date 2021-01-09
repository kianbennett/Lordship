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

    void Awake() {
        headsLookingUp = new bool[headContainers.Length];
        headXRotations = new float[headContainers.Length];
        barScales = new float[bars.Length];
    }

    void Update() {
        for(int i = 0; i < headContainers.Length; i++) {
            headXRotations[i] = Mathf.Lerp(headXRotations[i], headsLookingUp[i] ? -25f : 0, Time.deltaTime * 5);
            headContainers[i].localRotation = Quaternion.Euler(headXRotations[i], headContainers[i].localEulerAngles.y, 0);

            float scaleY = Mathf.Lerp(bars[i].localScale.y, barScales[i], Time.deltaTime * 10);
            bars[i].localScale = new Vector3(1, scaleY, 1);
        }
    }

    public void ShowResults(Season season, int year, Season electionSeason, int electionYear, CharacterAppearance[] characters, string[] names, int[] votes, int totalVotes) {
        gameObject.SetActive(true);
        headsParent.SetActive(true);

        textTitle.text = season + " " + year;
        bool actualElection = season == electionSeason && year == electionYear;
        if(actualElection) {
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
        StopAllCoroutines();
        gameObject.SetActive(false);
        headsParent.SetActive(false);
    }

    private IEnumerator showResultsIEnum(int[] votes, int totalVotes) {
        int[] currentVotes = new int[votes.Length];

        bool hasFinished = false;
        WaitForSeconds delayBefore = new WaitForSeconds(0.1f);

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

        yield return new WaitForSeconds(1.0f);

        for(int i = 0; i < headsLookingUp.Length; i++) {
            headsLookingUp[i] = true;
        }

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

            yield return delayBefore;
        }

        yield return new WaitForSeconds(0.2f);
        
        int position = getPosition(votes);
        switch(position) {
            case 1:
                textResult.text = "You are in the lead!";
                break;
            case 2:
                textResult.text = "You are in 2nd place.";
                break;
            case 3:
                textResult.text = "You are in 3nd place.";
                break;
            case 4:
                textResult.text = "You are in last place!";
                break;
        }

        yield return new WaitForSeconds(0.8f);

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

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueMenu : MonoBehaviour {

    [SerializeField] private TextDisplay npcSpeechText;
    [SerializeField] private TextMeshProUGUI textNpcName;
    [SerializeField] private TextMeshProUGUI textNpcAge;
    [SerializeField] private TextMeshProUGUI textNpcOccupation;
    [SerializeField] private TextMeshProUGUI textNpcWealth;
    [SerializeField] private Transform dispositionBar;
    [SerializeField] private RectTransform choicesPanel;
    [SerializeField] private Animator npcSpeechAnim, npcInfoAnim;

    [SerializeField] private Color flatterColour, threatenColour, bribeColour, rumoursColour, goodbyeColour;

    [SerializeField] private DialogueChoiceButton choiceButtonPrefab;

    public TextDisplay NpcSpeechText { get { return npcSpeechText; }}

    private List<DialogueChoiceButton> choiceButtons;

    private bool isActive;
    private bool isChoicesActive;
    private float choicePanelHeight;

    void Awake() {
        ShowChoicesPanel(
            new ChoiceData("Flatter...", ChoiceType.Flatter),
            new ChoiceData("Threaten...\nasd", ChoiceType.Threaten),
            new ChoiceData("Bribe...", ChoiceType.Bribe),
            new ChoiceData("Rumours\nasd", ChoiceType.Rumours),
            new ChoiceData("Goodbye\nasd", ChoiceType.Goodbye)
        );

        SetActive(true);
    }

    void Update() {
        // Update dialogue option panel position
        float velocity = 0;
        float y = Mathf.SmoothDamp(choicesPanel.anchoredPosition.y, isActive && isChoicesActive ? choicePanelHeight : -15, ref velocity, 0.05f, 3000f);
        choicesPanel.anchoredPosition = new Vector2(choicesPanel.anchoredPosition.x, y);

        if(Input.GetKeyDown(KeyCode.Escape)) {
            SetActive(!isActive);
        }
    }

    public void SetActive(bool active) {
        isActive = active;
        if(active) gameObject.SetActive(true);
        npcSpeechAnim.SetTrigger(active ? "Appear" : "Hide");
        npcInfoAnim.SetTrigger(active ? "Appear" : "Hide");
    }

    public void ShowChoicesPanel(params ChoiceData[] choices) {
        // Delete existing option buttons
        if(choiceButtons != null) {
            foreach(DialogueChoiceButton button in choiceButtons) Destroy(button.gameObject);
        }
        choiceButtons = new List<DialogueChoiceButton>();

        float buttonY = -18;
        for(int i = 0; i < choices.Length; i++) {
            DialogueChoiceButton button = Instantiate(choiceButtonPrefab, Vector3.zero, Quaternion.identity, choicesPanel);
            button.SetValues(i, choices[i]);
            button.transform.localPosition = Vector3.up * buttonY;
            buttonY -= button.GetHeight() + 10;
        }

        choicesPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -buttonY + 50);
        choicePanelHeight = -buttonY + 20;

        isChoicesActive = true;
    }

    public void HideChoicesPanel() {
        isChoicesActive = false;
    }

    public void ShowNpcSpeech(string text) {
        npcSpeechAnim.SetTrigger("Appear");
        npcSpeechText.Display(text);
    }

    public void HideNpcSpeech() {
        npcSpeechAnim.SetTrigger("Hide");
    }

    public void ShowNpcInfo(NPC npc) {
        npcInfoAnim.SetTrigger("Appear");
        textNpcName.text = npc.charName;
        textNpcAge.text = npc.GetAgeString();
        textNpcOccupation.text = npc.GetOccupationString();
        textNpcWealth.text = npc.GetWealthString();
        dispositionBar.localScale = new Vector3(npc.disposition / 100f, 1, 1);
    }

    public void HideNpcInfo() {
        npcInfoAnim.SetTrigger("Hide");
    }

    public void HideAll() {
        HideNpcSpeech();
        HideNpcInfo();
        HideChoicesPanel();
    }

    public Color GetDialogueChoiceColour(ChoiceType type) {
        switch(type) {
            case ChoiceType.Flatter:
                return flatterColour;
            case ChoiceType.Threaten:
                return threatenColour;
            case ChoiceType.Bribe:
                return bribeColour;
            case ChoiceType.Rumours:
                return rumoursColour;
            case ChoiceType.Goodbye:
                return goodbyeColour;
        }
        return Color.white;
    }
}

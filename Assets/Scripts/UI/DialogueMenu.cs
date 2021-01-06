using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueMenu : MonoBehaviour {

    [SerializeField] private TextDisplay npcSpeechText;
    [SerializeField] private TextMeshProUGUI textNpcName;
    [SerializeField] private TextMeshProUGUI textNpcAge;
    [SerializeField] private TextMeshProUGUI textNpcOccupation;
    [SerializeField] private TextMeshProUGUI textNpcWealth;
    [SerializeField] private Image dispositionBar;
    [SerializeField] private RectTransform choicesPanel;
    [SerializeField] private Animator npcSpeechAnim, npcInfoAnim;

    [SerializeField] private Color flatterColour, threatenColour, bribeColour, rumoursColour, goodbyeColour;
    [SerializeField] private Color dispositionColourPositive, dispositionColourNeutral, dispositionColourNegative;

    [SerializeField] private DialogueChoiceButton choiceButtonPrefab;

    public TextDisplay NpcSpeechText { get { return npcSpeechText; }}

    private List<DialogueChoiceButton> choiceButtons;

    private bool isActive;
    private bool isChoicesActive;
    private float choicePanelHeight;
    private int npcDisposition;

    void Awake() {
    }

    void Update() {
        // Update dialogue option panel position
        float velocity = 0;
        float targetY = isActive && isChoicesActive ? choicePanelHeight : -15;
        float y = Mathf.SmoothDamp(choicesPanel.anchoredPosition.y, targetY, ref velocity, 0.05f, 5000f);
        choicesPanel.anchoredPosition = new Vector2(choicesPanel.anchoredPosition.x, y);

        float scaleX = Mathf.Lerp(dispositionBar.transform.localScale.x, npcDisposition / 100f, Time.deltaTime * 10);
        dispositionBar.transform.localScale = new Vector3(scaleX, 1, 1);
        if(scaleX < 0.5f) {
            dispositionBar.color = Color.Lerp(dispositionColourNegative, dispositionColourNeutral, scaleX / 0.5f);
        } else {
            dispositionBar.color = Color.Lerp(dispositionColourNeutral, dispositionColourPositive, (scaleX - 0.5f) / 0.5f);
        }
    }

    public void SetActive(bool active) {
        isActive = active;
        if(active) gameObject.SetActive(true);
        HideAllImmediate();
        dispositionBar.transform.localScale = new Vector3(0, 1, 1);
        // npcSpeechAnim.SetTrigger(active ? "Appear" : "Hide");
        // npcInfoAnim.SetTrigger(active ? "Appear" : "Hide");
    }

    public void ShowChoicesPanel(params ChoiceData[] choices) {
        // Delete existing option buttons
        if(choiceButtons != null) {
            foreach(DialogueChoiceButton button in choiceButtons) Destroy(button.gameObject);
        }
        choiceButtons = new List<DialogueChoiceButton>();

        float buttonY = -15;
        for(int i = 0; i < choices.Length; i++) {
            DialogueChoiceButton button = Instantiate(choiceButtonPrefab, Vector3.zero, Quaternion.identity);
            button.transform.SetParent(choicesPanel, false);
            button.SetValues(i, choices[i].FormattedDisplayText(PlayerController.instance.npcSpeaking), choices[i].Type);
            button.transform.localPosition = Vector3.up * buttonY;
            // button.GetComponent<RectTransform>().localPosition = Vector2.zero;
            // Debug.Log(choicesPanel.anchoredPosition);
            buttonY -= button.GetHeight() + 10;
            choiceButtons.Add(button);
        }

        choicesPanel.gameObject.SetActive(true);
        choicesPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -buttonY + 50);
        choicePanelHeight = -buttonY + 20;

        isChoicesActive = true;
    }

    public void HideChoicesPanel() {
        isChoicesActive = false;
    }

    public void ShowNpcSpeech(string text, bool showAnimation = true) {
        npcSpeechAnim.gameObject.SetActive(true);
        npcSpeechAnim.SetTrigger("Appear");
        npcSpeechText.Display(text);
    }

    public void HideNpcSpeech() {
        npcSpeechAnim.SetTrigger("Hide");
    }

    public void ShowNpcInfo(NPC npc) {
        npcInfoAnim.gameObject.SetActive(true);
        npcInfoAnim.SetTrigger("Appear");
        textNpcName.text = npc.DisplayName;
        textNpcAge.text = npc.GetAgeString();
        textNpcOccupation.text = npc.GetOccupationString();
        textNpcWealth.text = npc.GetWealthString();
        UpdateDispositionBar(npc.disposition);
    }

    public void HideNpcInfo() {
        npcInfoAnim.SetTrigger("Hide");
    }

    public void HideAll() {
        HideNpcSpeech();
        HideNpcInfo();
        HideChoicesPanel();
    }

    public void HideAllImmediate() {
        npcSpeechAnim.gameObject.SetActive(false);
        npcInfoAnim.gameObject.SetActive(false);
        choicesPanel.gameObject.SetActive(false);
    }

    public Color GetDialogueChoiceColour(DialogueType type) {
        switch(type) {
            case DialogueType.Flatter:
                return flatterColour;
            case DialogueType.Threaten:
                return threatenColour;
            case DialogueType.Bribe:
                return bribeColour;
            case DialogueType.Rumours:
                return rumoursColour;
            case DialogueType.Goodbye:
                return goodbyeColour;
        }
        return Color.white;
    }

    public void UpdateDispositionBar(int disposition) {
        npcDisposition = disposition;
    }
}

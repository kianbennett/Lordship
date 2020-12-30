using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoiceButton : MonoBehaviour {

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;

    private int id;

    public void SetValues(int id, ChoiceData choice) {
        this.id = id;
        text.text = (id + 1) + ") " + choice.DisplayText;
        image.color = HUD.instance.dialogueMenu.GetDialogueChoiceColour(choice.Type);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.preferredHeight + 20);
    }

    public float GetHeight() {
        return rectTransform.sizeDelta.y;
    }

    public void Choose() {
        // Do something with the ID
    }
}

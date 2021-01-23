using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoiceButton : MonoBehaviour 
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private Outline outline;
    [SerializeField] private Color outlineColourSpecial;

    private int index;

    public float Height { get { return rectTransform.sizeDelta.y; } }

    public void SetValues(int index, string displayText, DialogueType type) 
    {
        this.index = index;
        text.text = (index + 1) + ") " + displayText;
        image.color = HUD.instance.dialogueMenu.GetDialogueChoiceColour(type);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.preferredHeight + 20);
    }

    public void Choose() 
    {
        AudioManager.instance.PlayButtonClick();
        DialogueSystem.instance.PickChoice(index);
    }

    // For rumour complete choice
    public void SetSpecial() 
    {
        outline.effectColor = outlineColourSpecial;
    }

    public void SetInteractible(bool interactable) 
    {
        button.interactable = interactable;
        text.color = interactable ? Color.white : new Color(1, 1, 1, 0.5f);
    }
}

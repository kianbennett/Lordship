using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType {
    Greeting, Flatter, Threaten, Bribe, Rumours, Goodbye
}

public enum ChoiceTextType {
    Predefined, RandomFlatter, RandomThreaten
}

[Serializable]
public class ChoiceData
{
    [SerializeField] private string _text;
    // [SerializeField] private TextList _textList;
    [SerializeField] private ChoiceTextType _textType;
    [SerializeField] private int _beatId;
    [SerializeField] private DialogueType _type;
    [SerializeField] private bool _correctChoice;

    // If the type is set to random then pick a random value from _randomTextList, otherwise use _text
    public string DisplayText { 
        get { return _text; } 
        set { _text = value; }
    }
    public int NextID { get { return _beatId; } }
    public DialogueType Type { get { return _type; }}
    public ChoiceTextType TextType { get { return _textType; } }
    // Used when needing to pick the correct choice
    public bool IsCorrectChoice { 
        get { return _correctChoice; } 
        set { _correctChoice = value; } 
    }

    // Replace "{name}"s with the NPCs first name
    public string FormattedDisplayText(NPC npc) {
        return DisplayText.Replace("{name}", npc.charName.Item1);
    }

    // public ChoiceData(string text, DialogueType type) {
    //     _text = text;
    //     _type = type;
    // }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType {
    Greeting, Flatter, Threaten, Bribe, Rumours, Goodbye
}

public enum ChoiceTextType {
    Predefined, RandomFlatter, RandomThreaten, RandomBribe
}

[Serializable]
public class ChoiceData
{
    [SerializeField] private string _text;
    [SerializeField] private TextList _textList;
    [SerializeField] private ChoiceTextType _textType;
    [SerializeField] private int _beatId;
    [SerializeField] private DialogueType _type;

    // If the type is set to random then pick a random value from _randomTextList, otherwise use _text
    public string DisplayText { get { 
        if(_textType == ChoiceTextType.Predefined) return _text;
            else return _textList.RandomText();
    } }
    public int NextID { get { return _beatId; } }
    public DialogueType Type { get { return _type; }}

    public ChoiceData(string text, DialogueType type) {
        _text = text;
        _type = type;
    }
}
using System;
using UnityEngine;

public enum ChoiceType {
    Flatter, Threaten, Bribe, Rumours, Goodbye
}

[Serializable]
public class ChoiceData
{
    [SerializeField] private string _text;
    [SerializeField] private int _beatId;
    [SerializeField] private ChoiceType _type; // Added

    public string DisplayText { get { return _text; } }
    public int NextID { get { return _beatId; } }
    public ChoiceType Type { get { return _type; }}

    public ChoiceData(string text, ChoiceType type) {
        _text = text;
        _type = type;
    }
}

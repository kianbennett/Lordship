using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChoiceType {
    Flatter, Threaten, Bribe, Rumours, Goodbye
}

[Serializable]
public class ChoiceData
{
    [SerializeField] private string _text;
    [SerializeField] private List<string> _randomTextList;
    [SerializeField] private DefineType _textType;
    [SerializeField] private int _beatId;
    [SerializeField] private ChoiceType _type;

    // If the type is set to random then pick a random value from _randomTextList, otherwise use _text
    public string DisplayText { get { 
        if(_textType == DefineType.Predefined) return _text;
            else return _randomTextList[UnityEngine.Random.Range(0, _randomTextList.Count)];
    } }
    public int NextID { get { return _beatId; } }
    public ChoiceType Type { get { return _type; }}

    public ChoiceData(string text, ChoiceType type) {
        _text = text;
        _type = type;
    }
}

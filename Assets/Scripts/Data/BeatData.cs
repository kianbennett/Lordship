using System;
using System.Collections.Generic;
using UnityEngine;

public enum DefineType { Predefined, Random }
public enum BeatCategory { None, Age, Wealth, Occupation }

[Serializable]
public class BeatData
{
    // Predefined choices or randomised
    [SerializeField] private DefineType _choicesType;
    [SerializeField] private List<ChoiceData> _choices;
    
    // If choices are randomised
    [SerializeField] private BeatCategory _beatCategory;
    // These are only used depending on what _beatCategory is set to
    [SerializeField] private CharacterAge _ageType;
    [SerializeField] private CharacterWealth _wealthType;
    [SerializeField] private CharacterOccupation _occupationType;

    // Predefined text or randomised
    [SerializeField] private DefineType _textType;
    [SerializeField] private string _text;
    [SerializeField] private TextList _randomTextList;

    [SerializeField] private int _id;

    public List<ChoiceData> Decision { get { 
        return _choices;
    } }
    // If the type is set to random then pick a random value from _randomTextList, otherwise use _text
    public string DisplayText { get { 
        if(_textType == DefineType.Predefined) return _text;
            else return _randomTextList.RandomText();
    } }
    public int ID { get { return _id; } }

    public BeatData(int id, string text, List<ChoiceData> choices) {
        _id = id;
        _text = text;
        _choices = choices;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

// public enum ChoiceDefineType { Predefined, Random }

[Serializable]
public class BeatData
{
    [SerializeField] private string _name;
    [SerializeField] private int _id;
    
    // Predefined choices or randomised
    [SerializeField] private bool _copyChoicesFromBeat; // If false then copy choices from another beat
    [SerializeField] private int _beatIdToCopyFrom;
    // [SerializeField] private ChoiceDefineType _choicesType;
    [SerializeField] private List<ChoiceData> _choices;
    
    // The type of beat
    [SerializeField] private DialogueType _beatType;
    // These are only used depending on what _beatCategory is set to
    // [SerializeField] private CharacterAge _ageType;
    // [SerializeField] private CharacterWealth _wealthType;
    // [SerializeField] private CharacterOccupation _occupationType;

    // [SerializeField] private BeatTextType _textType;
    // [SerializeField] private string _text;
    [SerializeField] private TextList _textListPositive, _textListNeutral, _textListNegative;

    public List<ChoiceData> Decision { get { 
        return _choices;
    } }
    // public string DisplayText { get { return _text; } }
    public int ID { get { return _id; } }
    public string Name { get { return _name; } }

    // public BeatData(int id, string text, List<ChoiceData> choices) {
    //     _id = id;
    //     // _text = text;
    //     _choices = choices;
    // }

    public string GetDisplayText(int disposition) {
        if(disposition < 35) return _textListNegative.RandomText();
        else if(disposition > 70) return _textListPositive.RandomText();
        else return _textListNeutral.RandomText();
    }
}
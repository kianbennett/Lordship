using System;
using System.Collections.Generic;
using UnityEngine;

// public enum ChoiceDefineType { Predefined, Random }
public enum SpeechType { Greeting, Listening, FlatterResponse, ThreatenResponse, BribeResponse, Rumour }

[Serializable]
public class BeatData
{
    [SerializeField] private string _name;
    [SerializeField] private int _id;
    
    // Predefined choices or randomised
    [SerializeField] private bool _copyChoicesFromBeat; // To reuse choices from another beat
    [SerializeField] private int _beatIdToCopyFrom;
    // [SerializeField] private ChoiceDefineType _choicesType;
    [SerializeField] private List<ChoiceData> _choices;
    // [SerializeField] private ChoiceTextType _choiceTextType;
    
    // The type of beat
    [SerializeField] private DialogueType _beatType;
    // These are only used depending on what _beatCategory is set to
    // [SerializeField] private CharacterAge _ageType;
    // [SerializeField] private CharacterWealth _wealthType;
    // [SerializeField] private CharacterOccupation _occupationType;

    // [SerializeField] private BeatTextType _textType;
    // [SerializeField] private string _text;
    // [SerializeField] private TextList _textListPositive, _textListNeutral, _textListNegative;
    // Speech type determines which TextLists to pull the text from
    [SerializeField] private SpeechType _displayTextType;

    public List<ChoiceData> Decision { 
        get { 
            return _choices;
        } 
        // Needs set if copying choices from other beat
        set {
            _choices = value;
        }
    }
    public SpeechType DisplayTextType { get { return _displayTextType; } }
    // public string DisplayText { get { return _text; } }
    public int ID { get { return _id; } }
    public string Name { get { return _name; } }
    public DialogueType Type { get { return _beatType; } }

    public bool CopyChoicesFromBeat { get { return _copyChoicesFromBeat; } }
    public int BeatIdToCopyFrom { get { return _beatIdToCopyFrom; } }

    public string GetDisplayText(DispositionType disposition) {
        DialogueTextData data = DialogueTextData.LoadData();

        switch(_displayTextType) {
            case SpeechType.Greeting:
                return data.GetRandomGreeting(disposition);
            case SpeechType.Listening:
                return data.GetRandomListening(disposition);
            case SpeechType.FlatterResponse:
                return data.GetRandomFlatteryResponse(disposition == DispositionType.Positive);
            case SpeechType.ThreatenResponse:
                return data.GetRandomThreatenResponse(disposition == DispositionType.Positive);
            case SpeechType.BribeResponse:
                return data.GetRandomBribeResponse(disposition == DispositionType.Positive);
        }
        return "";
    }

    // Success is used for responses to certain 
    public string GetDisplayText(bool success) {
        return GetDisplayText(success ? DispositionType.Positive : DispositionType.Negative);
    }
}
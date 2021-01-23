using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpeechType { Greeting, Listening, FlatterResponse, ThreatenResponse, BribeResponse, RumourStart, RumourEnd }

[Serializable]
public class BeatData
{
    [SerializeField] private string _name;
    [SerializeField] private int _id;
    
    // Predefined choices or randomised
    [SerializeField] private bool _copyChoicesFromBeat; // To reuse choices from another beat
    [SerializeField] private int _beatIdToCopyFrom;
    [SerializeField] private List<ChoiceData> _choices;
    
    // The type of beat
    [SerializeField] private DialogueType _beatType;

    // [SerializeField] private string _text;

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

    public string GetDisplayText(DialogueTextData textData, DispositionType disposition) {
        switch(_displayTextType) {
            case SpeechType.Greeting:
                return textData.GetRandomGreeting(disposition);
            case SpeechType.Listening:
                return textData.GetRandomListening(disposition);
            case SpeechType.FlatterResponse:
                return textData.GetRandomFlatteryResponse(disposition == DispositionType.Positive);
            case SpeechType.ThreatenResponse:
                return textData.GetRandomThreatenResponse(disposition == DispositionType.Positive);
            case SpeechType.BribeResponse:
                return textData.GetRandomBribeResponse(disposition == DispositionType.Positive);
        }
        return "";
    }

    // Success is used for responses to certain 
    public string GetDisplayText(DialogueTextData textData, bool success) {
        return GetDisplayText(textData, success ? DispositionType.Positive : DispositionType.Negative);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BeatData
{
    [SerializeField] private List<ChoiceData> _choices;
    [SerializeField] private string _text;
    [SerializeField] private int _id;

    public List<ChoiceData> Decision { get { return _choices; } }
    public string DisplayText { get { return _text; } }
    public int ID { get { return _id; } }
}

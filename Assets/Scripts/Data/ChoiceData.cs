using System;
using UnityEngine;

[Serializable]
public class ChoiceData
{
    [SerializeField] private string _text;
    [SerializeField] private int _beatId;

    public string DisplayText { get { return _text; } }
    public int NextID { get { return _beatId; } }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A SO that contains an array of strings, used when for random selection from a list of texts
// Uses SO so multiple beats/choices can pull from the same array of strings
// [CreateAssetMenu(fileName = "TextList", menuName = "Dialogue/Text List")]
[System.Serializable]
public class TextList {
    [SerializeField] private string[] _textList;

    // Always returns a string even if if string array is empty
    public string RandomText { get { 
        if(_textList.Length > 0) return _textList[Random.Range(0, _textList.Length)]; 
            else return "";
    } }
}

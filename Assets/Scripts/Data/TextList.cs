using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A SO that contains an array of strings, used when for random selection from a list of texts
// Uses SO so multiple beats/choices can pull from the same array of strings
[CreateAssetMenu(fileName = "TextList", menuName = "Dialogue/Text List")]
public class TextList : ScriptableObject {
    [TextArea(2, 10)]
    public string[] textList;

    public string RandomText() {
        return textList[Random.Range(0, textList.Length)];
    }
}

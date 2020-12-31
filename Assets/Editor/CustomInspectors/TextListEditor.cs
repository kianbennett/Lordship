using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TextList))]
public class TextListEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
}
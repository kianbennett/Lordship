using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TownGenerator))]
public class TownGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        TownGenerator generator = (TownGenerator) target;
        if (GUILayout.Button("Generate")) {
            generator.Generate();
            EditorUtility.SetDirty(target);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(CharacterActionHandler))]
public class CharacterActionHandlerEditor : Editor {

    public override void OnInspectorGUI() {
        CharacterActionHandler actionHandler = (CharacterActionHandler) target;
    
        if(EditorApplication.isPlaying) {
            CharacterAction currentAction = null;
            if (actionHandler.Actions.Count > 0) currentAction = actionHandler.Actions[0];
            EditorGUILayout.LabelField("Current action: " + currentAction);
            EditorGUILayout.Space();
        }

        DrawDefaultInspector();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
*   Custom inspector for SimpleObjectMovement that only shows the variables relevant to the selected movement type
*/

[CanEditMultipleObjects]
[CustomEditor(typeof(SimpleObjectMovement))]
public class SimpleObjectMovementEditor : Editor {

    public override void OnInspectorGUI() {
        SimpleObjectMovement movement = (SimpleObjectMovement) target;

        DrawDefaultInspector();

        if (movement.type == SimpleObjectMovement.MovementType.Rotate) {
            movement.rotationAxis = EditorGUILayout.Vector3Field("Rotation Axis", movement.rotationAxis);
            movement.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", movement.rotationSpeed);
        }

        if (movement.type == SimpleObjectMovement.MovementType.Bob) {
            movement.bobAxis = EditorGUILayout.Vector3Field("Bob Axis", movement.bobAxis);
            movement.bobDistance = EditorGUILayout.FloatField("Bob Distance", movement.bobDistance);
            movement.bobSpeed = EditorGUILayout.FloatField("Bob Speed", movement.bobSpeed);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
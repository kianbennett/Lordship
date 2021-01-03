using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RumourEditor : EditorWindow {

    private Vector2 _scroll = new Vector2();

    [MenuItem("SFAS/Show Rumour Editor")]
    public static void ShowRumourEditor() {
        GetWindow(typeof(RumourEditor));
    }

    void OnGUI() {
        RumourData data = RumourData.LoadData();
        SerializedObject dataObj = new SerializedObject(data);

        SerializedProperty startPoints = dataObj.FindProperty("_startPoints");
        SerializedProperty midPoints = dataObj.FindProperty("_midPoints");
        SerializedProperty endPoints = dataObj.FindProperty("_endPoints");

        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        for (int i = 0; i < startPoints.arraySize; i++) {
            SerializedProperty start = startPoints.GetArrayElementAtIndex(i);
            SerializedProperty mid = midPoints.GetArrayElementAtIndex(i);
            SerializedProperty end = endPoints.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rumour " + i);
            if(GUILayout.Button("Delete", GUILayout.Width(80))) {
                deleteRumour(startPoints, midPoints, endPoints, i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            float textAreaWidth = (position.width - 130) / 3f;
            float textAreaHeight = 60;

            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Start:", GUILayout.Width(35));
            start.stringValue = EditorGUILayout.TextArea(start.stringValue, GUILayout.Width(textAreaWidth), GUILayout.Height(textAreaHeight));
            GUILayout.Space(8);

            EditorGUILayout.LabelField("Mid:", GUILayout.Width(28));
            mid.stringValue = EditorGUILayout.TextArea(mid.stringValue, GUILayout.Width(textAreaWidth), GUILayout.Height(textAreaHeight));
            GUILayout.Space(8);

            EditorGUILayout.LabelField("End:", GUILayout.Width(28));
            end.stringValue = EditorGUILayout.TextArea(end.stringValue, GUILayout.Width(textAreaWidth), GUILayout.Height(textAreaHeight));

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(12);
        }
        
        if (GUILayout.Button("Add Rumour")) {
            addRumour(startPoints, midPoints, endPoints);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        dataObj.ApplyModifiedProperties();
    }

    private void addRumour(SerializedProperty startPoints, SerializedProperty midPoints, SerializedProperty endPoints) {
        int index = startPoints.arraySize;
        startPoints.arraySize++;
        midPoints.arraySize++;
        endPoints.arraySize++;

        startPoints.GetArrayElementAtIndex(index).stringValue = "";
        midPoints.GetArrayElementAtIndex(index).stringValue = "";
        endPoints.GetArrayElementAtIndex(index).stringValue = "";
    }

    private void deleteRumour(SerializedProperty startPoints, SerializedProperty midPoints, SerializedProperty endPoints, int index) {
        startPoints.DeleteArrayElementAtIndex(index);
        midPoints.DeleteArrayElementAtIndex(index);
        endPoints.DeleteArrayElementAtIndex(index);
    }
}
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

/*
*   Custom inspector to draw the array of colours as a Reorderable List
*   From https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
*/

[CanEditMultipleObjects]
[CustomEditor(typeof(ColourPalette))]
public class ColourPaletteEditor : Editor {

    private ReorderableList list;

    void OnEnable() {
        float colorWidth = 80, gap = 5;

        list = new ReorderableList(serializedObject, serializedObject.FindProperty("colours"), true, true, true, true) {
            // Draw Header
            drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Colours");
            },

            // Draw list items, raw the name and colour next to eachother on the same line
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - gap - colorWidth - gap, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("name"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + rect.width - colorWidth - gap, rect.y, colorWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("colour"), GUIContent.none);
            },

            // Add to the list
            onAddCallback = (ReorderableList l) => {
                int index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("name").stringValue = "";
                element.FindPropertyRelative("colour").colorValue = Color.white;
            },
        };
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        serializedObject.Update();
        EditorGUILayout.Space();
        list.DoLayoutList();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }
}
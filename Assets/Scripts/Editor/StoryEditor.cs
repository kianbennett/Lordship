using UnityEngine;
using UnityEditor;

public class StoryEditor : EditorWindow
{
    private enum View { List, Beat }

    private Vector2 _scroll = new Vector2();
    private int _currentIndex = -1;
    private View _view;

    [MenuItem("SFAS/Show Story Editor")]
    public static void ShowStoryEditor()
    {
        GetWindow(typeof(StoryEditor));
    }

    void OnGUI()
    {
        StoryData data = StoryData.LoadData();
        SerializedObject dataObj = new SerializedObject(data);
        SerializedProperty beatList = dataObj.FindProperty("_beats");

        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_view == View.Beat && _currentIndex != -1)
        {
            OnGUI_BeatView(beatList, _currentIndex);
        }
        else
        {
            OnGUI_ListView(beatList);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        dataObj.ApplyModifiedProperties();
    }

    private void OnGUI_ListView(SerializedProperty beatList)
    {
        EditorGUILayout.BeginVertical();

        if (beatList.arraySize == 0)
        {
            AddBeat(beatList, 1, "First Story Beat");
        }

        for (int count = 0; count < beatList.arraySize; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
            SerializedProperty text = arrayElement.FindPropertyRelative("_text");
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(id.intValue.ToString());

            if (GUILayout.Button("Edit"))
            {
                _view = View.Beat;
                _currentIndex = count;
                break;
            }

            if (GUILayout.Button("Delete"))
            {
                beatList.DeleteArrayElementAtIndex(count);
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(text);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void OnGUI_BeatView(SerializedProperty beatList, int index)
    {
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Beat ID: " + id.intValue.ToString());
        text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(200));

        OnGUI_BeatViewDecision(choiceList, beatList);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Return to Beat List", GUILayout.Height(50)))
        {
            _view = View.List;
            _currentIndex = -1;
        }
    }

    private void OnGUI_BeatViewDecision(SerializedProperty choiceList, SerializedProperty beatList)
    {
        EditorGUILayout.BeginHorizontal();

        for (int count = 0; count < choiceList.arraySize; ++count)
        {
            OnGUI_BeatViewChoice(choiceList, count, beatList);
        }

        if (GUILayout.Button((choiceList.arraySize == 0 ? "Add Choice" : "Add Another Choice"), GUILayout.Height(100)))
        {
            int newBeatId = FindUniqueId(beatList);
            AddBeat(beatList, newBeatId);
            AddChoice(choiceList, newBeatId);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI_BeatViewChoice(SerializedProperty choiceList, int index, SerializedProperty beatList)
    {
        SerializedProperty arrayElement = choiceList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty beatId = arrayElement.FindPropertyRelative("_beatId");

        EditorGUILayout.BeginVertical();

        text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(50));
        EditorGUILayout.LabelField("Leads to Beat ID: " + beatId.intValue.ToString());

        if (GUILayout.Button("Go to Beat"))
        {
            _currentIndex = FindIndexOfBeatId(beatList, beatId.intValue);
            GUI.FocusControl(null);
            Repaint();
        }

        EditorGUILayout.EndVertical();
    }

    private int FindUniqueId(SerializedProperty beatList)
    {
        int result = 1;

        while (IsIdInList(beatList, result))
        {
            ++result; 
        }

        return result;
    }

    private bool IsIdInList(SerializedProperty beatList, int beatId)
    {
        bool result = false;

        for (int count = 0; count < beatList.arraySize && !result; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");
            result = id.intValue == beatId;
        }

        return result;
    }

    private int FindIndexOfBeatId(SerializedProperty beatList, int beatId)
    {
        int result = -1;

        for (int count = 0; count < beatList.arraySize; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");
            if (id.intValue == beatId)
            {
                result = count;
                break;
            }
        }

        return result;
    }

    private void AddBeat(SerializedProperty beatList, int beatId, string initialText = "New Story Beat")
    {
        int index = beatList.arraySize;
        beatList.arraySize += 1;
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");

        text.stringValue = initialText;
        id.intValue = beatId;
    }

    private void AddChoice(SerializedProperty choiceList, int beatId, string initialText = "New Beat Choice")
    {
        int index = choiceList.arraySize;
        choiceList.arraySize += 1;
        SerializedProperty arrayElement = choiceList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty nextId = arrayElement.FindPropertyRelative("_beatId");

        text.stringValue = initialText;
        nextId.intValue = beatId;
    }
}

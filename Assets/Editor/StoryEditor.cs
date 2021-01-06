using System.Collections.Generic;
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

    // Shows list of beats
    private void OnGUI_ListView(SerializedProperty beatList)
    {
        EditorGUILayout.BeginVertical();

        if (beatList.arraySize == 0)
        {
            AddBeat(beatList, 1, "First Dialogue Beat");
        }

        for (int count = 0; count < beatList.arraySize; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
            SerializedProperty name = arrayElement.FindPropertyRelative("_name");
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID: " + id.intValue, GUILayout.Width(80));

            EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
            name.stringValue = EditorGUILayout.TextField(name.stringValue);

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

            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.PropertyField(name);
            // EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);
        if(GUILayout.Button("Add new beat")) {
            int newBeatId = FindUniqueId(beatList);
            AddBeat(beatList, newBeatId);
        }

        EditorGUILayout.EndVertical();
    }

    // Shows details of individual beat
    private void OnGUI_BeatView(SerializedProperty beatList, int index)
    {
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
        // SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");
        SerializedProperty name = arrayElement.FindPropertyRelative("_name");
        SerializedProperty type = arrayElement.FindPropertyRelative("_beatType");
        SerializedProperty displayTextType = arrayElement.FindPropertyRelative("_displayTextType");

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        // EditorGUIUtility.labelWidth = 90;
        EditorGUILayout.LabelField("Beat ID: " + id.intValue.ToString(), GUILayout.Width(110));
        EditorGUILayout.LabelField(name.stringValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Beat Type", GUILayout.Width(110));
        EditorGUILayout.PropertyField(type, GUIContent.none, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Display Text Type", GUILayout.Width(110));
        EditorGUILayout.PropertyField(displayTextType, GUIContent.none, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        // EditorGUILayout.BeginHorizontal();
        // if((DialogueType) type.enumValueIndex != DialogueType.Rumours) {
        //     SerializedProperty textListPositive = arrayElement.FindPropertyRelative("_textListPositive");
        //     SerializedProperty textListNeutral = arrayElement.FindPropertyRelative("_textListNeutral");
        //     SerializedProperty textListNegative = arrayElement.FindPropertyRelative("_textListNegative");

        //     EditorGUIUtility.labelWidth = 102;
        //     EditorGUILayout.PropertyField(textListPositive, new GUIContent("Speech Positive"), GUILayout.Width(260));
        //     GUILayout.Space(20);
        //     EditorGUILayout.PropertyField(textListNeutral, new GUIContent("Speech Neutral"), GUILayout.Width(260));
        //     GUILayout.Space(20);
        //     EditorGUILayout.PropertyField(textListNegative, new GUIContent("Speech Negative"), GUILayout.Width(260));
        // }
        // EditorGUILayout.EndHorizontal();

        // text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(200));

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Choices", GUILayout.Width(90));
        EditorGUIUtility.labelWidth = 150;
        SerializedProperty copyChoices = arrayElement.FindPropertyRelative("_copyChoicesFromBeat");
        copyChoices.boolValue = EditorGUILayout.Toggle("Copy Choices from Beat", copyChoices.boolValue, GUILayout.Width(200));
        if(copyChoices.boolValue) {
            SerializedProperty beatIdToCopyFrom = arrayElement.FindPropertyRelative("_beatIdToCopyFrom");
            int beatIndex = FindIndexOfBeatId(beatList, beatIdToCopyFrom.intValue);
            beatIndex = EditorGUILayout.Popup(beatIndex, GetBeatNames(beatList), GUILayout.Width(100));
            beatIdToCopyFrom.intValue = beatList.GetArrayElementAtIndex(beatIndex).FindPropertyRelative("_id").intValue;
        }
        EditorGUILayout.EndHorizontal();

        if(!copyChoices.boolValue) {
            OnGUI_BeatViewDecision(choiceList, beatList);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(12);

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
            AddChoice(choiceList, newBeatId);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI_BeatViewChoice(SerializedProperty choiceList, int index, SerializedProperty beatList)
    {
        SerializedProperty arrayElement = choiceList.GetArrayElementAtIndex(index);
        SerializedProperty choiceType = arrayElement.FindPropertyRelative("_type");
        SerializedProperty textType = arrayElement.FindPropertyRelative("_textType");
        SerializedProperty beatId = arrayElement.FindPropertyRelative("_beatId");
        
        EditorGUILayout.BeginVertical(GUILayout.MaxHeight(155));

        EditorGUIUtility.labelWidth = 90;
        EditorGUILayout.PropertyField(choiceType);
        EditorGUILayout.PropertyField(textType);

        if((ChoiceTextType) textType.enumValueIndex == ChoiceTextType.Predefined) {
            SerializedProperty text = arrayElement.FindPropertyRelative("_text");
            text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(50));
        }
        // EditorGUILayout.LabelField("Leads to Beat ID: " + beatId.intValue.ToString());

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Leads to Beat: ");

        // Select the beat with a dropdown box showing the names of all beats in StoryData
        
        int beatIndex = FindIndexOfBeatId(beatList, beatId.intValue);
        beatIndex = EditorGUILayout.Popup(beatIndex, GetBeatNames(beatList));
        beatId.intValue = beatList.GetArrayElementAtIndex(beatIndex).FindPropertyRelative("_id").intValue;

        // beatId.intValue = EditorGUILayout.IntField(beatId.intValue);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Go to Beat"))
        {
            _currentIndex = FindIndexOfBeatId(beatList, beatId.intValue);
            GUI.FocusControl(null);
            Repaint();
        }
        if(GUILayout.Button("Delete Choice")) {
            choiceList.DeleteArrayElementAtIndex(index);
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
        int result = 0;

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

    private string[] GetBeatNames(SerializedProperty beatList) {
        // List<BeatData> allBeats = StoryData.LoadData().Beats;
        string[] beatNames = new string[beatList.arraySize];
        for(int i = 0; i < beatNames.Length; i++) {
            beatNames[i] = beatList.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
        }
        return beatNames;
    }

    private void AddBeat(SerializedProperty beatList, int beatId, string initialName = "New Dialogue Beat")
    {
        int index = beatList.arraySize;
        beatList.arraySize += 1;
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty name = arrayElement.FindPropertyRelative("_name");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");

        name.stringValue = initialName;
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

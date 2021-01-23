using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Custom editor window to edit dialogue lines

public class DialogueTextEditor : EditorWindow
{
    private Vector2 _scroll = new Vector2();

    [MenuItem("SFAS/Show Dialogue Text Editor")]
    public static void ShowDialogueTextEditor()
    {
        GetWindow(typeof(DialogueTextEditor));
    }

    void OnGUI()
    {
        DialogueTextData data = DialogueTextData.LoadData();
        SerializedObject dataObj = new SerializedObject(data);

        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        bool hasDrawnGreetingLabel = false, hasDrawnListeningLabel = false, hasDrawnFlatteryLabel = false;
        bool hasDrawnThreatenLabel = false, hasDrawnBribeLabel = false, hasDrawnRumourLabel = false;

        // Iterate over each property in the object
        SerializedProperty iterator = dataObj.GetIterator();
        if (iterator.Next(true)) 
        {
            do 
            {
                // If the type is a TextList or SerializableDictionary
                if(iterator.type == "TextList" || iterator.type.StartsWith("SerializableDictionary")) 
                {
                    // If it's the first property with a certain prefix then draw a label for that category
                    if(!hasDrawnGreetingLabel && iterator.name.StartsWith("_greeting")) 
                    {
                        hasDrawnGreetingLabel = true;
                        EditorGUILayout.LabelField("Greetings", EditorStyles.boldLabel);
                    }
                    if(!hasDrawnListeningLabel && iterator.name.StartsWith("_listening")) 
                    {
                        hasDrawnListeningLabel = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Listening", EditorStyles.boldLabel);
                    }
                    if(!hasDrawnFlatteryLabel && iterator.name.StartsWith("_flattery")) 
                    {
                        hasDrawnFlatteryLabel = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Flattery", EditorStyles.boldLabel);
                    }
                    if(!hasDrawnThreatenLabel && iterator.name.StartsWith("_threaten")) 
                    {
                        hasDrawnThreatenLabel = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Threaten", EditorStyles.boldLabel);
                    }
                    if(!hasDrawnBribeLabel && iterator.name.StartsWith("_bribe")) 
                    {
                        hasDrawnBribeLabel = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Bribery", EditorStyles.boldLabel);
                    }
                    if(!hasDrawnRumourLabel && iterator.name.StartsWith("_rumour")) 
                    {
                        hasDrawnRumourLabel = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Rumours", EditorStyles.boldLabel);
                    }

                    // If it's a TextList variable
                    if(iterator.type == "TextList") 
                    {
                        // Draw an int field setting the size of the string array
                        SerializedProperty textList = iterator.FindPropertyRelative("_textList");
                        textList.arraySize = EditorGUILayout.IntField(iterator.displayName, textList.arraySize);

                        // For each string in the array draw a text field setting the value
                        for(int t = 0; t < textList.arraySize; t++) 
                        {
                            SerializedProperty text = textList.GetArrayElementAtIndex(t);
                            // Setting the label as a space gives no label but still indents the text field
                            text.stringValue = EditorGUILayout.TextField(" ", text.stringValue);
                        }
                    // If isn't a TextList then it'll be a SerializableDictionary
                    } 
                    else 
                    {
                        bool foldout = EditorGUILayout.Foldout(true, iterator.displayName);

                        if(foldout) 
                        {
                            // Get the keys and values variables from the dictinoary
                            SerializedProperty keys = iterator.FindPropertyRelative("keys");
                            SerializedProperty values = iterator.FindPropertyRelative("values");

                            string[] enumNames = null;
                            
                            // As far as I can tell there's no way to get the enum type, so check it by variable name (not great but only solution)
                            if(iterator.name.EndsWith("Occupation")) enumNames = System.Enum.GetNames(typeof(CharacterOccupation));
                            if(iterator.name.EndsWith("Age")) enumNames = System.Enum.GetNames(typeof(CharacterAge));
                            if(iterator.name.EndsWith("Wealth")) enumNames = System.Enum.GetNames(typeof(CharacterWealth));

                            if(enumNames != null) 
                            {
                                EditorGUI.indentLevel++;

                                keys.arraySize = enumNames.Length;
                                values.arraySize = enumNames.Length;

                                // Same as with TextList
                                for(int i = 0; i < enumNames.Length; i++) 
                                {
                                    // Set each key as the enum value, this means there is a TextList object for each enum value
                                    keys.GetArrayElementAtIndex(i).enumValueIndex = i;
                                    SerializedProperty textList = values.GetArrayElementAtIndex(i).FindPropertyRelative("_textList");
                                    textList.arraySize = EditorGUILayout.IntField(enumNames[i], textList.arraySize);

                                    for(int t = 0; t < textList.arraySize; t++) 
                                    {
                                        SerializedProperty text = textList.GetArrayElementAtIndex(t);
                                        text.stringValue = EditorGUILayout.TextField(" ", text.stringValue);
                                    }
                                }

                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                }                
            } 
            while (iterator.Next(false));
        }

        GUILayout.Space(20);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        dataObj.ApplyModifiedProperties();
    }
}

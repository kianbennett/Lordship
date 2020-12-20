using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {
        Rect buttonPosition = position;
        //buttonPosition.y += 20;
        buttonPosition.x = position.width - 34;
        buttonPosition.width = 48;
        buttonPosition.height = 14;

        if (prop.longValue == 0 || GUI.Button(buttonPosition, "New")) {
            //Guid guid = Guid.NewGuid();
            //prop.stringValue = guid.ToString();

            // Generates a unique long that won't be repeated for over 4000 years
            prop.longValue = generateUniqueLong();
        }

        // Place a label so it can't be edited by accident
        Rect textFieldPosition = position;
        textFieldPosition.height = 16;
        DrawLabelField(textFieldPosition, prop, label);
    }

    void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label) {
        EditorGUI.LabelField(position, label, new GUIContent(prop.longValue.ToString()));
    }

    //public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
    //    return 20;
    //}

    // Generates a 64 bit integer based on windows file time
    private long generateUniqueLong() {
        long time = System.DateTime.Now.ToFileTime();
        char[] chars = time.ToString().ToCharArray();
        // Sramble it using the last and third to last digits so it looks more "random"
        int last = int.Parse(chars[chars.Length - 1].ToString());
        int thirdToLast = int.Parse(chars[chars.Length - 3].ToString());
        int n = (last + thirdToLast) % 10;
        string newString = "";

        for (int i = 0; i < chars.Length - 2; i++) {
            int charAsInt = int.Parse(chars[i].ToString());
            charAsInt = (charAsInt + n) % 10;
            newString += charAsInt;
        }

        return long.Parse(newString);
    }
}

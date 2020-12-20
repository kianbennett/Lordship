using UnityEngine;
using UnityEditor;
using System.Linq;

/*
    EXPERIMENTAL
    Defines a custom layout for the [PaletteColour] property in the inspector
 */
[CustomPropertyDrawer(typeof(PaletteColourAttribute))]
public class PaletteColorDrawer : PropertyDrawer {

    private string[] colourNames;

    /* Keeps track of size of color list so name list doesn't update every time */
    private int colourCount;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        PaletteColourAttribute paletteColour = attribute as PaletteColourAttribute;

        if (colourCount != paletteColour.Palette.Length) {
            colourCount = paletteColour.Palette.Length;
            colourNames = paletteColour.Palette.Select(o => o.name).ToArray();
        }

        int value = property.intValue;

        PaletteColour paletteColor = paletteColour.Palette[value];

        GUILayout.BeginHorizontal();
        property.intValue = EditorGUILayout.Popup(property.displayName, property.intValue, colourNames, EditorStyles.popup);
        GUI.enabled = false;
        EditorGUILayout.ColorField(paletteColor.colour, GUILayout.MaxWidth(50));
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return -2;
    }
}
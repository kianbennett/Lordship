using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/*
    Defines a custom layout for the inspector of a CharacterAppearance component
 */

[CanEditMultipleObjects]
[CustomEditor(typeof(CharacterAppearance))]
public class CharacterAppearanceEditor : Editor {

    private string[] hairNames, bodyNames, handsNames, legsNames, feetNames, hatNames;
    private ColourPalette skinColors;

    private Dictionary<ColourPalette, string[]> colourNames = new Dictionary<ColourPalette, string[]>();

    void OnEnable() {
        hairNames = AssetManager.instance.hairMeshes.Select(o => o.name).ToArray();
        bodyNames = AssetManager.instance.bodyMeshes.Select(o => o.name).ToArray();
        handsNames = AssetManager.instance.handMeshes.Select(o => o.name).ToArray();
        hatNames = AssetManager.instance.hatMeshes.Select(o => o.name).ToArray();
        skinColors = AssetManager.instance.skinColours;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        CharacterAppearance appearance = (CharacterAppearance) target;

        /* Hair */
        EditorGUILayout.Space();
        appearance.hair = EditorGUILayout.Popup("Hair", appearance.hair, hairNames, EditorStyles.popup);
        drawColourPair("Hair Colours", "hairColour1", "hairColour2", AssetManager.instance.hairMeshes[appearance.hair].materials);

        /* Body */
        EditorGUILayout.Space();
        appearance.body = EditorGUILayout.Popup("Body", appearance.body, bodyNames, EditorStyles.popup);
        drawColourPair("Body Colours", "bodyColour1", "bodyColour2", AssetManager.instance.bodyMeshes[appearance.body].materials);

        /* Hands */
        EditorGUILayout.Space();
        appearance.hands = EditorGUILayout.Popup("Hands", appearance.hands, handsNames, EditorStyles.popup);
        drawColourPair("Hands Colours", "handsColour1", "handsColour2", AssetManager.instance.handMeshes[appearance.hands].materials);

        /* Hat */
        EditorGUILayout.Space();
        appearance.hat = EditorGUILayout.Popup("Hat", appearance.hat, hatNames, EditorStyles.popup);
        drawColourPair("Hat Colours", "hatColour1", "hatColour2", AssetManager.instance.hatMeshes[appearance.hat].materials);

        /* Skin */
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        appearance.skinColour = EditorGUILayout.IntSlider("Skin", appearance.skinColour, 0, skinColors.colours.Length - 1);
        GUI.enabled = false;
        EditorGUILayout.ColorField(skinColors.colours[appearance.skinColour].colour, GUILayout.MaxWidth(50));
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        /* Apply / Randomise */
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply")) appearance.ApplyAll();
        if (GUILayout.Button("Randomise")) appearance.Randomise();
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void drawColourPair(string name, string propName1, string propName2, MaterialSet materialSet) {
        // Get object properties
        SerializedProperty prop1 = serializedObject.FindProperty(propName1);
        SerializedProperty prop2 = serializedObject.FindProperty(propName2);

        // Draw properties
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(EditorGUIUtility.labelWidth - 2));
        if(materialSet.hasColour1) {
            // Limit property values to colour palette size
            if (materialSet.colourPalette1.colours.Length > 0 && prop1.intValue >= materialSet.colourPalette1.colours.Length) prop1.intValue = materialSet.colourPalette1.colours.Length - 1;
            prop1.intValue = EditorGUILayout.Popup("", prop1.intValue, getColourNames(materialSet.colourPalette1), EditorStyles.popup, GUILayout.MaxWidth(71));
            GUI.enabled = false;
            EditorGUILayout.ColorField(materialSet.colourPalette1.colours.Length > 0 ? materialSet.colourPalette1.colours[prop1.intValue].colour : Color.white, GUILayout.MaxWidth(35));
            GUI.enabled = true;
        }
        if (materialSet.hasColour2) {
            if (materialSet.colourPalette2.colours.Length > 0 && prop2.intValue >= materialSet.colourPalette2.colours.Length) prop2.intValue = materialSet.colourPalette2.colours.Length - 1;
            prop2.intValue = EditorGUILayout.Popup("", prop2.intValue, getColourNames(materialSet.colourPalette2), EditorStyles.popup, GUILayout.MaxWidth(71));
            GUI.enabled = false;
            EditorGUILayout.ColorField(materialSet.colourPalette2.colours.Length > 0 ? materialSet.colourPalette2.colours[prop2.intValue].colour : Color.white, GUILayout.MaxWidth(35));
            GUI.enabled = true;
        }
        GUILayout.EndHorizontal();
    }

    private string[] getColourNames(ColourPalette palette) {
        if (colourNames.ContainsKey(palette)) {
            return colourNames[palette];
        } else {
            string[] names = palette.colours.Select(o => o.name).ToArray();
            colourNames.Add(palette, names);
            return names;
        }
    }
}
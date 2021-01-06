using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(AudioClipAttribute))]
public class AudioClipDrawer : PropertyDrawer {

    // For each CustomAudio property show a volume slider and play/stop buttons on one li
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // EditorGUILayout.PropertyField(property);

        SerializedProperty propClip = property.FindPropertyRelative("clip");
        SerializedProperty propVolume = property.FindPropertyRelative("volume");

        // Naming convention I've used is music and sfx prefixes, remove this to make the display name shorter
        string name = property.displayName;
        name = name.Replace("Sfx ", "").Replace("Music ", "");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(EditorGUIUtility.labelWidth - 40));
        EditorGUILayout.PropertyField(propClip, GUIContent.none, GUILayout.Width(80));

        propVolume.floatValue = EditorGUILayout.Slider(propVolume.floatValue, 0, 2);

        if (GUILayout.Button(">", GUILayout.Width(18), GUILayout.Height(16))) {
            AudioManager.instance.sourceSFX.Stop();
            AudioManager.instance.sourceSFX.PlayOneShot(propClip.objectReferenceValue as AudioClip, propVolume.floatValue);
        }
        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(16))) {
            AudioManager.instance.sourceSFX.Stop();
        }

        EditorGUILayout.EndHorizontal();

        property.serializedObject.ApplyModifiedProperties();
    }

    // This sets the correct height for some reason, otherwise it is too wide
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 0;
    }
}
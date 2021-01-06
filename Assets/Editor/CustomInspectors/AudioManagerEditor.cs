using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// If it delete this class I get a weird error, no idea why

[CanEditMultipleObjects]
[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor {
}
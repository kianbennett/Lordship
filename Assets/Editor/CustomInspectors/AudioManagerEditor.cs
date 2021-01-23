using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// If I delete this class I get a weird error, no idea why as it doesn't do anything wtf

[CanEditMultipleObjects]
[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor 
{
}
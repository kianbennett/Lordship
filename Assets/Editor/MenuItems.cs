using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuItems {

    [MenuItem("Assets/Select AssetManager")]
    private static void SelectAssetManager() {
        AssetManager prefab = AssetDatabase.LoadAssetAtPath<AssetManager>("Assets/Prefabs/AssetManager.prefab");

        Selection.activeGameObject = prefab.gameObject;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable] public class MeshFloatDictionary : SerializableDictionary<MeshMaterialSet, float> {}
[Serializable] public class ColourFloatDictionary : SerializableDictionary<PaletteColour, float> {}

public class WeightMap<T> : ScriptableObject {

    public virtual SerializableDictionary<T, float> Map {
        get { return null; }
    }

    public int GetRandomIndex() {
        float totalWeight = 0;
        foreach(float weight in Map.Values) totalWeight += weight;

        float r = UnityEngine.Random.value;
        float rWeight = totalWeight * r;
        float weightCumul = 0;

        for(int i = 0; i < Map.Keys.Count; i++) {
            float weight = Map.GetValue(Map.Keys[i]);
            if (weight > 0) {
                weightCumul += weight;
                if (weightCumul > rWeight) return i;
            }
        }
        return 0;
    }

    public T GetRandom() {
        return Map.Keys[GetRandomIndex()];
    }
}
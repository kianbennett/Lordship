using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<K, V> {
    
    public List<K> keys = new List<K>();
    public List<V> values = new List<V>();

    public void Add(K key, V value) {
        keys.Add(key);
        values.Add(value);
    }

    public void SetValue(K key, V value) {
        int index = keys.IndexOf(key);
        if (index != -1) values[index] = value;
    }

    public V GetValue(K key) {
        int index = keys.IndexOf(key);
        return values[index];
    }

    public void Clear() {
        keys.Clear();
        values.Clear();
    }
}
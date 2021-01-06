using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use this instead of dictionary so it can be serialized and accessed by the unity editor
[System.Serializable]
public class SerializableDictionary<K, V> {
    
    [SerializeField] private List<K> keys = new List<K>();
    [SerializeField] private List<V> values = new List<V>();

    public List<K> Keys { get { return keys; } }
    public List<V> Values { get { return values; } }

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

    public bool ContainsKey(K key) {
        return keys.Contains(key);
    }

    public bool ContainsValue(V value) {
        return values.Contains(value);
    }

    public void Clear() {
        keys.Clear();
        values.Clear();
    }
}
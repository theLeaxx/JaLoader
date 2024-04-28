using System;
using System.Collections.Generic;
using UnityEngine;

namespace JaLoader
{
    /// <summary>
    /// A Dictionary that can be serialized.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
                Console.LogError($"There are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable");

            for (int i = 0; i < keys.Count; i++)
                Add(keys[i], values[i]);
        }
    }
} 
using UnityEngine;
using System.Collections.Generic;

namespace SolomonCopy.Meta
{
    [System.Serializable]
    public class LootEntry<T>
    {
        public T item;
        public float weight;
    }

    public static class LootTableProcessor
    {
        public static T Roll<T>(List<LootEntry<T>> table)
        {
            float totalWeight = 0;
            foreach (var entry in table) totalWeight += entry.weight;

            float roll = Random.Range(0, totalWeight);
            float cursor = 0;

            foreach (var entry in table)
            {
                cursor += entry.weight;
                if (roll <= cursor) return entry.item;
            }
            return default;
        }
    }
}

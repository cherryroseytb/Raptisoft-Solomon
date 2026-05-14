using UnityEngine;
using System.Collections.Generic;

namespace SolomonCopy.Magic
{
    public class ComboDiscoveryManager : MonoBehaviour
    {
        public static ComboDiscoveryManager Instance { get; private set; }
        public HashSet<string> discoveredCombos = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDiscovery();
        }

        public void RegisterCombo(BaseMagicId a, BaseMagicId b)
        {
            string key = GetKey(a, b);
            if (discoveredCombos.Add(key))
            {
                Debug.Log("[Combo] New Combo Discovered: " + key);
                SaveDiscovery();
            }
        }

        public bool IsDiscovered(BaseMagicId a, BaseMagicId b) => discoveredCombos.Contains(GetKey(a, b));

        private string GetKey(BaseMagicId a, BaseMagicId b)
        {
            return (int)a < (int)b ? a + "_" + b : b + "_" + a;
        }

        private void SaveDiscovery()
        {
            string list = string.Join(",", discoveredCombos);
            PlayerPrefs.SetString("DiscoveredCombos", list);
        }

        private void LoadDiscovery()
        {
            string list = PlayerPrefs.GetString("DiscoveredCombos", "");
            if (!string.IsNullOrEmpty(list))
            {
                discoveredCombos = new HashSet<string>(list.Split(','));
            }
        }
    }
}

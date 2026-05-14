using UnityEngine;
using System.Collections.Generic;

namespace SolomonCopy.Systems
{
    public class RunDataTracker : MonoBehaviour
    {
        public static RunDataTracker Instance { get; private set; }

        public int totalDamageDealt;
        public int totalKills;
        public float survivalTime;
        public Dictionary<string, int> killsByType = new Dictionary<string, int>();
        public Dictionary<string, int> magicUsageCount = new Dictionary<string, int>();

        private void Awake()
        {
            Instance = this;
        }

        public void TrackDamage(int amount) => totalDamageDealt += amount;
        
        public void TrackKill(string enemyType)
        {
            totalKills++;
            if (!killsByType.ContainsKey(enemyType)) killsByType[enemyType] = 0;
            killsByType[enemyType]++;
        }

        public void TrackMagicUse(string magicName)
        {
            if (!magicUsageCount.ContainsKey(magicName)) magicUsageCount[magicName] = 0;
            magicUsageCount[magicName]++;
        }

        private void Update()
        {
            if (LobbyStateMarker.InLobby == false) survivalTime += Time.deltaTime;
        }
    }
}

// UpgradePool.cs
// 모든 SkillUpgradeSO를 모아두는 풀. LevelUpController가 여기서 N개 랜덤 추출.
// Unity 에디터: Create > SolomonCopy > Upgrade > UpgradePool. 1개 생성 후 LevelUpController에 연결.

using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Upgrade
{
    [CreateAssetMenu(menuName = "SolomonCopy/Upgrade/UpgradePool", fileName = "UpgradePool")]
    public class UpgradePool : ScriptableObject
    {
        public List<SkillUpgradeSO> upgrades = new List<SkillUpgradeSO>();

        // playerLevel: 현재 플레이어 레벨, stackCount: 업그레이드별 현재 스택
        public List<SkillUpgradeSO> RandomPick(int count, int playerLevel, Dictionary<SkillUpgradeSO, int> stackCount)
        {
            var pool = new List<SkillUpgradeSO>();
            foreach (var u in upgrades)
            {
                if (u == null) continue;
                if (u.minPlayerLevel > playerLevel) continue;
                if (u.maxStacks > 0 && stackCount.TryGetValue(u, out var n) && n >= u.maxStacks) continue;
                pool.Add(u);
            }

            // Fisher-Yates 셔플 후 앞 count개
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            return pool.GetRange(0, Mathf.Min(count, pool.Count));
        }
    }
}

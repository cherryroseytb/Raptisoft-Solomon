using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolomonCopy.Meta
{
    [CreateAssetMenu(menuName = "SolomonCopy/Meta/LobbyTextTable", fileName = "LobbyTextTable")]
    public class LobbyTextTableSO : ScriptableObject
    {
        [Serializable]
        public class ServiceTextEntry
        {
            public LobbySkillEffectId id = LobbySkillEffectId.None;
            public string displayName = "";
            [TextArea] public string description = "";
        }

        [Serializable]
        public class FateTextEntry
        {
            public LobbyFeatureId id = LobbyFeatureId.None;
            public string displayName = "";
            [TextArea] public string description = "";
        }

        public List<ServiceTextEntry> services = new List<ServiceTextEntry>();
        public List<FateTextEntry> fates = new List<FateTextEntry>();

        [ContextMenu("Fill Korean Defaults")]
        public void FillKoreanDefaults()
        {
            services.Clear();
            fates.Clear();

            foreach (LobbySkillEffectId id in Enum.GetValues(typeof(LobbySkillEffectId)))
            {
                if (id == LobbySkillEffectId.None) continue;
                services.Add(new ServiceTextEntry
                {
                    id = id,
                    displayName = GetServiceNameKo(id),
                    description = GetServiceDescKo(id)
                });
            }

            foreach (LobbyFeatureId id in Enum.GetValues(typeof(LobbyFeatureId)))
            {
                if (id == LobbyFeatureId.None) continue;
                fates.Add(new FateTextEntry
                {
                    id = id,
                    displayName = GetFateNameKo(id),
                    description = GetFateDescKo(id)
                });
            }
        }

        public bool TryGetServiceText(LobbySkillEffectId id, out string displayName, out string description)
        {
            for (int i = 0; i < services.Count; i++)
            {
                var e = services[i];
                if (e != null && e.id == id)
                {
                    displayName = e.displayName;
                    description = e.description;
                    return true;
                }
            }
            displayName = "";
            description = "";
            return false;
        }

        public bool TryGetFateText(LobbyFeatureId id, out string displayName, out string description)
        {
            for (int i = 0; i < fates.Count; i++)
            {
                var e = fates[i];
                if (e != null && e.id == id)
                {
                    displayName = e.displayName;
                    description = e.description;
                    return true;
                }
            }
            displayName = "";
            description = "";
            return false;
        }

        private static string GetServiceNameKo(LobbySkillEffectId id)
        {
            switch (id)
            {
                case LobbySkillEffectId.Ringfinger: return "Ringfinger";
                case LobbySkillEffectId.TwoRingfingers: return "Two Ringfingers";
                case LobbySkillEffectId.MoreDrops: return "More Drops";
                case LobbySkillEffectId.MoreGoldDrops: return "More Gold Drops";
                case LobbySkillEffectId.Plus1ToAllSkills: return "+1 to All Skills";
                case LobbySkillEffectId.Plus2ToAllSkills: return "+2 to All Skills";
                case LobbySkillEffectId.MasterOfOffense: return "Master of Offense";
                case LobbySkillEffectId.MagicScavenger: return "Magic Scavenger";
                case LobbySkillEffectId.BattleMageStart: return "Battle Mage";
                case LobbySkillEffectId.TelekineticStart: return "Telekinetic";
                case LobbySkillEffectId.SecondSecondary: return "Second Secondary";
                case LobbySkillEffectId.CreativeCasting: return "Creative Casting";
                case LobbySkillEffectId.AutoPotion: return "Auto Potion";
                case LobbySkillEffectId.FasterCasterStart: return "Faster Caster";
                case LobbySkillEffectId.MagicShieldInstead: return "Magic Shield Instead";
                case LobbySkillEffectId.MagicShieldToo: return "Magic Shield Too!";
                case LobbySkillEffectId.MagicInventory: return "Magic Inventory";
                case LobbySkillEffectId.BlazeOfGlory: return "Blaze of Glory";
                case LobbySkillEffectId.Hardcore: return "Hardcore";
                default: return id.ToString();
            }
        }

        private static string GetServiceDescKo(LobbySkillEffectId id)
        {
            switch (id)
            {
                case LobbySkillEffectId.Ringfinger: return "시작 시 랜덤 반지 1개를 인벤토리에 추가합니다.";
                case LobbySkillEffectId.TwoRingfingers: return "시작 시 랜덤 반지 2개를 인벤토리에 추가합니다.";
                case LobbySkillEffectId.MoreDrops: return "반지/필드 보너스 드롭 확률이 증가합니다.";
                case LobbySkillEffectId.MoreGoldDrops: return "골드 드롭 확률과 획득량이 증가합니다.";
                case LobbySkillEffectId.Plus1ToAllSkills: return "시작 스킬 레벨 보너스 +1.";
                case LobbySkillEffectId.Plus2ToAllSkills: return "시작 스킬 레벨 보너스 +2.";
                case LobbySkillEffectId.MasterOfOffense: return "주공격 계열에 시작 보너스를 적용합니다.";
                case LobbySkillEffectId.MagicScavenger: return "오브 획득량 및 자석 반경이 증가합니다.";
                case LobbySkillEffectId.BattleMageStart: return "배틀 메이지 시작 지급 .";
                case LobbySkillEffectId.TelekineticStart: return "텔레키네틱 시작 지급 .";
                case LobbySkillEffectId.SecondSecondary: return "보조 스킬 1개를 무작위로 추가하여 시작합니다.";
                case LobbySkillEffectId.CreativeCasting: return "레벨업 시 선택 가능한 카드 수가 1장 늘어납니다.";
                case LobbySkillEffectId.AutoPotion: return "위기 체력에서 자동 포션을 사용합니다.";
                case LobbySkillEffectId.FasterCasterStart: return "패스터 캐스터 시작 지급 .";
                case LobbySkillEffectId.MagicShieldInstead: return "2nd 보조 스킬을 매직 실드로 대체 .";
                case LobbySkillEffectId.MagicShieldToo: return "추가 매직 실드를 시작 시 지급 .";
                case LobbySkillEffectId.MagicInventory: return "이전 런 아이템 유지 시작 (무덤 연계, 구현 보정 중).";
                case LobbySkillEffectId.BlazeOfGlory: return "사망 시 주변에 폭발 피해를 줍니다.";
                case LobbySkillEffectId.Hardcore: return "주는 피해/받는 피해가 모두 2배가 됩니다.";
                default: return "";
            }
        }

        private static string GetFateNameKo(LobbyFeatureId id)
        {
            switch (id)
            {
                case LobbyFeatureId.FeelingPerky: return "Feeling Perky";
                case LobbyFeatureId.ThePerkiest: return "The Perkiest";
                case LobbyFeatureId.MoreGraveyards: return "More Graveyards";
                case LobbyFeatureId.BossMonsters: return "Boss Monsters";
                case LobbyFeatureId.Griselda: return "Unlock Griselda";
                case LobbyFeatureId.Wegnus: return "Unlock Wegnus";
                case LobbyFeatureId.Vorpus: return "Unlock Vorpus";
                case LobbyFeatureId.Wazoo: return "Unlock Wazoo";
                case LobbyFeatureId.Athicus: return "Unlock Athicus";
                case LobbyFeatureId.Andra: return "Unlock Andra";
                case LobbyFeatureId.UnlockMagicTrap: return "Unlock Magic Trap";
                case LobbyFeatureId.UnlockExplosiveShield: return "Unlock Explosive Shield";
                case LobbyFeatureId.UnlockScavenger: return "Unlock Scavenger";
                case LobbyFeatureId.UnlockCallComet: return "Unlock Call Comet";
                case LobbyFeatureId.ReduceTonicCost: return "Reduce Tonic Cost";
                case LobbyFeatureId.NewCharacterUnlocked: return "New Character";
                case LobbyFeatureId.NewActiveSkillUnlocked: return "New Active Skill";
                default: return id.ToString();
            }
        }

        private static string GetFateDescKo(LobbyFeatureId id)
        {
            switch (id)
            {
                case LobbyFeatureId.FeelingPerky: return "기능 슬롯 확장 계열 .";
                case LobbyFeatureId.ThePerkiest: return "기능 슬롯 확장 상위 단계 .";
                case LobbyFeatureId.MoreGraveyards: return "맵 선택 슬롯을 추가로 해제합니다.";
                case LobbyFeatureId.BossMonsters: return "전투 중 보스 몬스터가 주기적으로 등장합니다.";
                case LobbyFeatureId.Griselda: return "Griselda 캐릭터를 해금합니다.";
                case LobbyFeatureId.Wegnus: return "Wegnus 캐릭터를 해금합니다.";
                case LobbyFeatureId.Vorpus: return "Vorpus 캐릭터를 해금합니다.";
                case LobbyFeatureId.Wazoo: return "Wazoo 캐릭터를 해금합니다.";
                case LobbyFeatureId.Athicus: return "Athicus 캐릭터를 해금합니다.";
                case LobbyFeatureId.Andra: return "Andra 캐릭터를 해금합니다.";
                case LobbyFeatureId.UnlockMagicTrap: return "Magic Trap 스킬을 해금합니다.";
                case LobbyFeatureId.UnlockExplosiveShield: return "Explosive Shield 스킬을 해금합니다.";
                case LobbyFeatureId.UnlockScavenger: return "무덤지기 NPC를 활성화합니다.";
                case LobbyFeatureId.UnlockCallComet: return "Call Comet 스킬을 해금합니다.";
                case LobbyFeatureId.ReduceTonicCost: return "구매할 때마다 토닉 가격이 추가 할인됩니다.";
                case LobbyFeatureId.NewCharacterUnlocked: return "신규 캐릭터 해금 슬롯(예비 데이터).";
                case LobbyFeatureId.NewActiveSkillUnlocked: return "신규 액티브 스킬 해금 슬롯(예비 데이터).";
                default: return "";
            }
        }
    }
}

namespace SolomonCopy.Meta
{
    // 로비 우측 NPC에서 구매/장착하는 "런 시작 효과" 계열.
    public enum LobbySkillEffectId
    {
        None = 0,
        Ringfinger = 1,                // 시작 시 반지 1개
        TwoRingfingers = 2,            // 시작 시 반지 2개
        MoreDrops = 3,                 // 반지/보너스 드랍 증가
        MoreGoldDrops = 4,             // 골드 드랍 증가
        Plus1ToAllSkills = 5,          // 시작 스킬 +1
        Plus2ToAllSkills = 6,          // 시작 스킬 +2
        MasterOfOffense = 7,           // 주공격 서브스킬 지급
        MagicScavenger = 8,            // 오브 크기/빈도 증가
        BattleMageStart = 9,           // 배틀메이지 시작 지급
        TelekineticStart = 10,         // 텔레키네틱 시작 지급
        SecondSecondary = 11,          // 랜덤 액티브 추가
        CreativeCasting = 12,          // 창의성 시작 지급
        AutoPotion = 13,               // 자동 포션
        FasterCasterStart = 14,        // 패캐 시작 지급
        MagicShieldInstead = 15,       // 2nd 대신 매직쉴드
        MagicShieldToo = 16,           // 매직쉴드 추가 지급
        MagicInventory = 17,           // 이전 판 인벤 반지 유지 시작
        BlazeOfGlory = 18,             // 사망 폭발
        Hardcore = 19,                 // 주고받는 데미지 2배
    }

    // 로비 우측 NPC에서 구매하는 "항상 적용 기능" 계열.
    public enum LobbyFeatureId
    {
        None = 0,
        FeelingPerky = 1,              // 장착 슬롯 +1 (1단계)
        ThePerkiest = 2,               // 장착 슬롯 +1 (2단계)
        MoreGraveyards = 3,            // 맵 추가 오픈
        BossMonsters = 4,              // 보스 등장 활성
        Griselda = 5,                  // 캐릭터 해금
        Wegnus = 6,                    // 캐릭터 해금
        Vorpus = 7,                    // 캐릭터 해금
        Wazoo = 8,                     // 캐릭터 해금(관측 기반)
        Athicus = 9,
        Andra = 10,
        UnlockMagicTrap = 11,
        UnlockExplosiveShield = 12,
        UnlockScavenger = 13,
        UnlockCallComet = 14,
        ReduceTonicCost = 15,
        NewCharacterUnlocked = 16,
        NewActiveSkillUnlocked = 17
    }
}

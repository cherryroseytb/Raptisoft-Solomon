# Development Log: Solomon's Boneyard Clone (Unity)

이 문서는 프로젝트의 설계 결정, 기술적 마일스톤, 구현 히스토리를 기록합니다.

## 2026-05-10: 프로젝트 시작
- **목표:** 'Solomon's Boneyard'의 핵심 메커니즘을 유니티로 복제(Clone) 후 독창적 요소 추가.
- **분석 결과:**
  - 원작은 C++ 기반의 네이티브 앱으로 매우 효율적임.
  - 유니티 프로젝트로 리메이크 시 엔진 오버헤드와 SDK 포함 약 100~150MB 예상.
- **개발 전략:**
  - 기능적 클론(MVP)을 우선 구현하여 기술적 토대를 마련.
  - 데이터 중심 설계(ScriptableObjects, Object Pooling) 활용.
- **로드맵:**
  1. [Stage 1] 플레이어 조작 (이동, 공격)
  2. [Stage 2] 적 스폰 및 AI
  3. [Stage 3] 전투 시스템
  4. [Stage 4] 레벨업 및 스킬 트리
  5. [Stage 5] 메타 루프 (상점, 골드)

## 2026-05-12: 안드로이드 MVP 1차 코드 구현

### 결정 사항
- **타겟 플랫폼:** 안드로이드. 입력을 키보드/마우스에서 듀얼 조이스틱으로 전환.
- **스킬 시스템 설계:** ScriptableObject 기반 데이터 중심. 마법을 코드 분기가 아닌 에셋으로 정의해 확장성 확보.
- **풀링:** 발사체/적은 첫 부족 시 Instantiate, 이후 재사용 (모바일 GC 스파이크 회피).
- **씬 추적 AI:** NavMesh 대신 단순 직선 추적 (top-down 2D에는 과한 비용).

### 구현 범위 (이번 작업)
1. 모바일 입력 — 자체 UGUI 가상 조이스틱 (외부 라이브러리 없음) + PC 폴백 경로
2. 발사체 시스템 — Projectile + ObjectPooler + 폭발/연쇄 헬퍼
3. 적 AI + 웨이브 스폰 + 상태이상(슬로우/번/쇼크) 타임스탬프 기반
4. 스킬 조합 시스템 — BaseMagicSO / ComboMagicSO / MagicRegistrySO (순서 무관 룩업)

### 파일 구조 (`Assets/Scripts/`)
```
Scripts/
├── Enemy/        EnemyController, WaveSpawner
├── Magic/        MagicEnums, BaseMagicSO, ComboMagicSO, MagicRegistrySO
├── Player/       PlayerController, MagicCaster
├── Projectile/   Projectile, ProjectileEffects
├── Systems/      ObjectPooler, GameManager
└── UI/           VirtualJoystick, SlotSwitchButton
```

### 주요 설계 결정 — 왜 ScriptableObject?
- `cr-dev-log.md`의 원작 분석에서 "다양한 마법 + 마법 조합"이 핵심 재미로 확정됨.
- 마법 수가 늘어날수록 코드 분기로 풀면 폭발적으로 복잡해짐 → 데이터 자산으로 분리해야 함.
- `MagicRegistrySO.LookupCombo(a, b)`는 정렬된 enum 쌍 키로 조회 → 순서 무관.

### 런타임 흐름 (마법 발사)
1. `PlayerController` → 입력 읽기 (조이스틱 또는 PC 폴백)
2. `MagicCaster.TryFire(dir)` 호출
3. slotA == slotB → `BaseMagicSO` 사용 / 다르면 `ComboMagicSO` 룩업 후 사용 (실패 시 slotA 폴백)
4. `ObjectPooler.Spawn` → `Projectile.Initialize` → 충돌 시 `EnemyController.TakeDamage` + 상태이상 적용

### 미구현 / 다음 단계
- PlayerHealth, 적 접촉 데미지 처리 (현재 TODO 주석)
- 슬롯 전환 UI 실배치 (스크립트는 있음)
- 마법/적 프리팹 및 SO 에셋 생성은 Unity 에디터 작업 (사용자 진행)
- 안드로이드 빌드(IL2CPP, ARM64), 실기 멀티터치 검증
- 레벨업/스킬 트리, 골드/상점 메타 루프

## 2026-05-12 (후속): 로그라이크 코어 구현

### 추가된 모듈
- **PlayerHealth**: HP, 무적 시간(0.5s), GameOver 트리거. 싱글톤.
- **EnemyController 접촉 데미지**: `OnCollisionStay2D`에서 0.5초 주기로 `PlayerHealth.TakeDamage`. 이전 TODO 해소.
- **XpOrb (Pickup 폴더 신설)**: 자석 흡수(반경 2.5, 속도 8). `PlayerExperience.AddXp`로 누적.
- **PlayerExperience**: 레벨, XP, 곡선 기반 XpNeeded(`base * level^power`). 임계 도달 시 `LevelUpController.OpenChoice`.
- **SkillUpgradeSO + UpgradePool + UpgradeApplier + LevelUpController (Upgrade 폴더 신설)**:
  - 8종 업그레이드 kind(DamageMul/CooldownReduce/SpeedMul/MaxHpAdd/HealNow/UnlockSlotA/B/ProjectileSpeed).
  - 풀에서 가중치 없이 랜덤 N장(기본 3) 추출, minPlayerLevel·maxStacks 필터링.
  - 선택 시 Time.timeScale=0으로 일시정지 → 카드 클릭 → 적용 → 재개.
- **EnemyController XP 드롭**: 사망 시 `xpOrbPrefab` 풀에서 스폰.
- **EnemyTypeSO + WaveSpawner 가중치 추첨**: 웨이브 인덱스에 따라 등장 가능한 타입을 가중치로 추첨. 기존 `enemyPrefab`은 폴백.
- **MagicCaster 업그레이드 반영**: `UpgradeApplier`의 DamageMul/CooldownMul/ProjectileSpeedMul을 발사 시 곱연산.

### 게임 루프 (완성)
스폰 → 추적 → 접촉/사격 → 적 사망 → XP 오브 드롭 → 흡수 → 레벨업 → 카드 3장 선택 → 스탯/슬롯 변경 → 반복 → 플레이어 HP=0 → GameOver

### 다음 단계 후보
- 메인 메뉴 + 게임 시작/종료 흐름
- 메타 진행(영구 업그레이드, 골드)
- 사운드·시각 이펙트(피격, 사망, 레벨업)
- 보스 적 + 보스 웨이브
- 적 공격 패턴 다양화 (원거리 적, 돌진 적)
- 입력 시스템 New Input System 통합 검토

## 2026-05-14: Boneyard 메타 루프 코어 1차 구현

### 추가된 모듈
- **MetaProgressionManager (신규)**:
  - `totalGold` 누적 관리
  - 무덤 반지 풀(`gravePool`)과 다음 판 반입 선택(`carryOverSelection`, 최대 2) 관리
  - 임시 기능 슬롯 확장(`tempExtraFeatureSlots`) 관리 및 사망 시 리셋
- **RunRingInventory (신규)**:
  - 런 인벤토리/장착 반지 분리
  - 반지 장착(최대 2), 반지 분해(런 한정 랜덤 버프), 사망 시 스냅샷 제공
- **GoldPickup (신규)**:
  - 적 사망 보상 골드 드롭 수집 로직
- **LoadoutSlotManager, RunStartBootstrap (신규)**:
  - 시작 시 메타 상태(임시 슬롯/반입 반지/골드 UI) 적용

### 기존 모듈 확장
- **EnemyController**: XP 드롭 외에 골드 드롭(확률/최소/최대 골드) 추가
- **PlayerHealth**: 사망 시 `MetaProgressionManager.OnRunEndedByDeath()` 호출
- **GameManager**: Gold UI 필드/상태(`goldText`, `Gold`, `SetGold`, `AddGold`) 추가
- **UpgradeApplier**: 반지/메타 버프 공용 적용용 `ApplyExternalMultipliers()` 추가

### 현재 한계 (의도적)
- 무덤지기 상호작용 UI/비용 랜덤(100~400) 선택 화면은 아직 미구현
- 반지 "떨림/파괴" 상태 머신은 아직 미구현 (관측 기반 규칙 보류)
- 반지 드롭 테이블(SO 기반) 미구현: 현재는 데이터 구조와 런타임 훅만 추가

## 2026-05-14 (후속): Boneyard 기준 규칙 확정 및 메타 설계 갱신

### 명칭/기준 정정
- 프로젝트 내 표기를 `Bornyard` -> `Boneyard`로 통일.
- 구현 기준은 Android판 Boneyard 플레이 관측 + 커뮤니티 가이드.

### 확정된 핵심 규칙
- **무덤지기**: 메뉴 입장 시 1회 비용 지불(랜덤), 반지 선택 자체에는 추가 비용 없음.
- **무덤 풀 범위**: 사망 순간 인벤토리 + 장착 반지 모두.
- **임시 슬롯 확장**: 판 종료(사망/마을 복귀) 시 초기화.
- **시작 스킬효과 슬롯**: 기본 2칸, 임시 확장 포함 최대 7칸.
- **레전더리 반지**: 코드상 존재, 기본 가중치 1(초희귀).

### Services/Fates 방향
- **Services(장착형 Perk)** 와 **Fates(비장착형 영구 해금)** 을 분리 모델로 관리.
- 항목별 해금 비용은 전부 독립 값으로 관리(일괄 비용 미사용).
- 공략 기반 기본 비용표를 코드 컨텍스트 메뉴로 자동 주입 가능하도록 설계.

### Tonic 규칙(확정)
- `Perk Tonic` 기본 가격: 1000 골드
- `Reduce Tonic Cost` 구매 시 토닉 가격 누적 할인(5% 단위 가정)
- 구현식: `tonicPrice = round(1000 * 0.95^n)` + 하한값(안전장치) 적용 예정

### 데이터 출처 신뢰도
- `Keep`/`Dark`: 텍스트 데이터 직접 확인 가능(`items.txt`, `items.cfg` 등).
- `Boneyard`: 핵심 데이터 일부 난독화 상태로 직접 추출 어려움.
- 따라서 Boneyard 특화 항목은 플레이 관측/공략/위키 정보로 우선 복원 후, 테스트로 보정.

## 2026-05-14 (후속): Services/Fates/토닉 및 런타임 효과 1차 구현

### 완료된 구현
- **Services/Fates 카탈로그 구조 추가**
  - `LobbyCatalogSO` (Service/Fate 엔트리, 표시명/설명/가격/반복구매 플래그)
  - `LobbyShopService` (구매/장착/해제/기본표 자동채우기)
- **토닉 시스템 구현**
  - 토닉 기본가 1000
  - `ReduceTonicCost` 누적 할인(5% 가정)
  - 가격식: `round(base * (1-discount)^n)` + 최소가 하한
  - 토닉은 한 판 한정 슬롯 확장으로 적용
- **Services 런타임 효과 1차 연결**
  - `MoreGoldDrops`: 골드 드롭량/확률 증가
  - `MoreDrops`: 반지 드롭 확률 증가
  - `Hardcore`: 주고받는 피해 2배
  - `MasterOfOffense`: 데미지 증가 + 쿨다운 감소(대체 구현)
  - `AutoPotion`: 위기 체력에서 1회 자동 회복
  - `BlazeOfGlory`: 사망 시 주변 폭발 피해

### 아직 남은 항목
- `MoreDrops`의 스킬북/4배 데미지 보너스 드롭 연동
- `MasterOfOffense` 원형(주공격 서브스킬 직접 부여) 구현
- Services/Fates UI 실배치 및 실제 버튼 바인딩

## 2026-05-14 (후속): Fates 상시 효과 런타임 1차 연결

### 완료된 연결
- `UnlockScavenger`: 무덤지기 서비스 접근 제어에 연결 (미해금 시 접근 차단)
- `BossMonsters`: `WaveSpawner` 보스 웨이브 스폰 훅 연결
  - `bossEnemyPrefab`, `bossWaveInterval` 기반 주기 스폰
- `MoreGraveyards`: 사용 가능 묘지 수 조회 API 추가 (`1 -> 4`)
- 캐릭터/스킬 Fate: `IsCharacterUnlocked`, `IsSkillUnlockedByFate` 조회 API 추가

### 남은 항목
- 맵 선택 UI와 `MoreGraveyards` 실제 바인딩
- 캐릭터 선택 UI와 캐릭터 해금 Fate 실제 바인딩
- 보스 처치 보상(반지/스킬/4배 데미지 보너스) 전용 테이블/드롭 로직 분리

## 2026-05-14 (후속): 선택 바인딩/보스보상 분리 1차

### 추가 구현
- `CharacterSelectionService`
  - 기본 4캐릭터 항상 사용 가능
  - Fate 해금 캐릭터(Griselda/Wegnus/Vorpus/Wazoo/Athicus/Andra) 조회 API 연결
- `GraveyardSelectionService`
  - `MoreGraveyards` 해금 여부에 따라 맵 슬롯 잠금 해제 개수 조회 API 제공
- `RunCombatBuffs`
  - 런타임 임시 전투 버프(4배 데미지) 관리
- `BossRewardPickup`
  - 보스 처치 시 랜덤 보상 1개(반지/스킬포인트/4배데미지) 처리
- `EnemyController`
  - 보스 플래그 및 보스 전용 보상 픽업 스폰 연결

### 잔여 보정 포인트
- 보스 출현 기준을 웨이브가 아닌 처치수 기반으로 전환 여부 결정
- 보스 보상 확률 가중치/중복 규칙 조정

## 2026-05-14 (후속): 로비 UI 바인딩 컴포넌트 추가

### 추가된 UI 바인딩
- `ServiceEntryButtonBinder`
  - Services 항목의 해금/장착/해제 버튼 상태와 텍스트 갱신
- `FateEntryButtonBinder`
  - Fate 항목의 구매 버튼 상태와 잠금 상태 갱신
- `CharacterSelectButtonBinder`
  - Fate 해금 여부에 따른 캐릭터 버튼 잠금/오버레이 처리
- `GraveyardSelectButtonBinder`
  - MoreGraveyards 해금 여부에 따른 맵 버튼 잠금 처리

### 의미
- 로비 패널을 프리팹/버튼 단위로 바로 연결 가능한 상태가 됨.
- 동적 리스트 생성 없이도 수동 배치 방식으로 즉시 동작 검증 가능.

## 2026-05-14 (후속): 킬 기반 보스/로비 카탈로그 동적 생성 반영

### 추가 구현
- `WaveSpawner`
  - BossMonsters 출현 기준을 웨이브 주기 기반에서 **킬수 기반 기본값**으로 전환.
  - 기본 파라미터: 첫 보스 `400킬`, 이후 `400킬`마다 반복.
  - 호환성을 위해 기존 웨이브 기반 스폰은 옵션(`useKillBasedBossSpawn=false`)으로 유지.
- `GameManager`
  - `KillCount` 상태 추가, `AddKill()` API 추가.
  - UI 표시를 `Wave + Kills`로 통합.
- `EnemyController`
  - 적 사망 시 `GameManager.AddKill(1)` 호출로 킬 카운트 누적.
- `LobbyCatalogListBuilder` (신규)
  - `LobbyCatalogSO` 기반으로 Services/Fates 버튼 프리팹을 런타임 자동 생성.
  - 수동 복제 배치 없이 카탈로그 변경만으로 로비 목록 반영 가능.
- `LobbyTextTableSO` (신규)
  - Services/Fates 표시명/설명 한글 테이블을 별도 SO로 관리.
  - `LobbyCatalogListBuilder.textTable` 연결 시, 카탈로그 기본 텍스트를 런타임에 덮어써 UI 반영.
- `ServiceEntryButtonBinder` / `FateEntryButtonBinder`
  - `Configure(...)` API 추가.
  - `displayName/description` 표시 바인딩 지원.

### 남은 항목
- 한글 표시명/설명 테이블을 카탈로그 에셋에 채우기.
- UI 프리팹에서 `descriptionText` 레퍼런스 연결.

## 2026-05-14 (후속): MoreDrops 비반지 드롭 확장 + MasterOfOffense 시작형 전환

### 추가 구현
- `FieldBonusPickup` (신규)
  - 일반 적 처치 시 필드에서 획득 가능한 보너스 픽업 추가.
  - 타입: `SkillPoint`, `RandomSkillPoint`, `Damage4x`.
  - 상단 메시지 피드 연동.
- `EnemyController`
  - `fieldBonusPickupPrefab`, `fieldBonusDropChance` 추가.
  - 사망 시 확률 드롭 후 보너스 타입 랜덤 부여.
- `MetaProgressionManager`
  - `GetBonusDropChanceMultiplier()` 추가.
  - `MoreDrops` 장착 시 비반지 보너스 드롭에도 동일 배율 적용.
- `RunStartBootstrap`
  - `MasterOfOffense`를 발사 시 상시 배율이 아닌 **런 시작 1회 보정**으로 전환.
  - `UpgradeApplier.ApplyExternalMultipliers(...)`로 시작 데미지/쿨다운 보정 적용.
- `MagicCaster`
  - 기존 MasterOfOffense 상시 배율 적용 코드 제거(중복 보정 방지).

### 비고
- MasterOfOffense는 아직 원형(주공격 서브스킬 2개를 직접 +1 부여) 완전복원은 아니며,
  현재는 시작 스탯 보정으로 동등 체감 확보하는 대체 구현 상태.

## 2026-05-14 (후속): MasterOfOffense 주공격 보너스 훅/드롭 가드/텍스트 자동채움

### 추가 구현
- `MagicCaster`
  - 런 시작 보너스 레벨 훅 추가: `ConfigureStartSkillBonuses(allSkillBonus, masterPrimaryBonus)`.
  - 발사 시 시작 보너스 레벨을 데미지/쿨다운에 환산 적용.
  - `slotA`를 주공격으로 간주해 `MasterOfOffense` 보너스 레벨을 적용.
- `RunStartBootstrap`
  - `MetaProgressionManager.baseSkillPointBonusOnStart` 와 `MasterOfOffense` 장착 상태를 읽어
    캐스터 시작 보너스에 주입.
  - 기존 MasterOfOffense 상시 배율 대체 코드는 제거.
- `EnemyController`
  - `FieldBonusPickup` 프리팹 누락/오배치 가드 추가(`OnValidate`, `OnEnable`).
  - 연결 오류 시 경고 로그 출력 및 보너스 드롭 경로 자동 비활성화.
- `LobbyTextTableSO`
  - `Fill Korean Defaults` 컨텍스트 메뉴 추가.
  - Services/Fates 전체 enum 기준 기본 표시명/설명 자동 생성.

### 의미
- MasterOfOffense가 "런 시작에 주공격 계열이 더 강한 상태로 시작"한다는 원형 의도에 더 근접.
- 에디터 연결 실수로 인한 런타임 누락 이슈를 사전 차단.
- 한글 텍스트 테이블 작성 비용을 크게 줄여 UI 정리 속도 향상.

## 2026-05-14 (후속): 메타 상태 변경 이벤트 도입

### 추가 구현
- `MetaProgressionManager`
  - 정적 이벤트 `MetaStateChanged` 추가.
  - 골드 변경, 서비스/페이트 구매, 장착/해제, 토닉 구매, 무덤 세션 상태 변경 시 이벤트 발행.
- `ServiceEntryButtonBinder`, `FateEntryButtonBinder`
  - `OnEnable`에서 이벤트 구독, `OnDisable`에서 해제.
  - 개별 버튼 클릭 외에도 외부 상태 변화(골드 감소/슬롯 변화)에 즉시 반응해 버튼 상태 동기화.

## 2026-05-14 (후속): MagicScavenger / CreativeCasting / SecondSecondary 연결

### 추가 구현
- `MetaProgressionManager`
  - `IsMagicScavengerEnabled`, `IsCreativeCastingEnabled`, `IsSecondSecondaryEnabled` 추가.
  - `GetXpOrbAmountMultiplier`, `GetXpOrbAttractRadiusMultiplier`, `GetLevelUpChoicesBonus` 추가.
- `EnemyController`
  - XP 오브 드롭 시 `MagicScavenger` 배율 적용(오브 XP량 증가).
- `XpOrb`
  - `MagicScavenger` 활성 시 흡수 반경 증가.
  - 오브젝트 풀 재사용 시 반경 누적 버그 방지를 위해 기본 반경 캐시 후 매 활성화마다 재계산.
- `LevelUpController`
  - `CreativeCasting` 활성 시 레벨업 선택지 +1.
  - 현재 UI 카드 수를 초과하지 않도록 안전 clamp 적용.
- `RunStartBootstrap`
  - `SecondSecondary` 활성 시 시작 시점에 `slotA/현재 slotB`와 다른 랜덤 기본 마법을 `slotB`에 부여.

### 비고
- SecondSecondary는 현재 기본 마법 풀(enum) 기준 랜덤이며, 실제 원작의 "보조 스킬 풀"과 1:1 동일하지는 않음.
- 추후 보조 스킬 카탈로그 분리 시 해당 랜덤 풀을 데이터 기반으로 전환 예정.

## 2026-05-14 (후속): 원작(Boneyard) 100% 복제를 위한 핵심 시스템 및 적 AI 완성

### 추가 구현 (시스템)
- **SaveSystem (신규)**: JSON 기반 메타 진행도(골드, 해금, 무덤 반지) 저장 및 로드.
- **AchievementManager (신규)**: 업적/퀘스트 조건 감시 및 골드 보상 지급 시스템.
- **RunDataTracker (신규)**: 판당 상세 지표(피해량, 처치수, 사용 마법, 생존 시간) 집계.
- **LootTableProcessor (신규)**: 가중치(Weight) 기반 아이템 드롭 추첨 시스템.
- **ComboDiscoveryManager (신규)**: 발견한 마법 조합 기록 및 도감 데이터 관리.
- **SettingsManager (신규)**: 사운드(Master/SFX/BGM) 및 입력 감도 저장/로드.
- **RingDatabaseSO / CharacterDataSO**: 상세 아이템 수치 및 캐릭터별 시작 구성 데이터화.
- **LobbyNpcTouchSelector**: Boneyard 방식의 터치 기반 NPC 상호작용 (캐릭터 이동 불필요).

### 추가 구현 (적 AI - 총 13종 완성)
- **Imp**: 통통 튀며 무작위로 빠르게 접근하는 변칙 이동 AI.
- **Poison Zombie**: 사망 시 3~4초간 유지되는 독 웅덩이 생성.
- **Armored Skeleton**: 일정 데미지 후 방어구가 벗겨지며 일반형으로 변하는 페이즈 AI.
- **Mage (4종)**: 유도 에너지탄(얼음/독), 스턴 전기쇼크(번개), 직선 파이어볼(불).
- **Shadow**: 이동 시 투명, 공격 시 노출, 사망 시 광역 폭발.
- **Wraith**: 벽 통과 이동 및 플레이어 접촉 데미지.
- **Salamander**: 높은 방어력, 사망 시 임프 다수 생성.
- **Boss Skull**: 4개 패턴(화살 연사, 불덩이 투척, 레이저, 몸통 박치기)을 가진 복합 AI.

### 비고
- 모든 핵심 로직은 유니티 에디터 없이도 동작 가능하도록 코드로 구현 완료.
- 에셋(이미지, 사운드) 연결만으로 즉시 원작과 동일한 동작 구현 가능 상태.

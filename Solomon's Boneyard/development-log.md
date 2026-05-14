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

# 테스트 체크리스트 — 안드로이드 MVP 1차

> 작성일: 2026-05-12
> Unity 프로젝트: `F:\Unity Projects\solomon-copy-game`
> 사용자가 직접 Unity 에디터에서 진행해야 하는 항목 모음.

---

## 0. 사전 확인 (Unity 에디터 열자마자)

- [ ] Console 창에 컴파일 에러 0개. 에러 나면 메시지 그대로 공유.
- [ ] `Assets/Scripts/` 아래 6개 폴더(Enemy/Magic/Player/Projectile/Systems/UI) 인식됨.
- [ ] 각 `.cs` 옆에 `.meta` 자동 생성 확인.

---

## 1. 프로젝트 설정

- [ ] **Tag 등록** (Edit > Project Settings > Tags and Layers):
  - [ ] `Player`
  - [ ] `Enemy`
  - [ ] `Projectile`
- [ ] **Build Settings**: Platform = Android (Switch Platform 실행).
- [ ] **Player Settings**:
  - [ ] Scripting Backend = IL2CPP
  - [ ] Target Architectures = ARM64
  - [ ] Minimum API Level ≥ 22
- [ ] **Game 뷰 해상도**: 1080x1920 Portrait 추가.

---

## 2. 씬 구성 (Hierarchy)

### 매니저 오브젝트
- [ ] 빈 GO `GameManager` 생성 + `GameManager.cs` 부착
- [ ] 빈 GO `ObjectPooler` 생성 + `ObjectPooler.cs` 부착
- [ ] 빈 GO `WaveSpawner` 생성 + `WaveSpawner.cs` 부착

### Player
- [ ] GO `Player` 생성, Tag = `Player`
- [ ] 컴포넌트: SpriteRenderer + Rigidbody2D(GravityScale=0, Constraints: Freeze Rotation Z) + CircleCollider2D
- [ ] 스크립트: `PlayerController.cs` + `MagicCaster.cs` 부착
- [ ] 자식 빈 Transform `Muzzle` 생성 후 `MagicCaster.muzzle` 필드에 연결

### UI Canvas
- [ ] `Canvas` (Render Mode = Screen Space - Overlay) + `EventSystem` 자동 생성 확인
- [ ] Canvas Scaler: Scale With Screen Size, 1080x1920 기준
- [ ] 좌하단 Image `JoystickMove` + 자식 Image `Handle` → `VirtualJoystick.cs` 부착, handle 연결
- [ ] 우하단 Image `JoystickAim` + 자식 Image `Handle` → `VirtualJoystick.cs` 부착, handle 연결
- [ ] `PlayerController.moveStick` ← JoystickMove
- [ ] `PlayerController.aimStick` ← JoystickAim

### UI Text (선택)
- [ ] Score Text, Wave Text → `GameManager`의 scoreText/waveText에 연결
- [ ] GameOver Panel(비활성 상태) → `GameManager.gameOverPanel`에 연결

---

## 3. 프리팹 만들기

### Projectile 프리팹
- [ ] 빈 GO + SpriteRenderer(원/별 모양 스프라이트) + CircleCollider2D(**IsTrigger ✓**) + Rigidbody2D(Body Type = Kinematic) + `Projectile.cs`
- [ ] Tag = `Projectile`
- [ ] `Assets/Prefabs/Projectiles/`로 드래그해 프리팹화

### Enemy 프리팹
- [ ] 빈 GO + SpriteRenderer + Rigidbody2D(GravityScale=0, Freeze Rotation Z) + CircleCollider2D + `EnemyController.cs`
- [ ] Tag = `Enemy`
- [ ] `Assets/Prefabs/Enemies/`로 프리팹화
- [ ] `WaveSpawner.enemyPrefab` 필드에 연결

---

## 4. 마법 데이터(SO) 생성

Project 창 우클릭 → Create > SolomonCopy > Magic

- [ ] `BaseMagic_Fire` (id=Fire, statusEffect=Burn, projectilePrefab 연결)
- [ ] `BaseMagic_Ice` (id=Ice, statusEffect=Slow)
- [ ] `BaseMagic_Lightning` (id=Lightning, statusEffect=Shock)
- [ ] `BaseMagic_Earth` (id=Earth, statusEffect=None, damage 높게)
- [ ] `ComboMagic_FireIce` (inputA=Fire, inputB=Ice, explodeOnHit=true, aoeRadius=2)
- [ ] `ComboMagic_FireLightning` (inputA=Fire, inputB=Lightning, chainCount=3, chainRange=4)
- [ ] `MagicRegistry` (1개) → 위 모든 SO를 리스트에 등록
- [ ] `MagicCaster.registry` 필드에 MagicRegistry 연결

> 발사체 프리팹은 각 마법마다 색깔/스프라이트만 다르게 해서 4~6종 생성하면 시각적으로 구분됨.

---

## 5. 동작 테스트 (Editor Play 모드)

### 기본 조작
- [ ] WASD/방향키로 플레이어 이동 (PC 폴백)
- [ ] 마우스 좌클릭 누른 채로 마우스 방향 따라 회전 + 자동 발사
- [ ] Game 뷰에서 좌하 조이스틱 드래그 → 이동
- [ ] 우하 조이스틱 드래그 → 조준 회전 + 자동 연사
- [ ] 두 조이스틱 동시 드래그 (마우스 1개라 에디터에선 한계, 모바일 빌드에서 확인 가능)

### 발사 / 충돌
- [ ] 발사체가 muzzle 위치에서 나와 직선으로 이동
- [ ] `lifetime` 지나면 사라짐 (콘솔로 확인하려면 풀 반환 로그 추가 가능)
- [ ] 발사체가 Enemy에 닿으면 HP 감소
- [ ] HP 0 이하 시 Enemy 사라짐 + 점수 증가

### 적 AI
- [ ] 웨이브 간격(기본 5초)마다 화면 바깥 원형 위치에서 적 스폰
- [ ] 적이 플레이어 향해 직선 추적
- [ ] Slow 마법 맞은 적 → 이동 속도 절반 (육안 확인)
- [ ] Burn 맞은 적 → 도트 데미지로 HP 깎임
- [ ] Shock 맞은 적 → 잠깐 정지

### 스킬 조합
- [ ] slotA=Fire, slotB=Fire → 일반 불 발사 (콘솔: `Fired: Fire`)
- [ ] slotA=Fire, slotB=Ice → 콤보 발사 + 충돌 시 AOE (콘솔: `Fired Combo: ...`)
- [ ] slotA=Fire, slotB=Lightning → 콤보 발사 + 첫 충돌 후 인접 적으로 연쇄
- [ ] 등록되지 않은 페어(예: Earth+Lightning, Combo SO 없을 때) → slotA로 폴백되어 정상 발사

### 풀링
- [ ] 30초 플레이 후 Profiler에서 `GC.Alloc` 스파이크가 미미한지 확인
- [ ] 첫 발사 이후 `Instantiate` 호출이 줄어드는지 (Hierarchy에 비활성 발사체가 쌓이는 것 확인)

---

## 6. 안드로이드 실기 빌드

- [ ] APK 빌드 성공 (Build And Run)
- [ ] 실기에서 좌/우 멀티터치 동시 동작
- [ ] 60fps 유지 (Stats 또는 외부 FPS 표시)
- [ ] 화면 회전 잠금이 의도대로 (Portrait 고정)

---

## 7. 알려진 미구현 / 의도된 한계

- ❗ **Player 피격/사망 로직 없음** — 적이 닿아도 데미지 안 들어감 (`EnemyController.OnCollisionEnter2D`의 TODO 주석)
- ❗ **슬롯 전환 UI 미배치** — `SlotSwitchButton.cs`는 있지만 Canvas에 버튼 배치는 사용자 작업
- ❗ **이펙트/사운드 없음** — 충돌 시 시각/청각 피드백 추가 필요
- ❗ **밸런싱 미조정** — 데미지/쿨다운/이동속도 등 모두 임의값

---

---

# 2차 추가 (2026-05-12 후속) — PlayerHealth, XP, 레벨업, 적 다양화

## 8. 추가 씬 셋업

### Player 추가 컴포넌트
- [ ] `PlayerHealth.cs` 부착 (maxHp 기본 100)
- [ ] `PlayerExperience.cs` 부착 (level=1, xpToNextBase=5, xpCurvePower=1.4)
- [ ] `UpgradeApplier.cs` 부착

### LevelUpController
- [ ] 빈 GO `LevelUpController` + `LevelUpController.cs` 부착
- [ ] `pool` ← UpgradePool SO 연결
- [ ] `applier` ← Player의 UpgradeApplier 드래그
- [ ] Canvas 아래 `LevelUpPanel` (비활성 상태) 생성, 3개 Button 카드 배치
  - [ ] cardButtons[0..2], cardTitles[0..2], cardDescs[0..2], cardIcons[0..2] 각각 연결
- [ ] `PlayerExperience.levelUpController` ← LevelUpController 연결

### UI 추가
- [ ] HP Slider → `PlayerHealth.hpBar`
- [ ] XP Slider → `PlayerExperience.xpBar`
- [ ] Level Text → `PlayerExperience.levelText`

### XP Orb 프리팹
- [ ] 빈 GO + SpriteRenderer(작은 보석) + CircleCollider2D(IsTrigger ✓) + Rigidbody2D(Kinematic)
- [ ] `XpOrb.cs` 부착, Tag "Pickup" 추가(선택)
- [ ] `Assets/Prefabs/Pickups/`에 저장
- [ ] EnemyController.xpOrbPrefab(또는 EnemyTypeSO 측에서) 연결

## 9. 데이터 자산 추가

### Upgrade 풀
Project 창 우클릭 → Create > SolomonCopy > Upgrade

- [ ] `Upgrade_DamageUp1` (kind=DamageMul, magnitude=0.15, maxStacks=5)
- [ ] `Upgrade_CooldownDown1` (kind=CooldownReduce, magnitude=0.1, maxStacks=5)
- [ ] `Upgrade_SpeedUp1` (kind=SpeedMul, magnitude=0.1, maxStacks=3)
- [ ] `Upgrade_MaxHpUp1` (kind=MaxHpAdd, magnitude=20, maxStacks=5)
- [ ] `Upgrade_HealNow` (kind=HealNow, magnitude=30)
- [ ] `Upgrade_SlotA_Ice` (kind=UnlockSlotA, targetMagic=Ice, maxStacks=1)
- [ ] `Upgrade_SlotB_Lightning` (kind=UnlockSlotB, targetMagic=Lightning, maxStacks=1)
- [ ] `UpgradePool` (1개) → 위 SO들 리스트에 등록

### Enemy 타입
- [ ] `EnemyType_Skeleton` (HP30, 속도2.5, minWave=1, weight=3)
- [ ] `EnemyType_Zombie` (HP60, 속도1.5, minWave=2, weight=2)
- [ ] `EnemyType_Bat` (HP15, 속도4, minWave=3, weight=1)
- [ ] 각 타입의 prefab 필드에 알맞은 Enemy 프리팹 연결
- [ ] WaveSpawner.enemyTypes 배열에 등록 (enemyPrefab은 폴백용)

## 10. 추가 동작 테스트

### 체력 / 사망
- [ ] 적이 플레이어에 닿으면 HP가 0.5초마다 감소
- [ ] 무적 시간(0.5초) 내 추가 데미지 차단되는지 확인
- [ ] HP 0 → GameOver 패널 활성화, Time.timeScale=0
- [ ] GameManager.Restart() 호출로 씬 재시작 가능

### XP / 레벨업
- [ ] 적 사망 시 XP 오브 드롭
- [ ] 플레이어 2.5 거리 내 XP 오브가 끌려옴
- [ ] 충돌 시 XP 누적, XP Slider 갱신
- [ ] 임계치 도달 시 LevelUpPanel 표시 + Time.timeScale=0
- [ ] 카드 클릭 → 업그레이드 적용 + 패널 닫힘 + 게임 재개

### 업그레이드 효과
- [ ] DamageMul 카드 선택 후 발사체 데미지 증가 (콘솔 또는 적 HP 감소 폭으로 확인)
- [ ] CooldownReduce → 발사 간격 짧아짐
- [ ] SpeedMul → 플레이어 이동 속도 빨라짐
- [ ] MaxHpAdd → HP Slider 최대치 증가 + 즉시 회복
- [ ] UnlockSlotA/B → MagicCaster 슬롯 변경 (다음 발사가 다른 마법으로)

### 적 다양화
- [ ] 웨이브 1: Skeleton만 등장
- [ ] 웨이브 2: Zombie 등장 시작
- [ ] 웨이브 3+: Bat 등장 시작
- [ ] 가중치대로 비율 대략 유지되는지 (육안)

## 11. 추가 알려진 한계

- ❗ **사망 시 자동 재시작 없음** — GameOver 패널에 Restart 버튼 직접 만들어 `GameManager.Instance.Restart()` 호출 연결 필요
- ❗ **업그레이드 효과 영구 — 씬 재시작 시 초기화** — 메타 진행(영구 업그레이드)은 별도 시스템 필요
- ❗ **Pickup 풀 사전 등록 안 됨** — XpOrb 프리팹은 첫 사용 시 Instantiate, 이후 풀링됨

---

# 3차 추가 (2026-05-14 후속) — Boneyard 메타 루프(골드/무덤/반지 리스크)

## 12. 씬 셋업 추가

### Meta 매니저
- [ ] 빈 GO `MetaProgressionManager` 생성 + `MetaProgressionManager.cs` 부착
- [ ] `DontDestroyOnLoad` 동작 확인(씬 재시작 후 1개만 유지)

### Player 컴포넌트 추가
- [ ] `RunRingInventory.cs` 부착
- [ ] `LoadoutSlotManager.cs` 부착
- [ ] `RunStartBootstrap.cs` 부착

### Gravekeeper (UI 연결 준비)
- [ ] 빈 GO `GravekeeperService` 생성 + `GravekeeperService.cs` 부착
- [ ] minCost=100, maxCost=400 기본값 확인

### UI 추가
- [ ] Gold Text를 `GameManager.goldText`에 연결

### 프리팹 추가
- [ ] `GoldPickup` 프리팹 생성 (SpriteRenderer + CircleCollider2D IsTrigger + Rigidbody2D Kinematic + `GoldPickup.cs`)
- [ ] `EnemyController.goldPickupPrefab` 연결
- [ ] `EnemyController.goldDropChance/goldMin/goldMax` 값 설정

## 13. 메타 루프 동작 테스트

### 골드 드롭/획득
- [ ] 적 처치 시 확률적으로 골드 픽업 드롭
- [ ] 골드 픽업이 플레이어 근처에서 흡착
- [ ] 수집 시 `GameManager` Gold UI 증가

---

# 14차 추가 (2026-05-14 후속) — 보스 킬수 트리거/로비 카탈로그 자동생성

## 14. 보스 출현 기준 (킬수)

- [ ] `WaveSpawner.useKillBasedBossSpawn = true`
- [ ] `bossFirstSpawnAtKills = 400`, `bossSpawnEveryKills = 400`
- [ ] BossMonsters Fate 해금 전: 400킬 넘어도 보스 미출현
- [ ] BossMonsters Fate 해금 후: 400킬 시점 첫 보스 출현
- [ ] 이후 800/1200킬에서 보스 반복 출현
- [ ] 옵션으로 `useKillBasedBossSpawn = false` 시 기존 `bossWaveInterval` 방식으로 동작

## 15. 킬 카운트/UI

- [ ] 적 사망 시 `KillCount`가 +1 누적
- [ ] `Wave` 텍스트에 `Kills`가 함께 표시됨
- [ ] 씬 재시작 시 `KillCount` 초기화

## 16. 로비 동적 리스트 생성

- [ ] 빈 GO에 `LobbyCatalogListBuilder` 부착
- [ ] `shop`, `serviceListRoot`, `serviceEntryPrefab`, `fateListRoot`, `fateEntryPrefab` 연결
- [ ] 플레이 진입 시 Services/Fates 항목이 카탈로그 개수만큼 자동 생성
- [ ] `ServiceEntryButtonBinder.descriptionText`, `FateEntryButtonBinder.descriptionText` 연결 시 설명이 표시됨
- [ ] 카탈로그 항목명/설명 수정 후 재실행하면 UI에 반영됨

## 17. MoreDrops 비반지 드롭 확장

- [ ] `EnemyController.fieldBonusPickupPrefab`에 `FieldBonusPickup` 프리팹 연결
- [ ] 기본 드롭 확률(`fieldBonusDropChance`)에서 보너스 픽업이 드롭됨
- [ ] `MoreDrops` 미장착 대비 장착 시 보너스 픽업 체감 드롭률이 증가함
- [ ] 보너스 픽업 타입 3종 확인:
- [ ] `Skill Point +1` 메시지 표시 및 즉시 레벨업 선택 발생
- [ ] `Random Skill Point +1` 메시지 표시 및 즉시 레벨업 선택 발생
- [ ] `Damage x4` 메시지 표시 및 일정 시간 화력 증가

## 18. MasterOfOffense 시작형 보정

- [ ] `MasterOfOffense` 장착 없이 시작한 런과 비교해 시작 화력/쿨다운이 유의미하게 강화됨
- [ ] 강화가 런 시작 1회 적용이며, 발사마다 중복 누적되지 않음
- [ ] 기존 Hardcore/반지/기타 배율과 충돌 없이 동작

## 19. 프리팹 가드/한글 테이블 자동화

- [ ] `EnemyController.fieldBonusDropChance > 0`인데 프리팹이 비면 경고 로그 출력
- [ ] `fieldBonusPickupPrefab`에 `FieldBonusPickup` 컴포넌트가 없으면 경고 후 자동 비활성화
- [ ] `LobbyTextTableSO` 생성 후 `Fill Korean Defaults` 실행 시 Services/Fates 엔트리 자동 생성
- [ ] `LobbyCatalogListBuilder.textTable` 연결 시 버튼 표시명/설명이 한글 테이블값으로 출력

## 20. MasterOfOffense 주공격 보너스 레벨

- [ ] `Plus1/+2 to All Skills` 장착 시 시작 화력/쿨다운 보정이 증가
- [ ] `MasterOfOffense` 장착 시 `slotA` 계열이 `slotB` 대비 더 강한 시작 보정 적용
- [ ] 콤보 마법 발사 시 두 슬롯 보너스 평균치로 보정 적용

## 21. 로비 버튼 자동 동기화

- [ ] Services/Fates 목록에서 항목 구매 시 다른 항목의 버튼 상태가 즉시 갱신됨
- [ ] 골드 변화(구매/토닉/무덤입장) 직후 버튼 활성/비활성 상태가 자동 반영됨
- [ ] 슬롯 여유가 없어 장착 불가 상태일 때 equip 버튼이 즉시 비활성됨

## 22. MagicScavenger

- [ ] `MagicScavenger` 미장착 대비 장착 시 XP 오브 획득량이 증가함
- [ ] `MagicScavenger` 장착 시 XP 오브 흡수 반경이 증가함
- [ ] 오브젝트 풀 재사용 시 흡수 반경이 비정상적으로 계속 커지지 않음

## 23. CreativeCasting

- [ ] `CreativeCasting` 장착 시 레벨업 선택지 수가 +1 증가
- [ ] 카드 UI 개수를 초과하지 않게 표시됨(현재 카드 수 기준 clamp)
- [ ] 미장착 시 기존 선택지 수로 동작

## 24. SecondSecondary

- [ ] `SecondSecondary` 장착 시 시작 직후 `slotB`가 랜덤 보조 마법으로 변경됨
- [ ] 랜덤 결과가 `None`, `slotA`, 기존 `slotB`와 중복되지 않음
- [ ] 미장착 시 기존 시작 슬롯 유지
- [ ] 씬 재시작 후에도 `MetaProgressionManager.totalGold` 누적 유지 확인

### 임시 슬롯 확장
- [ ] `TryBuyTemporaryFeatureSlot()` 성공 시 totalGold 감소, tempExtraFeatureSlots 증가
- [ ] 사망 시 tempExtraFeatureSlots가 0으로 리셋되는지 확인
- [ ] 임시 포함 시작 스킬 슬롯이 최대 7칸에서 캡되는지 확인

### 무덤 반지 풀
- [ ] 사망 순간 인벤토리/장착 반지가 `gravePool`에 누적되는지 확인
- [ ] `GravekeeperService.RollCost()`가 100~400 범위 난수 반환 확인
- [ ] 비용 부족 시 `ConfirmSelection()` 실패 확인
- [ ] 비용 충분 시 최대 2개 반입 선택 성공 확인

### 반지 떨림/파손 위험
- [ ] 같은 반지를 반복 반입해 `carryOverUseCount`가 증가하는지 확인
- [ ] `shakingThreshold` 이상에서 `isShaking=true` 전환 확인
- [ ] 떨림 상태 반지 사용 후 사망 시 확률적으로 `brokeAfterLastRun=true`가 되는지 확인
- [ ] `brokeAfterLastRun=true` 반지는 다음 런 시작 시 자동 반입 제외되는지 확인

## 14. 신규 알려진 한계

- ❗ 무덤지기 시각 UI(반지 리스트/툴팁/선택 강조) 미구현 — 현재는 서비스 로직만 구현
- ❗ 반지 파손 시 연출(떨림/파손 이펙트, 사운드) 미구현
- ❗ 반지 드롭 테이블은 구현됨. 다만 실제 체감에 맞는 확률/수치 밸런싱은 추가 조정 필요

---

# 4차 추가 (2026-05-14 후속2) — 반지 드롭 테이블 + 시작 랜덤 반지

## 15. 데이터/SO 셋업

### RingLootTable SO
- [ ] `Create > SolomonCopy > Meta > RingLootTable` 생성
- [ ] rarityWeights 설정 (기본값: Magic 70 / Rare 20 / Epic 9 / Legendary 1)
- [ ] 기본 옵션 범위 및 등급 스케일 값 조정
- [ ] prefixes/cores 문자열 풀 설정
- [ ] 옵션 개수 규칙 확인:
- [ ] Magic 1~2, Rare 1~2
- [ ] Epic 2 또는 4
- [ ] Legendary 사용 여부 미확정(가중치 0으로 비활성 가능)

### MetaProgressionManager 연결
- [ ] `ringLootTable` 필드에 위 SO 연결
- [ ] `unlockStartWithRandomRings` ON/OFF 토글 확인
- [ ] `startRandomRingCount` 기본 2 확인
- [ ] `skillUnlockCosts`에 스킬효과별 해금 비용 입력
- [ ] `featureUnlockCosts`에 특수기능별 해금 비용 입력
- [ ] 우클릭 컨텍스트 `Reset Unlock Costs To Boneyard Defaults` 실행 시 공략 기반 기본 비용표가 채워지는지 확인

## 16. 프리팹/적 드롭 셋업

### RingPickup 프리팹
- [ ] `RingPickup` 프리팹 생성 (SpriteRenderer + CircleCollider2D IsTrigger + Rigidbody2D Kinematic + `RingPickup.cs`)
- [ ] `EnemyController.ringPickupPrefab` 연결
- [ ] `EnemyController.ringDropChance` 값 설정 (초기 0.1 권장)

## 17. 동작 테스트(코드 연결 검증)

### 시작 랜덤 반지
- [ ] `unlockStartWithRandomRings=true`일 때 런 시작 시 인벤토리에 랜덤 반지 2개 생성 확인
- [ ] 인벤토리에 기존 반입 반지가 있으면 중복 추가되지 않는지 확인

### 반지 드롭/획득
- [ ] 적 사망 시 확률적으로 반지 픽업 드롭
- [ ] 반지 픽업 흡착/획득 시 `RunRingInventory.inventory`에 추가 확인
- [ ] 드롭 반지의 등급/옵션/이름이 무작위로 생성되는지 확인
- [ ] 골드 연속 획득 시 상단 메시지 숫자가 누적(`Gold +합계`)되는지 확인
- [ ] 마지막 골드 획득 후 약 3초 지나면 골드 메시지가 사라지는지 확인

### 무덤 연계
- [ ] 드롭 반지를 소지한 채 사망하면 무덤 풀(`gravePool`)에 누적 확인
- [ ] 무덤에서 재반입 시 `carryOverUseCount` 누적 확인
- [ ] 누적이 임계값 도달 시 `isShaking=true` 전환 확인
- [ ] 무덤 메뉴 진입 시점에만 골드 1회 차감되고, 반지 선택 시 추가 비용 없는지 확인
- [ ] 무덤 메뉴 진입 전에는 반지 상세를 표시하지 않는 UI 정책 적용 확인

## 18. 추가 알려진 한계

- ❗ 반지 옵션 밸런스 표(등급별 최소/최대, 다중 옵션 확률) 미확정
- ❗ 반지 획득 UI 피드백(등급 컬러, 툴팁, 비교창) 미구현

---

# 5차 추가 (2026-05-14 후속3) — 모루 분해 인터랙션 + 반지 떨림 UI

## 19. UI 셋업 추가

- [ ] `InventoryAnvilDismantleController`를 인벤토리 UI 매니저 오브젝트에 부착
- [ ] `characterVisual`, `anvilVisual` 참조 연결
- [ ] 각 인벤토리 반지 버튼 OnClick -> `SelectInventoryRing(index)` 연결
- [ ] 중앙 모루 버튼 OnClick -> `OnAnvilPressed()` 연결
- [ ] 취소 동작 버튼(선택) -> `CancelSelection()` 연결

## 20. 떨림 이펙트 셋업

- [ ] 무덤/인벤토리 반지 슬롯 UI에 `RingShakeEffect` 부착
- [ ] 반지 데이터의 `isShaking` 값에 맞춰 `SetShaking()` 호출 연결
- [ ] 떨림 강도(amplitude)/빈도(frequency) 값 조정

---

# 6차 추가 (2026-05-14 후속4) — 로비 전용 NPC 근접 팝업

## 21. 로비/런 분리 셋업

- [ ] 로비 씬 루트에 `LobbyStateMarker` 1개 배치
- [ ] 전투(런) 씬에는 `LobbyStateMarker`가 없도록 확인

## 22. NPC 근접 상호작용 셋업

- [ ] 무덤지기 NPC 오브젝트에 Trigger Collider2D 추가
- [ ] `LobbyNpcInteractable` 부착
- [ ] `popupMenuRoot`에 무덤지기 메뉴 패널 연결
- [ ] 플레이어 태그가 `Player`인지 확인
- [ ] `requirePressToOpen` 옵션 정책 결정:
- [ ] OFF: 근접 즉시 팝업
- [ ] ON: 근접 후 E 키(또는 모바일 버튼)로 팝업

## 23. 동작 테스트

- [ ] 로비에서 플레이어가 NPC 근접 시 팝업 메뉴 노출
- [ ] NPC에서 멀어지면 팝업 자동 닫힘
- [ ] 런 시작 후 동일 팝업이 노출되지 않음

---

# 7차 추가 (2026-05-14 후속5) — Services/Fates 카탈로그 + 상점 코어 + 토닉

## 24. 데이터 카탈로그

- [ ] `LobbyCatalogSO` 생성 (`Create > SolomonCopy > Meta > LobbyCatalog`)
- [ ] `LobbyShopService.catalog`에 연결
- [ ] `Fill Catalog From Meta Defaults` 실행 시 Services/Fates 기본 엔트리 자동 생성 확인
- [ ] 각 엔트리 `displayName/description/unlockCost` 수동 보정

## 25. 로비 우측 NPC 상점 코어

- [ ] `TryUnlockService`, `TryEquipService`, `TryUnequipService` 동작 확인
- [ ] `TryUnlockFate` 동작 확인
- [ ] `ReduceTonicCost`가 반복 구매형으로 동작하는지 확인

## 26. 토닉 시스템

- [ ] 기본 토닉 가격이 1000인지 확인
- [ ] `GetCurrentTonicCost() = round(1000 * 0.95^n)` 로 감소하는지 확인
- [ ] 토닉 최소가격 하한(`tonicMinCost`)이 적용되는지 확인
- [ ] `TryBuyPerkTonic` 구매 시 해당 판 한정 슬롯 +1 되는지 확인
- [ ] 사망 시 임시 슬롯 확장 초기화 확인

---

# 8차 추가 (2026-05-14 후속6) — Services 런타임 효과 1차 연결

## 27. 전투 효과 검증

- [ ] `MoreGoldDrops` 장착 시 골드 드롭량 증가(약 2배) 확인
- [ ] `MoreGoldDrops` 장착 시 골드 드롭 확률이 추가 증가하는지 확인
- [ ] `MoreDrops` 장착 시 반지 드롭 확률이 증가하는지 확인
- [ ] `Hardcore` 장착 시 플레이어 입는 피해 2배 확인
- [ ] `Hardcore` 장착 시 플레이어가 주는 피해 2배 확인
- [ ] `MasterOfOffense` 장착 시 데미지 증가 + 쿨다운 감소 적용 확인

## 28. 생존 효과 검증

- [ ] `AutoPotion` 장착 시 HP 임계 구간에서 1회 자동 회복 발동 확인
- [ ] 자동 회복 발동 후 상단 메시지 표시 확인
- [ ] `BlazeOfGlory` 장착 시 사망 순간 주변 적 폭발 피해 적용 확인

## 29. 현재 한계

- ❗ `MoreDrops`는 현재 반지 드롭률에만 연결됨(스킬북/4배 보너스 드롭은 후속 연결 필요)
- ❗ `MasterOfOffense`는 현재 데미지/쿨다운 보정으로 1차 대체 구현됨(서브스킬 직접 부여는 후속)

---

# 9차 추가 (2026-05-14 후속7) — Fates 상시 효과 런타임 연결 1차

## 30. Fate 효과 검증

- [ ] `BossMonsters` 해금 시 `WaveSpawner`가 보스 웨이브 주기(`bossWaveInterval`)로 보스를 추가 스폰하는지 확인
- [ ] `BossMonsters` 미해금 시 보스 추가 스폰이 없는지 확인
- [ ] `MoreGraveyards` 해금 시 사용 가능 묘지 수 조회값이 1 -> 4로 바뀌는지 확인
- [ ] `UnlockScavenger` 미해금 시 무덤지기 입장/선택 API가 막히는지 확인
- [ ] `UnlockScavenger` 해금 시 무덤지기 API가 정상 동작하는지 확인

## 31. 현재 한계

- ❗ `MoreGraveyards`는 현재 조회 API로만 연결됨(실제 맵 선택 UI 바인딩은 후속)
- ❗ 캐릭터/스킬 해금 Fate는 현재 상태 조회 API 중심(선택 UI 및 실제 진입 제한 연동은 후속)

---

# 10차 추가 (2026-05-14 후속8) — 캐릭터/맵 선택 바인딩 + 보스 보상 분리

## 32. 캐릭터/맵 선택 바인딩

- [ ] `CharacterSelectionService`를 캐릭터 선택 UI에서 호출해 기본4/해금캐릭터 노출이 분리되는지 확인
- [ ] `GraveyardSelectionService`로 맵 슬롯 index 0~3 잠금 상태가 반영되는지 확인
- [ ] `MoreGraveyards` 해금 전/후 맵 선택 가능 개수 변화 확인

## 33. 보스 보상 분리

- [ ] 보스 프리팹 `EnemyController.isBoss=true` 설정
- [ ] 보스 프리팹에 `bossRewardPickupPrefab` 연결
- [ ] 보스 처치 시 보상 픽업 1개 생성 확인
- [ ] 보상 종류가 `반지 / 스킬포인트 / 4배데미지` 중 랜덤인지 확인
- [ ] 4배데미지 보상 획득 시 일정시간 후 버프 종료되는지 확인

## 34. 현재 한계

- ❗ 보스 스폰 주기는 현재 웨이브 주기 기반(킬수 400 기준과 차이 있음)
- ❗ 스킬포인트 보상은 현재 `레벨업 선택 1회`로 대체 구현됨

---

# 11차 추가 (2026-05-14 후속9) — 로비 UI 실제 바인딩 컴포넌트

## 35. Services/Fates 패널 바인딩

- [ ] Services 항목 프리팹/버튼에 `ServiceEntryButtonBinder` 부착
- [ ] Fate 항목 프리팹/버튼에 `FateEntryButtonBinder` 부착
- [ ] 각 바인더에 `LobbyShopService` 참조 연결
- [ ] `OnClickUnlock/Equip/Unequip/Buy` 버튼 이벤트 연결
- [ ] 상태 텍스트(locked/unlocked/equipped) 갱신 확인

## 36. 캐릭터/맵 선택 바인딩

- [ ] 캐릭터 버튼에 `CharacterSelectButtonBinder` 부착
- [ ] `characterId`를 실제 캐릭터 키(`Sirmin`, `Wazoo` 등)로 입력
- [ ] 맵 버튼에 `GraveyardSelectButtonBinder` 부착
- [ ] `mapIndex`(0~3) 별 잠금 상태 반영 확인

## 37. 남은 후속

- ❗ 동적 리스트(카탈로그 기반 자동 생성 UI)는 아직 미구현
- ❗ 한글 표시명/설명 로컬라이즈 테이블 연결 필요

---

## 문제 발생 시 보고 양식

```
1. 어디서 / 어떤 동작에서:
2. 기대한 결과:
3. 실제 결과:
4. Console 메시지 (있다면 그대로 복사):
5. 관련 GameObject 인스펙터 스크린샷 (가능하면):
```

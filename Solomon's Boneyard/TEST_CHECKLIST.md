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

## 문제 발생 시 보고 양식

```
1. 어디서 / 어떤 동작에서:
2. 기대한 결과:
3. 실제 결과:
4. Console 메시지 (있다면 그대로 복사):
5. 관련 GameObject 인스펙터 스크린샷 (가능하면):
```

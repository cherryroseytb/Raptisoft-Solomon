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

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

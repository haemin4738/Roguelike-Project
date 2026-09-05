# Roguelike Portfolio Project

던그리드(Dungreed) 스타일의 2D 사이드스크롤 플랫포머 로그라이크 포트폴리오 프로젝트입니다.

**서버 레포:** [Roguelike-Project-Server](https://github.com/haemin4738/Roguelike-Project-Server)

---

## 플레이 영상

https://github.com/haemin4738/Roguelike-Project/releases/download/v1.0.0/2026-09-05.223733.mp4

---

## 기술 스택

| 분류 | 내용 |
|------|------|
| 엔진 | Unity 6 (6000.0.67f1), URP 2D |
| 언어 | C# |
| 입력 | New Input System |
| 아키텍처 | ScriptableObject 데이터 드리븐 + Manager 싱글톤 + EventBus pub/sub |
| 서버 | FastAPI + PostgreSQL + SQLAlchemy (Railway 배포) |
| 인증 | JWT (Bearer) + 이메일 인증 (Resend) |
| 에셋 | 0x72 DungeonTileset II (CC0), DungeonUI (CC0) |

---

## 핵심 구현 포인트

### 1. 절차적 던전 생성
- 랜덤 시드 기반 방/플랫폼/적 배치
- Start / Normal / Boss 룸 타입 구분
- 스파이크 트랩, 미로형 플랫폼 랜덤 배치

### 2. 메타 진행 시스템
- 런(Run) 간 어빌리티 포인트(AP) 영구 유지
- 6종 스킬트리: 분노 / 신속 / 인내 / 신비 / 탐욕 / 집중
- 레벨당 보너스 + 마일스톤(5lv/20lv) 특수 효과

### 3. 전투 시스템
- 근접 + 원거리 무기 동시 장착, 1/2키 전환
- 360도 마우스 방향 공격
- 대시: 마우스 방향, 2회 충전, 0.75초 자동 회복
- 코요테 타임 + 점프 버퍼, 하향 점프

### 4. 서버 연동 (풀스택)
- 회원가입 / 이메일 인증 / 로그인 (FastAPI + JWT)
- 런 종료 시 플레이어 데이터 자동 클라우드 저장
- 어빌리티 레벨 서버 동기화

### 5. EventBus 아키텍처
- 직접 참조 없이 `EventBus.Publish<T>()` / `Subscribe<T>()`로 컴포넌트 간 통신
- 주요 이벤트: DamageEvent, EnemyKilledEvent, RoomClearedEvent, PlayerDiedEvent

### 6. ScriptableObject 데이터 드리븐
- 26종 무기, 6종 어빌리티, 5종 캐릭터 전부 ScriptableObject 정의
- Editor 자동화 스크립트로 에셋 일괄 생성

---

## 구현 시스템 목록

| 시스템 | 내용 |
|--------|------|
| 플레이어 이동 | A/D 이동, 점프(코요테+버퍼), 하향점프, 대시(마우스방향, 2회 충전) |
| 전투 | 근접+원거리 동시 장착, 1/2키 전환, 360도 공격 |
| 던전 생성 | 절차적 방 생성, Start/Normal/Boss 룸, 플랫폼/미로 랜덤 배치, 스파이크 트랩 |
| 마을 방 | 상점/어빌리티/캐릭터선택 3존, NPC 자동 스폰 |
| 상점 | 26종 무기 랜덤 5개 표시, 골드 구매, 신비 어빌리티 할인 적용 |
| 어빌리티 | 6종 스킬트리, AP 소모, 마일스톤 특수효과 |
| 캐릭터 선택 | 5종 캐릭터(기사/마법사/엘프/도마뱀/드워프), 스탯 차별화 |
| 캐릭터 정보 UI | E키 패널, 캐릭터 스탯 실시간 표시 |
| 인벤토리 UI | I키 패널, 무기 슬롯 1/2 선택 |
| 보스 | 2종 랜덤 선택, 2페이즈(근접→원거리) |
| 게임오버/클리어 | 사망→"게임 오버", 보스 처치→"클리어!", 처치수/레벨 표시 |
| 메타 진행 | 런 간 AP/어빌리티 영구 유지 |
| 서버 연동 | 회원가입/이메일인증/로그인, 런 종료 시 데이터 자동 저장 |

---

## 캐릭터

| 이름 | 특성 |
|------|------|
| 기사 | HP +30, 공격력 +3 |
| 마법사 | HP -20, 공격력 +8, 공격속도 +0.3 |
| 엘프 | 이동속도 +1.5, 대시 +1 |
| 도마뱀 | HP +20, 이동속도 +0.5 |
| 드워프 | HP +50, 이동속도 -0.5, 공격력 +5 |

---

## 어빌리티 스킬트리

| 이름 | 레벨당 | 5lv 특수 | 20lv 특수 |
|------|--------|----------|-----------|
| 분노 | 공격력 +2 | 점프 주변피해 | 최대데미지 + 대시 +1 |
| 신속 | 이동 +0.05, 공격속도 +0.025 | 이단점프 + 대시 +1 | 대시 무적 + 대시 +1 |
| 인내 | 방어 +1.5, HP +1 | 마법방패 | 저체력 회복 + 대시 +1 |
| 신비 | 크리 +0.5, 회피 +0.5 | 상점 40% 할인 | 아이템보관 + 대시 +1 |
| 탐욕 | HP +2 | 골드드랍↑ | 액세서리슬롯 + 대시 +1 |
| 집중 | 크리데미지 +2.5 | 원거리 체력·방어↑ | 재장전도구 + 대시 +1 |

---

## 조작법

| 키 | 행동 |
|----|------|
| A / D | 이동 |
| W / Space | 점프 |
| S + 점프 | 하향 점프 |
| Shift / 우클릭 | 대시 (마우스 방향) |
| 좌클릭 | 공격 |
| 1 / 2 | 무기 전환 |
| E | 캐릭터 정보 패널 |
| I | 인벤토리 패널 |
| F | NPC 상호작용 |
| ESC | 일시정지 |

---

## 프로젝트 구조

```
Assets/
  _Scenes/
    MainMenu.unity
    Game.unity
  Scripts/
    Core/         GameManager, EventBus, GameEvents
    Player/       PlayerController, PlayerStats, PlayerCombat
    Enemy/        EnemyBase, EnemyAI, EnemySpawner
    Combat/       DamageSystem, ProjectileBase
    Dungeon/      RoomGenerator, DungeonManager, TownZone
    Item/         ShopManager, WeaponPickup
    Meta/         MetaProgress, MetaProgressApplicator, AbilityData
    UI/           HUDController, ShopUI, AbilityUI, CharacterSelectUI,
                  CharacterInfoUI, InventoryUI
    Editor/       WeaponAssetGenerator, EnemyPrefabGenerator, ...
  Data/
    Weapons/      26종 WeaponData + ShopItemData
    Characters/   5종 CharacterData
    Abilities/    6종 AbilityData
  Prefabs/
    Player/
    Enemy/        15종
    Projectiles/  4종
    UI/
```

---

## 서버 API

| 메서드 | 경로 | 설명 |
|--------|------|------|
| POST | /auth/register | 회원가입 + 이메일 인증 발송 |
| GET | /auth/verify/{token} | 이메일 인증 완료 |
| POST | /auth/login | 로그인 → JWT 반환 |
| GET | /player/data | 플레이어 데이터 조회 |
| PUT | /player/data | 플레이어 데이터 업데이트 |
| GET | /player/abilities | 어빌리티 레벨 조회 |
| PUT | /player/abilities | 어빌리티 레벨 저장 |

---

## 라이선스

게임 에셋: [0x72 DungeonTileset II](https://0x72.itch.io/dungeontileset-ii) — CC0

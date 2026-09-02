# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 기술 스택

- **언어**: C# (Unity)
- **엔진**: Unity 6 (6000.0.67f1), URP 2D
- **저장**: JSON (`Application.persistentDataPath/save.json`)
- **아키텍처**: ScriptableObject 기반 데이터 드리븐 + Manager 싱글톤 패턴 + EventBus (pub/sub)
- **목적**: 포트폴리오 — 던그리드 스타일 2D 사이드스크롤 플랫포머 로그라이크
- **입력**: New Input System

## 빌드 및 실행

```bash
# Unity Editor에서 열기 (Unity Hub 사용)
# 프로젝트 경로: D:/Unity/Portpolio/Roguelike/My project
# 씬: Assets/_Scenes/Game.unity

# 커맨드라인 빌드 (Windows)
"C:/Program Files/Unity/Hub/Editor/6000.0.67f1/Editor/Unity.exe" \
  -quit -batchmode \
  -projectPath "D:/Unity/Portpolio/Roguelike/My project" \
  -buildWindowsPlayer "Build/Roguelike.exe" \
  -logFile "Build/build.log"
```

## 프로젝트 구조

```
Assets/
  _Scenes/
    MainMenu.unity    메인 메뉴
    Game.unity        게임 플레이
  Scripts/
    Core/             GameManager, EventBus, GameEvents
    Player/           PlayerController, PlayerStats, PlayerCombat
    Enemy/            EnemyBase, EnemyAI, EnemySpawner
    Combat/           DamageSystem, ProjectileBase
    Dungeon/          RoomGenerator, DungeonManager, RoomData
    UI/               HUDController, PauseMenu, ItemSelectUI
  Art/
    Sprites/
      Player/         knight, dwarf, elf, lizard, wizzard 스프라이트
      Enemy/          goblin, orc, demon, zombie 등 스프라이트
      Tileset/        floor, wall, atlas 타일
      Items/          weapon, flask, coin, bomb 스프라이트
      Props/          chest, door, lever, crate 등
      UI/             button, ui_heart, dungeonui
    Animations/       AnimationClip 파일
  Audio/
    BGM/
    SFX/
  Prefabs/
    Player/
    Enemy/
    Dungeon/
    UI/
```

## 씬 구성

| 씬 | 역할 |
|----|------|
| MainMenu | 타이틀, 게임 시작, 설정 |
| Game | 던전 탐색, 전투, 아이템 선택, 보스 |

## 핵심 아키텍처

### EventBus

`EventBus.Publish<T>()` / `EventBus.Subscribe<T>()` 방식. 직접 참조 대신 이벤트로 통신.

주요 이벤트 (`GameEvents.cs`):
- `DamageEvent` — 데미지 발생 (Target: Enemy/Player)
- `EnemyKilledEvent` — 적 처치
- `RoomClearedEvent` — 방 클리어
- `PlayerDiedEvent` — 플레이어 사망 (런 종료)
- `ItemPickedEvent` — 아이템 획득
- `RunStartedEvent` — 런 시작

### 던전 생성

**DungeonManager가 단일 책임.** 방(Room) 단위로 프리팹 배치, 포탈로 연결.
- `RoomData` ScriptableObject로 방 유형 정의 (전투방, 상점방, 보스방)
- 런 시작 시 랜덤 시드로 방 배치 결정

### 스탯 구조

`PlayerStats`가 기본 스탯 + 아이템 보너스 집계해 최종 스탯 계산.
아이템 효과는 ScriptableObject(`ItemData`)로 정의.

### 저장 시스템

런 내 데이터는 메모리에서 관리 (런 종료 시 소멸).
영구 데이터(해금, 최고 기록 등)만 JSON 저장.

```csharp
ISaveProvider
  └── JsonSaveProvider  // persistentDataPath/save.json
```

## 에셋 정보

| 에셋 | 용도 | 라이선스 |
|------|------|---------|
| 0x72 DungeonTileset II | 타일, 캐릭터, 몬스터 스프라이트 | CC0 |
| DungeonUI (0x72) | UI 버튼, 패널, 체력바 | CC0 |

- 스프라이트 설정: Filter Mode `Point`, PPU `16`, Compression `None`
- Pixel Perfect Camera: Reference Resolution `640x360`, Crop Frame `Windowbox`

## Git 브랜치 전략

- 새로운 기능 추가 또는 기존 기능 수정 시 **반드시 새 브랜치를 생성**하고 작업한다
- 작업 완료 후 PR을 올린다 (직접 main에 push 금지)
- 브랜치 네이밍: `feat/기능명`, `fix/수정내용` 등 타입 기반

### 커밋 타입

| 이모지 | 타입 | 설명 |
|--------|------|------|
| ✨ | feat | 새로운 기능 추가 |
| 🐛 | fix | 버그 수정 |
| 🐛 | fix | 버그 수정 |
| 🎨 | style | 코드 스타일 변경 |
| 🔄 | refactor | 리팩토링 |
| 💄 | design | UI/스프라이트 등 디자인 수정 |
| 💬 | comment | 주석 추가/수정 |
| 📝 | docs | 문서 수정 |
| 🧪 | test | 테스트 추가/수정 |
| 🛠️ | chore | 설정/빌드 등 기타 변경 |
| 📛 | rename | 파일명/폴더명 변경 |
| 🗑️ | remove | 파일 삭제 |

커밋 메시지 형식: `✨ feat: 플레이어 이동 구현`

## 개발 시 주의사항

- `.meta` 파일은 반드시 함께 커밋한다 (Unity 에셋 참조 유지)
- ScriptableObject 에셋(`.asset`) 수정 시 해당 `.meta`도 함께 커밋한다
- 씬 파일(`.unity`) 충돌 방지를 위해 한 번에 한 명만 씬 편집한다
- `[SerializeField]` private 필드를 선호 — `public` 노출 최소화
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`는 `.gitignore`로 제외
- 런 내 임시 데이터와 영구 저장 데이터를 명확히 구분한다

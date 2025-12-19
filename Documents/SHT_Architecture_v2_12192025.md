# Shrunken Head Toss - Architecture Document

**Version:** 2
**Date Created:** December 19, 2025
**Last Updated:** December 19, 2025
**Developer:** TecVooDoo LLC
**Unity Version:** 6.3 (6000.3.0f1)
**Total Scripts:** TBD (project not yet started)

---

## Installed Packages

### Asset Store Packages

| Package | Version | Purpose |
|---------|---------|---------|
| DOTween Pro | 1.0.386 | Animation, tweening, juice effects |
| Feel | 5.9.1 | MMFeedbacks for impacts, screen shake, game feel |
| Init(args) | 1.5.5 | Dependency injection, service initialization |
| Odin Inspector and Serializer | 4.0.1.2 | Enhanced inspector, serialization |
| Odin Validator | 4.0.1.2 | Asset validation, error checking |
| Toolkit for Ballistics 2026 | 5.0.0 | Trajectory calculation, arc physics |

### Third-Party Packages (Local/Custom)

| Package | Version | Purpose |
|---------|---------|---------|
| MCP for Unity | 8.2.3 | Claude Code integration |
| UniTask | 2.5.10 | Async/await, cancellation tokens |

### Key Unity Packages

| Package | Version | Purpose |
|---------|---------|---------|
| 2D Animation | 13.0.2 | Sprite animation |
| 2D Aseprite Importer | 3.0.1 | Aseprite file support |
| 2D Sprite | 1.0.0 | Sprite rendering |
| 2D SpriteShape | 13.0.0 | Shape-based sprites |
| 2D Tilemap Editor | 1.0.0 | Tilemap tools |
| 2D Tilemap Extras | 6.0.1 | Additional tilemap features |
| Cinemachine | 3.1.5 | Camera control, zoom, follow |
| Input System | 1.16.0 | New input handling |
| TextMesh Pro | (included) | UI text rendering |
| Universal Render Pipeline | 17.3.0 | 2D URP rendering |
| Burst | 1.8.25 | Performance compilation |
| Mathematics | 1.3.3 | Math utilities |

---

## System Overview

```
                          +-------------------+
                          |   GameManager     |
                          | (Match/Round Flow)|
                          +---------+---------+
                                    |
          +-------------------------+-------------------------+
          |                         |                         |
+---------v---------+    +----------v----------+    +---------v---------+
|   TossController  |    |    ScoreManager     |    |    UIController   |
|  (Input/Physics)  |    |  (Points/Rounds)    |    |   (HUD/Menus)     |
+---------+---------+    +----------+----------+    +---------+---------+
          |                         |                         |
+---------v---------+    +----------v----------+    +---------v---------+
|   HeadManager     |    |   SpikeBedManager   |    |   AudioManager    |
| (Spawning/State)  |    | (Zones/Collision)   |    |   (SFX/Music)     |
+-------------------+    +---------------------+    +-------------------+
```

---

## Planned Namespaces

| Namespace | Purpose |
|-----------|---------|
| `SHT.Core` | Game state, match flow, scoring |
| `SHT.Gameplay` | Toss mechanics, physics, heads |
| `SHT.UI` | HUD, menus, popups |
| `SHT.Audio` | Sound effects, music |
| `SHT.Data` | ScriptableObjects, save data |
| `SHT.CustomHeads` | Photo upload, face processing |

---

## Planned File Structure

```
Assets/SHT/Scripts/
|
+-- Core/
|   |-- GameManager.cs              (Match/round state machine)
|   |-- ScoreManager.cs             (Point tracking, win conditions)
|   |-- PlayerData.cs               (Player state per match)
|   +-- GameSettings.cs             (ScriptableObject for config)
|
+-- Gameplay/
|   |-- TossController.cs           (Input handling, launch physics)
|   |-- HeadController.cs           (Individual head behavior)
|   |-- HeadManager.cs              (Head spawning, pooling)
|   |-- SpikeBedManager.cs          (Zone detection, scoring zones)
|   |-- SpikeZone.cs                (Individual zone collider)
|   +-- TrajectoryPreview.cs        (Optional arc preview)
|
+-- UI/
|   |-- MainMenuController.cs       (Menu navigation)
|   |-- GameplayHUD.cs              (Score, round, heads remaining)
|   |-- ResultsScreen.cs            (Round/match results)
|   |-- HeadGalleryUI.cs            (Head selection/creation)
|   +-- SettingsPanel.cs            (Audio, gore toggle)
|
+-- Audio/
|   |-- AudioManager.cs             (Singleton for SFX/music)
|   |-- SFXClipGroup.cs             (Reuse from DLYH)
|   +-- MusicManager.cs             (Background music)
|
+-- Data/
|   |-- HeadDataSO.cs               (Head stats, sprite refs)
|   |-- ArenaDataSO.cs              (Arena config, spike layout)
|   +-- PlayerProgressSO.cs         (Unlocks, achievements)
|
+-- CustomHeads/
|   |-- PhotoCaptureController.cs   (Camera/gallery access)
|   |-- FaceDetector.cs             (Face finding in photo)
|   |-- HeadGenerator.cs            (Warp photo to shrunken head)
|   +-- CustomHeadStorage.cs        (Local save/load)
```

---

## Key Components

### GameManager

**Purpose:** Controls overall match flow.

**States:**
- `MainMenu`
- `HeadSelection`
- `Playing`
- `RoundEnd`
- `MatchEnd`

**Responsibilities:**
- Track current round/match
- Manage turn order
- Trigger round/match end conditions

### TossController

**Purpose:** Handles player input for throwing.

**Input Flow:**
1. Detect drag start (mouse down / touch begin)
2. Track drag direction and distance
3. Calculate angle and power
4. On release, spawn and launch head
5. Lock input until head settles

**Key Properties:**
- `minPower`, `maxPower`
- `launchAngleRange`
- `currentPlayer`

### HeadController

**Purpose:** Individual head behavior after launch.

**States:**
- `Held` (before throw)
- `Flying` (in air)
- `Landed` (on spike or ground)
- `Stacked` (on another head)

**Physics:**
- Rigidbody2D with custom drag
- Rotation during flight
- Collision callbacks for scoring

### SpikeBedManager

**Purpose:** Manages the target spike bed.

**Responsibilities:**
- Define scoring zones (center, inner, outer, edge)
- Detect head collisions
- Report landing zone to ScoreManager
- Handle head stacking logic

### ScoreManager

**Purpose:** Track and calculate scores.

**Data:**
- Player 1 score (current round)
- Player 2 score (current round)
- Rounds won per player
- Match history

**Events:**
- `OnScoreChanged`
- `OnRoundEnd`
- `OnMatchEnd`

---

## Event Architecture

```
TossController
    +--- OnTossStarted ---> UIController (hide trajectory?)
    +--- OnTossReleased ---> HeadManager (spawn head)
    +--- OnTurnComplete ---> GameManager (switch player)

HeadController
    +--- OnHeadLanded ---> SpikeBedManager (determine zone)
    +--- OnHeadStacked ---> ScoreManager (bonus points)

SpikeBedManager
    +--- OnZoneHit(zone, head) ---> ScoreManager (add points)

ScoreManager
    +--- OnScoreChanged ---> UIController (update HUD)
    +--- OnRoundEnd ---> GameManager (next round or match end)
    +--- OnMatchEnd ---> UIController (show results)

GameManager
    +--- OnStateChanged ---> UIController (show/hide panels)
    +--- OnPlayerTurnChanged ---> TossController (enable input)
```

---

## Design Patterns (Carried from DLYH)

### 1. Controller Extraction
Keep MonoBehaviours small. Extract logic to plain C# classes early.

### 2. Event-Driven Communication
Use C# events for decoupling. No direct references between managers.

### 3. ScriptableObject Data
Use SOs for:
- Head definitions (HeadDataSO)
- Arena configs (ArenaDataSO)
- Game settings (GameSettingsSO)

### 4. Singleton Audio Managers
AudioManager and MusicManager as singletons with static convenience methods.

### 5. State Machine for Game Flow
GameManager uses explicit states, not booleans.

```csharp
public enum GameState
{
    MainMenu,
    HeadSelection,
    Playing,
    RoundEnd,
    MatchEnd
}
```

### 6. Object Pooling for Heads
Don't instantiate/destroy - pool and reuse head objects.

---

## Physics Setup

### Layers

| Layer | Purpose |
|-------|---------|
| Default | UI, background |
| Heads | Shrunken head objects |
| Spikes | Spike colliders |
| Ground | Miss zone |

### Collision Matrix

| | Heads | Spikes | Ground |
|---|---|---|---|
| Heads | Yes (stacking) | Yes | Yes |
| Spikes | Yes | No | No |
| Ground | Yes | No | No |

---

## Data Persistence

### Local Storage

**PlayerPrefs:**
- Audio volumes
- Gore toggle setting
- Last selected heads

**JSON Files:**
- Custom head data (base64 images + metadata)
- Unlock progress
- Achievement status

---

## Performance Considerations

### Mobile Optimization
- Object pooling for heads
- Sprite atlasing
- Limit particle count on impacts
- LOD for background elements

### Memory
- Compress custom head images
- Limit custom head count (10-20 max?)
- Unload unused arenas

---

## Testing Strategy

### Unit Tests (EditMode)
- Score calculation
- Round/match win conditions
- Zone detection logic

### Integration Tests (PlayMode)
- Full toss -> land -> score flow
- Turn switching
- Round transitions

---

**End of Architecture Document**

*Note: This document will be updated as development progresses and actual scripts are created.*

# Shrunken Head Toss - Architecture Document

**Version:** 1.0
**Date Created:** December 19, 2025
**Last Updated:** December 19, 2025
**Developer:** TecVooDoo LLC
**Total Scripts:** TBD (project not yet started)

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
- Full toss → land → score flow
- Turn switching
- Round transitions

---

**End of Architecture Document**

*Note: This document will be updated as development progresses and actual scripts are created.*

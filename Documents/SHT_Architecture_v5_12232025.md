# Shrunken Head Toss - Architecture Document

**Version:** 5
**Date Created:** December 19, 2025
**Last Updated:** December 23, 2025
**Developer:** TecVooDoo LLC
**Unity Version:** 6.3 (6000.3.0f1)
**Total Scripts:** 9 (Core gameplay functional)

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

## Current File Structure

```
Assets/SHT/Scripts/
|
+-- Core/
|   |-- IPlayer.cs                  [DONE] Interface for player abstraction (local/network)
|   |-- LocalPlayer.cs              [DONE] Local player implementation
|   |-- GameManager.cs              [DONE] Match/round/turn state machine
|   |-- ScoreManager.cs             [DONE] Score tracking with UI events
|   |-- PlayerData.cs               (PLANNED - Player state per match)
|   +-- GameSettings.cs             (PLANNED - ScriptableObject for config)
|
+-- Gameplay/
|   |-- ITossInput.cs               [DONE] Interface for input abstraction
|   |-- MouseTossInput.cs           [DONE] Mouse/keyboard input implementation
|   |-- TossController.cs           [DONE] Input handling, launch physics, trajectory preview
|   |-- HeadController.cs           [DONE] Individual head behavior, collision, scoring, parenting
|   |-- SpikeZone.cs                [DONE] Scoring zone definition, head tracking, capacity limits
|   |-- TossTestEnabler.cs          [DEPRECATED] Remove - GameManager handles input now
|   |-- HeadManager.cs              (PLANNED - Head spawning, pooling)
|   +-- SpikeBedManager.cs          (PLANNED - Zone detection)
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
- `Idle` (before throw)
- `Flying` (in air)
- `Landed` (scored and impaled on target)
- `Missed` (hit wrong target/ground/bounds)

**Key Properties:**
- `_ownerPlayerIndex` - Which player threw this head (0 or 1)
- `_impaledOnSide` - Which spike bed side this head is impaled on (Left/Right)
- `_impaledOnSpike` - Reference to SpikeZone for stacking
- `_rotationSpeed` - Rotation while flying

**Collision Logic:**
- First collision while Flying determines outcome
- Spikes layer (trigger): Check if valid target for player, score and impale if yes
- Heads layer: If other head impaled on valid target = stack, opponent's head = pass through
- Anything else (Ground, bounds) = miss

**Impalement (v5):**
- Head becomes child of spike tip transform
- Scale compensation applied (spike tips have non-uniform scale)
- Local position calculated from base reference + stack offset
- Rotation preserved from flight for variety

**Physics:**
- Rigidbody2D with custom gravity (20)
- Rotation during flight based on velocity direction
- Becomes Kinematic when impaled (frozen in place)

### SpikeZone

**Purpose:** Defines scoring zones and tracks impaled heads.

**Key Properties (v5):**
- `_zoneType` - Center (10), Inner (7), Outer (5), Edge (3)
- `_targetSide` - Left (Player 2 target) or Right (Player 1 target)
- `_baseTransform` - Reference to child base for impale position
- `_impaleOffset` - Vertical offset from base for first head
- `_headStackSpacing` - Vertical spacing between stacked heads
- `_maxHeadCapacity` - Maximum heads this spike can hold
- `_impaledHeads` - List tracking registered heads

**Methods:**
- `GetLocalImpalePosition()` - Returns local position for next head
- `GetSpikeTransform()` - Returns transform for parenting
- `HasRoom()` - Returns true if spike has capacity
- `RegisterImpaledHead()` - Adds head to tracking, disables collider when full
- `UnregisterImpaledHead()` - Removes head from tracking
- `ClearImpaledHeads()` - Resets for new round, re-enables collider
- `IsValidTargetForPlayer()` - Validates target for player index

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
    +--- OnHeadScored ---> ScoreManager (add points)

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
| Default | UI, background, SpikeBase visuals |
| Heads | Shrunken head objects |
| Spikes | Individual spike tip colliders (with SpikeZone component) |
| Ground | Miss zone |
| Bounds | Out of bounds triggers (screen edges) |

### Collision Matrix

| | Heads | Spikes | Ground | Bounds |
|---|---|---|---|---|
| Heads | Yes (stacking/pass-through) | Yes | Yes | N/A (trigger) |
| Spikes | Yes | No | No | No |
| Ground | Yes | No | No | No |
| Bounds | Trigger only | No | No | No |

### SpikeZone Component

Each spike has a `SpikeZone` component with:
- `ZoneType` enum: Center (10 pts), Inner (7 pts), Outer (5 pts), Edge (3 pts)
- `TargetSide` enum: Left (valid for Player 2) or Right (valid for Player 1)
- `IsValidTargetForPlayer(int playerIndex)` - Returns true if this spike is valid target for given player
- Head capacity tracking and collider management

### Scene Hierarchy

```
SpikeBed_Left (empty parent)
  - SpikeBase (visual only, Layer: Default)
  - Spike_Edge_Tip (Layer: Spikes, PolygonCollider2D trigger, SpikeZone script)
      - Spike_Edge_Base (Layer: Default, BoxCollider2D solid, referenced by parent SpikeZone)
  - Spike_Outer_Tip (Layer: Spikes, PolygonCollider2D trigger, SpikeZone script)
      - Spike_Outer_Base (Layer: Default, BoxCollider2D solid)
  - Spike_Inner_Tip (Layer: Spikes, PolygonCollider2D trigger, SpikeZone script)
      - Spike_Inner_Base (Layer: Default, BoxCollider2D solid)
  - Spike_Center_Tip (Layer: Spikes, PolygonCollider2D trigger, SpikeZone script)
      - Spike_Center_Base (Layer: Default, BoxCollider2D solid)
  - ... (7 spikes total per side)

SpikeBed_Right (empty parent)
  - (same structure, all zones set to TargetSide.Right)

Out_of_Bounds (trigger colliders)
  - Out_of_Bounds_Top
  - Out_of_Bounds_Bottom
  - Out_of_Bounds_Left
  - Out_of_Bounds_Right
```

**Spike Structure (v5):**
- Spike_Tip (parent): Layer "Spikes", PolygonCollider2D as TRIGGER, SpikeZone component
- Spike_Base (child): Layer "Default", BoxCollider2D as SOLID collider
- SpikeZone references its Base child for impale position calculation
- Head hits tip trigger -> scores -> parents to tip -> moves to calculated local position
- Tip collider disables when spike is full (heads bounce off top head)
- Base collider prevents heads from falling through

**Spike Capacities:**
- Center: 1 head (10 pts)
- Inner: 2 heads (7 pts)
- Outer: 3 heads (5 pts)
- Edge: 4 heads (3 pts)

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

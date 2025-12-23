# Shrunken Head Toss - Design Decisions and Lessons Learned

**Version:** 4
**Date Created:** December 19, 2025
**Last Updated:** December 23, 2025
**Developer:** TecVooDoo LLC

---

## Purpose

This document captures design decisions, technical insights, and lessons learned during development. It serves as a historical record and reference for future development.

---

## Version History

| Version | Date | Summary |
|---------|------|---------|
| 1.0 | Dec 19, 2025 | Initial document, pre-development |
| 4.0 | Dec 23, 2025 | Core gameplay implemented - scoring, player targeting, head stacking, pass-through mechanics |

---

## Project Origin

**Original Concept:** Created for a class project, combining cornhole mechanics with Beetlejuice-inspired dark humor.

**Core Pitch:** "If you like cornhole you are going to love this game. You and an opponent go head-to-head tossing tiny craniums onto a bed of spikes."

**Key Feature:** Upload photos of yourself or relatives to create unique game pieces. "Fun for the whole family!"

---

## Design Decisions

### Aesthetic: Norman Rockwell Meets 80s Gore

**Decision:** Warm, nostalgic Americana visuals with stylized (not realistic) gore.

**Rationale:**
- Creates unique visual identity
- Beetlejuice proved this tone works
- Dark humor without being off-putting
- Appeals to adults while remaining playful

**Implementation:**
- Soft, painterly backgrounds
- Vintage color palette (creams, soft greens, wood browns)
- Cartoonish blood splatter
- Option to reduce/disable gore

### Spike Bed Instead of Cornhole Board

**Decision:** Replace traditional cornhole holes with a bed of spikes.

**Rationale:**
- More visceral and thematic
- Creates varied scoring zones (spike heights)
- Allows for impale/bounce/slide mechanics
- Head stacking adds emergent gameplay

### Side View Gameplay

**Decision:** Players face each other across the spike pit, camera shows side view.

**Rationale:**
- Clearer trajectory visualization
- Both players visible on screen
- Natural arc display
- Simpler camera setup than top-down

### Photo Upload as Premium Feature

**Decision:** Custom head creation via photo upload.

**Rationale:**
- Strong viral/social potential
- Personal investment in gameplay
- Unique selling point vs other casual games
- Privacy-first: local storage only

### Local Multiplayer First

**Decision:** Focus on local PvP before online.

**Rationale:**
- Faster to implement
- Party game nature suits local play
- Prove the fun before adding network complexity
- Mobile pass-and-play is natural fit

### Stylized Gore with Toggle

**Decision:** Include cartoonish blood/impacts but provide toggle option.

**Rationale:**
- Core audience appreciates the humor
- Toggle expands audience to younger players
- App store compliance easier with toggle
- Doesn't compromise artistic vision

---

## Lessons Learned from DLYH

These lessons are carried forward from Don't Lose Your Head development:

### Code Architecture

**1. Extract Controllers Early**
- Don't let MonoBehaviours grow past 500 lines
- Extract to plain C# classes before it becomes painful
- DLYH had several 1000+ line scripts that required major refactoring

**2. Event Timing Matters**
- Update state BEFORE firing events
- Handlers often check state during event processing
```csharp
// CORRECT:
_currentState = NewState;
OnStateChanged?.Invoke();

// WRONG:
OnStateChanged?.Invoke();
_currentState = NewState;
```

**3. Initialize UI Explicitly**
- Don't rely on default Unity states
- Call Hide/Show explicitly on startup
- Set all UI to known state before gameplay begins

### Development Process

**4. Step-by-Step Verification**
- Complete one step, verify it works, then proceed
- Don't rush ahead with multiple changes
- Errors compound when steps are skipped

**5. ASCII Encoding Only**
- Avoid UTF-8 in scripts
- No smart quotes, em-dashes, or special characters
- Prevents encoding-related bugs

**6. Validate After MCP Edits**
- Always run `validate_script` after changes
- Check Unity console for errors
- Don't assume edits succeeded

### UI/UX

**7. Guard Batch Operations**
- Hide/reset UI before AND after batch operations
- Prevents visual glitches from event cascades

**8. Boolean Flags for Persistent State**
- Don't rely on show/hide state
- Use explicit boolean flags for important states

**9. Test Edge Cases Early**
- Playtest unusual scenarios before polishing
- Edge cases found late are expensive to fix

---

## Technical Decisions

### Physics System

**Decision:** Use Unity 2D physics (Rigidbody2D).

**Rationale:**
- Simple arc trajectories
- Built-in collision detection
- Easy to tune bounce/drag

### Object Pooling for Heads

**Decision:** Pool head objects instead of instantiate/destroy.

**Rationale:**
- Better mobile performance
- No GC spikes during gameplay
- Learned from general Unity best practices

### ScriptableObjects for Data

**Decision:** Use SOs for head definitions, arena configs, settings.

**Rationale:**
- Designer-friendly editing
- Easy to add content without code changes
- Proven pattern from DLYH

### State Machine for Game Flow

**Decision:** Explicit state enum instead of boolean flags.

**Rationale:**
- Clearer code flow
- Easier to debug
- Prevents invalid state combinations

---

## Scope Control

### MVP Definition

**Must Have (Week 1-2):**
- One arena
- 3 default heads
- Local 2-player
- Basic toss physics
- Spike bed scoring
- Sound effects

**Nice to Have (Week 3+):**
- Photo upload
- Additional arenas
- Achievements
- Online multiplayer

### Scope Creep Prevention

- No new features until MVP is playable
- Document feature requests, don't implement immediately
- Playtest before polish
- "Good enough" beats "perfect"

---

## Open Questions

These decisions are pending and will be resolved during development:

| Question | Options | Decision |
|----------|---------|----------|
| Trajectory preview line? | Show arc / Don't show | IMPLEMENTED - show arc (full arc for testing, shorten to ~50% for release) |
| Wind mechanic? | None to hurricane force based on difficulty | PLANNED - post-MVP |
| Head abilities? | Different weights/bounces / All same | TBD |
| Tie breaker | Sudden death / Replay round | TBD |
| Max custom heads | 10 / 20 / Unlimited | TBD |

---

## Bug Fix History

| Date | Issue | Fix |
|------|-------|-----|
| Dec 19, 2025 | Head scored then rolled off, registering as miss | Check `_hasScored` flag before treating ground collision as miss |
| Dec 19, 2025 | Turn doesn't switch if head rolls off ground edge | RESOLVED - Simplified scoring rules (first collision determines outcome) |
| Dec 23, 2025 | Player 2 always scoring even when missing | Added player index tracking and target validation per player |
| Dec 23, 2025 | Heads stacking too high, blocking opponent throws | Flying heads now pass through opponent's impaled heads |
| Dec 23, 2025 | Heads perching on spike tips instead of impaling | Added impaleDepth setting to settle heads down onto spikes |

---

## Implementation Status (Dec 23, 2025)

### Completed

**Core Scoring System:**
- First collision determines outcome (simplified from timeout/settle logic)
- Hit correct target = score and impale (head freezes in place)
- Hit wrong target, ground, bounds, or flying head = miss, turn ends
- HeadController states: Idle, Flying, Landed, Missed

**Player Targeting:**
- Player 1 (left side, index 0) targets SpikeBed_Right
- Player 2 (right side, index 1) targets SpikeBed_Left
- TossController passes playerIndex to HeadController on launch
- SpikeZone has TargetSide enum (Left/Right) with IsValidTargetForPlayer() validation

**Spike Bed Structure:**
- Each spike bed has 11 individual spikes with their own colliders
- Spike types: Edge (3 pts), Outer (5 pts), Inner (7 pts), Center (10 pts)
- Colliders should cover just the tips for proper impalement feel
- Edge spikes are tallest, Center is shortest

**Head Stacking:**
- Hitting a head impaled on YOUR valid target = score (5 pts for stacking)
- Hitting opponent's impaled head = pass through (Physics2D.IgnoreCollision)
- Hitting head on ground or flying head = miss

**Impalement:**
- Scored heads become Kinematic (frozen in place)
- impaleDepth setting (default 0.3) moves head down after scoring
- Heads track which side they're impaled on via ImpaledOnSide property

**Out of Bounds:**
- 4 trigger colliders on "Bounds" layer around screen edges
- OnTriggerEnter2D detects bounds and ends turn as miss

### Pending/Needs Adjustment

**Collider Tuning:**
- Spike colliders may need adjustment to cover only tips
- Head size was reduced to allow better stacking on taller spikes
- impaleDepth may need per-spike-type adjustment

**Visual Polish:**
- Heads stick at collision point, may look unnatural on some spikes
- Consider adding slight position randomization for variety

**Gameplay Balance:**
- Stack points currently fixed at 5 (could vary)
- Round/match flow works but needs UI feedback

---

## Playtest Feedback

**Dec 23, 2025 - Initial Testing:**
- Core loop works: toss, score/miss, switch turns
- Pass-through mechanic prevents blocking issues
- Spike collider size affects where heads can land
- Taller edge spikes allow more stacking room

---

## Future Considerations

### Game Expansion: "Head Games" (Post-MVP)

**Concept:** Rename from "Shrunken Head Toss" to "Head Games" - a collection of macabre lawn games.

**Planned Game Modes:**
1. **Shrunken Head Toss** - Cornhole-style (current core game)
2. **Bocce Ball** - Traditional bocce with shrunken heads
3. **Lawn Darts** - Head becomes the target (or accidentally hits other player)
4. **KanJam variant** - TBD mechanics

**Note:** These are documented for future reference. One game at a time - no scope creep.

### Wind Mechanic (Post-MVP)

**Concept:** Variable wind affecting trajectory, scaled by difficulty.
- Easy: No wind
- Medium: Light breeze
- Hard: Strong gusts
- Nightmare: Hurricane force

### Background Vignettes

**Concept:** Keep middle screen clear for background "flavor" animations.
- Norman Rockwell scenes with macabre twists
- Examples:
  - Dad at BBQ grill with human leg/foot
  - Mom chasing dog carrying an arm/hand
  - Kids playing with suspicious looking ball
- Non-interactive, purely atmospheric
- Reinforces "Americana meets 80s gore" aesthetic

**Implementation:** Parallax background layer, subtle animations, no gameplay impact.

### Potential Features (Post-Launch)

- Online matchmaking
- Ranked competitive mode
- Seasonal arenas (Halloween, Christmas)
- Head cosmetics (hats, accessories)
- Tournament mode
- Replay system

### Platform Expansion

- Steam release
- Mobile (iOS/Android)
- Console (Switch ideal fit for party game)

---

**End of Design Decisions Document**

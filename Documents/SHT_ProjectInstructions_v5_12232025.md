# Shrunken Head Toss - Claude Project Instructions

**Project:** Shrunken Head Toss
**Developer:** TecVooDoo LLC
**Designer:** Rune (Stephen Brandon)
**Unity Version:** 6.3 (6000.3.0f1)
**Project Path:** E:\Unity\ShrunkenHeadToss
**Document Version:** 5
**Last Updated:** December 23, 2025

---

## IMPORTANT: Project Path

**The Unity project will be located at: `E:\Unity\ShrunkenHeadToss`**

Do NOT use worktree paths - always use the E: drive path above for all file operations.

---

## Critical Development Protocols

### NEVER Assume Names Exist

**CRITICAL: Verify before referencing**

- NEVER assume file names, method names, or class names exist
- ALWAYS read/search for the actual names in the codebase first
- If a script might be named "HeadController" or "HeadManager", search to find the actual name
- Incorrect assumptions waste context and force complete rewrites

### Step-by-Step Verification Protocol

**CRITICAL: Never rush ahead with multiple steps**

- Provide ONE step at a time
- Wait for user confirmation via text OR screenshot before proceeding
- User will verify each step is complete before moving forward
- If a step fails, troubleshoot that specific step before continuing
- Assume nothing - verify everything

**Why this matters:** Jumping ahead when errors occur forces entire scripts to be redone, wasting context and causing chats to run out of room.

### Documentation Research Protocol

**CRITICAL: Use current documentation**

- ALWAYS fetch the most up-to-date documentation for Unity and packages before making recommendations
- User prefers waiting for accurate information over redoing work later
- Do not rely on potentially outdated knowledge - verify current APIs and patterns
- This applies to Unity 6.3, all installed packages, and any external libraries

---

## File Conventions

### Encoding Rule

**CRITICAL: ASCII Only**

- All scripts and text files MUST use ASCII encoding
- Do NOT use UTF-8 or other encodings
- Avoid special characters, smart quotes, em-dashes
- Use standard apostrophes (') not curly quotes
- Use regular hyphens (-) not em-dashes

### Core Document Naming Convention

**Format:** `SHT_DocumentName_v#_MMDDYYYY.md`

**Rules:**
- All four core documents share the SAME version number
- Increment version for ALL documents when ANY document is updated
- If a document has no changes, update the filename only (no content changes needed)
- Move old versions to `Documents/Archive/` folder

**Core Documents:**
- `SHT_ProjectInstructions_v#_MMDDYYYY.md`
- `SHT_GDD_v#_MMDDYYYY.md`
- `SHT_Architecture_v#_MMDDYYYY.md`
- `SHT_DesignDecisions_v#_MMDDYYYY.md`

**Example version bump:**
```
v4 -> v5 (all four files)
Old files moved to Documents/Archive/
```

---

## Code Editing Preferences

### Manual Copy/Paste (providing code to user)

- Provide COMPLETE file replacements
- Easier than hunting for specific lines
- Reduces errors from partial edits

### MCP Direct Edits (Claude editing via tools)

- Use `script_apply_edits` for method-level changes
- Use `apply_text_edits` for precise line/column edits
- Use `validate_script` to check for errors
- Direct MCP edits are preferred when connection is stable

---

## Unity MCP Tools Reference

### Script Operations

| Tool | Purpose |
|------|---------|
| `manage_script` (read) | Read script contents |
| `get_sha` | Get SHA256 hash + file size |
| `validate_script` | Check syntax/structure |
| `create_script` | Create new C# scripts |
| `delete_script` | Remove scripts |

### Script Editing (Preferred)

**`script_apply_edits`** - Structured C# edits:
- `replace_method` - Replace entire method body
- `insert_method` - Add new method
- `delete_method` - Remove a method
- `anchor_insert/delete/replace` - Pattern-based edits

**`apply_text_edits`** - Precise text edits:
- Line/column coordinate-based changes
- Atomic multi-edit batches

### Other Operations

| Tool | Purpose |
|------|---------|
| `manage_asset` | Import, create, modify, delete assets |
| `manage_scene` | Load, save, create scenes |
| `manage_gameobject` | Create, modify, find GameObjects |
| `manage_prefabs` | Open/close prefab stage |
| `manage_editor` | Play/pause/stop, tags, layers |
| `read_console` | Get Unity console messages |
| `run_tests` | Execute EditMode or PlayMode tests |

### MCP Best Practices

1. **Read before editing** - View the file before making changes
2. **Validate after edits** - Run `validate_script` to catch syntax errors
3. **Use structured edits** - Prefer `script_apply_edits` for methods
4. **Check console** - Use `read_console` after changes
5. **Avoid verbose output** - Don't display full read tool calls (causes lockup)
6. **Avoid hierarchy modifications** - MCP hierarchy changes can cause Unity lockups

---

## Coding Standards

### Required Frameworks

- **Odin Inspector** (4.0.1.2) - For data structures and Inspector UI
- **Odin Validator** (4.0.1.2) - For asset validation
- **DOTween Pro** (1.0.386) - For animations and juice
- **Feel** (5.9.1) - For MMFeedbacks, impacts, screen shake
- **UniTask** (2.5.10) - For async/await patterns
- **Init(args)** (1.5.5) - For dependency injection
- **Toolkit for Ballistics 2026** (5.0.0) - For trajectory/arc physics
- **New Input System** (1.16.0) - Use `Keyboard.current`, `Mouse.current`, `Touchscreen.current`

### Architecture Patterns

**SOLID Principles - Required:**
- **S**ingle Responsibility - One reason to change per class
- **O**pen/Closed - Open for extension, closed for modification
- **L**iskov Substitution - Subtypes must be substitutable
- **I**nterface Segregation - Many specific interfaces over one general
- **D**ependency Inversion - Depend on abstractions, not concretions

**ScriptableObject Architecture Pattern (SOAP):**
- Use ScriptableObjects for data, configuration, and events
- Decouple systems through SO-based channels
- Prefer SO events over direct references between managers

**Additional Patterns:**
- **Use Interfaces** - Define contracts for dependencies
- **Async over Coroutines** - Prefer UniTask async/await unless coroutine is trivial
- Keep scripts small and focused (learned from DLYH)
- Extract controllers/services early to avoid large MonoBehaviours
- Use events for decoupled communication

### File Size Limits

**CRITICAL: 800 lines maximum per script**

- When a script approaches 800 lines, it is time to refactor
- Extract logic into separate classes, services, or controllers
- Do not wait until scripts are unmanageable

### Event Patterns

- Set state BEFORE firing events that handlers may check
- Initialize UI to known states (explicit Hide/Show calls)
- Guard against event re-triggering during batch operations

---

## Project Documents

| Document | Purpose |
|----------|---------|
| SHT_GDD | Game design and mechanics |
| SHT_Architecture | Script catalog, packages, code structure |
| SHT_DesignDecisions | History, lessons learned, version tracking |
| SHT_ProjectInstructions | Development protocols (this document) |

**Note:** User will ask Claude to review core docs at the start of each chat. Read and follow these instructions carefully.

---

## Development Status

### Phase 1: Core Mechanics - DONE

| Item | Status |
|------|--------|
| Physics-based head tossing | DONE |
| Spike bed target system | DONE |
| Basic scoring | DONE |
| Two-player local gameplay | DONE |
| Player targeting (P1->Right, P2->Left) | DONE |
| Head stacking mechanics | DONE |
| Pass-through for opponent heads | DONE |
| Out of bounds detection | DONE |
| Spike capacity limits | DONE |

### Phase 2: Polish - TODO

| Item | Status |
|------|--------|
| Sound effects | TODO |
| Visual feedback (impacts, bounces) | TODO |
| Score UI | TODO |
| Collider tuning (spike tips) | TODO |
| Head scale fix when parented | IN PROGRESS |

### Phase 3: Features - TODO

| Item | Status |
|------|--------|
| Photo upload for custom heads | TODO |
| Head variety/unlockables | TODO |
| Multiple arenas | TODO |
| Online multiplayer | TODO |

**v5 Note:** See Design Decisions doc for detailed implementation status and pending adjustments.

---

## Quick Reference Checklist

**Always:**
- [ ] Verify file/method/class names exist before referencing
- [ ] Wait for user verification before proceeding
- [ ] Use ASCII encoding only
- [ ] Use New Input System
- [ ] Validate scripts after MCP edits
- [ ] Set state BEFORE firing events
- [ ] Use interfaces for dependencies
- [ ] Prefer async/await (UniTask) over coroutines
- [ ] Follow SOLID principles
- [ ] Use ScriptableObject architecture (SOAP)
- [ ] Fetch current documentation before recommendations

**Never:**
- [ ] Assume names exist without checking
- [ ] Rush ahead with multiple steps
- [ ] Use UTF-8 or special characters
- [ ] Use legacy Input class
- [ ] Display full read tool calls
- [ ] Use MCP for hierarchy modifications
- [ ] Let scripts grow past 800 lines without refactoring
- [ ] Make recommendations based on potentially outdated docs

---

## Lessons Learned from DLYH

1. **Extract controllers early** - Don't let MonoBehaviours grow large
2. **Event timing matters** - Update state BEFORE firing events
3. **Initialize UI explicitly** - Don't rely on default states
4. **Validate positions** - Check before UI placement
5. **Guard batch operations** - Hide/reset UI before AND after
6. **Use boolean flags** - For state that persists across show/hide
7. **Test edge cases early** - Don't wait for playtest to find bugs

---

**End of Project Instructions**

These instructions should be followed for every conversation in this project. User will ask Claude to review these docs at the start of each chat session.

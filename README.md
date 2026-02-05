# ComBoom - Block Puzzle Game

<p align="center">
  <img src="docs/screenshots/splash.png" alt="ComBoom Splash" width="250"/>
  <img src="docs/screenshots/menu.png" alt="Main Menu" width="250"/>
  <img src="docs/screenshots/gameplay.png" alt="Gameplay" width="250"/>
</p>

A polished mobile block puzzle game built with **Unity 6** for **iOS** and **Android**. Place blocks on an 8x8 grid, clear rows and columns, chain combos, and climb the leaderboard.

---

## Features

### Core Gameplay
- **8x8 Grid** with drag-and-drop piece placement
- **21 unique piece shapes** — single blocks, lines, L-shapes, T-shapes, squares
- **Row & column clearing** — fill any complete row or column to clear it
- **Multi-clear** — a single placement can clear multiple lines simultaneously

### Scoring & Combos
- **Placement scoring**: 10 points per block
- **Line clear scoring**: Exponential per-line (100 + 200 + 300...)
- **Combo multiplier**: Chain consecutive clears for 2x, 4x, 8x, 16x multipliers
- **Grid clear bonus**: 2x score for clearing the entire board
- **Persistent high score** tracking

### Progression System
- **Level-based XP** system with exponential growth curve
- **Level-up rewards**: +1 Undo, +1 Bomb, +1 Shuffle per level
- **Visual level-up celebration** with full-screen effects

### Power-Ups
| Power-Up | Effect |
|----------|--------|
| **Undo** | Reverts last piece placement (grid, score, combo restored) |
| **Bomb** | Tap to clear any single cell |
| **Shuffle** | Regenerates all unused pieces with new random shapes |

### Visual Polish
- Animated splash screen with block rain, convergence, and explosion effects
- Cell pop-in and clear animations with easing
- Brick break particle effects scaling with combo intensity
- Floating score popups with celebration text
- Combo display with shake animation
- Smooth drag preview with valid/invalid placement highlighting

### Audio
- **Procedurally generated SFX** — no external audio files needed
- Separate sound effects and music with independent mute controls
- Haptic feedback on iOS (light, medium, heavy patterns)

### Localization
6 languages supported:
- English, Turkish, German, Spanish, Italian, French

---

## Architecture

```
Assets/Scripts/
├── Core/                  # Game management
│   ├── GameManager.cs         # Central orchestrator, game flow & events
│   ├── GameState.cs           # State enum (Splash, Menu, Playing, Paused, GameOver)
│   ├── GridManager.cs         # 8x8 grid operations, line clearing
│   ├── ScoreManager.cs        # Score calculation & persistence
│   ├── LevelManager.cs        # XP & level progression
│   ├── PowerUpManager.cs      # Undo, Bomb, Shuffle management
│   ├── AudioManager.cs        # SFX & music playback
│   ├── LayoutManager.cs       # Responsive layout for different screens
│   ├── LocalizationManager.cs # Multi-language string management
│   └── HapticManager.cs       # iOS haptic feedback
│
├── Gameplay/              # Game mechanics
│   ├── Grid.cs                # Grid data structure & validation
│   ├── Cell.cs                # Cell visuals & animations
│   ├── Piece.cs               # Draggable piece behavior
│   ├── PieceData.cs           # Piece shape definitions
│   ├── PieceDatabase.cs       # 21 piece type catalog
│   ├── PieceSpawner.cs        # Random piece generation (3 per turn)
│   ├── SpriteGenerator.cs     # Runtime sprite creation
│   ├── AudioClipGenerator.cs  # Procedural audio synthesis
│   └── BrickBreakEffect.cs    # Particle effects on clear
│
├── Input/                 # User interaction
│   └── DragDropHandler.cs     # Touch drag & piece placement
│
├── UI/                    # User interface
│   ├── UIManager.cs           # Panel visibility coordinator
│   ├── MainMenuPanel.cs       # Main menu (level, score, buttons)
│   ├── GameOverPanel.cs       # Game over screen
│   ├── PausePanel.cs          # Pause menu with settings
│   ├── SplashPanel.cs         # Animated splash screen
│   ├── ScoreDisplay.cs        # Real-time score display
│   ├── ComboDisplay.cs        # Combo indicator with shake
│   ├── FloatingScore.cs       # Score popup animations
│   ├── LevelProgressBar.cs    # XP progress with level-up banner
│   ├── LevelUpEffect.cs       # Full-screen level-up notification
│   ├── RanksPanel.cs          # Leaderboard display
│   ├── SettingsPanel.cs       # Settings menu
│   ├── SafeAreaPanel.cs       # Notch/safe area handling
│   └── LocalizedText.cs       # Localized text component
│
└── Editor/                # Development tools
    ├── SceneSetup.cs          # One-click scene generation
    ├── iOSBuilder.cs          # iOS build automation
    └── AndroidBuilder.cs      # Android build automation
```

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Engine | Unity 6 (6000.3.6f1) |
| Language | C# |
| Backend | IL2CPP |
| Input | Unity Input System |
| UI | Unity UI (uGUI) + TextMeshPro |
| Platforms | iOS 15+, Android 7.0+ (API 24) |
| Architecture | ARM64 |

---

## Building

### Prerequisites
- Unity 6 (6000.3.6f1) with iOS and/or Android build support
- Xcode 15+ (for iOS)
- Android SDK API 34 (for Android)

### Quick Build (from Unity Editor)

**iOS:**
```
ComBoom > Build iOS
```
Generates Xcode project at `Builds/iOS/`, opens Xcode automatically.

**Android APK (testing):**
```
ComBoom > Build Android (APK)
```
Outputs `Builds/Android/ComBoom.apk`.

**Android AAB (Play Store):**
```
ComBoom > Build Android (AAB - Play Store)
```
Outputs `Builds/Android/ComBoom.aab`.

### Scene Setup
```
ComBoom > Setup Game Scene
```
Regenerates the entire game scene from code — all GameObjects, UI hierarchy, and component wiring.

### App Icon
```
ComBoom > Generate App Icon
```
Generates a 1024x1024 icon procedurally and assigns it to platform settings.

---

## Game Flow

```
Splash Screen ──→ Main Menu ──→ Gameplay ──→ Game Over
                    ↑   │         │   ↑          │
                    │   │         │   │          │
                    │   └── Settings  Pause ←────┘
                    │                  │
                    └──────────────────┘
```

---

## Scoring Formula

| Action | Points |
|--------|--------|
| Place piece | `blocks × 10` |
| Clear 1 line | `100 × combo` |
| Clear 2 lines | `300 × combo` |
| Clear 3 lines | `600 × combo` |
| Clear N lines | `(N×(N+1)/2 × 100) × combo` |
| Grid clear | `score × 2` |

**Combo multiplier**: `2^(combo - 1)` for consecutive clears.

---

## Screenshots

> Add screenshots to `docs/screenshots/` directory:

| Screen | File |
|--------|------|
| Splash | `docs/screenshots/splash.png` |
| Main Menu | `docs/screenshots/menu.png` |
| Gameplay | `docs/screenshots/gameplay.png` |
| Combo | `docs/screenshots/combo.png` |
| Game Over | `docs/screenshots/gameover.png` |
| Level Up | `docs/screenshots/levelup.png` |

---

## License

This project is proprietary. All rights reserved by M3Studios.

---

<p align="center">
  Built with Unity 6 by <strong>M3Studios</strong>
</p>

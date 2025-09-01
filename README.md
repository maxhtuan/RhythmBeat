# RhythmBeat

A Unity-based rhythm game that allows players to play piano notes in sync with music. The game features multiple game modes, comprehensive scoring, and a modular architecture built around a service locator pattern.

## 🎮 Game Overview

RhythmBeat is a rhythm game where players hit piano keys in time with music. The game supports:
- **Perform Mode**: Standard rhythm gameplay with music playback
- **Speed Up Mode**: Progressive difficulty with increasing tempo
- XML-based song data loading
- Real-time scoring and accuracy tracking
- Metronome synchronization
- Firebase integration for data persistence

## 🏗️ Architecture Overview

The game uses a **Service Locator Pattern** with manager-based architecture for clean separation of concerns and easy dependency management. The codebase is organized into distinct modules:

### Core Systems
The essential gameplay systems that drive the main game logic.

### Input Systems
Handles all player input and piano key interactions.

### UI Systems
Manages all user interface components and interactions.

### Audio Systems
Handles music playback, sound effects, and metronome functionality.

### Data Systems
Manages song data, settings, and persistence.

### Services
External integrations and support systems.

### Controllers
Game object specific behaviors and interactions.

## 🎯 Core Module

The core module contains the essential systems that drive the main gameplay.

### **Gameplay Manager** (`Core/GameplayManager.cs`)
- **Main orchestrator** of the entire game
- Coordinates all other managers and systems
- Handles game initialization and setup
- Manages game flow and state transitions
- **Key Responsibilities:**
  - Async game setup and initialization
  - Manager coordination
  - Game state management
  - Note spawning and timing
  - UI coordination

### **Game State Manager** (`Core/GameStateManager.cs`)
- Manages game states: `None`, `Preparing`, `Playing`, `End`
- Provides state change events for other systems
- Handles state-specific logic and transitions
- **Events:**
  - `OnGameStateChanged`: Fired when state changes
  - `OnGameStarted`: Fired when game begins
  - `OnGameEnded`: Fired when game ends

### **Time Manager** (`Core/TimeManager.cs`)
- Central timing system for the game
- Manages game time, beat timing, and synchronization
- Handles pause/resume functionality
- **Features:**
  - Beat-based timing
  - Time synchronization across systems
  - Pause/resume support
  - BPM calculations

### **Note Manager** (`Core/NoteManager.cs`)
- Handles note spawning, movement, and cleanup
- Manages note lifecycle from spawn to hit/miss
- **Key Features:**
  - XML-based note loading
  - Position-based note spawning
  - Note movement and timing
  - Speed-up mode pattern generation
  - Note cleanup and memory management

### **Game Settings Manager** (`Core/GameSettingsManager.cs`)
- Player preferences and settings
- Volume controls
- Difficulty settings
- Settings persistence

### **Game Mode Manager** (`Core/GameModeManager.cs`)
- Manages different game modes
- Handles mode transitions
- Coordinates mode-specific logic

### **Metronome Manager** (`Core/MetronomeManager.cs`)
- Provides rhythmic timing for gameplay
- Synchronizes with music BPM
- Visual and audio metronome feedback
- **Key Features:**
  - BPM synchronization
  - Beat visualization
  - Audio click sounds
  - Time synchronization with game state

### **Audio Manager** (`Core/AudioManager.cs`)
- Manages all audio playback (music and SFX)
- Handles piano key sound mapping
- Volume control for music and SFX
- **Features:**
  - Piano key sound mapping (C, D, E, F, G, A, B)
  - Hit/miss/combo sound effects
  - Separate music and SFX volume controls
  - Audio source management

## 🎯 Input Module

Handles all player input and piano key interactions.

### **Piano Input Handler** (`Input/PianoInputHandler.cs`)
- **Central input processing system**
- Manages all piano key interactions
- Handles note hit detection and accuracy calculation
- **Key Features:**
  - Piano key component management
  - Input event processing
  - Note hit detection and accuracy calculation
  - Vibration feedback
  - Input enable/disable functionality
  - Game control input (Space, R, Escape)

## 🖥️ UI Module

Manages all user interface components and interactions.

### **Game UI Manager** (`UI/GameUIManager.cs`)
- Main UI coordination and management
- Handles UI state changes
- Manages score display and feedback
- **Features:**
  - Score and combo display
  - Game state UI updates
  - Menu navigation
  - Visual feedback systems

### **Game Mode UI** (`UI/GameModeUI.cs`)
- Game mode specific UI elements
- Mode selection interface
- Mode-specific displays

## 📁 Data Module

Handles song data, settings, and persistence.

### **Data Handler** (`Data/DataHandler.cs`)
- XML song data parsing
- **Features:**
  - MusicXML format support
  - Note timing and pitch extraction
  - BPM and division parsing
  - Learner part (P1) filtering

### **Song Handler** (`Data/SongHandler.cs`)
- Song metadata management
- BPM and timing information
- Song selection and loading

## 🔧 Services Module

External integrations and support systems.

### **Service Locator** (`Services/ServiceLocator.cs`)
- Central registry for all game services
- Provides dependency injection for managers
- Handles service lifecycle (Initialize/Cleanup)
- Singleton pattern for global access

### **Firebase Manager** (`Services/FirebaseManager.cs`)
- Cloud data persistence
- Score and progress syncing
- User data management

### **Gameplay Logger** (`Services/GameplayLogger.cs`)
- Comprehensive logging system
- Performance tracking
- Debug information
- Event logging

### **Debug Song Loader** (`Services/DebugSongLoader.cs`)
- Debug utilities for song loading
- Development and testing tools

## 🎮 Game Modes Module

Different gameplay modes and their implementations.

### **Perform Mode** (`GameModes/PerformMode.cs`)
- Standard rhythm gameplay
- Music playback with note synchronization
- **Features:**
  - Music playback control
  - Metronome integration
  - Beat hit detection
  - Pattern recognition

### **Speed Up Mode** (`GameModes/SpeedUpMode.cs`)
- Progressive difficulty mode
- Increasing tempo and complexity
- **Features:**
  - Dynamic BPM changes
  - Pattern-based gameplay
  - Progressive difficulty scaling
  - Extended note spawning range

## 📝 Scoring & Progress

### **Score Manager** (`Core/ScoreManager.cs`)
- Comprehensive scoring system
- **Scoring Tiers:**
  - Perfect: 100 points (95%+ accuracy)
  - Great: 80 points (85%+ accuracy)
  - Good: 60 points (70%+ accuracy)
  - OK: 40 points (50%+ accuracy)
  - Miss: 0 points
- **Features:**
  - Combo system with multipliers
  - Accuracy tracking
  - Statistics (total notes, hits, misses)
  - Score persistence

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- .NET 4.x
- Firebase SDK (for cloud features)

### Setup
1. Clone the repository
2. Open in Unity
3. Configure Firebase settings (if using cloud features)
4. Add song XML files to `Resources/` folder
5. Configure audio clips in AudioManager
6. Run the game

### File Structure
```
Assets/_Game/Scripts/
├── Core/                    # Core gameplay systems
│   ├── GameplayManager.cs   # Main orchestrator
│   ├── TimeManager.cs       # Timing system
│   ├── NoteManager.cs       # Note management
│   ├── GameStateManager.cs  # Game state management
│   ├── GameSettingsManager.cs # Settings management
│   ├── GameModeManager.cs   # Game mode coordination
│   ├── MetronomeManager.cs  # Metronome system
│   ├── AudioManager.cs      # Audio management
│   └── ScoreManager.cs      # Scoring system
├── Input/                   # Input systems
│   └── PianoInputHandler.cs # Piano input processing
├── UI/                      # User interface
│   ├── GameUIManager.cs     # Main UI coordination
│   └── GameModeUI.cs        # Mode-specific UI
├── Services/                # Support services
│   ├── ServiceLocator.cs    # Service registry
│   ├── FirebaseManager.cs   # Cloud integration
│   ├── GameplayLogger.cs    # Logging system
│   └── DebugSongLoader.cs   # Debug utilities
├── Controllers/             # Game object controllers
│   ├── NoteController.cs    # Note behavior
│   ├── PianoKey.cs          # Piano key behavior
│   ├── TargetBarController.cs # Target bar
│   ├── NoteRenderer.cs      # Note rendering
│   └── NoteMovement.cs      # Note movement
├── GameModes/               # Game mode implementations
│   ├── PerformMode.cs       # Standard gameplay
│   └── SpeedUpMode.cs       # Progressive difficulty
├── Data/                    # Data handling
│   ├── DataHandler.cs       # XML parsing
│   └── SongHandler.cs       # Song management
├── Bootstrap/               # Initialization
├── Interfaces/              # Service interfaces
├── GameConfigs/             # Configuration
└── Parsers/                 # Data parsing utilities
```

##  Adding New Songs

1. Create MusicXML file with learner part (P1)
2. Place in `Resources/` folder
3. Update `DataHandler.xmlFileName` to match your file
4. Configure BPM and timing in the XML

## ⚙️ Configuration

### Audio Setup
- Assign piano key sounds in `AudioManager`
- Configure music and SFX volumes
- Set up audio sources

### Game Settings
- Adjust note length multiplier
- Configure spawn offsets
- Set difficulty parameters

## 📝 Development Notes

- **Service Pattern**: All managers implement `IService` interface
- **Async Setup**: Game initialization is asynchronous for better performance
- **Event-Driven**: Heavy use of events for loose coupling
- **Modular Design**: Easy to add new game modes or features
- **Performance**: Optimized note spawning and cleanup

## 🤝 Contributing

1. Follow the existing service pattern
2. Implement `IService` for new managers
3. Use the ServiceLocator for dependencies
4. Add comprehensive logging
5. Test with different game modes

##  License

[Add your license information here]
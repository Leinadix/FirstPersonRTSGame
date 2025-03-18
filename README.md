# First Person RTS Game

A prototype 3D real-time strategy game with first-person controls, built using C# and Silk.NET.

## Overview

This project is a first-person RTS game prototype that combines first-person controls with resource management and strategic gameplay elements. The game is rendered using OpenGL through the Silk.NET library.

## Features

- First-person navigation in a 3D world
- Resource gathering and management
- Building construction and management
- Ship controls and movement
- Autonomous ship harvesting and resource delivery
- OpenGL-based rendering
- Voxel-based terrain with destructible environments
- Day/night cycle affecting gameplay
- Complete graphical user interface (UI) system
  - Crosshair and target information
  - Resource and inventory displays
  - Building and ship status panels
  - Interactive menus
  - Modern, visually appealing design with rounded panels and consistent styling
  - Smooth animations and transitions between UI elements
  - Improved text rendering with outlines for better readability
  - Interactive buttons with hover effects and tooltips
  - HUD elements like crosshair and status indicators
  - Toast notification system for important game events

## Documentation

Comprehensive documentation is available in the [doc](doc/index.md) directory:

- [Project Overview](doc/overview.md)
- [Getting Started Guide](doc/getting-started.md)
- [User Guide](doc/user-guide.md)
- [API Reference](doc/api-reference.md)
- [Troubleshooting](doc/troubleshooting.md)

For developers:
- [Engine Documentation](doc/Engine/README.md)
- [Game Documentation](doc/Game/README.md)
- [Test Documentation](doc/Tests/README.md)
- [Development Workflow](doc/development-workflow.md)

## Controls

- **WASD**: Move the player
- **Mouse**: Look around
- **Space**: Move up
- **Shift**: Move down
- **E**: Interact with objects
- **I**: Show inventory
- **B**: Building menu
- **1-5**: Command ship to harvest resources (when targeting a ship)
  - 1: Wood | 2: Iron | 3: Gold | 4: Crystal | 5: Oil
- **R**: Command ship to return home (when targeting a ship)
- **F1**: Display help
- **Escape**: Exit game

## Ship State System

Ships implement an autonomous harvesting system with the following states:
- **Idle**: Ship is waiting for commands
- **MovingToResource**: Ship is traveling to a resource location
- **Harvesting**: Ship is actively gathering resources
- **MovingToDropoff**: Ship is traveling to a building to deliver resources
- **DroppingOff**: Ship is delivering resources to a building
- **ReturnToPosition**: Ship is returning to its home position

## UI System

The game features a complete UI system built with OpenGL:

- **Resource Panel**: Displays current resource counts in the top-left corner
- **Target Info**: Shows information about the currently targeted object (resource, building, or ship)
- **Ship Status**: Displays ship cargo and state when a ship is targeted
- **Building Menu**: Shows available buildings that can be constructed
- **Inventory Panel**: Shows player's current resource inventory
- **Crosshair**: Indicates the center of the screen for targeting
- **Notification System**: Displays toast notifications for important game events with color-coding based on message type (info, success, warning, error)

## Project Structure

The project is split into three main parts:

1. **Engine**: Core game systems and interfaces
   - Rendering
   - Input handling
   - Game object interfaces
   - Constants and utility classes

2. **Game**: Game-specific implementations
   - Building implementations
   - Ship implementations
   - Resource implementations
   - Player controls
   - World generation
   - UI system

3. **Tests**: Unit and integration tests
   - Test doubles
   - Mocks
   - Test categories

## Requirements

- .NET 6.0 SDK or later
- Compatible with Windows, macOS, and Linux
- Graphics card supporting OpenGL 4.5+
- Silk.NET packages (automatically restored by NuGet)

## Quick Start

1. Clone the repository
2. Navigate to the project directory
3. Build the project:
   ```
   dotnet build
   ```
4. Run the game:
   ```
   dotnet run --project FirstPersonRTSGame
   ```

See the [Getting Started Guide](doc/getting-started.md) for detailed setup instructions.

## Technical Details

- Written in C# with .NET 6.0
- Uses Silk.NET for windowing, input, and OpenGL binding
- Employs an interface-based architecture to separate engine and game logic
- Uses OpenGL for 3D rendering
- Custom UI system with text rendering capabilities

## Contributing

We welcome contributions! Please see our [Development Workflow](doc/development-workflow.md) document for guidelines on contributing to the project.

## Future Improvements

- Complete resource gathering mechanics
- Add more building types and functionalities
- Implement ship combat
- Enhance UI with animations and effects
- Implement saving and loading game state
- Add sound effects and music

## License

This project is licensed under Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0) with Commons Clause.

See the [LICENSE.md](LICENSE.md) file for full details.
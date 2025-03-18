# Getting Started with FirstPersonRTSGame

This guide will help you set up your development environment, build the project, and run the game.

## Prerequisites

Before you begin, make sure you have the following installed:

- **.NET 6.0 SDK** or newer (Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download))
- **Visual Studio 2022**, **Visual Studio Code**, or another compatible IDE
- **Git** for version control (Download from [git-scm.com](https://git-scm.com/downloads))

## Setting Up the Development Environment

### Clone the Repository

```bash
git clone https://github.com/leinadix/FirstPersonRTSGame.git
cd FirstPersonRTSGame
```

### Restore NuGet Packages

The project uses several NuGet packages, including Silk.NET. You can restore these packages by running:

```bash
dotnet restore
```

This will automatically download all required dependencies specified in the project files.

## Project Structure

The solution contains three main projects:

1. **FirstPersonRTSGame.Engine**: Core systems and interfaces
2. **FirstPersonRTSGame.Game**: Game-specific implementations
3. **FirstPersonRTSGame.Tests**: Unit and integration tests

## Building the Project

### Building from Command Line

To build the project from the command line:

```bash
dotnet build
```

This will compile all projects in the solution.

### Building from Visual Studio

1. Open the solution file `FirstPersonRTSGame.sln` in Visual Studio
2. Select the desired build configuration (Debug/Release)
3. Click on "Build" > "Build Solution" or press F6

## Running the Game

### Running from Command Line

To run the game from the command line:

```bash
dotnet run --project FirstPersonRTSGame
```

### Running from Visual Studio

1. Set the FirstPersonRTSGame project as the startup project
2. Click the "Start" button or press F5 to run in debug mode
3. Press Ctrl+F5 to run without debugging

## Game Controls

Once the game is running, you can use the following controls:

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

## Running Tests

To run the unit and integration tests:

```bash
dotnet test
```

To run specific test categories:

```bash
# Run all UI tests
dotnet test --filter "FullyQualifiedName~UI"

# Run all player tests
dotnet test --filter "FullyQualifiedName~Player"

# Run just the basic tests
dotnet test --filter "FullyQualifiedName~BasicTests"
```

## Troubleshooting Common Issues

### Missing Dependencies

If you encounter errors related to missing dependencies, try:

```bash
dotnet restore --force
```

### OpenGL-Related Errors

If you encounter OpenGL-related errors:

1. Make sure your graphics drivers are up to date
2. Verify that your system supports OpenGL 3.3 or higher
3. Check the application logs for specific error messages

### Build Errors

If you encounter build errors:

1. Check that you have the correct .NET SDK version installed
2. Verify that all NuGet packages have been properly restored
3. Look for specific error messages in the build output

## Next Steps

Once you have the project running, you might want to:

1. Explore the [Engine Documentation](Engine/README.md) to understand the core systems
2. Review the [Game Documentation](Game/README.md) to see how the game is implemented
3. Check out the [API Reference](api-reference.md) for detailed information on classes and interfaces
4. Try implementing a new feature or fixing a bug from the issue tracker

## Getting Help

If you encounter issues or have questions:

1. Check the [Troubleshooting Guide](troubleshooting.md) for common problems and solutions
2. Review existing issues in the project repository
3. Create a new issue if your problem hasn't been addressed 
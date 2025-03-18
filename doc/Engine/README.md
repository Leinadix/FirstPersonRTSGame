# FirstPersonRTSGame - Engine Documentation

This section documents the Engine components of the FirstPersonRTSGame project. The Engine provides the core framework that the Game implementation builds upon.

## Table of Contents

- [Overview](#overview)
- [Core Components](#core-components)
  - [Interfaces](#interfaces)
  - [Renderer](#renderer)
  - [Constants](#constants)
- [Architecture](#architecture)
- [Implementation Details](#implementation-details)

## Overview

The Engine project (`FirstPersonRTSGame.Engine`) acts as the foundation for the game. It provides interfaces, rendering capabilities, and core systems that enable the game-specific implementations in the Game project. The Engine is designed to be game-agnostic, meaning it could potentially be reused for other game projects with similar requirements.

## Core Components

### Interfaces

The Engine defines key interfaces that the Game implementation must adhere to:

- **IResource**: Represents a gatherable resource in the game world
- **IBuilding**: Represents a building that can be constructed and used
- **IShip**: Represents a movable ship unit in the game
- **IPlayer**: Represents the player character and controls
- **IWorld**: Represents the game world and its contents
- **INotificationSystem**: Handles game notifications
- **IUIManager**: Manages UI elements and rendering
- **IRenderer**: Handles rendering of the game world

For detailed information on these interfaces, see the [API Reference](../api-reference.md).

### Renderer

The Renderer is responsible for visualizing the game world using OpenGL:

```csharp
public class Renderer : IDisposable
{
    public Renderer(GL gl);
    public void Render(IPlayer player, IWorld world);
    public void Update(float deltaTime);
    // ...additional rendering methods
}
```

The Renderer handles:

1. **Terrain Rendering**: Drawing the landscape using height-based rendering
2. **Water Rendering**: Rendering water surfaces with dynamic effects
3. **Object Rendering**: Drawing ships, buildings, and resources
4. **Shader Management**: Compiling and using OpenGL shaders
5. **Camera Transformations**: Converting player position and orientation to view matrices

### Constants

The Constants class provides game-wide settings and values:

```csharp
public static class Constants
{
    // World settings
    public const int WorldSize = 1000;
    public const float WaterLevel = 0.0f;
    
    // Screen settings
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    
    // Game speed
    public const float GameSpeed = 1.0f;
    
    // ... additional constants
}
```

Key constant categories include:

1. **World Settings**: Defines the size and characteristics of the game world
2. **Resource Settings**: Configuration for resource types and amounts
3. **Building Settings**: Parameters for building types
4. **Ship Settings**: Configuration for ship types and behaviors
5. **UI Settings**: Parameters for user interface elements

## Architecture

The Engine implements a clean architecture approach with these key design principles:

1. **Interface-Based Design**: Core game elements are defined as interfaces
2. **Dependency Injection**: Components receive their dependencies via constructors
3. **Separation of Concerns**: Rendering, game logic, and input handling are separated
4. **Immutability**: Where appropriate, data structures are made immutable
5. **Component-Based System**: Game objects are composed of different components

### Data Flow

The general flow of data through the Engine is:

1. **Input Handling**: Process keyboard, mouse, and other inputs
2. **Game State Update**: Update game objects based on input and elapsed time
3. **Physics/Collision**: Handle physical interactions between objects
4. **Rendering**: Visualize the current game state

## Implementation Details

### OpenGL Rendering

The Engine uses Silk.NET's OpenGL bindings to render the game world. Key components include:

#### Shaders

The Engine includes several shaders for different rendering purposes:

- **Basic Shader**: For rendering simple 3D objects
- **Terrain Shader**: For height-based terrain rendering
- **Water Shader**: For animating water surfaces
- **UI Shader**: For rendering 2D user interface elements

#### Mesh Generation

Meshes for terrain, water, and basic shapes are generated procedurally:

```csharp
private void GenerateTerrainMesh()
{
    // Create a grid of vertices based on terrain resolution
    // Calculate height values for each vertex
    // Generate triangle indices for the mesh
    // Upload vertex and index data to GPU
}
```

#### Texturing

The Engine supports texture mapping for various surfaces:

```csharp
private uint LoadTexture(string path)
{
    // Load image data from file
    // Generate OpenGL texture
    // Configure texture parameters
    // Upload image data to GPU
    // Return texture handle
}
```

### Math Utilities

The Engine provides various math utilities for 3D rendering and game logic:

- **Matrix Transformations**: For positioning and orienting objects
- **Vector Operations**: For position, direction, and velocity calculations
- **Collision Detection**: For interaction between objects
- **Random Generation**: For procedural content

### Resource Management

The Engine handles resource management to ensure efficient use of memory and GPU resources:

```csharp
public void Dispose()
{
    // Delete shaders
    gl.DeleteProgram(basicShader);
    gl.DeleteProgram(waterShader);
    gl.DeleteProgram(terrainShader);
    
    // Delete buffers
    gl.DeleteBuffer(terrainVbo);
    gl.DeleteVertexArray(terrainVao);
    
    // Delete textures
    gl.DeleteTexture(waterTexture);
    gl.DeleteTexture(skyboxTexture);
}
```

## Best Practices for Engine Usage

When working with the Engine components:

1. **Always implement the interfaces** as defined, ensuring all properties and methods are properly implemented
2. **Dispose of resources properly** by calling Dispose() on disposable objects
3. **Maintain separation of concerns** by keeping game-specific logic in the Game project
4. **Use the Constants class** for game-wide settings rather than hardcoding values
5. **Handle exceptions appropriately** to prevent crashes, especially in rendering code

For more detailed information on working with specific Engine components, see the individual class documentation in the following sections. 
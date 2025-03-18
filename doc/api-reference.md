# FirstPersonRTSGame - API Reference

This document provides a comprehensive reference for the public interfaces and classes in the FirstPersonRTSGame project.

## Table of Contents

- [Engine Interfaces](#engine-interfaces)
  - [IResource](#iresource)
  - [IBuilding](#ibuilding)
  - [IShip](#iship)
  - [IPlayer](#iplayer)
  - [IWorld](#iworld)
  - [INotificationSystem](#inotificationsystem)
  - [IUIManager](#iuimanager)
  - [IRenderer](#irenderer)
- [Engine Enums](#engine-enums)
  - [ResourceType](#resourcetype)
  - [BuildingType](#buildingtype)
  - [ShipType](#shiptype)
- [Game Classes](#game-classes)
  - [Game](#game)
  - [World](#world)
  - [Player](#player)
  - [Ship](#ship)
  - [Building](#building)
  - [Resource](#resource)
  - [UIManager](#uimanager)

## Engine Interfaces

### IResource

Interface representing a resource in the game world.

```csharp
public interface IResource
{
    Vector3 Position { get; }
    ResourceType Type { get; }
    int Amount { get; }
    Guid Id { get; }
    
    bool IsDepleted();
}
```

#### Properties

- `Position`: The 3D position of the resource in the world
- `Type`: The type of resource (Wood, Iron, etc.)
- `Amount`: The remaining amount of the resource
- `Id`: Unique identifier for the resource

#### Methods

- `IsDepleted()`: Returns true if the resource has been completely harvested

### IBuilding

Interface representing a building in the game world.

```csharp
public interface IBuilding
{
    Vector3 Position { get; }
    BuildingType Type { get; }
    float Health { get; }
    float MaxHealth { get; }
    bool IsActive { get; }
    float ConstructionProgress { get; }
    
    bool CanProduce(ResourceType resourceType);
    int GetResourceAmount(ResourceType resourceType);
    bool ConsumeResource(ResourceType resourceType, int amount);
    bool AddResource(ResourceType resourceType, int amount);
    void TakeDamage(float amount);
    void Repair(float amount);
}
```

#### Properties

- `Position`: The 3D position of the building in the world
- `Type`: The type of building (Headquarters, Shipyard, etc.)
- `Health`: Current health points of the building
- `MaxHealth`: Maximum health points of the building
- `IsActive`: Whether the building is currently active
- `ConstructionProgress`: Progress of construction (0.0 to 1.0)

#### Methods

- `CanProduce(ResourceType)`: Returns true if the building can produce the given resource type
- `GetResourceAmount(ResourceType)`: Gets the amount of the specified resource stored in the building
- `ConsumeResource(ResourceType, int)`: Attempts to consume the specified amount of resource, returns success
- `AddResource(ResourceType, int)`: Attempts to add the specified amount of resource, returns success
- `TakeDamage(float)`: Reduces the building's health by the specified amount
- `Repair(float)`: Increases the building's health by the specified amount

### IShip

Interface representing a ship in the game world.

```csharp
public interface IShip
{
    Vector3 Position { get; }
    Vector3 Rotation { get; }
    float Speed { get; }
    float Health { get; }
    float MaxHealth { get; }
    ShipType Type { get; }
    
    void Update(float deltaTime);
    int GetCargoAmount(ResourceType resourceType);
    bool CanAddCargo(ResourceType resourceType, int amount);
    bool AddCargo(ResourceType resourceType, int amount);
    int RemoveCargo(ResourceType resourceType, int amount);
    void TakeDamage(float amount);
    void Repair(float amount);
}
```

#### Properties

- `Position`: The 3D position of the ship in the world
- `Rotation`: The rotation of the ship
- `Speed`: The movement speed of the ship
- `Health`: Current health points of the ship
- `MaxHealth`: Maximum health points of the ship
- `Type`: The type of ship (Harvester, Scout, etc.)

#### Methods

- `Update(float)`: Updates the ship's state based on elapsed time
- `GetCargoAmount(ResourceType)`: Gets the amount of the specified resource in the ship's cargo
- `CanAddCargo(ResourceType, int)`: Checks if the ship can add the specified amount of resource
- `AddCargo(ResourceType, int)`: Attempts to add the specified amount of resource to cargo, returns success
- `RemoveCargo(ResourceType, int)`: Removes and returns the specified amount of resource from cargo
- `TakeDamage(float)`: Reduces the ship's health by the specified amount
- `Repair(float)`: Increases the ship's health by the specified amount

### IPlayer

Interface representing the player in the game world.

```csharp
public interface IPlayer
{
    Vector3 Position { get; }
    Vector3 Front { get; }
    Vector3 Up { get; }
    Vector3 Right { get; }
    float Yaw { get; }
    float Pitch { get; }
    
    void Update(float deltaTime, bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool moveUp, bool moveDown);
    void OnMouseMove(float mouseX, float mouseY);
    void OnMouseScroll(float yOffset);
    void OnKeyDown(Silk.NET.Input.Key key);
}
```

#### Properties

- `Position`: The 3D position of the player in the world
- `Front`: The forward vector of the player's view
- `Up`: The up vector of the player's view
- `Right`: The right vector of the player's view
- `Yaw`: The horizontal rotation angle of the player's view
- `Pitch`: The vertical rotation angle of the player's view

#### Methods

- `Update(float, bool, bool, bool, bool, bool, bool)`: Updates the player's state based on movement inputs
- `OnMouseMove(float, float)`: Handles mouse movement input
- `OnMouseScroll(float)`: Handles mouse scroll input
- `OnKeyDown(Key)`: Handles key press events

### IWorld

Interface representing the game world.

```csharp
public interface IWorld
{
    float WaterLevel { get; }
    float TimeOfDay { get; }
    IEnumerable<IResource> Resources { get; }
    IEnumerable<IBuilding> Buildings { get; }
    IEnumerable<IShip> Ships { get; }
    void Update(float deltaTime);
    float GetHeightAt(float x, float z);
}
```

#### Properties

- `WaterLevel`: The Y-coordinate of the water surface
- `TimeOfDay`: The current time of day (0.0 to 24.0)
- `Resources`: Collection of all resources in the world
- `Buildings`: Collection of all buildings in the world
- `Ships`: Collection of all ships in the world

#### Methods

- `Update(float)`: Updates the world state based on elapsed time
- `GetHeightAt(float, float)`: Returns the terrain height at the specified X,Z coordinates

### INotificationSystem

Interface for the game's notification system.

```csharp
public interface INotificationSystem
{
    void AddNotification(string message);
    void Update(float deltaTime);
}
```

#### Methods

- `AddNotification(string)`: Adds a new notification with the specified message
- `Update(float)`: Updates the notification system based on elapsed time

### IUIManager

Interface for the game's UI manager.

```csharp
public interface IUIManager
{
    int ScreenWidth { get; }
    int ScreenHeight { get; }
    void ShowNotification(string message);
    void Update(float deltaTime);
    void RenderUI(IPlayer player, IShip targetedShip, IBuilding targetedBuilding, Matrix4x4 projection);
}
```

#### Properties

- `ScreenWidth`: The width of the screen in pixels
- `ScreenHeight`: The height of the screen in pixels

#### Methods

- `ShowNotification(string)`: Displays a notification with the specified message
- `Update(float)`: Updates the UI state based on elapsed time
- `RenderUI(IPlayer, IShip, IBuilding, Matrix4x4)`: Renders the UI with the current game state

### IRenderer

Interface for the game's rendering system.

```csharp
public interface IRenderer
{
    void Render(IPlayer player, IWorld world);
    void Update(float deltaTime);
}
```

#### Methods

- `Render(IPlayer, IWorld)`: Renders the game world from the player's perspective
- `Update(float)`: Updates the renderer state based on elapsed time

## Engine Enums

### ResourceType

Enum representing the different types of resources in the game.

```csharp
public enum ResourceType
{
    Wood,
    Iron,
    Gold,
    Crystal,
    Oil,
    Money,
    Cobalt,
    Fuel,
    NuclearWaste,
    Hydrogen,
    Ammunition,
    NuclearFuel
}
```

### BuildingType

Enum representing the different types of buildings in the game.

```csharp
public enum BuildingType
{
    Headquarters,
    Shipyard,
    Workshop,
    Mine,
    Refinery,
    OilRig,
    Laboratory,
    Market,
    CobaltEnrichment,
    NuclearRecycler,
    Electrolysis,
    OilPlatform
}
```

### ShipType

Enum representing the different types of ships in the game.

```csharp
public enum ShipType
{
    Harvester,
    Scout,
    Cruiser,
    Transport,
    MarketTransporter,
    AmmunitionShip,
    NuclearFreighter,
    WarShip
}
```

## Game Classes

### Game

The main game class that coordinates all game systems.

```csharp
public class Game : IDisposable
{
    // Constructor
    public Game(IWindow window);
    
    // Methods
    public void Run();
    public void Update(float deltaTime);
    public void Render(float deltaTime);
    public void OnMouseMove(IMouse mouse, float x, float y);
    public void OnKeyDown(IKeyboard keyboard, Key key, int scancode);
    public void OnKeyUp(IKeyboard keyboard, Key key, int scancode);
    public void Dispose();
}
```

### World

Class implementing the game world.

```csharp
public class World : IWorld
{
    // Constructor
    public World(GL gl);
    
    // IWorld Implementation
    public float WaterLevel { get; }
    public float TimeOfDay { get; }
    public IEnumerable<IResource> Resources { get; }
    public IEnumerable<IBuilding> Buildings { get; }
    public IEnumerable<IShip> Ships { get; }
    public void Update(float deltaTime);
    public float GetHeightAt(float x, float z);
    
    // Additional Methods
    public void AddResource(Resource resource);
    public void AddBuilding(Building building);
    public void AddShip(Ship ship);
    public IResource? GetNearestResource(Vector3 position, ResourceType type, float maxDistance);
}
```

### Player

Class implementing the player character.

```csharp
public class Player : IPlayer
{
    // Constructor
    public Player(Vector3 startPosition);
    
    // IPlayer Implementation
    public Vector3 Position { get; }
    public Vector3 Front { get; }
    public Vector3 Up { get; }
    public Vector3 Right { get; }
    public float Yaw { get; }
    public float Pitch { get; }
    public void Update(float deltaTime, bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool moveUp, bool moveDown);
    public void OnMouseMove(float mouseX, float mouseY);
    public void OnMouseScroll(float yOffset);
    public void OnKeyDown(Silk.NET.Input.Key key);
    
    // Additional Methods
    public void SetWorld(World world);
    public bool Interact();
    public IShip? GetTargetedShip();
    public IBuilding? GetTargetedBuilding();
    public IResource? GetTargetedResource();
}
```

For more detailed information on specific class implementations, please refer to the corresponding class documentation in the Engine and Game documentation sections. 
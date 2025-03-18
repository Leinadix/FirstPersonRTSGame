# FirstPersonRTSGame - Game Documentation

This section documents the Game components of the FirstPersonRTSGame project. The Game project contains the concrete implementations of game objects and game-specific logic.

## Table of Contents

- [Overview](#overview)
- [Main Components](#main-components)
  - [Game Class](#game-class)
  - [World](#world)
  - [Player](#player)
  - [Ships](#ships)
  - [Buildings](#buildings)
  - [Resources](#resources)
- [UI System](#ui-system)
- [Game Mechanics](#game-mechanics)
- [Gameplay Systems](#gameplay-systems)

## Overview

The Game project (`FirstPersonRTSGame.Game`) implements the game-specific logic, building on the interfaces and systems provided by the Engine project. It contains concrete implementations of game objects (ships, buildings, resources), gameplay systems, and user interface components.

## Main Components

### Game Class

The Game class is the central coordinator for the game, managing the lifecycle and interaction between different game components:

```csharp
public class Game : IDisposable
{
    // Game components
    private IWindow window;
    private IInputContext input;
    private World world;
    private Player player;
    private Renderer renderer;
    private UIManager uiManager;
    
    // Game state flags
    private bool showInventory;
    private bool showBuildingMenu;
    
    // Constructor
    public Game(IWindow window)
    {
        // Initialize game components
    }
    
    // Main game loop
    public void Run()
    {
        // Run the game loop until exit
    }
    
    // Update game state
    public void Update(float deltaTime)
    {
        // Update world, player, and UI
    }
    
    // Render the game
    public void Render(float deltaTime)
    {
        // Render world and UI
    }
    
    // Input handlers
    public void OnMouseMove(IMouse mouse, float x, float y)
    {
        // Handle mouse movement
    }
    
    public void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        // Handle key presses
    }
    
    // Resource cleanup
    public void Dispose()
    {
        // Clean up resources
    }
}
```

The Game class:
- Initializes the game world, player, renderer, and UI
- Manages the main game loop
- Handles input from the user
- Updates game state based on elapsed time
- Renders the current game state
- Manages game-wide settings and flags

### World

The World class manages the game world and its contents:

```csharp
public class World : IWorld
{
    // Collections of game objects
    private List<Resource> resources;
    private List<Building> buildings;
    private List<Ship> ships;
    
    // World state
    private float timeOfDay;
    private float waterLevel;
    
    // Terrain system
    private TerrainGenerator terrainGenerator;
    
    // Constructor
    public World(GL gl)
    {
        // Initialize world and generate terrain
    }
    
    // IWorld implementation
    public float WaterLevel => waterLevel;
    public float TimeOfDay => timeOfDay;
    public IEnumerable<IResource> Resources => resources;
    public IEnumerable<IBuilding> Buildings => buildings;
    public IEnumerable<IShip> Ships => ships;
    
    public void Update(float deltaTime)
    {
        // Update world state and all contained objects
    }
    
    public float GetHeightAt(float x, float z)
    {
        // Return terrain height at specified coordinates
    }
    
    // World management methods
    public void AddResource(Resource resource)
    {
        // Add a resource to the world
    }
    
    public void AddBuilding(Building building)
    {
        // Add a building to the world
    }
    
    public void AddShip(Ship ship)
    {
        // Add a ship to the world
    }
    
    // Resource finding method
    public IResource? GetNearestResource(Vector3 position, ResourceType type, float maxDistance)
    {
        // Find nearest resource of specified type
    }
}
```

The World:
- Contains and manages all game objects (resources, buildings, ships)
- Maintains the terrain system
- Updates all contained objects each frame
- Provides querying capabilities for finding objects
- Manages time-of-day and other environmental factors

### Player

The Player class represents the user-controlled character:

```csharp
public class Player : IPlayer
{
    // Player state
    private Vector3 position;
    private Vector3 velocity;
    private float yaw;
    private float pitch;
    private Vector3 front;
    private Vector3 right;
    private Vector3 up;
    
    // Interaction
    private IResource? targetedResource;
    private IBuilding? targetedBuilding;
    private IShip? targetedShip;
    
    // Reference to world
    private World? world;
    
    // Inventory
    private Dictionary<ResourceType, int> inventory;
    
    // Constructor
    public Player(Vector3 startPosition)
    {
        // Initialize player state and inventory
    }
    
    // IPlayer implementation
    public Vector3 Position => position;
    public Vector3 Front => front;
    public Vector3 Up => up;
    public Vector3 Right => right;
    public float Yaw => yaw;
    public float Pitch => pitch;
    
    public void Update(float deltaTime, bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool moveUp, bool moveDown)
    {
        // Update player position and state
    }
    
    public void OnMouseMove(float mouseX, float mouseY)
    {
        // Update player view based on mouse movement
    }
    
    public void OnMouseScroll(float yOffset)
    {
        // Handle mouse scroll (e.g., for zoom)
    }
    
    public void OnKeyDown(Silk.NET.Input.Key key)
    {
        // Handle key presses
    }
    
    // World interaction
    public void SetWorld(World world)
    {
        // Set reference to world
    }
    
    public bool Interact()
    {
        // Interact with targeted object
    }
    
    // Getters for targeted objects
    public IShip? GetTargetedShip()
    {
        // Return currently targeted ship
    }
    
    public IBuilding? GetTargetedBuilding()
    {
        // Return currently targeted building
    }
    
    public IResource? GetTargetedResource()
    {
        // Return currently targeted resource
    }
}
```

The Player:
- Handles user input for movement and interaction
- Maintains player position and orientation
- Tracks what objects the player is targeting
- Manages the player's inventory
- Handles object interaction

### Ships

Ships are autonomous units that can gather resources and transport them:

```csharp
public class Ship : IShip
{
    // Ship properties
    private Vector3 position;
    private Vector3 rotation;
    private float speed;
    private float health;
    private float maxHealth;
    private ShipType type;
    
    // Cargo
    private Dictionary<ResourceType, int> cargo;
    private int maxCargoCapacity;
    private int currentCargoAmount;
    
    // State machine
    private ShipState currentState;
    
    // Constructor
    public Ship(Vector3 startPosition, ShipType shipType)
    {
        // Initialize ship based on type
    }
    
    // IShip implementation
    public Vector3 Position => position;
    public Vector3 Rotation => rotation;
    public float Speed => speed;
    public float Health => health;
    public float MaxHealth => maxHealth;
    public ShipType Type => type;
    
    public void Update(float deltaTime)
    {
        // Update ship state and position
    }
    
    public int GetCargoAmount(ResourceType resourceType)
    {
        // Return amount of specific resource in cargo
    }
    
    public bool CanAddCargo(ResourceType resourceType, int amount)
    {
        // Check if ship can hold more cargo
    }
    
    public bool AddCargo(ResourceType resourceType, int amount)
    {
        // Add resources to cargo
    }
    
    public int RemoveCargo(ResourceType resourceType, int amount)
    {
        // Remove resources from cargo
    }
    
    public void TakeDamage(float amount)
    {
        // Reduce ship health
    }
    
    public void Repair(float amount)
    {
        // Increase ship health
    }
    
    // Ship commands
    public void StartHarvesting(ResourceType resourceType)
    {
        // Command ship to harvest resources
    }
    
    public void SetTargetPosition(Vector3 target)
    {
        // Command ship to move to position
    }
    
    public void StopMovement()
    {
        // Command ship to stop
    }
}
```

Ships feature:
- Type-specific properties (speed, cargo capacity, etc.)
- Cargo management system
- Health and damage system
- State machine for autonomous behavior
- Command interface for player control

### Buildings

Buildings provide various functions in the game:

```csharp
public class Building : IBuilding
{
    // Building properties
    private Vector3 position;
    private BuildingType type;
    private float health;
    private float maxHealth;
    private bool isActive;
    private float constructionProgress;
    
    // Resources
    private Dictionary<ResourceType, int> storedResources;
    
    // Constructor
    public Building(Vector3 position, BuildingType type)
    {
        // Initialize building based on type
    }
    
    // IBuilding implementation
    public Vector3 Position => position;
    public BuildingType Type => type;
    public float Health => health;
    public float MaxHealth => maxHealth;
    public bool IsActive => isActive;
    public float ConstructionProgress => constructionProgress;
    
    public bool CanProduce(ResourceType resourceType)
    {
        // Check if building can produce resource type
    }
    
    public int GetResourceAmount(ResourceType resourceType)
    {
        // Get amount of stored resource
    }
    
    public bool ConsumeResource(ResourceType resourceType, int amount)
    {
        // Try to consume stored resources
    }
    
    public bool AddResource(ResourceType resourceType, int amount)
    {
        // Add resources to storage
    }
    
    public void TakeDamage(float amount)
    {
        // Reduce building health
    }
    
    public void Repair(float amount)
    {
        // Increase building health
    }
    
    // Building-specific methods
    public void Update(float deltaTime)
    {
        // Update building state
        
        // Progress construction if not complete
        if (constructionProgress < 1.0f)
        {
            constructionProgress += 0.1f * deltaTime;
            if (constructionProgress >= 1.0f)
            {
                constructionProgress = 1.0f;
                isActive = true;
            }
        }
        
        if (isActive)
        {
            // Building-specific behavior
            switch (type)
            {
                case BuildingType.Refinery:
                    ProcessResources(deltaTime);
                    break;
                case BuildingType.Shipyard:
                    RepairNearbyShips(deltaTime);
                    break;
                // Other building types...
            }
        }
    }
}
```

Buildings feature:
- Type-specific functionality
- Construction progress system
- Resource storage and processing
- Health and damage system
- Active/inactive state

### Resources

Resources are collectible items in the game world:

```csharp
public class Resource : IResource
{
    // Resource properties
    private Vector3 position;
    private ResourceType type;
    private int amount;
    private Guid id;
    
    // Constructor
    public Resource(Vector3 position, ResourceType type, int amount)
    {
        // Initialize resource
        this.position = position;
        this.type = type;
        this.amount = amount;
        this.id = Guid.NewGuid();
    }
    
    // IResource implementation
    public Vector3 Position => position;
    public ResourceType Type => type;
    public int Amount => amount;
    public Guid Id => id;
    
    public bool IsDepleted()
    {
        // Check if resource is depleted
        return amount <= 0;
    }
    
    // Resource-specific methods
    public int Harvest(int requestedAmount)
    {
        // Calculate how much can be harvested
        int harvestAmount = Math.Min(requestedAmount, amount);
        
        // Reduce resource amount
        amount -= harvestAmount;
        
        // Return harvested amount
        return harvestAmount;
    }
}
```

Resources feature:
- Type-specific properties
- Harvesting system
- Depletion tracking
- Unique identification

## UI System

The Game project includes a comprehensive UI system:

### UIManager

```csharp
public class UIManager : IUIManager, IDisposable
{
    // UI components
    private GL gl;
    private UIRenderer uiRenderer;
    private TextRenderer textRenderer;
    private UIIcons uiIcons;
    private NotificationSystem notificationSystem;
    
    // Screen dimensions
    private int screenWidth;
    private int screenHeight;
    
    // UI state
    private UIAnimations animations;
    private bool showInventory;
    private bool showBuildingMenu;
    private bool showHelp;
    
    // Constructor
    public UIManager(GL gl, int screenWidth, int screenHeight)
    {
        // Initialize UI components
    }
    
    // IUIManager implementation
    public int ScreenWidth => screenWidth;
    public int ScreenHeight => screenHeight;
    
    public void ShowNotification(string message)
    {
        // Display notification
    }
    
    public void Update(float deltaTime)
    {
        // Update UI components
    }
    
    public void RenderUI(IPlayer player, IShip targetedShip, IBuilding targetedBuilding, Matrix4x4 projection)
    {
        // Render UI elements
    }
    
    // UI element rendering
    private void RenderResourcePanel()
    {
        // Render resource display
    }
    
    private void RenderTargetInfo(IPlayer player, IShip targetedShip, IBuilding targetedBuilding)
    {
        // Render information about targeted object
    }
    
    private void RenderInventoryPanel(IPlayer player)
    {
        // Render player inventory
    }
    
    private void RenderBuildingMenu()
    {
        // Render building construction menu
    }
    
    private void RenderHelpPanel()
    {
        // Render help information
    }
    
    private void RenderCrosshair()
    {
        // Render targeting crosshair
    }
    
    // Resource cleanup
    public void Dispose()
    {
        // Clean up resources
    }
}
```

The UI system includes:
- Resource display
- Target information panel
- Inventory panel
- Building menu
- Help panel
- Notification system
- Crosshair and other HUD elements

### NotificationSystem

```csharp
public class NotificationSystem : INotificationSystem, IDisposable
{
    // Notification properties
    private List<Notification> activeNotifications;
    private int maxNotifications;
    private int screenWidth;
    private int screenHeight;
    
    // Rendering components
    private GL gl;
    private UIRenderer uiRenderer;
    private TextRenderer textRenderer;
    
    // Constructor
    public NotificationSystem(GL gl, UIRenderer uiRenderer, TextRenderer textRenderer, int screenWidth, int screenHeight)
    {
        // Initialize notification system
    }
    
    // INotificationSystem implementation
    public void AddNotification(string message)
    {
        // Add new notification
    }
    
    public void Update(float deltaTime)
    {
        // Update notifications (fade out, remove expired)
    }
    
    // Rendering
    public void Render(Matrix4x4 projection)
    {
        // Render active notifications
    }
    
    // Resource cleanup
    public void Dispose()
    {
        // Clean up resources
    }
}
```

The notification system provides:
- Toast-style notifications for game events
- Different notification types (info, warning, error)
- Automatic fade-in/fade-out animations
- Queuing system for multiple notifications

## Game Mechanics

### Resource Management

The game implements a resource management system where:
- Resources exist in the world and can be harvested
- Ships can collect and transport resources
- Buildings can store, process, and consume resources
- The player has an inventory of resources

The cycle works as follows:
1. Player commands ships to gather specific resources
2. Ships autonomously find and harvest resources
3. Ships transport resources to appropriate buildings
4. Buildings store and/or process resources
5. Processed resources can be used for construction, upgrades, etc.

### Building System

The building system allows:
- Placement of various building types
- Construction progress over time
- Resource requirements for construction
- Building-specific functionality when active
- Damage and repair mechanics

### Ship AI

Ships feature an autonomous behavior system:
- State machine controls behavior based on current state
- Ships can find the nearest appropriate resource
- Ships can navigate to resources and buildings
- Ships can harvest resources automatically
- Ships can find dropoff points for resources
- Ships can return to home position when idle

## Gameplay Systems

### Player Interaction

The player can interact with the world in several ways:
- Moving around in first-person view
- Looking at objects to target them
- Issuing commands to ships
- Placing buildings
- Collecting resources
- Viewing inventory and UI elements

### World Generation

The world generation system:
- Creates a procedural terrain with varying height
- Places water at a specific level
- Distributes resources throughout the world
- Places initial buildings and ships

### Day/Night Cycle

The game includes a day/night cycle that:
- Changes lighting conditions over time
- Affects visibility
- Provides visual variety

For more detailed information on specific Game components, see the individual class documentation in the following sections. 
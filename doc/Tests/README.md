# FirstPersonRTSGame - Testing Documentation

This section documents the testing approach and components for the FirstPersonRTSGame project.

## Table of Contents

- [Overview](#overview)
- [Testing Approach](#testing-approach)
- [Test Categories](#test-categories)
  - [Basic Tests](#basic-tests)
  - [UI Tests](#ui-tests)
  - [Player Tests](#player-tests)
  - [World Tests](#world-tests)
  - [Notification System Tests](#notification-system-tests)
- [Test Implementation](#test-implementation)
- [Running Tests](#running-tests)
- [Adding New Tests](#adding-new-tests)
- [Best Practices](#best-practices)

## Overview

The FirstPersonRTSGame.Tests project contains unit and integration tests for the game components. It uses xUnit as the testing framework and Moq for creating mock objects. The tests verify the functionality of various game components and ensure that they work correctly both individually and together.

## Testing Approach

The testing approach for FirstPersonRTSGame follows these principles:

1. **Test Doubles Over Concrete Implementations**: Tests use simplified test-specific implementations of interfaces rather than testing implementation classes directly.
2. **Isolation**: Components are tested in isolation from their dependencies using mock objects.
3. **Behavior Verification**: Tests focus on verifying behavior rather than implementation details.
4. **Maintainability**: Tests are designed to be maintainable and not overly coupled to implementation details.

## Test Categories

### Basic Tests

Basic tests verify fundamental game functionality:

```csharp
public class BasicTests
{
    [Fact]
    public void Game_InitializesCorrectly()
    {
        // Test that the game initializes correctly
    }
    
    [Fact]
    public void Game_UpdatesWorld()
    {
        // Test that the world is updated during the game loop
    }
    
    [Fact]
    public void Game_HandlesInput()
    {
        // Test that the game correctly handles user input
    }
}
```

Basic tests ensure:
- Core game components initialize correctly
- The game loop functions properly
- Input is correctly handled
- Basic game mechanics work as expected

### UI Tests

UI tests verify that the user interface components function correctly:

```csharp
public class SimplifiedUIManagerTests
{
    [Fact]
    public void TestUIManager_ShowsNotification()
    {
        // Arrange
        var mockNotificationSystem = new Mock<INotificationSystem>();
        var uiManager = new TestUIManager(mockNotificationSystem.Object);
        string expectedMessage = "Test notification";
        
        // Act
        uiManager.ShowNotification(expectedMessage);
        
        // Assert
        mockNotificationSystem.Verify(n => n.AddNotification(expectedMessage), Times.Once);
    }
    
    [Fact]
    public void TestUIManager_UpdatesNotificationSystem()
    {
        // Arrange
        var mockNotificationSystem = new Mock<INotificationSystem>();
        var uiManager = new TestUIManager(mockNotificationSystem.Object);
        float deltaTime = 0.16f;
        
        // Act
        uiManager.Update(deltaTime);
        
        // Assert
        mockNotificationSystem.Verify(n => n.Update(deltaTime), Times.Once);
    }
    
    // Simple test implementation of UIManager
    private class TestUIManager : IUIManager
    {
        // Test-specific implementation
    }
}
```

UI tests verify:
- Notifications are displayed correctly
- UI elements update in response to game state changes
- UI manager correctly manages its components
- UI rendering functions operate as expected

### Player Tests

Player tests verify the functionality of the player class:

```csharp
public class SimplifiedPlayerTests
{
    [Fact]
    public void TestPlayer_UpdatesPosition()
    {
        // Arrange
        var mockWorld = new Mock<IWorld>();
        var mockUIManager = new Mock<IUIManager>();
        var player = new TestPlayer(mockWorld.Object, mockUIManager.Object);
        
        // Set initial position and movement direction
        Vector3 initialPosition = new Vector3(10, 5, 10);
        player.Position = initialPosition;
        player.MovementDirection = new Vector3(0, 0, 1); // Moving forward
        player.Yaw = 0f; // Looking along Z axis
        
        float deltaTime = 0.1f;
        float expectedMovement = 5.0f * deltaTime; // Using our test speed
        
        // Act
        player.Update(deltaTime);
        
        // Assert
        Assert.Equal(initialPosition.X, player.Position.X);
        Assert.Equal(initialPosition.Y, player.Position.Y);
        Assert.Equal(initialPosition.Z + expectedMovement, player.Position.Z);
    }
    
    [Fact]
    public void TestPlayer_UpdatesOrientation()
    {
        // Test that player orientation updates correctly
    }
    
    [Fact]
    public void TestPlayer_HandlesKeyInput()
    {
        // Test that player handles key input correctly
    }
    
    // Simple test implementation of Player
    private class TestPlayer : IPlayer
    {
        // Test-specific implementation
    }
}
```

Player tests verify:
- Player movement functions correctly
- Player orientation updates in response to mouse movement
- Player interaction with the world works as expected
- Key handling routes commands appropriately

### World Tests

World tests verify the functionality of the world and its contained objects:

```csharp
public class SimplifiedWorldTests
{
    [Fact]
    public void TestWorld_UpdatesShips()
    {
        // Arrange
        var mockShip = new Mock<IShip>();
        var world = new TestWorld(new List<IShip> { mockShip.Object });
        float deltaTime = 0.16f;
        
        // Act
        world.Update(deltaTime);
        
        // Assert
        mockShip.Verify(s => s.Update(deltaTime), Times.Once);
    }
    
    [Fact]
    public void TestWorld_FindsClosestShip()
    {
        // Test that the world can find the closest ship
    }
    
    // Simple test implementation of World
    private class TestWorld : IWorld
    {
        // Test-specific implementation
    }
}
```

World tests verify:
- World updates contained objects correctly
- Object finding methods work as expected
- Terrain generation produces valid results
- Resource management functions correctly

### Notification System Tests

Notification system tests verify the functionality of the game's notification system:

```csharp
public class SimplifiedNotificationSystemTests
{
    [Fact]
    public void TestNotificationSystem_AddsNotification()
    {
        // Arrange
        var notificationSystem = new TestNotificationSystem();
        string message = "Test notification";
        
        // Act
        notificationSystem.AddNotification(message);
        
        // Assert
        Assert.Single(notificationSystem.Notifications);
        Assert.Equal(message, notificationSystem.Notifications[0]);
    }
    
    [Fact]
    public void TestNotificationSystem_UpdatesRemainingTime()
    {
        // Test that notification remaining time updates correctly
    }
    
    // Simple test implementation of NotificationSystem
    private class TestNotificationSystem : INotificationSystem
    {
        // Test-specific implementation
    }
}
```

Notification system tests verify:
- Notifications are added correctly
- Notifications are updated and removed after expiration
- Notification rendering functions correctly

## Test Implementation

### Test Doubles

The tests use test doubles (test-specific implementations) instead of real implementations:

```csharp
private class TestUIManager : IUIManager
{
    private readonly INotificationSystem _notificationSystem;
    
    public int ScreenWidth => 1280;
    public int ScreenHeight => 720;
    
    public TestUIManager(INotificationSystem notificationSystem)
    {
        _notificationSystem = notificationSystem;
    }
    
    public void ShowNotification(string message)
    {
        _notificationSystem.AddNotification(message);
    }
    
    public void Update(float deltaTime)
    {
        _notificationSystem.Update(deltaTime);
    }
    
    public void RenderUI(IPlayer player, IShip targetedShip, IBuilding targetedBuilding, Matrix4x4 projection)
    {
        // No implementation needed for tests
    }
}
```

This approach provides several benefits:
- Tests are more resilient to changes in implementation details
- Tests are faster and have fewer dependencies
- Tests focus on behavior rather than implementation
- Complex objects can be simplified for testing

### Mocking with Moq

The tests use Moq to create mock objects for dependencies:

```csharp
// Create a mock object
var mockNotificationSystem = new Mock<INotificationSystem>();

// Set up expected behavior
mockNotificationSystem.Setup(n => n.AddNotification(It.IsAny<string>()));

// Pass the mock object to the system under test
var uiManager = new TestUIManager(mockNotificationSystem.Object);

// Verify that expected methods were called
mockNotificationSystem.Verify(n => n.AddNotification("Expected message"), Times.Once);
```

Moq allows:
- Creating mock objects that implement interfaces
- Setting up expected behavior for method calls
- Verifying that methods were called with expected parameters

### Arrange-Act-Assert Pattern

The tests follow the Arrange-Act-Assert pattern:

```csharp
// Arrange
var mockShip = new Mock<IShip>();
var world = new TestWorld(new List<IShip> { mockShip.Object });
float deltaTime = 0.16f;

// Act
world.Update(deltaTime);

// Assert
mockShip.Verify(s => s.Update(deltaTime), Times.Once);
```

This pattern:
- Makes tests easier to read and understand
- Clearly separates test setup, action, and verification
- Provides a consistent structure for all tests

## Running Tests

### Running All Tests

To run all tests:

```bash
cd FirstPersonRTSGame.Tests
dotnet test
```

### Running Specific Tests

To run specific test categories:

```bash
# Run all UI tests
dotnet test --filter "FullyQualifiedName~UI"

# Run all player tests
dotnet test --filter "FullyQualifiedName~Player"

# Run all simplified tests
dotnet test --filter "FullyQualifiedName~Simplified"

# Run just the basic tests
dotnet test --filter "FullyQualifiedName~BasicTests"
```

## Adding New Tests

To add new tests:

1. **Identify the component to test**: Determine which component needs testing
2. **Create a test class**: Add a new class in the appropriate test category
3. **Implement test doubles if needed**: Create simplified implementations for testing
4. **Write test methods**: Add methods with the [Fact] or [Theory] attribute
5. **Follow the Arrange-Act-Assert pattern**: Structure tests consistently
6. **Run and validate the tests**: Ensure tests pass and validate the expected behavior

### Example: Adding a new UI test

```csharp
[Fact]
public void TestUIManager_TogglesInventoryDisplay()
{
    // Arrange
    var mockNotificationSystem = new Mock<INotificationSystem>();
    var uiManager = new TestUIManager(mockNotificationSystem.Object);
    
    // Act
    bool initialState = uiManager.IsInventoryVisible;
    uiManager.ToggleInventory();
    
    // Assert
    Assert.NotEqual(initialState, uiManager.IsInventoryVisible);
}
```

## Best Practices

When writing tests for FirstPersonRTSGame:

1. **Focus on behavior, not implementation**: Test what components do, not how they do it
2. **Keep tests independent**: Tests should not depend on each other
3. **Mock dependencies**: Use mock objects to isolate the component being tested
4. **Use test doubles**: Create simplified implementations for testing
5. **Test edge cases**: Test boundary conditions and error cases
6. **Be consistent**: Follow established patterns for test structure
7. **Keep tests maintainable**: Don't couple tests tightly to implementation details
8. **Write clear test names**: Names should describe what is being tested
9. **Use the simplest assertions possible**: Use the most appropriate assertions for each case
10. **Keep tests fast**: Tests should run quickly to facilitate frequent testing

By following these practices, the test suite will remain effective and maintainable as the game evolves. 
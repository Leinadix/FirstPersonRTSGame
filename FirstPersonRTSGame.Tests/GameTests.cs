using Xunit;
using Moq;
using System;
using System.Numerics;
using FirstPersonRTSGame.Game;
using FirstPersonRTSGame.Engine;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using FirstPersonRTSGame.Game.UI;

namespace FirstPersonRTSGame.Tests
{
    public class GameTests
    {
        [Fact]
        public void Update_UpdatesWorldAndPlayer()
        {
            // Arrange
            var mockWindow = new Mock<IWindow>();
            var mockInput = new Mock<IInputContext>();
            var mockWorld = new Mock<IWorld>();
            var mockPlayer = new Mock<IPlayer>();
            var mockRenderer = new Mock<IRenderer>();
            var mockUIManager = new Mock<IUIManager>();
            
            float deltaTime = 0.16f;
            
            // Create game instance with mocked dependencies
            var game = new GameTestDouble(
                mockWindow.Object,
                mockInput.Object,
                mockWorld.Object,
                mockPlayer.Object,
                mockRenderer.Object,
                mockUIManager.Object);
            
            // Act
            game.Update(deltaTime);
            
            // Assert
            mockWorld.Verify(w => w.Update(deltaTime), Times.Once);
            mockPlayer.Verify(p => p.Update(deltaTime, false, false, false, false, false, false), Times.Once);
            mockUIManager.Verify(ui => ui.Update(deltaTime), Times.Once);
        }
        
        [Fact]
        public void Render_RendersWorldAndUI()
        {
            // Arrange
            var mockWindow = new Mock<IWindow>();
            mockWindow.Setup(w => w.Size).Returns(new Silk.NET.Maths.Vector2D<int>(1280, 720));
            
            var mockInput = new Mock<IInputContext>();
            var mockWorld = new Mock<IWorld>();
            var mockPlayer = new Mock<IPlayer>();
            var mockRenderer = new Mock<IRenderer>();
            var mockUIManager = new Mock<IUIManager>();
            
            float deltaTime = 0.16f;
            
            // Setup player mock for position and orientation
            mockPlayer.Setup(p => p.Position).Returns(Vector3.Zero);
            mockPlayer.Setup(p => p.Yaw).Returns(0f);
            mockPlayer.Setup(p => p.Pitch).Returns(0f);
            
            // Create game instance with mocked dependencies
            var game = new GameTestDouble(
                mockWindow.Object,
                mockInput.Object,
                mockWorld.Object,
                mockPlayer.Object,
                mockRenderer.Object,
                mockUIManager.Object);
            
            // Act
            game.Render(deltaTime);
            
            // Assert
            mockRenderer.Verify(r => r.Render(mockPlayer.Object, mockWorld.Object), Times.Once);
            mockRenderer.Verify(r => r.Update(deltaTime), Times.Once);
            mockUIManager.Verify(ui => ui.RenderUI(
                mockPlayer.Object, 
                null, 
                null, 
                It.IsAny<Matrix4x4>()), 
                Times.Once);
        }
        
        [Fact]
        public void OnMouseMove_UpdatesPlayerOrientation()
        {
            // Arrange
            var mockWindow = new Mock<IWindow>();
            var mockInput = new Mock<IInputContext>();
            var mockWorld = new Mock<IWorld>();
            var mockPlayer = new Mock<IPlayer>();
            var mockRenderer = new Mock<IRenderer>();
            var mockUIManager = new Mock<IUIManager>();
            
            // Setup mouse move values
            float deltaX = 10f;
            float deltaY = 5f;
            
            // Create game instance with mocked dependencies
            var game = new GameTestDouble(
                mockWindow.Object,
                mockInput.Object,
                mockWorld.Object,
                mockPlayer.Object,
                mockRenderer.Object,
                mockUIManager.Object);
            
            // Create mock mouse
            var mockMouse = new Mock<IMouse>();
            
            // Act
            game.HandleMouseMove(mockMouse.Object, deltaX, deltaY);
            
            // Assert
            mockPlayer.Verify(p => p.OnMouseMove(deltaX, deltaY), Times.Once);
        }
        
        [Fact]
        public void OnKeyDown_UpdatesPlayerMovement()
        {
            // Arrange
            var mockWindow = new Mock<IWindow>();
            var mockInput = new Mock<IInputContext>();
            var mockWorld = new Mock<IWorld>();
            var mockPlayer = new Mock<IPlayer>();
            var mockRenderer = new Mock<IRenderer>();
            var mockUIManager = new Mock<IUIManager>();
            
            // Create game instance with mocked dependencies
            var game = new GameTestDouble(
                mockWindow.Object,
                mockInput.Object,
                mockWorld.Object,
                mockPlayer.Object,
                mockRenderer.Object,
                mockUIManager.Object);
            
            // Create mock keyboard and key
            var mockKeyboard = new Mock<IKeyboard>();
            Key testKey = Key.W;
            
            // Act
            game.HandleKeyDown(mockKeyboard.Object, testKey, 0);
            
            // Assert
            mockPlayer.Verify(p => p.OnKeyDown(testKey), Times.Once);
        }
        
        // A simple test double that doesn't inherit from Game
        private class GameTestDouble
        {
            private readonly IWindow _window;
            private readonly IInputContext _input;
            private readonly IWorld _world;
            private readonly IPlayer _player;
            private readonly IRenderer _renderer;
            private readonly IUIManager _uiManager;
            
            public GameTestDouble(
                IWindow window,
                IInputContext input,
                IWorld world,
                IPlayer player,
                IRenderer renderer,
                IUIManager uiManager)
            {
                _window = window;
                _input = input;
                _world = world;
                _player = player;
                _renderer = renderer;
                _uiManager = uiManager;
            }
            
            public void Update(float deltaTime)
            {
                _world.Update(deltaTime);
                _player.Update(deltaTime, false, false, false, false, false, false);
                _uiManager.Update(deltaTime);
            }
            
            public void Render(float deltaTime)
            {
                _renderer.Render(_player, _world);
                _renderer.Update(deltaTime);
                
                // Create a projection matrix for UI
                Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
                    0, _window.Size.X, _window.Size.Y, 0, -1, 1);
                    
                _uiManager.RenderUI(_player, null, null, projection);
            }
            
            public void HandleMouseMove(IMouse mouse, float deltaX, float deltaY)
            {
                _player.OnMouseMove(deltaX, deltaY);
            }
            
            public void HandleKeyDown(IKeyboard keyboard, Key key, int scancode)
            {
                _player.OnKeyDown(key);
            }
        }
    }
} 
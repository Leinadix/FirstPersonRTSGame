using Xunit;
using Moq;
using System;
using System.Numerics;
using FirstPersonRTSGame.Game;
using FirstPersonRTSGame.Game.UI;
using FirstPersonRTSGame.Engine;
using Silk.NET.Input;

namespace FirstPersonRTSGame.Tests.Player
{
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
            // Arrange
            var mockWorld = new Mock<IWorld>();
            var mockUIManager = new Mock<IUIManager>();
            var player = new TestPlayer(mockWorld.Object, mockUIManager.Object);
            
            // Set initial yaw and pitch
            float initialYaw = 0f;
            float initialPitch = 0f;
            player.Yaw = initialYaw;
            player.Pitch = initialPitch;
            
            // Mouse movement values
            float deltaX = 10f;
            float deltaY = 5f;
            float sensitivity = 0.1f;
            
            // Act
            player.OnMouseMove(deltaX, deltaY);
            
            // Assert
            Assert.Equal(initialYaw + deltaX * sensitivity, player.Yaw);
            Assert.Equal(initialPitch - deltaY * sensitivity, player.Pitch); // Inverted Y
        }
        
        [Fact]
        public void TestPlayer_HandlesKeyInput()
        {
            // Arrange
            var mockWorld = new Mock<IWorld>();
            var mockUIManager = new Mock<IUIManager>();
            var player = new TestPlayer(mockWorld.Object, mockUIManager.Object);
            
            // Initial movement direction
            player.MovementDirection = Vector3.Zero;
            
            // Act - Press W (forward)
            player.OnKeyDown(Key.W);
            
            // Assert
            Assert.Equal(1f, player.MovementDirection.Z);
            
            // Act - Press D (right)
            player.OnKeyDown(Key.D);
            
            // Assert - Both forward and right
            Assert.Equal(1f, player.MovementDirection.Z);
            Assert.Equal(1f, player.MovementDirection.X);
            
            // Act - Release W
            player.OnKeyUp(Key.W);
            
            // Assert - Only right remains
            Assert.Equal(0f, player.MovementDirection.Z);
            Assert.Equal(1f, player.MovementDirection.X);
        }
        
        // Simple test implementation of Player
        private class TestPlayer : IPlayer
        {
            private readonly IWorld _world;
            private readonly IUIManager _uiManager;
            private const float Speed = 5.0f;
            private const float MouseSensitivity = 0.1f;
            
            public Vector3 Position { get; set; } = new Vector3(500, 5, 500);
            public Vector3 MovementDirection { get; set; } = Vector3.Zero;
            public float Yaw { get; set; } = 0f;
            public float Pitch { get; set; } = 0f;
            public Vector3 Front => GetLookDirection();
            public Vector3 Up => Vector3.UnitY;
            public Vector3 Right => Vector3.Cross(Front, Up);
            
            public TestPlayer(IWorld world, IUIManager uiManager)
            {
                _world = world;
                _uiManager = uiManager;
            }
            
            public void Update(float deltaTime, bool moveForward = false, bool moveBackward = false, bool moveLeft = false, bool moveRight = false, bool moveUp = false, bool moveDown = false)
            {
                // Simple forward movement based on yaw
                float yawRadians = Yaw;
                
                if (MovementDirection != Vector3.Zero)
                {
                    // Create a rotation matrix for yaw
                    Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationY(yawRadians);
                    
                    // Apply rotation to movement direction
                    Vector3 rotatedMovement = Vector3.Transform(MovementDirection, rotationMatrix);
                    
                    // Update position based on movement
                    Position += rotatedMovement * Speed * deltaTime;
                }
            }
            
            public void OnMouseMove(float deltaX, float deltaY)
            {
                Yaw += deltaX * MouseSensitivity;
                Pitch -= deltaY * MouseSensitivity; // Inverted Y axis
            }
            
            public void OnMouseScroll(float yOffset)
            {
                // Not implemented for these tests
            }
            
            public void OnKeyDown(Key key)
            {
                switch (key)
                {
                    case Key.W:
                        MovementDirection = new Vector3(MovementDirection.X, MovementDirection.Y, 1);
                        break;
                    case Key.S:
                        MovementDirection = new Vector3(MovementDirection.X, MovementDirection.Y, -1);
                        break;
                    case Key.A:
                        MovementDirection = new Vector3(-1, MovementDirection.Y, MovementDirection.Z);
                        break;
                    case Key.D:
                        MovementDirection = new Vector3(1, MovementDirection.Y, MovementDirection.Z);
                        break;
                }
            }
            
            public void OnKeyUp(Key key)
            {
                switch (key)
                {
                    case Key.W:
                    case Key.S:
                        MovementDirection = new Vector3(MovementDirection.X, MovementDirection.Y, 0);
                        break;
                    case Key.A:
                    case Key.D:
                        MovementDirection = new Vector3(0, MovementDirection.Y, MovementDirection.Z);
                        break;
                }
            }
            
            private Vector3 GetLookDirection()
            {
                // Calculate forward vector based on yaw and pitch
                float yawRadians = Yaw;
                float pitchRadians = Pitch;
                
                float x = (float)Math.Sin(yawRadians) * (float)Math.Cos(pitchRadians);
                float y = (float)Math.Sin(pitchRadians);
                float z = (float)Math.Cos(yawRadians) * (float)Math.Cos(pitchRadians);
                
                return Vector3.Normalize(new Vector3(x, y, z));
            }
        }
    }
} 
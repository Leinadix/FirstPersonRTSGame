using Xunit;
using Moq;
using System;
using System.Numerics;
using System.Collections.Generic;
using FirstPersonRTSGame.Game.UI;
using FirstPersonRTSGame.Game;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Tests.UI
{
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
                // No implementation needed for these tests
            }
        }
    }
} 
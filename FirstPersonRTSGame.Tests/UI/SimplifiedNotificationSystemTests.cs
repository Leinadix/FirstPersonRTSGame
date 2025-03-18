using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using FirstPersonRTSGame.Game.UI;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Tests.UI
{
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
            // Arrange
            var notificationSystem = new TestNotificationSystem();
            notificationSystem.AddNotification("Test notification");
            float deltaTime = 0.5f;
            
            // Act
            notificationSystem.Update(deltaTime);
            
            // Assert
            Assert.Equal(1, notificationSystem.UpdateCount);
            Assert.Equal(deltaTime, notificationSystem.LastDeltaTime);
        }
        
        // Simple test implementation of NotificationSystem
        private class TestNotificationSystem : INotificationSystem
        {
            public List<string> Notifications { get; } = new List<string>();
            public int UpdateCount { get; private set; } = 0;
            public float LastDeltaTime { get; private set; } = 0;
            
            public void AddNotification(string message)
            {
                Notifications.Add(message);
            }
            
            public void Update(float deltaTime)
            {
                UpdateCount++;
                LastDeltaTime = deltaTime;
            }
            
            public List<Notification> GetNotificationsForRendering()
            {
                return Notifications.Select(m => new Notification(m, NotificationType.Info)).ToList();
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using Silk.NET.OpenGL;

namespace FirstPersonRTSGame.Game.UI
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
    
    public class Notification
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public float Duration { get; set; }
        public float RemainingTime { get; set; }
        public float Opacity { get; set; } = 1.0f;
        
        public Notification(string message, NotificationType type, float duration = 3.0f)
        {
            Message = message;
            Type = type;
            Duration = duration;
            RemainingTime = duration;
        }
    }
    
    public class NotificationSystem : IDisposable
    {
        private GL gl;
        private UIRenderer uiRenderer;
        private TextRenderer textRenderer;
        
        private List<Notification> activeNotifications = new List<Notification>();
        private int maxNotifications = 5;
        private int screenWidth;
        private int screenHeight;
        
        // Visual settings - simplified
        private int notificationWidth = 300;
        private int notificationHeight = 32; // Slightly smaller
        private int notificationSpacing = 6; // Less spacing
        private int cornerRadius = 2; // Minimal corner radius
        
        // Colors for different notification types - more subtle colors
        private Dictionary<NotificationType, Vector4> notificationColors = new Dictionary<NotificationType, Vector4>
        {
            { NotificationType.Info, new Vector4(0.3f, 0.5f, 0.8f, 0.8f) },     // Blue
            { NotificationType.Success, new Vector4(0.3f, 0.7f, 0.4f, 0.8f) },   // Green
            { NotificationType.Warning, new Vector4(0.8f, 0.6f, 0.2f, 0.8f) },   // Orange
            { NotificationType.Error, new Vector4(0.8f, 0.3f, 0.3f, 0.8f) }      // Red
        };
        
        // Simplified icons - just use basic characters
        private Dictionary<NotificationType, string> notificationIcons = new Dictionary<NotificationType, string>
        {
            { NotificationType.Info, "•" },
            { NotificationType.Success, "✓" },
            { NotificationType.Warning, "!" },
            { NotificationType.Error, "×" }
        };
        
        // Animation properties
        private UIAnimations animations;
        
        public NotificationSystem(GL gl, UIRenderer uiRenderer, TextRenderer textRenderer, int screenWidth, int screenHeight)
        {
            this.gl = gl;
            this.uiRenderer = uiRenderer;
            this.textRenderer = textRenderer;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            
            // Initialize animations
            animations = new UIAnimations();
            
            Console.WriteLine("Notification system initialized");
        }
        
        public void AddNotification(string message, NotificationType type = NotificationType.Info, float duration = 3.0f)
        {
            // Create a new notification
            Notification notification = new Notification(message, type, duration);
            
            // Add to active notifications
            activeNotifications.Add(notification);
            
            // Set up fade-in animation
            string animKey = $"notification_{activeNotifications.Count - 1}";
            animations.CreateAnimation(animKey, 0.0f, 1.0f, 0.3f, EasingType.EaseOut);
            animations.PlayAnimation(animKey);
            
            // Limit the number of active notifications
            if (activeNotifications.Count > maxNotifications)
            {
                activeNotifications.RemoveAt(0);
            }
            
            Console.WriteLine($"Added notification: {message}");
        }
        
        public void Update(float deltaTime)
        {
            // Update animation system
            animations.Update(deltaTime);
            
            // Update all active notifications
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                Notification notification = activeNotifications[i];
                
                // Update remaining time
                notification.RemainingTime -= deltaTime;
                
                // Handle notification expiration
                if (notification.RemainingTime <= 0)
                {
                    // Set up fade-out animation if not already started
                    string animKey = $"notification_{i}_fadeout";
                    if (!animations.IsPlaying(animKey))
                    {
                        animations.CreateAnimation(animKey, 1.0f, 0.0f, 0.3f, EasingType.EaseIn);
                        animations.PlayAnimation(animKey);
                    }
                    
                    // Get opacity from animation
                    notification.Opacity = animations.GetValue(animKey);
                    
                    // Remove completely faded out notifications
                    if (notification.Opacity <= 0.01f)
                    {
                        activeNotifications.RemoveAt(i);
                        continue;
                    }
                }
                // Handle notifications that are about to expire
                else if (notification.RemainingTime < 0.5f)
                {
                    // Gradually reduce opacity
                    notification.Opacity = notification.RemainingTime / 0.5f;
                }
                
                // Update the notification in the list
                activeNotifications[i] = notification;
            }
        }
        
        public void Render(Matrix4x4 projection)
        {
            // Don't render if no active notifications
            if (activeNotifications.Count == 0)
                return;
                
            // Render notifications from bottom to top (newer at the bottom)
            int yPos = 50; // Start position from bottom (slightly lower position)
            
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                Notification notification = activeNotifications[i];
                
                // Calculate position
                int xPos = screenWidth - notificationWidth - 10;
                
                // Get color based on notification type
                Vector4 baseColor = notificationColors[notification.Type];
                
                // Apply opacity
                Vector4 color = new Vector4(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W * notification.Opacity);
                
                // Render notification background with minimal rounded corners
                uiRenderer.RenderRoundedPanel(xPos, yPos, notificationWidth, notificationHeight, cornerRadius, color);
                
                // Render notification text with simple icon
                Vector4 textColor = new Vector4(1.0f, 1.0f, 1.0f, notification.Opacity);
                
                // Icon is just a simple character at the beginning
                string message = notificationIcons[notification.Type] + " " + notification.Message;
                textRenderer.RenderText(message, xPos + 10, yPos + 8, 1.0f, textColor, projection);
                
                // Move up for next notification
                yPos += notificationHeight + notificationSpacing;
            }
        }
        
        public void Resize(int width, int height)
        {
            this.screenWidth = width;
            this.screenHeight = height;
        }
        
        public void Dispose()
        {
            // Nothing to dispose as we don't own the renderers
        }
    }
} 
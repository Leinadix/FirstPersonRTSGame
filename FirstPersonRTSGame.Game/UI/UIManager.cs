using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Input;

namespace FirstPersonRTSGame.Game.UI
{
    public class UIManager : IDisposable
    {
        private GL gl;
        
        // UI components
        private UIRenderer uiRenderer;
        private TextRenderer textRenderer;
        private UIIcons uiIcons;
        private NotificationSystem notificationSystem;
        
        // UI state
        private bool showInventory = false;
        private bool showBuildingMenu = false;
        private bool showShipInfo = false;
        private bool showHelp = false;
        
        // Screen dimensions
        private int screenWidth;
        private int screenHeight;
        
        // UI element properties
        private float uiScale = 1.0f;
        private Vector4 primaryColor = new Vector4(0.15f, 0.15f, 0.15f, 0.85f); // Dark gray with transparency
        private Vector4 secondaryColor = new Vector4(0.08f, 0.08f, 0.08f, 0.75f); // Darker gray with transparency
        private Vector4 accentColor = new Vector4(0.4f, 0.7f, 1.0f, 1.0f); // Light blue
        private Vector4 textColor = new Vector4(0.9f, 0.9f, 0.9f, 1.0f); // Off-white
        private Vector4 warningColor = new Vector4(0.9f, 0.4f, 0.3f, 1.0f); // Soft red
        
        // Tooltip system
        private string currentTooltip = "";
        private Vector2 tooltipPosition;
        private bool showTooltip = false;
        
        // Button tracking for interactivity
        private List<UIButton> activeButtons = new List<UIButton>();
        private UIButton? hoveredButton = null;
        
        // Animation system
        private UIAnimations animations;
        private float deltaTime = 1.0f / 60.0f; // Default value, will be updated
        
        // UI sizing constants
        private const int PANEL_PADDING = 10;
        private const int ELEMENT_SPACING = 8;
        private const int BUTTON_HEIGHT = 30;
        private const int CORNER_RADIUS = 3; // Smaller corner radius for minimal look
        
        public UIManager(GL gl, int screenWidth, int screenHeight)
        {
            this.gl = gl;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            
            // Initialize UI components
            uiRenderer = new UIRenderer(gl, screenWidth, screenHeight);
            textRenderer = new TextRenderer(gl);
            uiIcons = new UIIcons(gl);
            
            // Initialize notification system
            notificationSystem = new NotificationSystem(gl, uiRenderer, textRenderer, screenWidth, screenHeight);
            
            // Initialize animations
            animations = new UIAnimations();
            SetupAnimations();
            
            // Initialize UI elements
            InitializeUI();
            
            // Log initialization
            Console.WriteLine("UI Manager initialized with screen dimensions: " + screenWidth + "x" + screenHeight);
        }
        
        private void SetupAnimations()
        {
            // Panel animations
            animations.CreateAnimation("inventory_panel", 0.0f, 1.0f, 0.3f, EasingType.EaseOut);
            animations.CreateAnimation("building_panel", 0.0f, 1.0f, 0.3f, EasingType.EaseOut);
            animations.CreateAnimation("help_panel", 0.0f, 1.0f, 0.3f, EasingType.EaseOut);
            
            // Color transitions
            animations.CreateColorTransition("button_hover", primaryColor, accentColor, 0.2f, EasingType.EaseOut);
            animations.CreateColorTransition("highlight_pulse", accentColor, 
                new Vector4(accentColor.X, accentColor.Y * 0.7f, accentColor.Z * 0.5f, accentColor.W),
                1.0f, EasingType.EaseInOut);
            
            // Start pulsating highlight
            animations.PlayAnimation("highlight_pulse");
        }
        
        public void Render(Player player, World world)
        {
            // Reset active buttons for this frame
            activeButtons.Clear();
            
            // Calculate common projection matrix for UI
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
                0, screenWidth, screenHeight, 0, -1, 1);
                
            // Start UI rendering
            gl.Disable(EnableCap.DepthTest);
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            // Render base UI components
            RenderHUD(player, projection);
            
            // Render optional UI panels based on state
            if (showInventory)
            {
                RenderInventoryPanel(player, projection);
            }
            
            if (showBuildingMenu)
            {
                RenderBuildingMenu(player, projection);
            }
            
            if (showShipInfo && player.GetTargetedShip() != null)
            {
                RenderShipInfoPanel(player, projection);
            }
            
            if (showHelp)
            {
                RenderHelpPanel(projection);
            }
            
            // Render tooltip if needed
            if (showTooltip)
            {
                RenderTooltip(projection);
            }
            
            // Render notifications on top of everything else
            notificationSystem.Render(projection);
            
            // Restore OpenGL state
            gl.Enable(EnableCap.DepthTest);
            gl.Disable(EnableCap.Blend);
        }
        
        private void RenderHUD(Player player, Matrix4x4 projection)
        {
            // Display crosshair in the center of the screen
            RenderCrosshair(projection);
            
            // Display resource counts in top-left corner
            RenderResourceDisplay(player, projection);
            
            // Display target info (resource/building/ship) if player is targeting something
            if (player.GetTargetedResource() != null || player.GetTargetedBuilding() != null || player.GetTargetedShip() != null)
            {
                RenderTargetInfo(player, projection);
            }
            
            // Display notifications
            notificationSystem.Render(projection);
        }
        
        private void RenderCrosshair(Matrix4x4 projection)
        {
            // Center of the screen
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;
            
            // Simple dot crosshair
            int size = 2;
            uiRenderer.RenderPanel(centerX - size, centerY - size, size * 2, size * 2, textColor);
            
            // Simple lines extending from the center
            int lineLength = 5;
            int lineGap = 2;
            
            // Horizontal line (left)
            uiRenderer.RenderPanel(centerX - lineLength - lineGap, centerY - 1, lineLength, 2, textColor);
            // Horizontal line (right)
            uiRenderer.RenderPanel(centerX + lineGap, centerY - 1, lineLength, 2, textColor);
            // Vertical line (top)
            uiRenderer.RenderPanel(centerX - 1, centerY - lineLength - lineGap, 2, lineLength, textColor);
            // Vertical line (bottom)
            uiRenderer.RenderPanel(centerX - 1, centerY + lineGap, 2, lineLength, textColor);
        }
        
        private void RenderResourceDisplay(Player player, Matrix4x4 projection)
        {
            // Resources to display
            string[] resourceLabels = { "Wood", "Iron", "Gold", "Crystal", "Oil" };
            int[] resourceValues = {
                player.GetResourceAmount(Engine.ResourceType.Wood),
                player.GetResourceAmount(Engine.ResourceType.Iron),
                player.GetResourceAmount(Engine.ResourceType.Gold),
                player.GetResourceAmount(Engine.ResourceType.Crystal),
                player.GetResourceAmount(Engine.ResourceType.Oil)
            };
            
            // Position in top-left corner
            int xPos = 10;
            int yPos = 10;
            int width = 120;
            int height = 10 + (resourceLabels.Length * 20) + 10;
            
            // Render background panel with minimal style
            uiRenderer.RenderRoundedPanel(xPos, yPos, width, height, CORNER_RADIUS, primaryColor);
            
            // Simple "Resources" title at the top
            textRenderer.RenderText("Resources", xPos + 10, yPos + 10, 1.0f, textColor, projection);
            
            // Render each resource count
            for (int i = 0; i < resourceLabels.Length; i++)
            {
                int resourceY = yPos + 30 + (i * 20);
                textRenderer.RenderText(resourceLabels[i] + ":", xPos + 10, resourceY, 0.9f, textColor, projection);
                textRenderer.RenderText(resourceValues[i].ToString(), xPos + 70, resourceY, 0.9f, textColor, projection);
            }
        }
        
        private void RenderTargetInfo(Player player, Matrix4x4 projection)
        {
            IResource? targetedResource = player.GetTargetedResource();
            IBuilding? targetedBuilding = player.GetTargetedBuilding();
            IShip? targetedShip = player.GetTargetedShip();
            
            if (targetedResource != null)
            {
                RenderResourceTargetInfo(targetedResource, projection);
            }
            else if (targetedBuilding != null)
            {
                RenderBuildingTargetInfo(targetedBuilding, projection);
            }
            else if (targetedShip != null)
            {
                RenderShipTargetInfo(targetedShip, projection);
            }
        }
        
        private void RenderResourceTargetInfo(IResource resource, Matrix4x4 projection)
        {
            // Calculate position in bottom-middle
            int width = 200;
            int height = 70;
            int xPos = (screenWidth - width) / 2;
            int yPos = screenHeight - height - 20;
            
            // Render simple panel
            uiRenderer.RenderRoundedPanel(xPos, yPos, width, height, CORNER_RADIUS, secondaryColor);
            
            // Resource type
            string typeStr = resource.Type.ToString();
            textRenderer.RenderText(typeStr, xPos + 10, yPos + 10, 1.1f, accentColor, projection);
            
            // Resource amount
            textRenderer.RenderText($"Amount: {resource.Amount}", xPos + 10, yPos + 35, 1.0f, textColor, projection);
        }
        
        private void RenderBuildingTargetInfo(IBuilding building, Matrix4x4 projection)
        {
            // Calculate position in bottom-middle
            int width = 200;
            int height = 70;
            int xPos = (screenWidth - width) / 2;
            int yPos = screenHeight - height - 20;
            
            // Render simple panel
            uiRenderer.RenderRoundedPanel(xPos, yPos, width, height, CORNER_RADIUS, secondaryColor);
            
            // Building type
            string typeStr = building.Type.ToString();
            textRenderer.RenderText(typeStr, xPos + 10, yPos + 10, 1.1f, accentColor, projection);
            
            // Construction progress (if we have this information)
            textRenderer.RenderText($"Progress: {building.ConstructionProgress * 100:F0}%", xPos + 10, yPos + 35, 1.0f, textColor, projection);
            
            // Health
            textRenderer.RenderText($"Health: {building.Health:F0}%", xPos + 10, yPos + 50, 1.0f, textColor, projection);
        }
        
        private void RenderShipTargetInfo(IShip ship, Matrix4x4 projection)
        {
            // Calculate position in bottom-middle
            int width = 200;
            int height = 60;
            int xPos = (screenWidth - width) / 2;
            int yPos = screenHeight - height - 20;
            
            // Render simple panel
            uiRenderer.RenderRoundedPanel(xPos, yPos, width, height, CORNER_RADIUS, secondaryColor);
            
            // Ship type
            string typeStr = ship.Type.ToString();
            textRenderer.RenderText(typeStr, xPos + 10, yPos + 10, 1.1f, accentColor, projection);
            
            // Health
            textRenderer.RenderText($"Health: {ship.Health:F0}%", xPos + 10, yPos + 35, 1.0f, textColor, projection);
        }
        
        private void RenderInventoryPanel(Player player, Matrix4x4 projection)
        {
            if (!showInventory)
                return;
            
            // Calculate position (centered, taking up most of the screen)
            int width = 300;
            int height = 400;
            int xPos = (screenWidth - width) / 2;
            int yPos = (screenHeight - height) / 2;
            
            // Get animation value (0.0 to 1.0)
            float animValue = animations.GetValue("inventory_panel");
            
            // Apply animation - slide in from top
            yPos = (int)((yPos - height) * (1.0f - animValue) + yPos * animValue);
            
            // Render main panel with subtly rounded corners
            uiRenderer.RenderRoundedPanel(xPos, yPos, width, height, CORNER_RADIUS, primaryColor);
            
            // Title
            int titleY = yPos + 15;
            textRenderer.RenderText("Inventory", xPos + 15, titleY, 1.2f, accentColor, projection);
            
            // Divider - just a thin line
            uiRenderer.RenderPanel(xPos + 10, titleY + 25, width - 20, 1, new Vector4(0.3f, 0.3f, 0.3f, 0.5f));
            
            // Resource list
            int resourceY = titleY + 40;
            int resourceSpacing = 22;
            
            // Column headers
            textRenderer.RenderText("Resource", xPos + 20, resourceY, 1.0f, textColor, projection);
            textRenderer.RenderText("Amount", xPos + 200, resourceY, 1.0f, textColor, projection);
            
            resourceY += resourceSpacing + 5;
            
            // Resource entries
            string[] resourceNames = Enum.GetNames(typeof(Engine.ResourceType));
            for (int i = 0; i < resourceNames.Length; i++)
            {
                string resourceName = resourceNames[i];
                int amount = player.GetResourceAmount((Engine.ResourceType)i);
                
                textRenderer.RenderText(resourceName, xPos + 20, resourceY + i * resourceSpacing, 0.9f, textColor, projection);
                textRenderer.RenderText(amount.ToString(), xPos + 200, resourceY + i * resourceSpacing, 0.9f, textColor, projection);
            }
            
            // Close button - simple text in the footer
            int footerY = yPos + height - 30;
            textRenderer.RenderText("Press I to close", xPos + width / 2 - 50, footerY, 0.9f, textColor, projection);
        }
        
        private void RenderBuildingMenu(Player player, Matrix4x4 projection)
        {
            // Apply animation for sliding in
            float animProgress = animations.GetValue("building_panel");
            int yOffset = (int)((1.0f - animProgress) * 300);
            
            // Panel positioning
            int xPos = screenWidth / 2 - 250;
            int yPos = screenHeight / 2 - 200;
            int width = 500;
            int height = 400;
            
            // Render panel background
            uiRenderer.RenderPanel(xPos, yPos, width, height, secondaryColor);
            
            // Render header
            textRenderer.RenderText("BUILDING MENU", xPos + width/2 - 80, yPos + 20, 1.5f, accentColor, projection);
            uiRenderer.RenderLine(xPos + 20, yPos + 50, xPos + width - 20, yPos + 50, 2, primaryColor);
            
            // Building options - grid layout
            string[] buildingTypes = { "Headquarters", "Shipyard", "Workshop", "Mine" };
            int gridSize = 2; // 2x2 grid
            int itemWidth = 220;
            int itemHeight = 150;
            int paddingX = 20;
            int paddingY = 30;
            int startY = yPos + 70;
            
            for (int i = 0; i < buildingTypes.Length; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;
                
                int itemX = xPos + paddingX + col * (itemWidth + paddingX);
                int itemY = startY + row * (itemHeight + paddingY);
                
                // Draw building option panel
                string iconName = buildingTypes[i].ToLower();
                Vector4 itemColor = secondaryColor;
                itemColor.W = 0.9f;
                
                // Draw item background
                uiRenderer.RenderPanel(itemX, itemY, itemWidth, itemHeight, itemColor);
                
                // Draw building icon
                uiIcons.RenderIcon(iconName, itemX + itemWidth/2 - 32, itemY + 20, 64, textColor, projection);
                
                // Draw building name
                textRenderer.RenderText(buildingTypes[i], itemX + itemWidth/2 - buildingTypes[i].Length * 5, itemY + 95, 1.2f, accentColor, projection);
                
                // Draw cost information
                textRenderer.RenderText("Cost: 100 Wood, 50 Iron", itemX + 10, itemY + 125, 0.8f, textColor, projection);
                
                // Add button functionality
                int index = i; // Capture for lambda
                UIButton buildButton = AddButton(
                    itemX, itemY, itemWidth, itemHeight, 
                    "", new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), 
                    () => {
                        Console.WriteLine($"Building {buildingTypes[index]}");
                        showBuildingMenu = false;
                    });
                
                // Add tooltip to button
                buildButton.Tooltip = $"Build a {buildingTypes[i]}";
            }
            
            // Add close button
            int closeButtonX = xPos + width - 40;
            int closeButtonY = yPos + 15;
            UIButton closeButton = AddButton(
                closeButtonX, closeButtonY, 25, 25, 
                "X", accentColor, secondaryColor, 
                () => { showBuildingMenu = false; });
                
            // Render footer with close instructions
            textRenderer.RenderText("Press B to close", xPos + width/2 - 60, yPos + height - 30, 1.0f, primaryColor, projection);
        }
        
        private void RenderShipInfoPanel(Player player, Matrix4x4 projection)
        {
            if (player.GetTargetedShip() is not Ship ship) return;
            
            // Panel positioning
            int xPos = screenWidth - 250;
            int yPos = 20;
            int width = 230;
            int height = 180;
            
            // Render panel background
            uiRenderer.RenderPanel(xPos, yPos, width, height, secondaryColor);
            
            // Render header
            textRenderer.RenderText("SHIP STATUS", xPos + 20, yPos + 20, 1.2f, accentColor, projection);
            uiRenderer.RenderLine(xPos + 10, yPos + 40, xPos + width - 10, yPos + 40, 1, primaryColor);
            
            // Render ship state
            string state = "Active";  // Default fallback
            if (ship != null)
            {
                state = ship.Type.ToString();  // Use type info if CurrentState isn't available
            }
            textRenderer.RenderText($"State: {state}", xPos + 20, yPos + 60, 1.0f, textColor, projection);
            
            // Render cargo header
            textRenderer.RenderText("Cargo:", xPos + 20, yPos + 85, 1.0f, accentColor, projection);
            
            // Render cargo contents
            int cargoY = yPos + 110;
            bool hasCargo = false;
            
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                int amount = ship.GetCargoAmount(resourceType);
                if (amount > 0)
                {
                    string resourceName = resourceType.ToString();
                    string iconName = resourceName.ToLower();
                    
                    // Draw resource icon and amount
                    uiIcons.RenderIcon(iconName, xPos + 30, cargoY, 16, textColor, projection);
                    textRenderer.RenderText($"{resourceName}: {amount}", xPos + 55, cargoY, 0.9f, textColor, projection);
                    cargoY += 20;
                    hasCargo = true;
                }
            }
            
            if (!hasCargo)
            {
                textRenderer.RenderText("No cargo", xPos + 30, cargoY, 0.9f, textColor, projection);
            }
        }
        
        private void RenderHelpPanel(Matrix4x4 projection)
        {
            // Apply animation for sliding in
            float animProgress = animations.GetValue("help_panel");
            int yOffset = (int)((1.0f - animProgress) * 300);
            
            // Panel positioning
            int xPos = screenWidth / 2 - 250;
            int yPos = screenHeight / 2 - 250;
            int width = 500;
            int height = 500;
            
            // Render panel background
            uiRenderer.RenderPanel(xPos, yPos, width, height, secondaryColor);
            
            // Render header
            textRenderer.RenderText("GAME CONTROLS", xPos + width/2 - 80, yPos + 20, 1.5f, accentColor, projection);
            uiRenderer.RenderLine(xPos + 20, yPos + 50, xPos + width - 20, yPos + 50, 2, primaryColor);
            
            // Control groups
            Dictionary<string, List<(string, string)>> controlGroups = new Dictionary<string, List<(string, string)>>
            {
                { "Movement", new List<(string, string)> {
                    ("WASD", "Move player"),
                    ("Mouse", "Look around"),
                    ("Space", "Move up"),
                    ("Shift", "Move down")
                }},
                { "Interaction", new List<(string, string)> {
                    ("E", "Interact with objects"),
                    ("I", "Toggle inventory"),
                    ("B", "Toggle building menu"),
                    ("F1", "Toggle help")
                }},
                { "Ship Commands", new List<(string, string)> {
                    ("1", "Harvest Wood"),
                    ("2", "Harvest Iron"),
                    ("3", "Harvest Gold"),
                    ("4", "Harvest Crystal"),
                    ("5", "Harvest Oil"),
                    ("R", "Return to base")
                }}
            };
            
            // Render control groups
            int groupY = yPos + 70;
            foreach (var group in controlGroups)
            {
                // Group header
                textRenderer.RenderText(group.Key, xPos + 30, groupY, 1.2f, accentColor, projection);
                uiRenderer.RenderLine(xPos + 30, groupY + 25, xPos + 200, groupY + 25, 1, primaryColor);
                groupY += 40;
                
                // Controls in this group
                foreach (var control in group.Value)
                {
                    // Key/button
                    uiRenderer.RenderPanel(xPos + 40, groupY - 5, 80, 30, primaryColor);
                    textRenderer.RenderText(control.Item1, xPos + 45, groupY, 1.0f, textColor, projection);
                    
                    // Description
                    textRenderer.RenderText(control.Item2, xPos + 140, groupY, 1.0f, textColor, projection);
                    
                    groupY += 35;
                }
                
                groupY += 20; // Space between groups
            }
            
            // Add close button
            int closeButtonX = xPos + width - 40;
            int closeButtonY = yPos + 15;
            UIButton closeButton = AddButton(
                closeButtonX, closeButtonY, 25, 25, 
                "X", accentColor, secondaryColor, 
                () => { showHelp = false; });
                
            // Render footer with close instructions
            textRenderer.RenderText("Press F1 to close", xPos + width/2 - 70, yPos + height - 30, 1.0f, primaryColor, projection);
        }
        
        private void RenderTooltip(Matrix4x4 projection)
        {
            if (string.IsNullOrEmpty(currentTooltip))
                return;
                
            float textWidth = currentTooltip.Length * 8; // Estimate text width
            int xPos = (int)tooltipPosition.X;
            int yPos = (int)tooltipPosition.Y;
            int width = (int)textWidth + 20;
            int height = 30;
            
            // Ensure tooltip stays within screen bounds
            if (xPos + width > screenWidth)
                xPos = screenWidth - width - 10;
            if (yPos + height > screenHeight)
                yPos = yPos - height - 10;
            
            // Render tooltip background
            uiRenderer.RenderPanel(xPos, yPos, width, height, secondaryColor);
            
            // Render tooltip text
            textRenderer.RenderText(currentTooltip, xPos + 10, yPos + 7, 0.9f, textColor, projection);
        }
        
        public UIButton AddButton(int x, int y, int width, int height, string text, Vector4 textColor, Vector4 backgroundColor, Action onClick)
        {
            UIButton button = new UIButton
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = text,
                TextColor = textColor,
                BackgroundColor = backgroundColor,
                OnClick = onClick
            };
            
            activeButtons.Add(button);
            return button;
        }
        
        public void HandleMouseMove(float mouseX, float mouseY)
        {
            hoveredButton = null;
            showTooltip = false;
            
            // Check if mouse is over any button
            foreach (var button in activeButtons)
            {
                if (mouseX >= button.X && mouseX <= button.X + button.Width &&
                    mouseY >= button.Y && mouseY <= button.Y + button.Height)
                {
                    hoveredButton = button;
                    
                    // Show tooltip if the button has one
                    if (!string.IsNullOrEmpty(button.Tooltip))
                    {
                        currentTooltip = button.Tooltip;
                        tooltipPosition = new Vector2(mouseX, mouseY - 30);
                        showTooltip = true;
                    }
                    
                    break;
                }
            }
        }
        
        public void HandleMouseClick(float mouseX, float mouseY)
        {
            // Check if any button was clicked
            foreach (var button in activeButtons)
            {
                if (mouseX >= button.X && mouseX <= button.X + button.Width &&
                    mouseY >= button.Y && mouseY <= button.Y + button.Height)
                {
                    // Execute button's action
                    button.OnClick?.Invoke();
                    break;
                }
            }
        }
        
        public void ToggleInventory()
        {
            showInventory = !showInventory;
            
            // Play animation
            animations.PlayAnimation("inventory_panel", !showInventory);
            
            // Close other panels when opening inventory
            if (showInventory)
            {
                showBuildingMenu = false;
                showHelp = false;
                animations.PlayAnimation("building_panel", true);
                animations.PlayAnimation("help_panel", true);
            }
        }
        
        public void ToggleBuildingMenu()
        {
            showBuildingMenu = !showBuildingMenu;
            
            // Play animation
            animations.PlayAnimation("building_panel", !showBuildingMenu);
            
            // Close other panels when opening building menu
            if (showBuildingMenu)
            {
                showInventory = false;
                showHelp = false;
                animations.PlayAnimation("inventory_panel", true);
                animations.PlayAnimation("help_panel", true);
            }
        }
        
        public void ToggleHelp()
        {
            showHelp = !showHelp;
            
            // Play animation
            animations.PlayAnimation("help_panel", !showHelp);
            
            // Close other panels when opening help panel
            if (showHelp)
            {
                showInventory = false;
                showBuildingMenu = false;
                animations.PlayAnimation("inventory_panel", true);
                animations.PlayAnimation("building_panel", true);
            }
        }
        
        public void Update(Player player, World world, Vector2 mousePosition, float deltaTime)
        {
            // Store delta time for animations
            this.deltaTime = deltaTime;
            
            // Update animations
            animations.Update(deltaTime);
            
            // Update notifications
            notificationSystem.Update(deltaTime);
        }
        
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3.0f)
        {
            notificationSystem.AddNotification(message, type, duration);
        }
        
        public void Resize(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            uiRenderer.Resize(width, height);
            notificationSystem.Resize(width, height);
        }
        
        public void Dispose()
        {
            uiRenderer.Dispose();
            textRenderer.Dispose();
            uiIcons.Dispose();
            notificationSystem.Dispose();
        }
        
        public void AdjustFontSettings(int fontSize, bool useAntiAliasing)
        {
            if (textRenderer != null)
            {
                textRenderer.SetFontSize(fontSize);
                textRenderer.SetAntiAliasing(useAntiAliasing);
                
                // Re-create any UI elements that depend on text rendering
                InitializeUI();
            }
        }
        
        public void IncreaseFontReadability()
        {
            // Use a larger font size with anti-aliasing enabled for maximum readability
            AdjustFontSettings(22, true);
        }
        
        // Initialize UI elements that depend on text renderer settings
        private void InitializeUI()
        {
            // This method will reinitialize any UI elements that depend on text rendering settings
            // Currently not needed for basic functionality, but called when font settings change
            // You can add specific UI reinitializations here if needed
        }
    }
    
    public class UIButton
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; } = "";
        public Vector4 TextColor { get; set; }
        public Vector4 BackgroundColor { get; set; }
        public string Tooltip { get; set; } = "";
        public Action? OnClick { get; set; }
    }
} 
using System;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using FirstPersonRTSGame.Game.UI;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace FirstPersonRTSGame.Game
{
    public class Game
    {
        // Window and input
        private IWindow window;
        private IInputContext? input;
        
        // Game components
        private GL? gl;
        private World? world;
        private Player? player;
        private Renderer? renderer;
        
        // UI system
        private UIRenderer? uiRenderer;
        private TextRenderer? textRenderer;
        private UIIcons? uiIcons;
        private UIManager? uiManager;
        
        // UI state
        private bool showInventory = false;
        private bool showBuildingMenu = false;
        
        // Delta time tracking
        private float deltaTime;
        private double lastFrameTime;
        
        public Game()
        {
            // Create window options
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(Constants.ScreenWidth, Constants.ScreenHeight);
            options.Title = "First Person RTS Game";
            options.VSync = true;
            
            // Create window
            window = Window.Create(options);
            
            // Set up event handlers
            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Closing += OnClose;
            window.Resize += OnResize;
        }
        
        public void Run()
        {
            // Start the game loop
            window.Run();
        }
        
        private void OnLoad()
        {
            Console.WriteLine("Game loading...");
            
            // Initialize GL
            gl = GL.GetApi(window);
            
            // Initialize input
            input = window.CreateInput();
            
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
            }
            
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].MouseMove += OnMouseMove;
                input.Mice[i].Scroll += OnMouseScroll;
                
                // Capture and lock the mouse cursor
                input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                input.Mice[i].Cursor.IsConfined = true;
            }
            
            // Initialize rendering systems
            uiRenderer = new UIRenderer(gl, window.Size.X, window.Size.Y);
            textRenderer = new TextRenderer(gl); 
            uiIcons = new UIIcons(gl);
            
            // Initialize UI Manager
            uiManager = new UIManager(gl, window.Size.X, window.Size.Y);
            
            // Initialize game world and player
            world = new World();
            player = new Player(new Vector3(Constants.WorldSize / 2, 5.0f, Constants.WorldSize / 2));
            
            // Connect player to world
            player.SetWorld(world);
            
            // Set up renderer
            renderer = new Renderer(gl);
            
            // Set initial frame time
            lastFrameTime = window.Time;
            
            // Show accessibility notification
            uiManager.ShowNotification("Press F2 to toggle high readability font mode", NotificationType.Info, 5.0f);
            
            // Set up OpenGL state
            gl.Enable(EnableCap.DepthTest);
            gl.ClearColor(0.5f, 0.8f, 1.0f, 1.0f); // Sky blue
            
            Console.WriteLine("Game loaded successfully!");
        }
        
        private void OnUpdate(double deltaTime)
        {
            this.deltaTime = (float)deltaTime;
            
            // Update player
            if (player != null && input != null)
            {
                // Check for keyboard input
                var keyboard = input.Keyboards[0];
                
                // Movement keys
                bool moveForward = keyboard.IsKeyPressed(Key.W);
                bool moveBackward = keyboard.IsKeyPressed(Key.S);
                bool moveLeft = keyboard.IsKeyPressed(Key.A);
                bool moveRight = keyboard.IsKeyPressed(Key.D);
                bool moveUp = keyboard.IsKeyPressed(Key.Space);
                bool moveDown = keyboard.IsKeyPressed(Key.ShiftLeft);
                
                // Update player based on input
                player.Update(this.deltaTime, moveForward, moveBackward, moveLeft, moveRight, moveUp, moveDown);
            }
            
            // Update world
            if (world != null)
            {
                world.Update(this.deltaTime);
            }
            
            // Update renderer
            if (renderer != null)
            {
                renderer.Update(this.deltaTime);
            }
            
            // Update UI state
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            // Update UI visibility based on input
            if (input != null)
            {
                var keyboard = input.Keyboards[0];
                
                if (keyboard.IsKeyPressed(Key.Tab))
                {
                    uiManager?.ToggleInventory();
                }
                
                if (keyboard.IsKeyPressed(Key.B))
                {
                    uiManager?.ToggleBuildingMenu();
                }
                
                if (keyboard.IsKeyPressed(Key.F1))
                {
                    uiManager?.ToggleHelp();
                }
            }
            
            // Update UI manager
            if (player != null && world != null && uiManager != null)
            {
                uiManager.Update(player, world, input?.Mice[0].Position ?? Vector2.Zero, deltaTime);
            }
        }
        
        private void OnRender(double obj)
        {
            // Clear the screen
            if (gl != null)
            {
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            
            // Render the game world
            if (renderer != null && world != null && player != null)
            {
                renderer.Render(world, player);
                
                // Render UI
                RenderUI();
            }
        }
        
        private void RenderUI()
        {
            // Enable blending for UI
            gl?.Enable(EnableCap.Blend);
            gl?.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl?.Disable(EnableCap.DepthTest);
            
            // Render UI using UI Manager
            if (player != null && world != null && uiManager != null)
            {
                uiManager.Render(player, world);
            }
            
            // Restore GL state
            gl?.Enable(EnableCap.DepthTest);
            gl?.Disable(EnableCap.Blend);
        }
        
        private void OnClose()
        {
            // Clean up resources
            renderer?.Dispose();
            world?.Dispose();
            uiRenderer?.Dispose();
            textRenderer?.Dispose();
            uiIcons?.Dispose();
            
            if (input != null)
            {
                input.Dispose();
            }
            
            if (player is IDisposable disposablePlayer)
            {
                disposablePlayer.Dispose();
            }
            
            // Dispose UI resources
            uiManager?.Dispose();
        }
        
        private void OnResize(Vector2D<int> size)
        {
            if (gl != null)
            {
                // Update viewport
                gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
                
                // Update UI dimensions
                uiManager?.Resize(size.X, size.Y);
            }
        }
        
        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            // Only handle mouse movement if the window has focus
            // Mouse movement used for camera rotation
            if (player != null)
            {
                player.OnMouseMove(position.X, position.Y);
            }
        }
        
        private void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
        {
            // Mouse scroll used for zooming
            if (player != null)
            {
                player.OnMouseScroll(scrollWheel.Y);
            }
        }
        
        private void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            // Skip if player is not initialized
            if (player == null || uiManager == null) return;
            
            // Toggle UI panels
            switch (key)
            {
                case Key.I:
                    uiManager.ToggleInventory();
                    break;
                    
                case Key.B:
                    uiManager.ToggleBuildingMenu();
                    break;
                    
                case Key.F1:
                    uiManager.ToggleHelp();
                    break;
                    
                case Key.Escape:
                    // Toggle mouse cursor lock
                    if (input != null && input.Mice.Count > 0)
                    {
                        var mouse = input.Mice[0];
                        if (mouse.Cursor.CursorMode == CursorMode.Raw)
                        {
                            // Unlock cursor
                            mouse.Cursor.CursorMode = CursorMode.Normal;
                            mouse.Cursor.IsConfined = false;
                        }
                        else
                        {
                            // Lock cursor
                            mouse.Cursor.CursorMode = CursorMode.Raw;
                            mouse.Cursor.IsConfined = true;
                        }
                    }
                    break;
                    
                // Add a font accessibility option - F2 toggles improved font readability
                case Key.F2:
                    // Toggle between normal and high readability font settings
                    if (textRenderer != null)
                    {
                        // Static variable to track state
                        bool highReadabilityEnabled = false;
                        
                        // Toggle state
                        highReadabilityEnabled = !highReadabilityEnabled;
                        
                        if (highReadabilityEnabled)
                        {
                            // Enable high readability mode
                            uiManager.IncreaseFontReadability();
                            uiManager.ShowNotification("High readability font enabled", NotificationType.Info);
                        }
                        else
                        {
                            // Reset to default font settings
                            uiManager.AdjustFontSettings(16, false);
                            uiManager.ShowNotification("Default font enabled", NotificationType.Info);
                        }
                    }
                    break;
                    
                // Handle ship commands with number keys
                case Key.Number1:
                case Key.Keypad1:
                    CommandShipToHarvest(ResourceType.Wood);
                    break;
                    
                case Key.Number2:
                case Key.Keypad2:
                    CommandShipToHarvest(ResourceType.Iron);
                    break;
                    
                case Key.Number3:
                case Key.Keypad3:
                    CommandShipToHarvest(ResourceType.Gold);
                    break;
                    
                case Key.Number4:
                case Key.Keypad4:
                    CommandShipToHarvest(ResourceType.Crystal);
                    break;
                    
                case Key.Number5:
                case Key.Keypad5:
                    CommandShipToHarvest(ResourceType.Oil);
                    break;
                    
                case Key.R:
                    CommandShipToReturnHome();
                    break;
            }
        }
        
        private void CommandShipToHarvest(ResourceType resourceType)
        {
            if (player != null)
            {
                IShip? targetedShip = player.GetTargetedShip();
                if (targetedShip != null && targetedShip is Ship ship)
                {
                    if (ship.Type == ShipType.Harvester)
                    {
                        ship.StartHarvesting(resourceType);
                        Console.WriteLine($"Commanded harvester to gather {resourceType}");
                        
                        // Show notification
                        uiManager?.ShowNotification($"Ship commanded to harvest {resourceType}", NotificationType.Info);
                    }
                    else
                    {
                        Console.WriteLine("Only harvesters can gather resources");
                        
                        // Show warning notification
                        uiManager?.ShowNotification("Only harvesters can gather resources", NotificationType.Warning);
                    }
                }
                else
                {
                    Console.WriteLine("No ship targeted");
                    
                    // Show error notification
                    uiManager?.ShowNotification("No ship targeted", NotificationType.Error);
                }
            }
        }
        
        private void CommandShipToReturnHome()
        {
            if (player != null)
            {
                IShip? targetedShip = player.GetTargetedShip();
                if (targetedShip != null && targetedShip is Ship ship)
                {
                    ship.StopMovement();
                    ship.SetTargetPosition(ship.Position with { X = Constants.WorldSize / 2, Z = Constants.WorldSize / 2 });
                    Console.WriteLine("Commanded ship to return to headquarters");
                    
                    // Show notification
                    uiManager?.ShowNotification("Ship returning to headquarters", NotificationType.Info);
                }
                else
                {
                    Console.WriteLine("No ship targeted");
                    
                    // Show error notification
                    uiManager?.ShowNotification("No ship targeted", NotificationType.Error);
                }
            }
        }
    }
} 
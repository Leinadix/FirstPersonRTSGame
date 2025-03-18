using System;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace FirstPersonRTSGame.Engine
{
    public class Game : IDisposable
    {
        // Window and input
        private IWindow window = null!;
        private IInputContext input = null!;
        private GL gl = null!;
        
        // Game components
        private IWorld world = null!;
        private IPlayer player = null!;
        private Renderer renderer = null!;
        
        // UI state
        private bool isPaused;
        private bool mouseGrabbed;
        
        // Game loop
        private double lastFrameTime;
        private float deltaTime;

        // Factory delegate to create game objects
        private Func<Game, GL, (IWorld, IPlayer)> gameObjectFactory;
        
        public Game(Func<Game, GL, (IWorld, IPlayer)> gameObjectFactory)
        {
            // Store factory function
            this.gameObjectFactory = gameObjectFactory;
            
            // Set up window options
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(Constants.ScreenWidth, Constants.ScreenHeight);
            options.Title = "First-Person RTS Prototype";
            
            // Create window
            window = Window.Create(options);
            
            // Set up events
            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Closing += OnClose;
            
            // Game state
            isPaused = false;
            mouseGrabbed = true;
        }
        
        public void Run()
        {
            // Start the game loop
            window.Run();
        }
        
        private void OnLoad()
        {
            // Initialize OpenGL
            gl = GL.GetApi(window);
            
            // Initialize input
            input = window.CreateInput();
            
            // Set up mouse move callback
            foreach (var mouse in input.Mice)
            {
                mouse.MouseMove += OnMouseMove;
                mouse.Scroll += OnMouseScroll;
            }
            
            // Set up keyboard callbacks
            foreach (var keyboard in input.Keyboards)
            {
                keyboard.KeyDown += OnKeyDown;
            }
            
            // Center the mouse cursor in the window
            CenterMouse();
            
            // Create the renderer
            renderer = new Renderer(gl);
            
            // Create game objects using the factory
            (world, player) = gameObjectFactory(this, gl);
            
            // Hide the cursor and grab mouse input
            GrabMouse();
            
            // Initialize frame timing
            lastFrameTime = window.Time;
        }
        
        private void OnUpdate(double deltaTime)
        {
            // Calculate delta time
            this.deltaTime = (float)deltaTime;
            
            // Skip updates if paused
            if (isPaused)
                return;
                
            // Handle input
            HandleInput();
            
            // Process keyboard input for player movement
            bool moveForward = false;
            bool moveBackward = false;
            bool moveLeft = false;
            bool moveRight = false;
            bool moveUp = false;
            bool moveDown = false;
            
            if (input.Keyboards.Count > 0)
            {
                var keyboard = input.Keyboards[0];
                moveForward = keyboard.IsKeyPressed(Key.W);
                moveBackward = keyboard.IsKeyPressed(Key.S);
                moveLeft = keyboard.IsKeyPressed(Key.A);
                moveRight = keyboard.IsKeyPressed(Key.D);
                moveUp = keyboard.IsKeyPressed(Key.Space);
                moveDown = keyboard.IsKeyPressed(Key.ShiftLeft);
            }
            
            // Update game components
            player.Update(this.deltaTime, moveForward, moveBackward, moveLeft, moveRight, moveUp, moveDown);
            world.Update(this.deltaTime);
            renderer.Update(this.deltaTime);
        }
        
        private void HandleInput()
        {
            // Keyboard shortcuts are handled in OnKeyDown event
            
            // Other input processing can be done here
        }
        
        private void OnRender(double obj)
        {
            // Render the game
            renderer.Render(world, player);
        }
        
        private void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            // Handle key presses
            switch (key)
            {
                case Key.Escape:
                    // Toggle pause
                    isPaused = !isPaused;
                    if (isPaused)
                        ReleaseMouse();
                    else
                        GrabMouse();
                    break;
                    
                case Key.Number1:
                case Key.Keypad1:
                    if (!isPaused)
                        Console.WriteLine("Switch to ship 1");
                    break;
                    
                case Key.Number2:
                case Key.Keypad2:
                    if (!isPaused)
                        Console.WriteLine("Switch to ship 2");
                    break;
                    
                case Key.Number3:
                case Key.Keypad3:
                    if (!isPaused)
                        Console.WriteLine("Switch to ship 3");
                    break;
                    
                case Key.E:
                    if (!isPaused)
                        Console.WriteLine("Interact with nearby object");
                    break;
                    
                case Key.Space:
                    if (!isPaused)
                    {
                        // Toggle building menu (would need UI implementation)
                        Console.WriteLine("Toggle building menu");
                    }
                    break;
                    
                case Key.I:
                    if (!isPaused)
                    {
                        // Toggle inventory display (would need UI implementation)
                        Console.WriteLine("Toggle inventory");
                    }
                    break;
                    
                case Key.B:
                    if (!isPaused)
                    {
                        // Toggle building interaction (would need UI implementation)
                        Console.WriteLine("Building interaction");
                    }
                    break;
            }
        }
        
        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (isPaused || !mouseGrabbed)
                return;
                
            // Pass mouse position directly to player
            player.OnMouseMove(position.X, position.Y);
        }
        
        private void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
        {
            if (!isPaused)
            {
                player.OnMouseScroll(scrollWheel.Y);
            }
        }
        
        private void OnClose()
        {
            // Cleanup
            Dispose();
        }
        
        private void CenterMouse()
        {
            foreach (var mouse in input.Mice)
            {
                mouse.Position = new Vector2(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
            }
        }
        
        private void GrabMouse()
        {
            // Use Silk.NET API to hide cursor and grab mouse focus
            foreach (var mouse in input.Mice)
            {
                mouse.Cursor.CursorMode = CursorMode.Raw;
            }
            
            mouseGrabbed = true;
        }
        
        private void ReleaseMouse()
        {
            // Use Silk.NET API to show cursor and release mouse focus
            foreach (var mouse in input.Mice)
            {
                mouse.Cursor.CursorMode = CursorMode.Normal;
            }
            
            mouseGrabbed = false;
        }
        
        public void Dispose()
        {
            // Cleanup resources
            renderer?.Dispose();
            (world as IDisposable)?.Dispose();
            (player as IDisposable)?.Dispose();
            input?.Dispose();
            window?.Dispose();
        }
    }
} 
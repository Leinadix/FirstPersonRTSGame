using System;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using Silk.NET.OpenGL;

namespace FirstPersonRTSGame.Game
{
    public static class GameObjectFactory
    {
        public static (IWorld, IPlayer) CreateGameObjects(Engine.Game engineGame, GL gl)
        {
            // Create the world
            World world = new World();
            
            // Create the player
            float startX = Constants.WorldSize / 2;
            float startY = 5.0f;  // Eye height
            float startZ = Constants.WorldSize / 2;
            Player player = new Player(new Vector3(startX, startY, startZ));
            
            // Connect player to world
            player.SetWorld(world);
            
            // Return the created objects
            return (world, player);
        }
        
        public static void Initialize()
        {
            // Create and run the game
            Engine.Game game = new Engine.Game(CreateGameObjects);
            game.Run();
        }
    }
} 
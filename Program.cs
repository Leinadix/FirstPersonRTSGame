using Silk.NET.Windowing;
using System;
using FirstPersonRTSGame.Game;
using FirstPersonRTSGame.Engine;

// Entry point for our First-Person RTS Game
namespace FirstPersonRTSGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Create the game instance using the parameterless constructor
                var game = new FirstPersonRTSGame.Game.Game();
                
                // Run the game
                game.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}

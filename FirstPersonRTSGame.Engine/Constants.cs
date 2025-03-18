using System;
using System.Collections.Generic;
using System.Numerics;

namespace FirstPersonRTSGame.Engine
{
    /// <summary>
    /// Global constants for the game
    /// </summary>
    public static class Constants
    {
        // World settings
        public const int WorldSize = 1000;
        public const float WaterLevel = 0.0f;
        
        // Screen settings
        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;
        
        // Game speed
        public const float GameSpeed = 1.0f;
        
        // Ship settings
        public const float DefaultShipSpeed = 10.0f;
        public const float DefaultShipTurnSpeed = 2.0f;
        public const int DefaultShipHealth = 100;
        
        // Building settings
        public const int DefaultBuildingHealth = 200;
        public const float ConstructionRate = 0.1f; // 10% per second
        
        // Resource settings
        public const int DefaultResourceAmount = 100;
        
        // Resource types
        public static readonly string[] ResourceTypes = new string[]
        {
            "Wood",
            "Iron",
            "Gold",
            "Crystal",
            "Oil",
            "Money",
            "Cobalt",
            "Fuel",
            "NuclearWaste",
            "Hydrogen",
            "Ammunition",
            "NuclearFuel"
        };
        
        // Mouse sensitivity
        public const float MouseSensitivity = 0.1f;
        
        // Ship types - stores properties for each ship type
        public static readonly Dictionary<string, Dictionary<string, float>> ShipTypes = new Dictionary<string, Dictionary<string, float>>
        {
            {
                "Schnellboot", new Dictionary<string, float>
                {
                    { "speed", 1.5f },
                    { "fuel_consumption", 0.5f },
                    { "cargo_capacity", 20.0f }
                }
            },
            {
                "Frachter", new Dictionary<string, float>
                {
                    { "speed", 0.8f },
                    { "fuel_consumption", 0.3f },
                    { "cargo_capacity", 100.0f }
                }
            },
            {
                "Tanker", new Dictionary<string, float>
                {
                    { "speed", 0.6f },
                    { "fuel_consumption", 0.2f },
                    { "cargo_capacity", 200.0f }
                }
            }
        };
        
        // Building types - stores properties for each building type
        public static readonly Dictionary<string, Dictionary<string, object>> BuildingTypes = new Dictionary<string, Dictionary<string, object>>
        {
            {
                "Markt", new Dictionary<string, object>
                {
                    { "build_time", 10.0f },
                    { "size", new Vector2(8, 8) },
                    { "cost_geld", 500.0f }
                }
            },
            {
                "Ölplattform", new Dictionary<string, object>
                {
                    { "build_time", 15.0f },
                    { "size", new Vector2(6, 6) },
                    { "cost_geld", 2000.0f },
                    { "production", "Öl" }
                }
            },
            {
                "Raffinerie", new Dictionary<string, object>
                {
                    { "build_time", 20.0f },
                    { "size", new Vector2(5, 5) },
                    { "cost_geld", 3000.0f },
                    { "input", "Öl" },
                    { "output", "Kraftstoff" }
                }
            },
            {
                "Kobaltanreicherung", new Dictionary<string, object>
                {
                    { "build_time", 25.0f },
                    { "size", new Vector2(4, 4) },
                    { "cost_geld", 5000.0f },
                    { "input", "Kobalt" },
                    { "output", "Kobalt" }
                }
            }
        };
    }
} 
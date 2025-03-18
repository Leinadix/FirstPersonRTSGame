using System;
using System.Collections.Generic;
using System.Numerics;

namespace FirstPersonRTSGame.Engine
{
    // Enums for resource, building, and ship types
    public enum ResourceType
    {
        Wood,
        Iron,
        Gold,
        Crystal,
        Oil,
        // New resource types from documentation
        Money,
        Cobalt,
        Fuel,
        NuclearWaste,
        Hydrogen,
        Ammunition,
        NuclearFuel
    }
    
    public enum BuildingType
    {
        Headquarters,
        Shipyard,
        Workshop,
        Mine,
        Refinery,
        OilRig,
        Laboratory,
        // New building types from documentation
        Market,
        CobaltEnrichment,
        NuclearRecycler,
        Electrolysis,
        OilPlatform
    }
    
    public enum ShipType
    {
        Harvester,
        Scout,
        Cruiser,
        Transport,
        // New ship types from documentation
        MarketTransporter,
        AmmunitionShip,
        NuclearFreighter,
        WarShip
    }
    
    // Interfaces for game objects that Engine needs to know about
    // These allow Engine to reference these objects without directly depending on Game namespace implementations

    public interface IResource
    {
        Vector3 Position { get; }
        FirstPersonRTSGame.Engine.ResourceType Type { get; }
        int Amount { get; }
        Guid Id { get; }
        
        bool IsDepleted();
    }

    public interface IBuilding
    {
        Vector3 Position { get; }
        FirstPersonRTSGame.Engine.BuildingType Type { get; }
        float Health { get; }
        float MaxHealth { get; }
        bool IsActive { get; }
        float ConstructionProgress { get; }
        
        bool CanProduce(FirstPersonRTSGame.Engine.ResourceType resourceType);
        int GetResourceAmount(FirstPersonRTSGame.Engine.ResourceType resourceType);
        bool ConsumeResource(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount);
        bool AddResource(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount);
        void TakeDamage(float amount);
        void Repair(float amount);
    }

    public interface IShip
    {
        Vector3 Position { get; }
        Vector3 Rotation { get; }
        float Speed { get; }
        float Health { get; }
        float MaxHealth { get; }
        FirstPersonRTSGame.Engine.ShipType Type { get; }
        
        void Update(float deltaTime);
        int GetCargoAmount(FirstPersonRTSGame.Engine.ResourceType resourceType);
        bool CanAddCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount);
        bool AddCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount);
        int RemoveCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount);
        void TakeDamage(float amount);
        void Repair(float amount);
    }

    public interface IPlayer
    {
        Vector3 Position { get; }
        Vector3 Front { get; }
        Vector3 Up { get; }
        Vector3 Right { get; }
        float Yaw { get; }
        float Pitch { get; }
        
        void Update(float deltaTime, bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool moveUp, bool moveDown);
        void OnMouseMove(float mouseX, float mouseY);
        void OnMouseScroll(float yOffset);
        void OnKeyDown(Silk.NET.Input.Key key);
    }

    public interface IWorld
    {
        float WaterLevel { get; }
        float TimeOfDay { get; }
        IEnumerable<IResource> Resources { get; }
        IEnumerable<IBuilding> Buildings { get; }
        IEnumerable<IShip> Ships { get; }
        void Update(float deltaTime);
        float GetHeightAt(float x, float z);
    }

    public interface INotificationSystem
    {
        void AddNotification(string message);
        void Update(float deltaTime);
    }

    public interface IUIManager
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        void ShowNotification(string message);
        void Update(float deltaTime);
        void RenderUI(IPlayer player, IShip targetedShip, IBuilding targetedBuilding, Matrix4x4 projection);
    }

    public interface IRenderer
    {
        void Render(IPlayer player, IWorld world);
        void Update(float deltaTime);
    }
} 
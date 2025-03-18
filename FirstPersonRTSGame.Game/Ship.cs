using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Game
{
    public enum ShipState
    {
        Idle,
        MovingToResource,
        Harvesting,
        MovingToDropoff,
        DroppingOff,
        ReturnToPosition
    }
    
    public class Ship : IShip
    {
        // Ship properties
        private Vector3 position;
        private Vector3 rotation;
        private float speed;
        private float health;
        private float maxHealth;
        private FirstPersonRTSGame.Engine.ShipType type;
        
        // Cargo
        private Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> cargo = new Dictionary<FirstPersonRTSGame.Engine.ResourceType, int>();
        private int maxCargoCapacity;
        private int currentCargoAmount;
        
        // Target information
        private Vector3? targetPosition;
        private bool isMoving;
        
        // Harvesting state
        private ShipState currentState = ShipState.Idle;
        private IResource? targetResource;
        private IBuilding? dropoffBuilding;
        private Vector3 homePosition;
        private float actionTimer;
        private float harvestingRate = 5.0f; // Units per second
        private FirstPersonRTSGame.Engine.ResourceType preferredResourceType = FirstPersonRTSGame.Engine.ResourceType.Wood;
        
        // World reference for autonomous behavior
        private World? world;
        
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public float Speed => speed;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public FirstPersonRTSGame.Engine.ShipType Type => type;
        public ShipState State => currentState;
        
        public Ship(Vector3 startPosition, FirstPersonRTSGame.Engine.ShipType shipType)
        {
            // Initialize position and rotation
            position = startPosition;
            rotation = Vector3.Zero;
            homePosition = startPosition;
            
            // Set ship type
            type = shipType;
            
            // Initialize properties based on ship type
            switch (type)
            {
                case FirstPersonRTSGame.Engine.ShipType.Harvester:
                    speed = 3.0f;
                    maxHealth = 100.0f;
                    maxCargoCapacity = 200;
                    harvestingRate = 5.0f;
                    break;
                
                case FirstPersonRTSGame.Engine.ShipType.Scout:
                    speed = 7.0f;
                    maxHealth = 50.0f;
                    maxCargoCapacity = 20;
                    harvestingRate = 1.0f;
                    break;
                
                case FirstPersonRTSGame.Engine.ShipType.Cruiser:
                    speed = 4.0f;
                    maxHealth = 250.0f;
                    maxCargoCapacity = 50;
                    harvestingRate = 0.0f; // Combat ships don't harvest
                    break;
                    
                case FirstPersonRTSGame.Engine.ShipType.Transport:
                    speed = 5.0f;
                    maxHealth = 150.0f;
                    maxCargoCapacity = 500;
                    harvestingRate = 2.0f;
                    break;

                // New ship types from documentation
                case FirstPersonRTSGame.Engine.ShipType.MarketTransporter:
                    speed = 6.0f;
                    maxHealth = 120.0f;
                    maxCargoCapacity = 400;
                    harvestingRate = 0.0f; // Market transporters don't harvest
                    break;
                    
                case FirstPersonRTSGame.Engine.ShipType.AmmunitionShip:
                    speed = 4.5f;
                    maxHealth = 180.0f;
                    maxCargoCapacity = 300;
                    harvestingRate = 0.0f; // Ammunition ships produce ammo, not harvest
                    break;
                    
                case FirstPersonRTSGame.Engine.ShipType.NuclearFreighter:
                    speed = 3.5f;
                    maxHealth = 200.0f;
                    maxCargoCapacity = 600;
                    harvestingRate = 0.0f; // Nuclear freighters transport nuclear fuel
                    break;
                    
                case FirstPersonRTSGame.Engine.ShipType.WarShip:
                    speed = 5.5f;
                    maxHealth = 350.0f;
                    maxCargoCapacity = 30;
                    harvestingRate = 0.0f; // Combat ships don't harvest
                    break;
            }
            
            // Set initial health to max
            health = maxHealth;
            
            // Initialize cargo
            foreach (FirstPersonRTSGame.Engine.ResourceType resourceType in Enum.GetValues(typeof(FirstPersonRTSGame.Engine.ResourceType)))
            {
                cargo[resourceType] = 0;
            }
            currentCargoAmount = 0;
            
            // No initial target
            targetPosition = null;
            isMoving = false;
            actionTimer = 0f;
        }
        
        public void SetWorld(World world)
        {
            this.world = world;
        }
        
        public void Update(float deltaTime)
        {
            // Handle autonomous behavior if enabled
            switch (currentState)
            {
                case ShipState.Idle:
                    // Do nothing while idle
                    break;
                    
                case ShipState.MovingToResource:
                    UpdateMovingToResource(deltaTime);
                    break;
                    
                case ShipState.Harvesting:
                    UpdateHarvesting(deltaTime);
                    break;
                    
                case ShipState.MovingToDropoff:
                    UpdateMovingToDropoff(deltaTime);
                    break;
                    
                case ShipState.DroppingOff:
                    UpdateDroppingOff(deltaTime);
                    break;
                    
                case ShipState.ReturnToPosition:
                    UpdateReturnToPosition(deltaTime);
                    break;
            }
            
            // Move towards target if we have one
            if (targetPosition.HasValue && isMoving)
            {
                // Calculate direction to target
                Vector3 direction = targetPosition.Value - position;
                
                // Ignore Y component for water movement
                direction.Y = 0;
                
                // Check if we're close enough to target
                if (direction.Length() < 0.5f)
                {
                    // We've reached the target
                    isMoving = false;
                    
                    // Update state based on what we were doing
                    switch (currentState)
                    {
                        case ShipState.MovingToResource:
                            if (targetResource != null)
                            {
                                currentState = ShipState.Harvesting;
                                actionTimer = 0f;
                            }
                            else
                            {
                                currentState = ShipState.Idle;
                            }
                            break;
                            
                        case ShipState.MovingToDropoff:
                            if (dropoffBuilding != null)
                            {
                                currentState = ShipState.DroppingOff;
                                actionTimer = 0f;
                            }
                            else
                            {
                                currentState = ShipState.Idle;
                            }
                            break;
                            
                        case ShipState.ReturnToPosition:
                            currentState = ShipState.Idle;
                            break;
                    }
                }
                else
                {
                    // Normalize direction
                    direction = Vector3.Normalize(direction);
                    
                    // Move towards target
                    position += direction * speed * deltaTime;
                    
                    // Update rotation to face movement direction
                    float targetAngle = (float)Math.Atan2(direction.X, direction.Z);
                    rotation = new Vector3(0, targetAngle, 0);
                }
            }
        }
        
        private void UpdateMovingToResource(float deltaTime)
        {
            // Check if target resource still exists
            if (targetResource == null || targetResource.IsDepleted())
            {
                // Find a new resource
                FindNearestResource();
                
                if (targetResource == null)
                {
                    // No resources found, return home
                    currentState = ShipState.ReturnToPosition;
                    SetTargetPosition(homePosition);
                }
            }
        }
        
        private void UpdateHarvesting(float deltaTime)
        {
            if (targetResource == null || targetResource.IsDepleted())
            {
                // Resource is gone, find another
                FindNearestResource();
                
                if (targetResource == null)
                {
                    // No resources found, go drop off what we have
                    if (currentCargoAmount > 0)
                    {
                        FindDropoffPoint();
                    }
                    else
                    {
                        // Nothing to drop off, return home
                        currentState = ShipState.ReturnToPosition;
                        SetTargetPosition(homePosition);
                    }
                }
                return;
            }
            
            // Increment action timer
            actionTimer += deltaTime;
            
            // Harvest resource at regular intervals
            if (actionTimer >= 1.0f)
            {
                actionTimer = 0f;
                
                // Calculate harvest amount
                int harvestAmount = (int)harvestingRate;
                
                // Try to harvest
                Resource resource = (Resource)targetResource;
                
                // Check if we can fit the harvest
                if (CanAddCargo(resource.Type, harvestAmount))
                {
                    // Harvest and add to cargo
                    int harvested = resource.Harvest(harvestAmount);
                    AddCargo(resource.Type, harvested);
                    
                    // Console.WriteLine($"Ship harvested {harvested} {resource.Type}");
                    
                    // Check if cargo is full or resource is depleted
                    if (currentCargoAmount >= maxCargoCapacity * 0.9f || resource.IsDepleted())
                    {
                        // Find somewhere to drop off resources
                        FindDropoffPoint();
                    }
                }
                else
                {
                    // Cargo full, find dropoff point
                    FindDropoffPoint();
                }
            }
        }
        
        private void UpdateMovingToDropoff(float deltaTime)
        {
            // Check if dropoff building still exists
            if (dropoffBuilding == null)
            {
                // Find a new dropoff point
                FindDropoffPoint();
                
                if (dropoffBuilding == null)
                {
                    // No dropoff point found, return home
                    currentState = ShipState.ReturnToPosition;
                    SetTargetPosition(homePosition);
                }
            }
        }
        
        private void UpdateDroppingOff(float deltaTime)
        {
            if (dropoffBuilding == null)
            {
                // Building is gone, find another
                FindDropoffPoint();
                
                if (dropoffBuilding == null)
                {
                    // No dropoff point found, return home
                    currentState = ShipState.ReturnToPosition;
                    SetTargetPosition(homePosition);
                }
                return;
            }
            
            // Increment action timer
            actionTimer += deltaTime;
            
            // Drop off resources at regular intervals
            if (actionTimer >= 0.5f)
            {
                actionTimer = 0f;
                
                // Transfer cargo to building
                bool anyTransferred = false;
                
                foreach (var entry in cargo.ToArray())
                {
                    if (entry.Value > 0)
                    {
                        if (dropoffBuilding.AddResource(entry.Key, entry.Value))
                        {
                            // Successfully transferred
                            int amount = entry.Value;
                            cargo[entry.Key] = 0;
                            currentCargoAmount -= amount;
                            anyTransferred = true;
                            
                            // Console.WriteLine($"Ship transferred {amount} {entry.Key} to {dropoffBuilding.Type}");
                        }
                    }
                }
                
                // If we've transferred everything or can't transfer anymore, go back to harvesting
                if (currentCargoAmount <= 0 || !anyTransferred)
                {
                    // Go find more resources
                    FindNearestResource();
                    
                    if (targetResource == null)
                    {
                        // No resources found, return home
                        currentState = ShipState.ReturnToPosition;
                        SetTargetPosition(homePosition);
                    }
                }
            }
        }
        
        private void UpdateReturnToPosition(float deltaTime)
        {
            // Just moving back to home position, nothing special to do here
        }
        
        private void FindNearestResource()
        {
            if (world == null) return;
            
            // Find the nearest resource of the preferred type
            targetResource = world.GetNearestResource(position, preferredResourceType, 500f);
            
            if (targetResource != null)
            {
                // Move to the resource
                currentState = ShipState.MovingToResource;
                SetTargetPosition(targetResource.Position);
            }
        }
        
        private void FindDropoffPoint()
        {
            if (world == null) return;
            
            // Find a dropoff building
            float closestDistance = float.MaxValue;
            dropoffBuilding = null;
            
            foreach (var building in world.Buildings)
            {
                if (building.Type == FirstPersonRTSGame.Engine.BuildingType.Headquarters || building.Type == FirstPersonRTSGame.Engine.BuildingType.Shipyard)
                {
                    float distance = Vector3.Distance(position, building.Position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        dropoffBuilding = building;
                    }
                }
            }
            
            if (dropoffBuilding != null)
            {
                // Move to the dropoff point
                currentState = ShipState.MovingToDropoff;
                SetTargetPosition(dropoffBuilding.Position);
            }
        }
        
        public void StartHarvesting(FirstPersonRTSGame.Engine.ResourceType resourceType)
        {
            preferredResourceType = resourceType;
            FindNearestResource();
        }
        
        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = target;
            isMoving = true;
        }
        
        public void SetHomePosition(Vector3 home)
        {
            homePosition = home;
        }
        
        public void StopMovement()
        {
            isMoving = false;
            currentState = ShipState.Idle;
        }
        
        public bool AddCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            // Check if adding this cargo would exceed capacity
            if (currentCargoAmount + amount > maxCargoCapacity)
                return false;
                
            // Add cargo
            cargo[resourceType] += amount;
            currentCargoAmount += amount;
            return true;
        }
        
        public int RemoveCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            // Check how much we can actually remove
            int actualAmount = Math.Min(amount, cargo[resourceType]);
            
            // Remove cargo
            cargo[resourceType] -= actualAmount;
            currentCargoAmount -= actualAmount;
            
            return actualAmount;
        }
        
        public int GetCargoAmount(FirstPersonRTSGame.Engine.ResourceType resourceType)
        {
            return cargo[resourceType];
        }
        
        public int GetTotalCargoAmount()
        {
            return currentCargoAmount;
        }
        
        public int GetMaxCargoCapacity()
        {
            return maxCargoCapacity;
        }
        
        public float GetRadius()
        {
            // Return ship radius based on type
            switch (type)
            {
                case FirstPersonRTSGame.Engine.ShipType.Harvester: return 3.0f;
                case FirstPersonRTSGame.Engine.ShipType.Scout: return 1.5f;
                case FirstPersonRTSGame.Engine.ShipType.Cruiser: return 4.0f;
                case FirstPersonRTSGame.Engine.ShipType.Transport: return 5.0f;
                // New ship types from documentation
                case FirstPersonRTSGame.Engine.ShipType.MarketTransporter: return 4.5f;
                case FirstPersonRTSGame.Engine.ShipType.AmmunitionShip: return 3.5f;
                case FirstPersonRTSGame.Engine.ShipType.NuclearFreighter: return 6.0f;
                case FirstPersonRTSGame.Engine.ShipType.WarShip: return 4.0f;
                default: return 2.0f;
            }
        }
        
        public void TakeDamage(float amount)
        {
            health = Math.Max(0, health - amount);
        }
        
        public void Repair(float amount)
        {
            health = Math.Min(maxHealth, health + amount);
        }
        
        public bool CanAddCargo(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            // Check if adding this cargo would exceed capacity
            return currentCargoAmount + amount <= maxCargoCapacity;
        }
    }
} 
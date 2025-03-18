using System;
using System.Numerics;
using System.Collections.Generic;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Game
{
    public class Player : IPlayer
    {
        private Vector3 position;
        private Vector3 velocity;
        private float yaw;
        private float pitch;
        private Vector3 front;
        private Vector3 right;
        private Vector3 up;
        
        // Mouse sensitivity
        private const float MouseSensitivity = 0.1f;
        
        // Movement speed
        private const float MoveSpeed = 10.0f;
        private const float SprintMultiplier = 2.0f;
        
        // Interaction
        private const float InteractionDistance = 5.0f;
        private IResource? targetedResource;
        private IBuilding? targetedBuilding;
        private IShip? targetedShip;
        
        // World reference
        private World? world;
        
        // Inventory
        private Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> inventory = new Dictionary<FirstPersonRTSGame.Engine.ResourceType, int>();
        
        // Mouse handling
        private float lastX;
        private float lastY;
        private bool firstMouse = true;
        
        public Vector3 Position => position;
        public Vector3 Front => front;
        public Vector3 Right => right;
        public Vector3 Up => up;
        public float Yaw => yaw;
        public float Pitch => pitch;
        
        public Player(Vector3 startPosition)
        {
            // Initialize position
            position = startPosition;
            velocity = Vector3.Zero;
            
            // Initialize orientation
            yaw = -90.0f; // Default looking north
            pitch = 0.0f;
            
            // Calculate initial direction vectors
            UpdateVectors();
            
            // Initialize inventory
            foreach (FirstPersonRTSGame.Engine.ResourceType type in Enum.GetValues(typeof(FirstPersonRTSGame.Engine.ResourceType)))
            {
                inventory[type] = 0;
            }
            
            // Give player some starting resources
            inventory[FirstPersonRTSGame.Engine.ResourceType.Wood] = 50;
            inventory[FirstPersonRTSGame.Engine.ResourceType.Iron] = 25;
            inventory[FirstPersonRTSGame.Engine.ResourceType.Money] = 100;
            inventory[FirstPersonRTSGame.Engine.ResourceType.Fuel] = 50;
            inventory[FirstPersonRTSGame.Engine.ResourceType.Cobalt] = 10;
        }
        
        public void SetWorld(World world)
        {
            this.world = world;
        }
        
        public void Update(float deltaTime, bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool moveUp, bool moveDown)
        {
            // Calculate movement direction based on input
            Vector3 moveDirection = Vector3.Zero;
            
            if (moveForward)
                moveDirection += front * new Vector3(1, 0, 1); // Only move on xz plane
            
            if (moveBackward)
                moveDirection -= front * new Vector3(1, 0, 1); // Only move on xz plane
            
            if (moveRight)
                moveDirection += right;
            
            if (moveLeft)
                moveDirection -= right;
            
            if (moveUp)
                moveDirection += Vector3.UnitY;
            
            if (moveDown)
                moveDirection -= Vector3.UnitY;
            
            // Normalize movement direction if moving
            if (moveDirection != Vector3.Zero)
            {
                moveDirection = Vector3.Normalize(moveDirection);
            }
            
            // Apply movement speed
            velocity = moveDirection * MoveSpeed;
            
            // Update position
            position += velocity * deltaTime;
            
            // Clamp player to world bounds
            position.X = Math.Clamp(position.X, 0, Constants.WorldSize);
            position.Z = Math.Clamp(position.Z, 0, Constants.WorldSize);
            position.Y = Math.Max(position.Y, 0.5f); // Keep above water level
            
            // Update interactions
            UpdateInteractions();
        }
        
        public void OnMouseMove(float mouseX, float mouseY)
        {
            // Check if this is the first mouse input
            if (firstMouse)
            {
                lastX = mouseX;
                lastY = mouseY;
                firstMouse = false;
                return;
            }
            
            // Calculate mouse offset
            float xOffset = mouseX - lastX;
            float yOffset = lastY - mouseY; // Reversed since Y coordinates go from bottom to top
            
            // Update last position
            lastX = mouseX;
            lastY = mouseY;
            
            // Apply sensitivity
            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;
            
            // Update yaw and pitch
            yaw += xOffset;
            pitch += yOffset;
            
            // Constrain pitch to avoid flipping
            pitch = Math.Clamp(pitch, -89.0f, 89.0f);
            
            // Update camera vectors
            UpdateVectors();
        }
        
        public void OnMouseScroll(float yOffset)
        {
            // Implement zoom functionality if needed
            // For example, modify field of view
        }
        
        public void OnKeyDown(Silk.NET.Input.Key key)
        {
            // Handle key press events
            // Implementation depends on game-specific key mappings
            // This method is required by the IPlayer interface
        }
        
        private void UpdateVectors()
        {
            // Calculate new front vector from yaw and pitch
            front.X = (float)(Math.Cos(Math.PI / 180 * yaw) * Math.Cos(Math.PI / 180 * pitch));
            front.Y = (float)Math.Sin(Math.PI / 180 * pitch);
            front.Z = (float)(Math.Sin(Math.PI / 180 * yaw) * Math.Cos(Math.PI / 180 * pitch));
            front = Vector3.Normalize(front);
            
            // Recalculate right and up vectors
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
        
        private void UpdateInteractions()
        {
            if (world == null) return;
            
            // Reset targeted objects
            targetedResource = null;
            targetedBuilding = null;
            targetedShip = null;
            
            // Cast ray from player position in the direction they're looking
            Vector3 rayOrigin = position;
            Vector3 rayDirection = front;
            
            // Check for resources in line of sight
            foreach (var resource in world.Resources)
            {
                if (RayIntersectsSphere(rayOrigin, rayDirection, resource.Position, 1.0f, out float distance))
                {
                    if (distance <= InteractionDistance)
                    {
                        targetedResource = resource;
                        break;
                    }
                }
            }
            
            // Check for buildings in line of sight
            foreach (var building in world.Buildings)
            {
                if (RayIntersectsBox(rayOrigin, rayDirection, building.Position, new Vector3(3, 3, 3), out float distance))
                {
                    if (distance <= InteractionDistance)
                    {
                        targetedBuilding = building;
                        break;
                    }
                }
            }
            
            // Check for ships in line of sight
            foreach (var ship in world.Ships)
            {
                if (RayIntersectsSphere(rayOrigin, rayDirection, ship.Position, 2.0f, out float distance))
                {
                    if (distance <= InteractionDistance)
                    {
                        targetedShip = ship;
                        break;
                    }
                }
            }
        }
        
        public bool Interact()
        {
            // Try to interact with the currently targeted object
            if (targetedResource != null)
            {
                // Harvest resource
                return HarvestResource((Resource)targetedResource);
            }
            else if (targetedBuilding != null)
            {
                // Interact with building
                return InteractWithBuilding((Building)targetedBuilding);
            }
            else if (targetedShip != null)
            {
                // Interact with ship
                return InteractWithShip((Ship)targetedShip);
            }
            
            return false;
        }
        
        private bool HarvestResource(Resource resource)
        {
            int harvestAmount = Math.Min(10, resource.Amount); // Harvest up to 10 units at a time
            
            if (harvestAmount <= 0)
                return false;
                
            // Add to inventory
            if (!inventory.ContainsKey(resource.Type))
                inventory[resource.Type] = 0;
                
            inventory[resource.Type] += harvestAmount;
            
            // Harvest from resource
            int remaining = resource.Harvest(harvestAmount);
            
            Console.WriteLine($"Harvested {harvestAmount} units of {resource.Type}. Now have {inventory[resource.Type]} units.");
            
            // If resource is depleted, remove it from the world
            if (remaining <= 0 && world != null)
            {
                world.RemoveResource(resource);
            }
            
            return true;
        }
        
        private bool InteractWithBuilding(Building building)
        {
            Console.WriteLine($"Interacting with {building.Type} building");
            
            // Each building type has different interactions
            switch (building.Type)
            {
                case BuildingType.Headquarters:
                    DisplayInventory();
                    return true;
                    
                case BuildingType.Shipyard:
                    // TODO: Show ship building interface
                    Console.WriteLine("Shipyard: Construct new ships (not implemented yet)");
                    return true;
                    
                case BuildingType.Workshop:
                    // TODO: Show crafting interface
                    Console.WriteLine("Workshop: Craft items (not implemented yet)");
                    return true;
                    
                default:
                    Console.WriteLine($"No specific interaction for {building.Type} yet");
                    return false;
            }
        }
        
        private bool InteractWithShip(Ship ship)
        {
            Console.WriteLine($"Interacting with {ship.Type} ship");
            
            // Each ship type has different interactions
            switch (ship.Type)
            {
                case FirstPersonRTSGame.Engine.ShipType.Harvester:
                    // Check if ship has cargo
                    int totalCargo = ship.GetTotalCargoAmount();
                    
                    if (totalCargo > 0)
                    {
                        // Display cargo info
                        Console.WriteLine($"Harvester cargo: {totalCargo}/{ship.GetMaxCargoCapacity()}");
                        
                        foreach (FirstPersonRTSGame.Engine.ResourceType resourceType in Enum.GetValues(typeof(FirstPersonRTSGame.Engine.ResourceType)))
                        {
                            int amount = ship.GetCargoAmount(resourceType);
                            if (amount > 0)
                            {
                                Console.WriteLine($"- {resourceType}: {amount}");
                            }
                        }
                        
                        // Offer command options
                        Console.WriteLine("Press 1-5 to command harvester to gather resources:");
                        Console.WriteLine("1: Wood | 2: Iron | 3: Gold | 4: Crystal | 5: Oil | R: Return home");
                        
                        return true;
                    }
                    else
                    {
                        // Offer command options
                        Console.WriteLine("Press 1-5 to command harvester to gather resources:");
                        Console.WriteLine("1: Wood | 2: Iron | 3: Gold | 4: Crystal | 5: Oil | R: Return home");
                        
                        return true;
                    }
                    
                case FirstPersonRTSGame.Engine.ShipType.Scout:
                    // Scout ship reveals more of the map (not implemented yet)
                    Console.WriteLine("Scout ship: Use for exploration");
                    Console.WriteLine("Press R to command scout to return to base");
                    return true;
                    
                default:
                    Console.WriteLine($"No specific interaction for {ship.Type} yet");
                    return false;
            }
        }
        
        public void DisplayInventory()
        {
            Console.WriteLine("=== PLAYER INVENTORY ===");
            foreach (var item in inventory)
            {
                Console.WriteLine($"{item.Key}: {item.Value} units");
            }
            Console.WriteLine("=======================");
        }
        
        private bool RayIntersectsSphere(Vector3 rayOrigin, Vector3 rayDirection, Vector3 sphereCenter, float radius, out float distance)
        {
            distance = 0;
            
            Vector3 m = rayOrigin - sphereCenter;
            float b = Vector3.Dot(m, rayDirection);
            float c = Vector3.Dot(m, m) - radius * radius;
            
            // Exit if ray origin outside sphere and going away from sphere
            if (c > 0 && b > 0)
                return false;
                
            float discriminant = b * b - c;
            
            // Ray misses sphere
            if (discriminant < 0)
                return false;
                
            // Ray hits sphere
            distance = -b - (float)Math.Sqrt(discriminant);
            
            // If distance is negative, ray is inside sphere, so we need to use the other intersection point
            if (distance < 0)
                distance = -b + (float)Math.Sqrt(discriminant);
                
            return distance >= 0;
        }
        
        private bool RayIntersectsBox(Vector3 rayOrigin, Vector3 rayDirection, Vector3 boxCenter, Vector3 boxSize, out float distance)
        {
            distance = float.MaxValue;
            
            // Calculate box min and max points
            Vector3 boxMin = boxCenter - boxSize * 0.5f;
            Vector3 boxMax = boxCenter + boxSize * 0.5f;
            
            // Check for ray intersection with each axis-aligned slab
            float tMin = float.NegativeInfinity;
            float tMax = float.PositiveInfinity;
            
            // Check X axis slab
            if (Math.Abs(rayDirection.X) < float.Epsilon)
            {
                // Ray is parallel to slab
                if (rayOrigin.X < boxMin.X || rayOrigin.X > boxMax.X)
                    return false;
            }
            else
            {
                float t1 = (boxMin.X - rayOrigin.X) / rayDirection.X;
                float t2 = (boxMax.X - rayOrigin.X) / rayDirection.X;
                
                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                
                if (tMin > tMax)
                    return false;
            }
            
            // Check Y axis slab
            if (Math.Abs(rayDirection.Y) < float.Epsilon)
            {
                // Ray is parallel to slab
                if (rayOrigin.Y < boxMin.Y || rayOrigin.Y > boxMax.Y)
                    return false;
            }
            else
            {
                float t1 = (boxMin.Y - rayOrigin.Y) / rayDirection.Y;
                float t2 = (boxMax.Y - rayOrigin.Y) / rayDirection.Y;
                
                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                
                if (tMin > tMax)
                    return false;
            }
            
            // Check Z axis slab
            if (Math.Abs(rayDirection.Z) < float.Epsilon)
            {
                // Ray is parallel to slab
                if (rayOrigin.Z < boxMin.Z || rayOrigin.Z > boxMax.Z)
                    return false;
            }
            else
            {
                float t1 = (boxMin.Z - rayOrigin.Z) / rayDirection.Z;
                float t2 = (boxMax.Z - rayOrigin.Z) / rayDirection.Z;
                
                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                
                if (tMin > tMax)
                    return false;
            }
            
            // If we get here, ray intersects box
            distance = tMin;
            
            return true;
        }
        
        // Inventory management methods
        public int GetResourceAmount(FirstPersonRTSGame.Engine.ResourceType type)
        {
            if (inventory.ContainsKey(type))
                return inventory[type];
            return 0;
        }
        
        public bool HasResources(Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> requiredResources)
        {
            foreach (var resource in requiredResources)
            {
                if (!inventory.ContainsKey(resource.Key) || inventory[resource.Key] < resource.Value)
                    return false;
            }
            return true;
        }
        
        public bool ConsumeResources(Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> resourcesToConsume)
        {
            // Check if we have enough resources
            if (!HasResources(resourcesToConsume))
                return false;
                
            // Consume resources
            foreach (var resource in resourcesToConsume)
            {
                inventory[resource.Key] -= resource.Value;
            }
            
            return true;
        }
        
        public void AddResources(Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> resourcesToAdd)
        {
            foreach (var resource in resourcesToAdd)
            {
                if (!inventory.ContainsKey(resource.Key))
                    inventory[resource.Key] = 0;
                    
                inventory[resource.Key] += resource.Value;
            }
        }
        
        public IResource? GetTargetedResource() => targetedResource;
        public IBuilding? GetTargetedBuilding() => targetedBuilding;
        public IShip? GetTargetedShip() => targetedShip;
        
        public Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> GetInventory() => inventory;
    }
} 
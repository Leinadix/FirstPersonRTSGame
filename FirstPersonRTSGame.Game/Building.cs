using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Game
{
    public class Building : IBuilding, IDisposable
    {
        private Vector3 position;
        private FirstPersonRTSGame.Engine.BuildingType type;
        private float health;
        private float maxHealth;
        private bool isActive;
        private float constructionProgress;
        
        // Production properties
        private Dictionary<FirstPersonRTSGame.Engine.ResourceType, int> inventory;
        private FirstPersonRTSGame.Engine.ResourceType? productionInput;
        private FirstPersonRTSGame.Engine.ResourceType? productionOutput;
        private float productionRate;
        private float productionTimer;
        
        public Vector3 Position => position;
        public FirstPersonRTSGame.Engine.BuildingType Type => type;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public bool IsActive => isActive;
        public float ConstructionProgress => constructionProgress;
        
        public Building(Vector3 position, FirstPersonRTSGame.Engine.BuildingType type)
        {
            this.position = position;
            this.type = type;
            
            // Set properties based on building type
            switch (type)
            {
                case FirstPersonRTSGame.Engine.BuildingType.Headquarters:
                    maxHealth = 500;
                    productionInput = null;
                    productionOutput = null;
                    productionRate = 0;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Shipyard:
                    maxHealth = 300;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.Iron;
                    productionOutput = null; // Creates ships, not resources
                    productionRate = 0.1f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Workshop:
                    maxHealth = 200;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.Wood;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Iron;
                    productionRate = 0.5f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Mine:
                    maxHealth = 150;
                    productionInput = null;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Iron;
                    productionRate = 0.2f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Refinery:
                    maxHealth = 250;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.Oil;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Crystal;
                    productionRate = 0.3f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.OilRig:
                    maxHealth = 325;
                    productionInput = null; // Directly harvests from oil deposits
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Oil;
                    productionRate = 0.3f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Laboratory:
                    maxHealth = 200;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.Crystal;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Gold;
                    productionRate = 0.1f;
                    break;
                    
                // New building types from documentation
                case FirstPersonRTSGame.Engine.BuildingType.Market:
                    maxHealth = 350;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.Gold;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Money;
                    productionRate = 0.4f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.CobaltEnrichment:
                    maxHealth = 250;
                    productionInput = null; // Directly harvests from cobalt deposits
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Cobalt;
                    productionRate = 0.2f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.NuclearRecycler:
                    maxHealth = 400;
                    productionInput = FirstPersonRTSGame.Engine.ResourceType.NuclearWaste;
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.NuclearFuel;
                    productionRate = 0.15f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.Electrolysis:
                    maxHealth = 200;
                    productionInput = null; // Uses water, which is not a collectible resource
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Hydrogen;
                    productionRate = 0.3f;
                    break;
                    
                case FirstPersonRTSGame.Engine.BuildingType.OilPlatform:
                    maxHealth = 300;
                    productionInput = null; // Directly harvests from oil deposits
                    productionOutput = FirstPersonRTSGame.Engine.ResourceType.Oil;
                    productionRate = 0.25f;
                    break;
                    
                default:
                    maxHealth = 100;
                    productionInput = null;
                    productionOutput = null;
                    productionRate = 0;
                    break;
            }
            
            // Initialize health and construction
            health = maxHealth;
            isActive = true;
            constructionProgress = 1.0f; // Fully built by default
            
            // Initialize inventory
            inventory = new Dictionary<FirstPersonRTSGame.Engine.ResourceType, int>();
            foreach (FirstPersonRTSGame.Engine.ResourceType resourceType in Enum.GetValues(typeof(FirstPersonRTSGame.Engine.ResourceType)))
            {
                inventory[resourceType] = 0;
            }
            
            // Initialize production timer
            productionTimer = 0;
        }
        
        public void Update(float deltaTime)
        {
            if (!isActive || constructionProgress < 1.0f)
                return;
                
            // Update production if this building produces something
            if (productionOutput.HasValue && productionRate > 0)
            {
                // Check if we need input resources
                if (productionInput.HasValue)
                {
                    // Check if we have input resources
                    if (inventory[productionInput.Value] > 0)
                    {
                        // Increment production timer
                        productionTimer += deltaTime * productionRate;
                        
                        // Check if production is complete
                        if (productionTimer >= 1.0f)
                        {
                            // Consume input
                            inventory[productionInput.Value]--;
                            
                            // Generate output
                            inventory[productionOutput.Value]++;
                            
                            // Reset timer
                            productionTimer = 0;
                            
                            Console.WriteLine($"Building {type} produced {productionOutput.Value}");
                        }
                    }
                }
                else
                {
                    // No input needed, just produce output
                    productionTimer += deltaTime * productionRate;
                    
                    // Check if production is complete
                    if (productionTimer >= 1.0f)
                    {
                        // Generate output
                        inventory[productionOutput.Value]++;
                        
                        // Reset timer
                        productionTimer = 0;
                        
                        Console.WriteLine($"Building {type} produced {productionOutput.Value}");
                    }
                }
            }
        }
        
        public float GetRadius()
        {
            // Return building radius based on type
            switch (type)
            {
                case FirstPersonRTSGame.Engine.BuildingType.Headquarters: return 8.0f;
                case FirstPersonRTSGame.Engine.BuildingType.Shipyard: return 7.0f;
                case FirstPersonRTSGame.Engine.BuildingType.Workshop: return 5.0f;
                case FirstPersonRTSGame.Engine.BuildingType.Mine: return 4.0f;
                case FirstPersonRTSGame.Engine.BuildingType.Refinery: return 6.0f;
                case FirstPersonRTSGame.Engine.BuildingType.OilRig: return 5.0f;
                case FirstPersonRTSGame.Engine.BuildingType.Laboratory: return 4.0f;
                default: return 3.0f;
            }
        }
        
        public void TakeDamage(float amount)
        {
            health = Math.Max(0, health - amount);
            
            // Deactivate if destroyed
            if (health <= 0)
            {
                isActive = false;
            }
        }
        
        public void Repair(float amount)
        {
            health = Math.Min(maxHealth, health + amount);
            
            // Reactivate if repaired
            if (health > 0 && !isActive)
            {
                isActive = true;
            }
        }
        
        public bool AddResource(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            inventory[resourceType] += amount;
            return true;
        }
        
        public int RemoveResource(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            int availableAmount = inventory[resourceType];
            int amountToRemove = Math.Min(availableAmount, amount);
            
            inventory[resourceType] -= amountToRemove;
            
            return amountToRemove;
        }
        
        public int GetResourceAmount(FirstPersonRTSGame.Engine.ResourceType resourceType)
        {
            return inventory[resourceType];
        }
        
        public bool CanProduce(FirstPersonRTSGame.Engine.ResourceType resourceType)
        {
            // Check if this building can produce the given resource type
            return productionOutput.HasValue && productionOutput.Value == resourceType;
        }
        
        public bool ConsumeResource(FirstPersonRTSGame.Engine.ResourceType resourceType, int amount)
        {
            // Check if we have enough of the resource
            if (inventory.ContainsKey(resourceType) && inventory[resourceType] >= amount)
            {
                // Consume the resource
                inventory[resourceType] -= amount;
                return true;
            }
            
            return false;
        }
        
        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
} 
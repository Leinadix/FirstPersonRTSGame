using System;
using System.Collections.Generic;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Game
{
    public class ResourceManager
    {
        // Global resources (sum of all ship inventories)
        public Dictionary<string, float> GlobalResources { get; private set; }
        
        // Market prices for resources
        public Dictionary<string, float> ResourcePrices { get; private set; }
        
        // Price fluctuation
        private float priceUpdateTimer;
        private const float PriceUpdateInterval = 30.0f; // Update prices every 30 seconds
        
        public ResourceManager()
        {
            // Initialize global resources
            GlobalResources = new Dictionary<string, float>
            {
                { "Geld", 500.0f } // Start with some money
            };
            
            // Initialize market prices
            ResourcePrices = new Dictionary<string, float>
            {
                { "Öl", 10.0f },
                { "Kobalt", 20.0f },
                { "Kraftstoff", 15.0f },
                { "Nuklearabfall", 5.0f },
                { "Wasserstoff", 25.0f },
                { "Munition", 30.0f }
            };
            
            priceUpdateTimer = 0.0f;
        }
        
        public void Update(float deltaTime)
        {
            // Update resource prices periodically
            priceUpdateTimer += deltaTime;
            
            if (priceUpdateTimer >= PriceUpdateInterval)
            {
                priceUpdateTimer = 0.0f;
                UpdateResourcePrices();
            }
        }
        
        public bool HasResource(string resourceType, float amount)
        {
            if (!GlobalResources.ContainsKey(resourceType))
                return false;
                
            return GlobalResources[resourceType] >= amount;
        }
        
        public bool SpendResource(string resourceType, float amount)
        {
            if (!HasResource(resourceType, amount))
                return false;
                
            GlobalResources[resourceType] -= amount;
            
            // Remove resource if amount reaches zero
            if (GlobalResources[resourceType] <= 0)
            {
                GlobalResources.Remove(resourceType);
            }
            
            return true;
        }
        
        public void AddResource(string resourceType, float amount)
        {
            if (GlobalResources.ContainsKey(resourceType))
            {
                GlobalResources[resourceType] += amount;
            }
            else
            {
                GlobalResources[resourceType] = amount;
            }
        }
        
        public float SellResource(string resourceType, float amount)
        {
            if (!HasResource(resourceType, amount))
                return 0.0f;
                
            // Calculate value of the resource
            float price = ResourcePrices.ContainsKey(resourceType) ? ResourcePrices[resourceType] : 1.0f;
            float value = amount * price;
            
            // Remove the resource
            SpendResource(resourceType, amount);
            
            // Add money
            AddResource("Geld", value);
            
            return value;
        }
        
        public bool BuyResource(string resourceType, float amount)
        {
            // Calculate cost
            float price = ResourcePrices.ContainsKey(resourceType) ? ResourcePrices[resourceType] : 1.0f;
            float cost = amount * price * 1.2f; // Add 20% markup for buying
            
            // Check if we have enough money
            if (!HasResource("Geld", cost))
                return false;
                
            // Spend the money
            SpendResource("Geld", cost);
            
            // Add the resource
            AddResource(resourceType, amount);
            
            return true;
        }
        
        private void UpdateResourcePrices()
        {
            Random random = new Random();
            
            // Update each resource price with a small random change
            foreach (var resource in ResourcePrices.Keys)
            {
                // Random price fluctuation between -10% and +10%
                float fluctuation = (float)(random.NextDouble() * 0.2 - 0.1);
                
                // Apply fluctuation
                ResourcePrices[resource] *= (1.0f + fluctuation);
                
                // Ensure minimum price
                ResourcePrices[resource] = Math.Max(1.0f, ResourcePrices[resource]);
            }
        }
    }
} 
using System;
using System.Numerics;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Game
{
    // ResourceType enum has been moved to Engine namespace
    // using FirstPersonRTSGame.Engine.ResourceType instead
    
    public class Resource : IResource
    {
        // Resource properties
        private Vector3 position;
        private FirstPersonRTSGame.Engine.ResourceType type;
        private int amount;
        private Guid id;
        
        // Visual properties
        private float visualScale = 1.0f;
        
        // Regeneration properties (some resources regrow over time)
        private bool canRegenerate;
        private float regenerationRate;
        private int maxAmount;
        private float regenerationTimer;
        
        // Interface implementation properties
        public Vector3 Position => position;
        public FirstPersonRTSGame.Engine.ResourceType Type => type;
        public int Amount => amount;
        public Guid Id => id;
        public float VisualScale => visualScale;
        
        public Resource(Vector3 position, FirstPersonRTSGame.Engine.ResourceType type, int amount)
        {
            this.position = position;
            this.type = type;
            this.amount = amount;
            this.id = Guid.NewGuid();
            
            // Set default visual scale based on amount
            this.visualScale = 0.5f + (amount / 200.0f);
            
            // Initialize regeneration properties based on resource type
            this.maxAmount = amount;
            
            // Only certain resource types can regenerate
            switch (type)
            {
                case FirstPersonRTSGame.Engine.ResourceType.Wood:
                    canRegenerate = true;
                    regenerationRate = 0.02f; // Units per second
                    break;
                
                case FirstPersonRTSGame.Engine.ResourceType.Crystal:
                    canRegenerate = true;
                    regenerationRate = 0.01f; // Units per second
                    break;
                
                default:
                    canRegenerate = false;
                    regenerationRate = 0f;
                    break;
            }
            
            regenerationTimer = 0f;
        }
        
        public void Update(float deltaTime)
        {
            // Regenerate resources over time if applicable
            if (canRegenerate && amount < maxAmount)
            {
                regenerationTimer += deltaTime;
                
                // Only regenerate at certain intervals
                if (regenerationTimer >= 5.0f) // Every 5 seconds
                {
                    regenerationTimer = 0f;
                    int regeneratedAmount = (int)(regenerationRate * 5.0f);
                    amount = Math.Min(amount + regeneratedAmount, maxAmount);
                    
                    // Update visual scale based on new amount
                    visualScale = 0.5f + (amount / 200.0f);
                }
            }
        }
        
        public int Harvest(int requestedAmount)
        {
            // Calculate how much can actually be harvested
            int harvestedAmount = Math.Min(requestedAmount, amount);
            
            // Reduce the resource amount
            amount -= harvestedAmount;
            
            // Update visual scale based on new amount
            visualScale = 0.5f + (amount / 200.0f);
            
            // Return the harvested amount (not the remaining amount)
            return harvestedAmount;
        }
        
        public bool IsDepleted()
        {
            return amount <= 0;
        }
        
        public override string ToString()
        {
            return $"{type} Resource: {amount} units at {position}";
        }
    }
} 
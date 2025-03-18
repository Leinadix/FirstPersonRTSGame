using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using Silk.NET.OpenGL;

namespace FirstPersonRTSGame.Game
{
    public class World : IWorld, IDisposable
    {
        // World properties
        public float Size { get; private set; }
        
        // Game objects collections
        private List<Resource> resources = new List<Resource>();
        private List<Building> buildings = new List<Building>();
        private List<Ship> ships = new List<Ship>();
        
        // Quick lookup for resources by type
        private Dictionary<FirstPersonRTSGame.Engine.ResourceType, List<Resource>> resourcesByType = new Dictionary<FirstPersonRTSGame.Engine.ResourceType, List<Resource>>();
        
        // Random number generator
        private Random random;
        
        // Water level of the world
        private float waterLevel = 0.0f;
        
        // Time of day (0 to 1)
        private float timeOfDay = 0.5f;
        private float timeRate = 0.005f;
        
        // Terrain generation parameters
        private float baseHeight = 0.0f;
        private float mountainHeight = 40.0f;
        private float hillHeight = 15.0f;
        private float noiseScale = 0.01f;
        private float mountainNoiseScale = 0.005f;
        private List<Vector3> mountainPeaks = new List<Vector3>();
        private List<Vector3> plateaus = new List<Vector3>();
        
        // Interface implementation properties
        float IWorld.WaterLevel => waterLevel;
        float IWorld.TimeOfDay => timeOfDay;
        IEnumerable<IResource> IWorld.Resources => resources;
        IEnumerable<IBuilding> IWorld.Buildings => buildings;
        IEnumerable<IShip> IWorld.Ships => ships;
        
        // Add public properties to expose collections
        public IEnumerable<Resource> Resources => resources;
        public IEnumerable<Building> Buildings => buildings;
        public IEnumerable<Ship> Ships => ships;
        
        public World()
        {
            // Initialize world size
            Size = Constants.WorldSize;
            
            // Initialize object collections
            resources = new List<Resource>();
            buildings = new List<Building>();
            ships = new List<Ship>();
            
            // Initialize random number generator
            random = new Random(42); // Fixed seed for reproducibility
            
            // Generate terrain features first
            GenerateTerrainFeatures();
            
            // Initialize resource type lookup
            foreach (FirstPersonRTSGame.Engine.ResourceType type in Enum.GetValues(typeof(FirstPersonRTSGame.Engine.ResourceType)))
            {
                resourcesByType[type] = new List<Resource>();
            }
            
            // Generate the world
            GenerateWorld();
            
            Console.WriteLine($"World initialized with {resources.Count} resources, {buildings.Count} buildings, and {ships.Count} ships");
        }
        
        public void Update(float deltaTime)
        {
            // Update time of day
            timeOfDay += timeRate * deltaTime;
            if (timeOfDay > 1.0f)
                timeOfDay -= 1.0f;
                
            // Update all resources
            foreach (var resource in resources.ToList())
            {
                resource.Update(deltaTime);
                
                // Check if resources have regenerated or are depleted
                if (resource.IsDepleted())
                {
                    resources.Remove(resource);
                    if (resourcesByType.ContainsKey(resource.Type))
                    {
                        resourcesByType[resource.Type].Remove(resource);
                    }
                }
            }
            
            // Update all ships
            foreach (var ship in ships)
            {
                ship.Update(deltaTime);
            }
            
            // Update all buildings
            foreach (var building in buildings)
            {
                building.Update(deltaTime);
            }
        }
        
        public void Render(GL gl, Player player)
        {
            // This method would be responsible for rendering the world
            // For now, it's just a placeholder
        }
        
        private void GenerateWorld()
        {
            // Add some initial resources
            GenerateResources();
            
            // Add some initial buildings
            GenerateBuildings();
            
            // Add some initial ships
            GenerateShips();
        }
        
        private void GenerateResources()
        {
            // Generate resource patches
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Wood, 20, 10);
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Iron, 15, 8);
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Gold, 10, 5);
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Crystal, 5, 3);
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Oil, 8, 3);
            
            // Generate new resource types from documentation
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Cobalt, 6, 4);
            GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType.Hydrogen, 4, 2);
            
            // Note: Money, Fuel, NuclearWaste, Ammunition, and NuclearFuel are produced by buildings
            // rather than spawning naturally in the world
        }
        
        private void GenerateResourcePatches(FirstPersonRTSGame.Engine.ResourceType type, int patchCount, int resourcesPerPatch)
        {
            for (int i = 0; i < patchCount; i++)
            {
                // Generate a central position for the patch
                float x = (float)random.NextDouble() * Constants.WorldSize;
                float z = (float)random.NextDouble() * Constants.WorldSize;
                
                // Generate resources around that position
                for (int j = 0; j < resourcesPerPatch; j++)
                {
                    float offsetX = (float)(random.NextDouble() * 10.0 - 5.0);
                    float offsetZ = (float)(random.NextDouble() * 10.0 - 5.0);
                    
                    float resourceX = x + offsetX;
                    float resourceZ = z + offsetZ;
                    
                    // Keep within world bounds
                    resourceX = Math.Clamp(resourceX, 0, Constants.WorldSize);
                    resourceZ = Math.Clamp(resourceZ, 0, Constants.WorldSize);
                    
                    // Get height at this position
                    float y = GetHeightAt(resourceX, resourceZ);
                    
                    // Create resource with random amount
                    int amount = random.Next(50, 200);
                    Resource resource = new Resource(new System.Numerics.Vector3(resourceX, y, resourceZ), type, amount);
                    
                    // Add to collections
                    resources.Add(resource);
                    resourcesByType[type].Add(resource);
                }
            }
        }
        
        private void GenerateBuildings()
        {
            // Add headquarters in the center of the map
            float hqX = Constants.WorldSize / 2;
            float hqZ = Constants.WorldSize / 2;
            float hqY = GetHeightAt(hqX, hqZ);
            
            Building headquarters = new Building(new System.Numerics.Vector3(hqX, hqY, hqZ), 
                                               FirstPersonRTSGame.Engine.BuildingType.Headquarters);
            buildings.Add(headquarters);
            
            // Add a shipyard near the headquarters
            float shipyardX = hqX + 15;
            float shipyardZ = hqZ;
            float shipyardY = GetHeightAt(shipyardX, shipyardZ);
            
            Building shipyard = new Building(new System.Numerics.Vector3(shipyardX, shipyardY, shipyardZ), 
                                           FirstPersonRTSGame.Engine.BuildingType.Shipyard);
            buildings.Add(shipyard);
            
            // Add a market for trading resources
            float marketX = hqX - 20;
            float marketZ = hqZ + 10;
            float marketY = GetHeightAt(marketX, marketZ);
            
            Building market = new Building(new System.Numerics.Vector3(marketX, marketY, marketZ),
                                         FirstPersonRTSGame.Engine.BuildingType.Market);
            buildings.Add(market);
            
            // Add a cobalt enrichment facility
            float cobaltX = hqX + 25;
            float cobaltZ = hqZ - 15;
            float cobaltY = GetHeightAt(cobaltX, cobaltZ);
            
            Building cobaltEnrichment = new Building(new System.Numerics.Vector3(cobaltX, cobaltY, cobaltZ),
                                                   FirstPersonRTSGame.Engine.BuildingType.CobaltEnrichment);
            buildings.Add(cobaltEnrichment);
        }
        
        private void GenerateShips()
        {
            // Add a harvester ship near the headquarters
            float hqX = Constants.WorldSize / 2;
            float hqZ = Constants.WorldSize / 2;
            
            Ship harvester = new Ship(new System.Numerics.Vector3(hqX + 5, GetHeightAt(hqX + 5, hqZ + 5) + 1, hqZ + 5), 
                                    FirstPersonRTSGame.Engine.ShipType.Harvester);
            
            // Set the world reference to enable autonomous behavior
            harvester.SetWorld(this);
            
            // Start the ship harvesting wood
            harvester.StartHarvesting(FirstPersonRTSGame.Engine.ResourceType.Wood);
            
            ships.Add(harvester);
            
            // Add a scout ship
            Ship scout = new Ship(new System.Numerics.Vector3(hqX - 10, GetHeightAt(hqX - 10, hqZ - 10) + 1, hqZ - 10),
                                FirstPersonRTSGame.Engine.ShipType.Scout);
            scout.SetWorld(this);
            ships.Add(scout);
            
            // Add a market transporter ship
            Ship marketShip = new Ship(new System.Numerics.Vector3(hqX - 15, GetHeightAt(hqX - 15, hqZ + 15) + 1, hqZ + 15),
                                    FirstPersonRTSGame.Engine.ShipType.MarketTransporter);
            marketShip.SetWorld(this);
            ships.Add(marketShip);
            
            // Add an ammunition ship
            Ship ammoShip = new Ship(new System.Numerics.Vector3(hqX + 15, GetHeightAt(hqX + 15, hqZ - 15) + 1, hqZ - 15),
                                   FirstPersonRTSGame.Engine.ShipType.AmmunitionShip);
            ammoShip.SetWorld(this);
            ships.Add(ammoShip);
        }
        
        private void GenerateTerrainFeatures()
        {
            // Generate mountain peaks
            int mountainCount = 8;
            for (int i = 0; i < mountainCount; i++)
            {
                float x = (float)random.NextDouble() * Constants.WorldSize;
                float z = (float)random.NextDouble() * Constants.WorldSize;
                
                // Keep mountains away from the center of the map where the player starts
                while (Vector2.Distance(new Vector2(x, z), new Vector2(Constants.WorldSize / 2, Constants.WorldSize / 2)) < 200)
                {
                    x = (float)random.NextDouble() * Constants.WorldSize;
                    z = (float)random.NextDouble() * Constants.WorldSize;
                }
                
                mountainPeaks.Add(new Vector3(x, 0, z));
            }
            
            // Generate plateaus
            int plateauCount = 5;
            for (int i = 0; i < plateauCount; i++)
            {
                float x = (float)random.NextDouble() * Constants.WorldSize;
                float z = (float)random.NextDouble() * Constants.WorldSize;
                float radius = 50 + (float)random.NextDouble() * 100; // plateau size
                
                plateaus.Add(new Vector3(x, 5 + (float)random.NextDouble() * 10, z)); // position and height
            }
        }
        
        public float GetHeightAt(float x, float z)
        {
            // Base height
            float height = baseHeight;
            
            // Add perlin-like noise for basic terrain
            height += Engine.MathHelper.GenerateNoise(x * noiseScale, z * noiseScale) * hillHeight;
            
            // Add mountain ranges
            foreach (var peak in mountainPeaks)
            {
                float distanceFromPeak = Vector2.Distance(new Vector2(x, z), new Vector2(peak.X, peak.Z));
                float mountainRadius = 200.0f; // Control the spread of the mountain
                
                if (distanceFromPeak < mountainRadius)
                {
                    // Mountain height decreases with distance from peak using cubic falloff
                    float falloff = 1.0f - (distanceFromPeak / mountainRadius);
                    falloff = falloff * falloff * falloff; // Cubic falloff for steeper mountains
                    
                    // Add some noise to the mountain surface
                    float mountainNoise = Engine.MathHelper.GenerateNoise((x + peak.X) * mountainNoiseScale, (z + peak.Z) * mountainNoiseScale) * 10.0f;
                    
                    // Add the mountain height
                    height += (mountainHeight * falloff) + (mountainNoise * falloff);
                }
            }
            
            // Add plateaus (flat areas)
            foreach (var plateau in plateaus)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, z), new Vector2(plateau.X, plateau.Z));
                float plateauRadius = 100.0f;
                
                if (distanceFromCenter < plateauRadius)
                {
                    // Plateau effect with smooth transition at edges
                    float edgeWidth = 20.0f;
                    float smoothFactor = 1.0f;
                    
                    if (distanceFromCenter > plateauRadius - edgeWidth)
                    {
                        smoothFactor = 1.0f - ((distanceFromCenter - (plateauRadius - edgeWidth)) / edgeWidth);
                    }
                    
                    // Blend the plateau height with existing height
                    height = Engine.MathHelper.Lerp(height, plateau.Y, smoothFactor);
                }
            }
            
            // Add valleys with river beds
            float riverNoise = Engine.MathHelper.GenerateNoise(x * 0.003f + 100, z * 0.003f + 100);
            if (riverNoise > 0.7f && riverNoise < 0.75f)
            {
                height = Math.Max(height - 10.0f, baseHeight); // Create a valley for the river
            }
            
            // Add small details with higher frequency noise
            height += Engine.MathHelper.GenerateNoise(x * 0.05f, z * 0.05f) * 1.0f;
            
            // Ensure minimum height (water level)
            height = Math.Max(height, waterLevel);
            
            return height;
        }
        
        // Use the MathHelper.GenerateNoise function from the Engine namespace instead of this one
        private float GenerateNoise(float x, float y)
        {
            return Engine.MathHelper.GenerateNoise(x, y);
        }
        
        public bool IsPositionValid(float x, float z, Vector2 size)
        {
            // Check if position is within world bounds
            if (x < 0 || x + size.X > Size || z < 0 || z + size.Y > Size)
                return false;
                
            // Check if position overlaps with existing buildings
            foreach (var building in buildings)
            {
                // Simple rectangular collision check
                Vector2 buildingPos = new Vector2(building.Position.X, building.Position.Z);
                Vector2 buildingSize = new Vector2(5, 5); // Assuming buildings are 5x5 units
                
                if (x < buildingPos.X + buildingSize.X &&
                    x + size.X > buildingPos.X &&
                    z < buildingPos.Y + buildingSize.Y &&
                    z + size.Y > buildingPos.Y)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void AddResource(Resource resource)
        {
            resources.Add(resource);
            
            if (!resourcesByType.ContainsKey(resource.Type))
            {
                resourcesByType[resource.Type] = new List<Resource>();
            }
            
            resourcesByType[resource.Type].Add(resource);
        }
        
        public void RemoveResource(Resource resource)
        {
            resources.Remove(resource);
            
            if (resourcesByType.ContainsKey(resource.Type))
            {
                resourcesByType[resource.Type].Remove(resource);
            }
        }
        
        public Resource? GetNearestResource(System.Numerics.Vector3 position, FirstPersonRTSGame.Engine.ResourceType type, float maxDistance)
        {
            if (!resourcesByType.ContainsKey(type) || resourcesByType[type].Count == 0)
            {
                return null;
            }
            
            Resource? nearest = null;
            float nearestDistance = maxDistance;
            
            foreach (var resource in resourcesByType[type])
            {
                float distance = System.Numerics.Vector3.Distance(position, resource.Position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = resource;
                }
            }
            
            return nearest;
        }
        
        public void AddBuilding(Building building)
        {
            buildings.Add(building);
        }
        
        public void RemoveBuilding(Building building)
        {
            buildings.Remove(building);
        }
        
        public void AddShip(Ship ship)
        {
            ships.Add(ship);
        }
        
        public void RemoveShip(Ship ship)
        {
            ships.Remove(ship);
        }
        
        public void Dispose()
        {
            // Clean up any resources
            foreach (var building in buildings)
            {
                if (building is IDisposable disposable)
                    disposable.Dispose();
            }
            
            foreach (var ship in ships)
            {
                if (ship is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
} 
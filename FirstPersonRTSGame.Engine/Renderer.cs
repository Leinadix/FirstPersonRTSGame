using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;

namespace FirstPersonRTSGame.Engine
{
    public unsafe class Renderer : IDisposable
    {
        private GL gl;
        
        // Shaders
        private uint basicShader;
        private uint waterShader;
        private uint terrainShader;
        private uint skyboxShader;
        
        // Buffers
        private uint cubeVao;
        private uint waterVao;
        private uint terrainVao;
        private uint terrainVbo;
        private uint skyboxVao;
        
        // Textures
        private uint waterTexture;
        private uint skyboxTexture;
        
        // Terrain mesh
        private int terrainResolution = 100; // Number of grid cells per side
        private float terrainSize;
        
        // Camera
        private Matrix4x4 projectionMatrix;
        private Matrix4x4 viewMatrix;
        
        // Uniforms
        private int viewProjUniformLoc;
        private int modelUniformLoc;
        private int colorUniformLoc;
        private int timeUniformLoc;
        private int heightUniformLoc; // For terrain shader
        
        // Animation
        private float time;
        
        // Window size
        private int windowWidth;
        private int windowHeight;
        
        public Renderer(GL gl)
        {
            this.gl = gl;
            
            // Set default window size
            this.windowWidth = Constants.ScreenWidth;
            this.windowHeight = Constants.ScreenHeight;
            
            // Set terrain size
            terrainSize = Constants.WorldSize;
            
            // Initialize rendering components
            InitializeShaders();
            InitializeGeometry();
            InitializeTextures();
            
            // Set up projection matrix
            float aspectRatio = (float)windowWidth / windowHeight;
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                (float)(Math.PI / 3.0), // 60 degrees FOV
                aspectRatio,
                0.1f,                    // Near plane
                3000.0f                  // Far plane - increased for distant horizon
            );
            
            // Initialize view matrix
            viewMatrix = Matrix4x4.Identity;
            
            // Initialize time
            time = 0.0f;
        }
        
        public void Update(float deltaTime)
        {
            // Update animation time
            time += deltaTime;
        }
        
        public void Render(IWorld world, IPlayer player)
        {
            // Update view matrix based on player's position and orientation
            viewMatrix = CreateViewMatrix(player);
            
            // Create combined view-projection matrix
            Matrix4x4 viewProj = viewMatrix * projectionMatrix;
            
            // Render skybox/horizon first (always at the back)
            RenderSkybox(viewProj, player.Position);
            
            // Render terrain
            RenderTerrain(world, viewProj);
            
            // Render water
            RenderWater(viewProj);
            
            // Render resources
            RenderResources(world.Resources, viewProj);
            
            // Render buildings
            RenderBuildings(world.Buildings, viewProj);
            
            // Render ships
            RenderShips(world.Ships, viewProj);
        }
        
        private Matrix4x4 CreateViewMatrix(IPlayer player)
        {
            // Create look-at matrix based on player's position and orientation
            return Matrix4x4.CreateLookAt(
                player.Position,
                player.Position + player.Front,
                player.Up
            );
        }
        
        private void RenderSkybox(Matrix4x4 viewProj, Vector3 playerPosition)
        {
            // Use skybox shader
            gl.UseProgram(skyboxShader);
            
            // Set time uniform for sky animation (clouds, day/night cycle)
            gl.Uniform1(timeUniformLoc, time);
            
            // Remove translation component from view-projection matrix to keep skybox centered on player
            Matrix4x4 viewWithoutTranslation = viewMatrix;
            viewWithoutTranslation.M41 = 0;
            viewWithoutTranslation.M42 = 0;
            viewWithoutTranslation.M43 = 0;
            Matrix4x4 skyboxViewProj = viewWithoutTranslation * projectionMatrix;
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(skyboxViewProj));
            
            // Render skybox cube
            gl.DepthFunc(DepthFunction.Less); // Change depth function to LEqual alternative
            gl.BindVertexArray(skyboxVao);
            gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            gl.DepthFunc(DepthFunction.Less); // Restore default depth function
        }
        
        private void RenderTerrain(IWorld world, Matrix4x4 viewProj)
        {
            // Update terrain heights from world
            UpdateTerrainHeights(world);
            
            // Use terrain shader
            gl.UseProgram(terrainShader);
            
            // Set time uniform for terrain animation (if any)
            gl.Uniform1(timeUniformLoc, time);
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(viewProj));
            
            // Create model matrix for terrain
            Matrix4x4 modelMatrix = Matrix4x4.CreateScale(1.0f, 1.0f, 1.0f) *
                                   Matrix4x4.CreateTranslation(0, 0, 0);
            
            // Set model matrix uniform
            gl.UniformMatrix4(modelUniformLoc, 1, false, GetMatrix4x4Values(modelMatrix));
            
            // Set water level for coloring
            gl.Uniform1(heightUniformLoc, world.WaterLevel);
            
            // Render terrain
            gl.BindVertexArray(terrainVao);
            gl.DrawElements(PrimitiveType.Triangles, (uint)((terrainResolution) * (terrainResolution) * 6), DrawElementsType.UnsignedInt, null);
        }
        
        private void UpdateTerrainHeights(IWorld world)
        {
            // Update heights for terrain mesh using the world's height data
            int gridSize = terrainResolution + 1;
            float cellSize = terrainSize / terrainResolution;
            
            // Create vertex data array with updated heights
            float[] vertices = new float[gridSize * gridSize * 4]; // Position (x,y,z) + Height
            
            // Generate vertices with correct heights
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    int vertexIndex = (z * gridSize + x) * 4;
                    
                    // Calculate world position
                    float posX = x * cellSize;
                    float posZ = z * cellSize;
                    
                    // Get height from world
                    float height = world.GetHeightAt(posX, posZ);
                    
                    // Store position with updated height
                    vertices[vertexIndex + 0] = posX;
                    vertices[vertexIndex + 1] = height;
                    vertices[vertexIndex + 2] = posZ;
                    vertices[vertexIndex + 3] = height; // Store height separately for coloring
                }
            }
            
            // Update vertex buffer with new heights
            gl.BindVertexArray(terrainVao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, terrainVbo);
            
            // Upload updated vertex data
            fixed (float* v = &vertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
            }
            
            gl.BindVertexArray(0);
        }
        
        private void RenderWater(Matrix4x4 viewProj)
        {
            // Use water shader
            gl.UseProgram(waterShader);
            
            // Set time uniform for water animation
            gl.Uniform1(timeUniformLoc, time);
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(viewProj));
            
            // Create model matrix for water plane
            Matrix4x4 modelMatrix = Matrix4x4.CreateTranslation(
                Constants.WorldSize / 2,
                0.0f,
                Constants.WorldSize / 2
            ) * Matrix4x4.CreateScale(Constants.WorldSize, 1.0f, Constants.WorldSize);
            
            // Set model matrix uniform
            gl.UniformMatrix4(modelUniformLoc, 1, false, GetMatrix4x4Values(modelMatrix));
            
            // Bind water texture
            gl.BindTexture(TextureTarget.Texture2D, waterTexture);
            
            // Render water plane
            gl.BindVertexArray(waterVao);
            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
        
        private void RenderResources(IEnumerable<IResource> resources, Matrix4x4 viewProj)
        {
            // Use basic shader
            gl.UseProgram(basicShader);
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(viewProj));
            
            // Render each resource
            foreach (var resource in resources)
            {
                // Create model matrix for resource
                Matrix4x4 modelMatrix = Matrix4x4.CreateScale(1.0f, 1.0f, 1.0f) *
                                       Matrix4x4.CreateTranslation(resource.Position);
                
                // Set model matrix uniform
                gl.UniformMatrix4(modelUniformLoc, 1, false, GetMatrix4x4Values(modelMatrix));
                
                // Set color based on resource type
                Vector4 color = GetResourceColor(resource.Type);
                gl.Uniform4(colorUniformLoc, color.X, color.Y, color.Z, color.W);
                
                // Render resource cube
                gl.BindVertexArray(cubeVao);
                gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }
        
        private Vector4 GetResourceColor(object resourceType)
        {
            // Convert resource type to string and determine color
            if (resourceType is ResourceType type)
            {
                switch (type)
                {
                    case ResourceType.Wood:
                        return new Vector4(0.6f, 0.3f, 0.1f, 1.0f); // Brown
                        
                    case ResourceType.Iron:
                        return new Vector4(0.7f, 0.7f, 0.7f, 1.0f); // Silver
                        
                    case ResourceType.Gold:
                        return new Vector4(1.0f, 0.84f, 0.0f, 1.0f); // Gold
                        
                    case ResourceType.Crystal:
                        return new Vector4(0.2f, 0.8f, 0.8f, 1.0f); // Cyan
                        
                    case ResourceType.Oil:
                        return new Vector4(0.1f, 0.1f, 0.1f, 1.0f); // Black
                        
                    // New resource types from documentation
                    case ResourceType.Money:
                        return new Vector4(0.0f, 0.8f, 0.0f, 1.0f); // Green
                        
                    case ResourceType.Cobalt:
                        return new Vector4(0.0f, 0.0f, 0.8f, 1.0f); // Blue
                        
                    case ResourceType.Fuel:
                        return new Vector4(0.9f, 0.4f, 0.0f, 1.0f); // Orange
                        
                    case ResourceType.NuclearWaste:
                        return new Vector4(0.0f, 0.8f, 0.0f, 1.0f); // Green (toxic)
                        
                    case ResourceType.Hydrogen:
                        return new Vector4(0.8f, 0.8f, 1.0f, 1.0f); // Light blue
                        
                    case ResourceType.Ammunition:
                        return new Vector4(0.5f, 0.5f, 0.2f, 1.0f); // Olive
                        
                    case ResourceType.NuclearFuel:
                        return new Vector4(0.8f, 0.8f, 0.0f, 1.0f); // Yellow
                        
                    default:
                        return new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White for unknown
                }
            }
            
            // Default color if not a recognized resource type
            return new Vector4(1.0f, 0.0f, 1.0f, 1.0f); // Magenta for errors
        }
        
        private void RenderBuildings(IEnumerable<IBuilding> buildings, Matrix4x4 viewProj)
        {
            // Use basic shader
            gl.UseProgram(basicShader);
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(viewProj));
            
            // Render each building
            foreach (var building in buildings)
            {
                // Get building size/scale based on type
                float scale = 3.0f; // Default size
                
                // Create model matrix for building
                Matrix4x4 modelMatrix = Matrix4x4.CreateScale(scale, scale * building.ConstructionProgress, scale) *
                                       Matrix4x4.CreateTranslation(building.Position);
                
                // Set model matrix uniform
                gl.UniformMatrix4(modelUniformLoc, 1, false, GetMatrix4x4Values(modelMatrix));
                
                // Set color based on building type
                Vector4 color = GetBuildingColor(building.Type);
                gl.Uniform4(colorUniformLoc, color.X, color.Y, color.Z, color.W);
                
                // Render building cube
                gl.BindVertexArray(cubeVao);
                gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }
        
        private Vector4 GetBuildingColor(object buildingType)
        {
            // Convert building type to enum and determine color
            if (buildingType is BuildingType type)
            {
                switch (type)
                {
                    case BuildingType.Headquarters:
                        return new Vector4(0.2f, 0.6f, 0.9f, 1.0f); // Blue
                        
                    case BuildingType.Shipyard:
                        return new Vector4(0.9f, 0.6f, 0.2f, 1.0f); // Orange
                        
                    case BuildingType.Workshop:
                        return new Vector4(0.7f, 0.7f, 0.2f, 1.0f); // Yellow
                        
                    case BuildingType.Mine:
                        return new Vector4(0.5f, 0.5f, 0.5f, 1.0f); // Gray
                        
                    case BuildingType.Refinery:
                        return new Vector4(0.8f, 0.2f, 0.2f, 1.0f); // Red
                        
                    case BuildingType.OilRig:
                        return new Vector4(0.3f, 0.3f, 0.3f, 1.0f); // Dark Gray
                        
                    case BuildingType.Laboratory:
                        return new Vector4(0.8f, 0.2f, 0.8f, 1.0f); // Purple
                        
                    // New building types from documentation
                    case BuildingType.Market:
                        return new Vector4(0.0f, 0.8f, 0.0f, 1.0f); // Green
                        
                    case BuildingType.CobaltEnrichment:
                        return new Vector4(0.0f, 0.0f, 0.8f, 1.0f); // Blue
                        
                    case BuildingType.NuclearRecycler:
                        return new Vector4(0.8f, 0.8f, 0.0f, 1.0f); // Yellow
                        
                    case BuildingType.Electrolysis:
                        return new Vector4(0.8f, 0.8f, 1.0f, 1.0f); // Light blue
                        
                    case BuildingType.OilPlatform:
                        return new Vector4(0.3f, 0.3f, 0.3f, 1.0f); // Dark Gray
                        
                    default:
                        return new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White for unknown
                }
            }
            
            // Default color if not a recognized building type
            return new Vector4(1.0f, 0.0f, 1.0f, 1.0f); // Magenta for errors
        }
        
        private void RenderShips(IEnumerable<IShip> ships, Matrix4x4 viewProj)
        {
            // Use basic shader
            gl.UseProgram(basicShader);
            
            // Set view-projection matrix
            gl.UniformMatrix4(viewProjUniformLoc, 1, false, GetMatrix4x4Values(viewProj));
            
            // Render each ship
            foreach (var ship in ships)
            {
                // Create model matrix for ship (including rotation)
                Matrix4x4 modelMatrix = Matrix4x4.CreateScale(1.0f, 0.5f, 2.0f) * // Make ships longer than they are wide
                                       Matrix4x4.CreateRotationY(ship.Rotation.Y) *
                                       Matrix4x4.CreateTranslation(ship.Position);
                
                // Set model matrix uniform
                gl.UniformMatrix4(modelUniformLoc, 1, false, GetMatrix4x4Values(modelMatrix));
                
                // Set color based on ship type
                Vector4 color = GetShipColor(ship.Type);
                gl.Uniform4(colorUniformLoc, color.X, color.Y, color.Z, color.W);
                
                // Render ship cube
                gl.BindVertexArray(cubeVao);
                gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }
        
        private Vector4 GetShipColor(object shipType)
        {
            // Convert ship type to enum and determine color
            if (shipType is ShipType type)
            {
                switch (type)
                {
                    case ShipType.Harvester:
                        return new Vector4(0.2f, 0.8f, 0.3f, 1.0f); // Green
                        
                    case ShipType.Scout:
                        return new Vector4(0.8f, 0.8f, 0.2f, 1.0f); // Yellow
                        
                    case ShipType.Cruiser:
                        return new Vector4(0.8f, 0.2f, 0.2f, 1.0f); // Red
                        
                    case ShipType.Transport:
                        return new Vector4(0.5f, 0.5f, 0.8f, 1.0f); // Light Blue
                        
                    // New ship types from documentation
                    case ShipType.MarketTransporter:
                        return new Vector4(0.0f, 0.8f, 0.0f, 1.0f); // Green
                        
                    case ShipType.AmmunitionShip:
                        return new Vector4(0.5f, 0.5f, 0.2f, 1.0f); // Olive
                        
                    case ShipType.NuclearFreighter:
                        return new Vector4(0.8f, 0.8f, 0.0f, 1.0f); // Yellow
                        
                    case ShipType.WarShip:
                        return new Vector4(0.9f, 0.2f, 0.2f, 1.0f); // Red
                        
                    default:
                        return new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White for unknown
                }
            }
            
            // Default color if not a recognized ship type
            return new Vector4(1.0f, 0.0f, 1.0f, 1.0f); // Magenta for errors
        }
        
        private void InitializeShaders()
        {
            // Basic vertex shader
            string basicVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                
                uniform mat4 uModel;
                uniform mat4 uViewProj;
                
                void main()
                {
                    gl_Position = uViewProj * uModel * vec4(aPosition, 1.0);
                }
            ";
            
            // Basic fragment shader
            string basicFragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                uniform vec4 uColor;
                
                void main()
                {
                    FragColor = uColor;
                }
            ";
            
            // Water vertex shader (with animation)
            string waterVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec2 aTexCoord;
                
                uniform mat4 uModel;
                uniform mat4 uViewProj;
                uniform float uTime;
                
                out vec2 TexCoord;
                
                void main()
                {
                    // Apply wave animation
                    vec3 pos = aPosition;
                    pos.y = sin(pos.x * 0.1 + uTime) * 0.1 + sin(pos.z * 0.15 + uTime * 0.7) * 0.1;
                    
                    // Output position
                    gl_Position = uViewProj * uModel * vec4(pos, 1.0);
                    
                    // Pass texture coordinates
                    TexCoord = aTexCoord;
                }
            ";
            
            // Water fragment shader
            string waterFragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                in vec2 TexCoord;
                
                uniform sampler2D uTexture;
                uniform float uTime;
                
                void main()
                {
                    // Distort texture coordinates for water effect
                    vec2 distortedTexCoord = TexCoord;
                    distortedTexCoord.x += sin(TexCoord.y * 10.0 + uTime * 0.5) * 0.01;
                    distortedTexCoord.y += cos(TexCoord.x * 10.0 + uTime * 0.7) * 0.01;
                    
                    // Sample texture
                    vec4 texColor = texture(uTexture, distortedTexCoord);
                    
                    // Add blue tint
                    vec4 waterColor = vec4(0.0, 0.3, 0.7, 0.8);
                    
                    // Blend texture with blue
                    FragColor = mix(waterColor, texColor, 0.3);
                }
            ";
            
            // Terrain vertex shader
            string terrainVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in float aHeight;
                
                uniform mat4 uModel;
                uniform mat4 uViewProj;
                uniform float uWaterLevel;
                
                out float Height;
                
                void main()
                {
                    // Pass height to fragment shader for coloring
                    Height = aHeight;
                    
                    // Calculate position with actual height
                    vec3 pos = aPosition;
                    
                    // Output position
                    gl_Position = uViewProj * uModel * vec4(pos, 1.0);
                }
            ";
            
            // Terrain fragment shader
            string terrainFragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                in float Height;
                
                uniform float uWaterLevel;
                
                void main()
                {
                    // Basic coloring based on height
                    vec4 sandColor = vec4(0.76, 0.70, 0.50, 1.0);    // Sandy shores
                    vec4 grassColor = vec4(0.20, 0.48, 0.25, 1.0);   // Grassland
                    vec4 forestColor = vec4(0.09, 0.32, 0.18, 1.0);  // Forest
                    vec4 rockColor = vec4(0.50, 0.50, 0.50, 1.0);    // Rocky terrain
                    vec4 snowColor = vec4(0.95, 0.95, 0.95, 1.0);    // Snow caps
                    
                    // Determine color based on height
                    vec4 color;
                    float waterLevel = uWaterLevel;
                    
                    if (Height < waterLevel + 1.0) {
                        color = sandColor; // Beach just above water
                    } else if (Height < 5.0) {
                        // Blend sand to grass
                        float blend = (Height - (waterLevel + 1.0)) / 4.0;
                        color = mix(sandColor, grassColor, blend);
                    } else if (Height < 20.0) {
                        // Grassland
                        color = grassColor;
                    } else if (Height < 30.0) {
                        // Blend grass to forest
                        float blend = (Height - 20.0) / 10.0;
                        color = mix(grassColor, forestColor, blend);
                    } else if (Height < 40.0) {
                        // Forest to rocks
                        float blend = (Height - 30.0) / 10.0;
                        color = mix(forestColor, rockColor, blend);
                    } else {
                        // Rocks to snow
                        float blend = min((Height - 40.0) / 20.0, 1.0);
                        color = mix(rockColor, snowColor, blend);
                    }
                    
                    FragColor = color;
                }
            ";
            
            // Skybox vertex shader
            string skyboxVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                
                uniform mat4 uViewProj;
                
                out vec3 TexCoords;
                
                void main()
                {
                    TexCoords = aPosition;
                    vec4 pos = uViewProj * vec4(aPosition * 2000.0, 1.0); // Scale to make skybox large
                    gl_Position = pos.xyww; // Force z coordinate to be at far plane
                }
            ";
            
            // Skybox fragment shader
            string skyboxFragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                in vec3 TexCoords;
                
                uniform float uTime;
                
                void main()
                {
                    // Calculate sky color based on direction
                    vec3 direction = normalize(TexCoords);
                    
                    // Sky gradient from zenith to horizon
                    float t = max(direction.y, 0.0); // Height above horizon (0 to 1)
                    
                    // Sky colors - blue at zenith, lighter at horizon
                    vec3 zenithColor = vec3(0.0, 0.2, 0.4);   // Deep blue
                    vec3 horizonColor = vec3(0.8, 0.9, 1.0);  // Light blue/white
                    
                    // Calculate sky color using gradient
                    vec3 skyColor = mix(horizonColor, zenithColor, t);
                    
                    // Add simple clouds
                    if (direction.y > 0.0) {
                        float cloudiness = 0.0;
                        
                        // Simple procedural clouds - can be improved with more complex algorithms
                        cloudiness = sin(direction.x * 10.0 + uTime * 0.1) * sin(direction.z * 10.0 + uTime * 0.05) * 0.5 + 0.5;
                        cloudiness = pow(cloudiness, 8.0); // Make clouds more defined
                        cloudiness *= (1.0 - direction.y); // Fewer clouds at zenith
                        
                        // Blend clouds with sky
                        skyColor = mix(skyColor, vec3(1.0), cloudiness * 0.5);
                    }
                    
                    // Add a sun at a fixed position that moves with time
                    vec3 sunDirection = normalize(vec3(sin(uTime * 0.1), 0.4, cos(uTime * 0.1)));
                    float sunIntensity = max(dot(direction, sunDirection), 0.0);
                    sunIntensity = pow(sunIntensity, 256.0); // Tight sun disc
                    
                    // Add sun glow
                    float sunGlow = max(dot(direction, sunDirection), 0.0);
                    sunGlow = pow(sunGlow, 8.0); // Wider glow
                    
                    // Add sun to sky color
                    skyColor += vec3(1.0, 0.9, 0.7) * sunIntensity; // Bright white-yellow sun
                    skyColor += vec3(0.8, 0.6, 0.3) * sunGlow * 0.3; // Orange-yellow glow
                    
                    // Set final color
                    FragColor = vec4(skyColor, 1.0);
                }
            ";
            
            // Create shader programs
            basicShader = CreateShaderProgram(basicVertexShaderSource, basicFragmentShaderSource);
            waterShader = CreateShaderProgram(waterVertexShaderSource, waterFragmentShaderSource);
            terrainShader = CreateShaderProgram(terrainVertexShaderSource, terrainFragmentShaderSource);
            skyboxShader = CreateShaderProgram(skyboxVertexShaderSource, skyboxFragmentShaderSource);
            
            // Get uniform locations for basic shader
            gl.UseProgram(basicShader);
            viewProjUniformLoc = gl.GetUniformLocation(basicShader, "uViewProj");
            modelUniformLoc = gl.GetUniformLocation(basicShader, "uModel");
            colorUniformLoc = gl.GetUniformLocation(basicShader, "uColor");
            
            // Get uniform locations for water shader
            gl.UseProgram(waterShader);
            timeUniformLoc = gl.GetUniformLocation(waterShader, "uTime");
            
            // Get uniform locations for terrain shader
            gl.UseProgram(terrainShader);
            heightUniformLoc = gl.GetUniformLocation(terrainShader, "uWaterLevel");
            
            // Get uniform locations for skybox shader
            gl.UseProgram(skyboxShader);
            
            // Reset to default shader
            gl.UseProgram(0);
        }
        
        private void InitializeGeometry()
        {
            // Create cube vertices for objects (buildings, ships, resources)
            float[] cubeVertices = {
                // Front face
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,
                
                // Back face
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                
                // Left face
                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                
                // Right face
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                
                // Bottom face
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,
                -0.5f, -0.5f, -0.5f,
                
                // Top face
                -0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f
            };
            
            // Create cube VAO/VBO
            cubeVao = gl.GenVertexArray();
            uint cubeVbo = gl.GenBuffer();
            
            gl.BindVertexArray(cubeVao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, cubeVbo);
            fixed (float* v = &cubeVertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(cubeVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
            
            // Position attribute for cube
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);
            
            // Create water quad vertices
            float[] waterVertices = {
                // Position              // Texture coords
                -0.5f, 0.0f, -0.5f,     0.0f, 0.0f,
                 0.5f, 0.0f, -0.5f,     1.0f, 0.0f,
                -0.5f, 0.0f,  0.5f,     0.0f, 1.0f,
                 0.5f, 0.0f,  0.5f,     1.0f, 1.0f
            };
            
            // Create water VAO/VBO
            waterVao = gl.GenVertexArray();
            uint waterVbo = gl.GenBuffer();
            
            gl.BindVertexArray(waterVao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, waterVbo);
            fixed (float* v = &waterVertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(waterVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
            
            // Position attribute for water
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);
            
            // Texture coordinate attribute for water
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(1);
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
            
            // Create skybox cube (same as regular cube but rendered from inside)
            skyboxVao = gl.GenVertexArray();
            uint skyboxVbo = gl.GenBuffer();
            
            gl.BindVertexArray(skyboxVao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, skyboxVbo);
            fixed (float* v = &cubeVertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(cubeVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
            
            // Position attribute for skybox
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
            
            // Initialize terrain mesh
            GenerateTerrainMesh();
        }
        
        private void GenerateTerrainMesh()
        {
            // Generate terrain mesh based on heightmap
            int gridSize = terrainResolution + 1; // Grid points
            float cellSize = terrainSize / terrainResolution;
            
            // Calculate total vertices and indices
            int vertexCount = gridSize * gridSize;
            int indexCount = terrainResolution * terrainResolution * 6; // 2 triangles per grid cell, 3 vertices per triangle
            
            // Create vertex and index arrays
            float[] vertices = new float[vertexCount * 4]; // Position (x,y,z) + Height
            uint[] indices = new uint[indexCount];
            
            // Generate vertices
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    int vertexIndex = (z * gridSize + x) * 4;
                    
                    // Calculate position
                    float posX = x * cellSize;
                    float posZ = z * cellSize;
                    float height = 0.0f; // Will be set at runtime
                    
                    // Store position and initial height
                    vertices[vertexIndex + 0] = posX;
                    vertices[vertexIndex + 1] = height;
                    vertices[vertexIndex + 2] = posZ;
                    vertices[vertexIndex + 3] = height; // Store height separately for coloring
                }
            }
            
            // Generate indices for triangles
            int indexIndex = 0;
            for (int z = 0; z < terrainResolution; z++)
            {
                for (int x = 0; x < terrainResolution; x++)
                {
                    uint topLeft = (uint)(z * gridSize + x);
                    uint topRight = topLeft + 1;
                    uint bottomLeft = (uint)((z + 1) * gridSize + x);
                    uint bottomRight = bottomLeft + 1;
                    
                    // First triangle (top-right half of quad)
                    indices[indexIndex++] = topLeft;
                    indices[indexIndex++] = bottomLeft;
                    indices[indexIndex++] = topRight;
                    
                    // Second triangle (bottom-left half of quad)
                    indices[indexIndex++] = topRight;
                    indices[indexIndex++] = bottomLeft;
                    indices[indexIndex++] = bottomRight;
                }
            }
            
            // Create OpenGL buffers
            terrainVao = gl.GenVertexArray();
            terrainVbo = gl.GenBuffer();
            uint ebo = gl.GenBuffer();
            
            gl.BindVertexArray(terrainVao);
            
            // Upload vertex data
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, terrainVbo);
            fixed (float* v = &vertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
            }
            
            // Upload index data
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (uint* i = &indices[0])
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
            
            // Set vertex attributes
            gl.EnableVertexAttribArray(0); // Position
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
            
            gl.EnableVertexAttribArray(1); // Height
            gl.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(3 * sizeof(float)));
            
            gl.BindVertexArray(0);
        }
        
        private void InitializeTextures()
        {
            // Create a simple procedural water texture (in a real game, you'd load this from a file)
            int width = 256;
            int height = 256;
            byte[] data = new byte[width * height * 4]; // RGBA format
            
            // Generate water texture data
            Random random = new Random(42); // Fixed seed for reproducibility
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;
                    
                    // Base blue color with some variation
                    double noise = 0.7 + 0.3 * random.NextDouble();
                    data[index + 0] = (byte)(20 * noise); // R
                    data[index + 1] = (byte)(100 * noise); // G
                    data[index + 2] = (byte)(200 * noise); // B
                    data[index + 3] = 255; // A
                }
            }
            
            // Create OpenGL texture
            waterTexture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, waterTexture);
            
            // Set texture parameters
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Upload texture data
            fixed (byte* pixels = &data[0])
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            }
            
            // Generate mipmaps
            gl.GenerateMipmap(TextureTarget.Texture2D);
            
            // Unbind
            gl.BindTexture(TextureTarget.Texture2D, 0);
            
            // Initialize a simple skybox texture (a solid color texture in this simplified version)
            skyboxTexture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, skyboxTexture);
            
            // Create a small solid color texture for the skybox (2x2 light blue texture)
            byte[] skyboxData = new byte[4 * 4]; // 2x2 RGBA texture
            for (int i = 0; i < 4; i++)
            {
                skyboxData[i * 4 + 0] = 128; // R
                skyboxData[i * 4 + 1] = 200; // G
                skyboxData[i * 4 + 2] = 255; // B
                skyboxData[i * 4 + 3] = 255; // A
            }
            
            // Upload texture data
            fixed (byte* pixels = &skyboxData[0])
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 2, 2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            }
            
            // Set texture parameters
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Unbind
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        
        private uint CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            CheckShaderCompileErrors(vertexShader);
            
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            CheckShaderCompileErrors(fragmentShader);
            
            uint program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);
            CheckProgramLinkErrors(program);
            
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            
            return program;
        }
        
        private void CheckShaderCompileErrors(uint shader)
        {
            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = gl.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER::COMPILATION_FAILED\n{infoLog}");
            }
        }
        
        private void CheckProgramLinkErrors(uint program)
        {
            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = gl.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM::LINKING_FAILED\n{infoLog}");
            }
        }
        
        private float[] GetMatrix4x4Values(Matrix4x4 matrix)
        {
            return new float[]
            {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            };
        }
        
        public void Dispose()
        {
            // Delete shaders
            gl.DeleteProgram(basicShader);
            gl.DeleteProgram(waterShader);
            gl.DeleteProgram(terrainShader);
            gl.DeleteProgram(skyboxShader);
            
            // Delete buffers
            gl.DeleteVertexArray(cubeVao);
            gl.DeleteVertexArray(waterVao);
            gl.DeleteVertexArray(terrainVao);
            gl.DeleteVertexArray(skyboxVao);
            
            // Delete textures
            gl.DeleteTexture(waterTexture);
            gl.DeleteTexture(skyboxTexture);
        }
    }
} 
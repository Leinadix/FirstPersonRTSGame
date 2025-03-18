using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;

namespace FirstPersonRTSGame.Game.UI
{
    public class UIIcons : IDisposable
    {
        private GL gl;
        
        // Shader program
        private uint shader;
        private uint vao;
        private uint vbo;
        
        // Icon textures
        private uint[]? textures;
        private Dictionary<string, int> iconMap = new Dictionary<string, int>();
        
        public UIIcons(GL gl)
        {
            this.gl = gl;
            
            // Initialize icon rendering
            InitializeIcons();
        }
        
        private void InitializeIcons()
        {
            // Create shader for icon rendering
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec4 vertex; // <position, texCoords>
                
                out vec2 TexCoords;
                
                uniform mat4 projection;
                
                void main()
                {
                    gl_Position = projection * vec4(vertex.xy, 0.0, 1.0);
                    TexCoords = vertex.zw;
                }
            ";
            
            string fragmentShaderSource = @"
                #version 330 core
                in vec2 TexCoords;
                out vec4 color;
                
                uniform sampler2D icon;
                uniform vec4 iconColor;
                
                void main()
                {    
                    vec4 texColor = texture(icon, TexCoords);
                    color = texColor * iconColor;
                }
            ";
            
            // Compile shaders
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            
            // Create shader program
            shader = gl.CreateProgram();
            gl.AttachShader(shader, vertexShader);
            gl.AttachShader(shader, fragmentShader);
            gl.LinkProgram(shader);
            
            // Delete shaders after linking
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            
            // Create VAO and VBO for icon rendering
            vao = gl.GenVertexArray();
            vbo = gl.GenBuffer();
            
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            
            // 6 vertices per quad (2 triangles), 4 floats per vertex (pos + tex)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(6 * 4 * sizeof(float)), IntPtr.Zero, BufferUsageARB.DynamicDraw);
            
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
            
            // Create procedural icons (in a real implementation, load textures from files)
            CreateProceduralIcons();
        }
        
        private unsafe void CreateProceduralIcons()
        {
            // Create icons for resources, buildings, and ships
            string[] iconNames = {
                "wood", "iron", "gold", "crystal", "oil",
                "headquarters", "shipyard", "workshop", "mine",
                "harvester", "scout", "cruiser", "transport",
                "health", "cargo", "construction"
            };
            
            // Generate textures
            textures = new uint[iconNames.Length];
            gl.GenTextures((uint)iconNames.Length, textures);
            
            // Create a simple procedural texture for each icon
            for (int i = 0; i < iconNames.Length; i++)
            {
                // Map icon name to texture index
                iconMap[iconNames[i]] = i;
                
                // Bind texture
                gl.BindTexture(TextureTarget.Texture2D, textures[i]);
                
                // Set texture parameters
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                
                // Generate procedural icon
                int size = 64;
                byte[] data = new byte[size * size * 4]; // RGBA
                
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = (y * size + x) * 4;
                        
                        // Default color (transparent)
                        data[index] = 0;     // R
                        data[index + 1] = 0; // G
                        data[index + 2] = 0; // B
                        data[index + 3] = 0; // A
                        
                        // Create simple shapes based on icon type
                        float centerX = size / 2.0f;
                        float centerY = size / 2.0f;
                        float distanceFromCenter = MathF.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                        
                        // Different icon types get different shapes/colors
                        switch (iconNames[i])
                        {
                            case "wood":
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 139;     // R (brown)
                                    data[index + 1] = 69;  // G
                                    data[index + 2] = 19;  // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            case "iron":
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 128;     // R (gray)
                                    data[index + 1] = 128; // G
                                    data[index + 2] = 128; // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            case "gold":
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 212;     // R (gold)
                                    data[index + 1] = 175; // G
                                    data[index + 2] = 55;  // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            case "crystal":
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 173;     // R (light blue)
                                    data[index + 1] = 216; // G
                                    data[index + 2] = 230; // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            case "oil":
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 0;       // R (black)
                                    data[index + 1] = 0;   // G
                                    data[index + 2] = 0;   // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            case "headquarters":
                                if (Math.Abs(x - centerX) < size / 4 && Math.Abs(y - centerY) < size / 4)
                                {
                                    data[index] = 70;      // R (building color)
                                    data[index + 1] = 130; // G
                                    data[index + 2] = 180; // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                                
                            // Add more cases for other icons...
                                
                            default:
                                // Default circular icon
                                if (distanceFromCenter < size / 3)
                                {
                                    data[index] = 200;     // R
                                    data[index + 1] = 200; // G
                                    data[index + 2] = 200; // B
                                    data[index + 3] = 255; // A
                                }
                                break;
                        }
                    }
                }
                
                // Upload texture data
                fixed (byte* ptr = data)
                {
                    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 
                        (uint)size, (uint)size, 0, PixelFormat.Rgba, 
                        PixelType.UnsignedByte, ptr);
                }
            }
            
            // Unbind texture
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        
        public unsafe void RenderIcon(string iconName, float x, float y, float size, Vector4 color, Matrix4x4 projection)
        {
            // Check if icon exists
            if (!iconMap.TryGetValue(iconName.ToLower(), out int iconIndex))
            {
                // Use default icon if not found
                iconIndex = 0;
            }
            
            // Bind shader
            gl.UseProgram(shader);
            
            // Set uniforms
            int projLocation = gl.GetUniformLocation(shader, "projection");
            
            // Convert Matrix4x4 to float array
            float[] matrixData = new float[]
            {
                projection.M11, projection.M12, projection.M13, projection.M14,
                projection.M21, projection.M22, projection.M23, projection.M24,
                projection.M31, projection.M32, projection.M33, projection.M34,
                projection.M41, projection.M42, projection.M43, projection.M44
            };
            
            fixed (float* ptr = matrixData)
            {
                gl.UniformMatrix4(projLocation, 1, false, in *ptr);
            }
            
            int colorLocation = gl.GetUniformLocation(shader, "iconColor");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Bind texture
            gl.ActiveTexture(TextureUnit.Texture0);
            if (textures != null)
            {
                gl.BindTexture(TextureTarget.Texture2D, textures[iconIndex]);
            }
            
            // Create quad vertices
            float[] vertices = {
                // pos            // tex
                x,         y + size, 0.0f, 1.0f, // bottom left
                x,         y,         0.0f, 0.0f, // top left
                x + size,  y,         1.0f, 0.0f, // top right
                
                x,         y + size, 0.0f, 1.0f, // bottom left
                x + size,  y,         1.0f, 0.0f, // top right
                x + size,  y + size, 1.0f, 1.0f  // bottom right
            };
            
            // Bind VAO and update VBO
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), in *ptr, BufferUsageARB.StaticDraw);
            }
            
            // Draw icon
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            // Unbind
            gl.BindVertexArray(0);
            gl.BindTexture(TextureTarget.Texture2D, 0);
            gl.UseProgram(0);
        }
        
        public void Dispose()
        {
            // Clean up resources
            gl.DeleteProgram(shader);
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            if (textures != null)
            {
                gl.DeleteTextures((uint)textures.Length, textures);
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Numerics;
using FirstPersonRTSGame.Engine;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using FirstPersonRTSGame.Game;

namespace FirstPersonRTSGame.Game.UI
{
    public class UIRenderer : IDisposable
    {
        private GL gl;
        
        // Shaders for UI rendering
        private uint shader;
        private uint vao;
        private uint vbo;
        
        // UI elements
        private Vector4 defaultColor = new Vector4(1.0f, 1.0f, 1.0f, 0.8f);
        
        // Screen dimensions
        private int screenWidth;
        private int screenHeight;
        
        // UI state
        private bool showInventory = false;
        private bool showBuildingMenu = false;
        private bool showShipInfo = false;
        
        public UIRenderer(GL gl, int screenWidth, int screenHeight)
        {
            this.gl = gl;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            
            // Initialize UI rendering
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            Console.WriteLine("Initializing UI renderer...");
            
            // Create shader for UI rendering
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPos;
                
                uniform mat4 projection;
                
                void main()
                {
                    gl_Position = projection * vec4(aPos, 0.0, 1.0);
                }
            ";
            
            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                uniform vec4 color;
                
                void main()
                {
                    FragColor = color;
                }
            ";
            
            // Compile shaders
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader, "UI Vertex Shader");
            
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader, "UI Fragment Shader");
            
            // Create shader program
            shader = gl.CreateProgram();
            gl.AttachShader(shader, vertexShader);
            gl.AttachShader(shader, fragmentShader);
            gl.LinkProgram(shader);
            CheckProgramLinking(shader, "UI Shader Program");
            
            // Delete shaders after linking
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            
            // Create VAO and VBO for UI elements
            vao = gl.GenVertexArray();
            vbo = gl.GenBuffer();
            
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            
            // Set up vertex attributes
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            gl.EnableVertexAttribArray(0);
            
            // Unbind
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
            
            Console.WriteLine("UI renderer initialized successfully.");
        }
        
        public unsafe void RenderPanel(int x, int y, int width, int height, Vector4 color)
        {
            // Set color uniform
            gl.UseProgram(shader);
            int colorLocation = gl.GetUniformLocation(shader, "color");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Set up orthographic projection matrix for UI
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
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
                gl.UniformMatrix4(projLocation, 1, false, ptr);
            }
            
            // Draw filled rectangle
            float[] vertices = {
                x, y,
                x + width, y,
                x + width, y + height,
                x, y + height
            };
            
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
            
            gl.DrawArrays((GLEnum)PrimitiveType.TriangleFan, 0, (uint)4);
            
            // Draw border with slightly lighter color
            Vector4 borderColor = new Vector4(
                Math.Min(color.X + 0.2f, 1.0f), 
                Math.Min(color.Y + 0.2f, 1.0f), 
                Math.Min(color.Z + 0.2f, 1.0f), 
                color.W);
            gl.Uniform4(colorLocation, borderColor.X, borderColor.Y, borderColor.Z, borderColor.W);
            
            float[] borderVertices = {
                x, y,
                x + width, y,
                x + width, y + height,
                x, y + height,
                x, y
            };
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = borderVertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(borderVertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
            
            gl.DrawArrays((GLEnum)PrimitiveType.LineStrip, 0, (uint)5);
            
            gl.UseProgram(0);
        }
        
        public unsafe void RenderRoundedPanel(int x, int y, int width, int height, int cornerRadius, Vector4 color)
        {
            // Use the shader for UI rendering
            gl.UseProgram(shader);
            
            // Set the color
            int colorLocation = gl.GetUniformLocation(shader, "color");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Create the projection matrix (orthographic for 2D UI)
            var projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
            
            // Set the projection matrix
            int projectionLocation = gl.GetUniformLocation(shader, "projection");
            gl.UniformMatrix4(projectionLocation, 1, false, GetMatrix4x4Values(projection));
            
            // Ensure corner radius is not too large
            cornerRadius = Math.Min(cornerRadius, Math.Min(width / 2, height / 2));
            
            // Calculate the bounds of the panel
            float left = x;
            float right = x + width;
            float top = y;
            float bottom = y + height;
            
            gl.BindVertexArray(vao);
            
            // Main panel body (excluding corners)
            float[] mainVertices = {
                // Center rectangle
                left + cornerRadius, top,
                right - cornerRadius, top,
                right - cornerRadius, bottom,
                right - cornerRadius, bottom,
                left + cornerRadius, bottom,
                left + cornerRadius, top,
                
                // Top rectangle
                left + cornerRadius, top - cornerRadius,
                right - cornerRadius, top - cornerRadius,
                right - cornerRadius, top,
                right - cornerRadius, top,
                left + cornerRadius, top,
                left + cornerRadius, top - cornerRadius,
                
                // Bottom rectangle
                left + cornerRadius, bottom,
                right - cornerRadius, bottom,
                right - cornerRadius, bottom + cornerRadius,
                right - cornerRadius, bottom + cornerRadius,
                left + cornerRadius, bottom + cornerRadius,
                left + cornerRadius, bottom
            };
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* v = &mainVertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(mainVertices.Length * sizeof(float)), v, BufferUsageARB.StreamDraw);
            }
            
            gl.DrawArrays(PrimitiveType.Triangles, 0, 18);
            
            // Add a subtle top highlight for a clean look
            Vector4 highlightColor = new Vector4(color.X + 0.05f, color.Y + 0.05f, color.Z + 0.05f, color.W * 0.5f);
            gl.Uniform4(colorLocation, highlightColor.X, highlightColor.Y, highlightColor.Z, highlightColor.W);
            
            float[] highlightVertices = {
                left + cornerRadius, top,
                right - cornerRadius, top,
                right - cornerRadius, top + 1,
                right - cornerRadius, top + 1,
                left + cornerRadius, top + 1,
                left + cornerRadius, top
            };
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* v = &highlightVertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(highlightVertices.Length * sizeof(float)), v, BufferUsageARB.StreamDraw);
            }
            
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            // Draw corners
            if (cornerRadius > 0)
            {
                // Reset to original color
                gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
                
                // Draw the 4 corners
                DrawRoundedCorner(left + cornerRadius, top + cornerRadius, cornerRadius, (float)Math.PI, (float)(Math.PI * 1.5), 8, color);
                DrawRoundedCorner(right - cornerRadius, top + cornerRadius, cornerRadius, (float)(Math.PI * 1.5), (float)(Math.PI * 2.0), 8, color);
                DrawRoundedCorner(right - cornerRadius, bottom - cornerRadius, cornerRadius, 0, (float)(Math.PI * 0.5), 8, color);
                DrawRoundedCorner(left + cornerRadius, bottom - cornerRadius, cornerRadius, (float)(Math.PI * 0.5), (float)Math.PI, 8, color);
            }
            
            gl.BindVertexArray(0);
        }
        
        private unsafe void DrawRoundedCorner(float centerX, float centerY, float radius, float startAngle, float endAngle, int segments, Vector4 color)
        {
            // Set color uniform
            gl.UseProgram(shader);
            int colorLocation = gl.GetUniformLocation(shader, "color");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Generate vertices for the corner
            float[] vertices = new float[(segments + 2) * 2];
            
            // Center vertex
            vertices[0] = centerX;
            vertices[1] = centerY;
            
            // Generate vertices along the arc
            float angleStep = (endAngle - startAngle) * MathF.PI / 180.0f / segments;
            float currentAngle = startAngle * MathF.PI / 180.0f;
            
            for (int i = 0; i <= segments; i++)
            {
                vertices[(i + 1) * 2] = centerX + MathF.Cos(currentAngle) * radius;
                vertices[(i + 1) * 2 + 1] = centerY + MathF.Sin(currentAngle) * radius;
                currentAngle += angleStep;
            }
            
            // Draw the corner
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
            
            gl.DrawArrays((GLEnum)PrimitiveType.TriangleFan, 0, (uint)(segments + 2));
        }
        
        public unsafe void RenderLine(float x1, float y1, float x2, float y2, float thickness, Vector4 color)
        {
            // Set color uniform
            gl.UseProgram(shader);
            int colorLocation = gl.GetUniformLocation(shader, "color");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Set up orthographic projection matrix for UI
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
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
                gl.UniformMatrix4(projLocation, 1, false, ptr);
            }
            
            // Calculate normal vector
            Vector2 lineDir = new Vector2(x2 - x1, y2 - y1);
            float length = MathF.Sqrt(lineDir.X * lineDir.X + lineDir.Y * lineDir.Y);
            
            if (length < 0.0001f)
                return;
                
            lineDir = new Vector2(lineDir.X / length, lineDir.Y / length);
            Vector2 normal = new Vector2(-lineDir.Y, lineDir.X);
            
            // Calculate vertices for thick line
            float halfThickness = thickness * 0.5f;
            Vector2 offset = new Vector2(normal.X * halfThickness, normal.Y * halfThickness);
            
            float[] vertices = {
                x1 - offset.X, y1 - offset.Y,
                x1 + offset.X, y1 + offset.Y,
                x2 + offset.X, y2 + offset.Y,
                x2 - offset.X, y2 - offset.Y
            };
            
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
            
            gl.DrawArrays((GLEnum)PrimitiveType.TriangleFan, 0, (uint)4);
            
            gl.UseProgram(0);
        }
        
        public unsafe void RenderGradientPanel(int x, int y, int width, int height, Vector4 topColor, Vector4 bottomColor)
        {
            // For full gradient panels, we'd need a different shader
            // This is a simplified approach using two rectangles
            
            // Render top half with top color
            RenderPanel(x, y, width, height / 2, topColor);
            
            // Calculate middle color
            Vector4 middleColor = new Vector4(
                (topColor.X + bottomColor.X) * 0.5f,
                (topColor.Y + bottomColor.Y) * 0.5f,
                (topColor.Z + bottomColor.Z) * 0.5f,
                (topColor.W + bottomColor.W) * 0.5f
            );
            
            // Render bottom half with bottom color
            RenderPanel(x, y + height / 2, width, height / 2, bottomColor);
        }
        
        public unsafe void RenderCircle(float centerX, float centerY, float radius, Vector4 color, int segments = 32)
        {
            // Set color uniform
            gl.UseProgram(shader);
            int colorLocation = gl.GetUniformLocation(shader, "color");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            // Set up orthographic projection matrix for UI
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
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
                gl.UniformMatrix4(projLocation, 1, false, ptr);
            }
            
            // Generate vertices for the circle
            float[] vertices = new float[(segments + 2) * 2];
            
            // Center vertex
            vertices[0] = centerX;
            vertices[1] = centerY;
            
            // Generate vertices along the perimeter
            float angleStep = 2.0f * MathF.PI / segments;
            float currentAngle = 0.0f;
            
            for (int i = 0; i <= segments; i++)
            {
                vertices[(i + 1) * 2] = centerX + MathF.Cos(currentAngle) * radius;
                vertices[(i + 1) * 2 + 1] = centerY + MathF.Sin(currentAngle) * radius;
                currentAngle += angleStep;
            }
            
            // Draw the circle
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* ptr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
            
            gl.DrawArrays((GLEnum)PrimitiveType.TriangleFan, 0, (uint)(segments + 2));
            
            gl.UseProgram(0);
        }
        
        public unsafe void RenderOutlinedText(TextRenderer textRenderer, string text, float x, float y, float scale, Vector4 textColor, Vector4 outlineColor, Matrix4x4 projection)
        {
            // Render outline by drawing text multiple times with offsets
            float outlineSize = 1.0f;
            
            // Render outline
            textRenderer.RenderText(text, x - outlineSize, y - outlineSize, scale, outlineColor, projection);
            textRenderer.RenderText(text, x + outlineSize, y - outlineSize, scale, outlineColor, projection);
            textRenderer.RenderText(text, x - outlineSize, y + outlineSize, scale, outlineColor, projection);
            textRenderer.RenderText(text, x + outlineSize, y + outlineSize, scale, outlineColor, projection);
            
            // Render main text on top
            textRenderer.RenderText(text, x, y, scale, textColor, projection);
        }
        
        public void SetShowInventory(bool show)
        {
            showInventory = show;
        }
        
        public void SetShowBuildingMenu(bool show)
        {
            showBuildingMenu = show;
        }
        
        public void Resize(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
        }
        
        private void CheckShaderCompilation(uint shader, string name)
        {
            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status != 1)
            {
                gl.GetShader(shader, ShaderParameterName.InfoLogLength, out int length);
                string info = gl.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR: {name} compilation failed: {info}");
            }
        }
        
        private void CheckProgramLinking(uint program, string name)
        {
            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status != 1)
            {
                gl.GetProgram(program, ProgramPropertyARB.InfoLogLength, out int length);
                string info = gl.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR: {name} linking failed: {info}");
            }
        }
        
        // Helper method to convert Matrix4x4 to float array
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
            // Clean up resources
            gl.DeleteProgram(shader);
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace FirstPersonRTSGame.Game.UI
{
    public class TextRenderer : IDisposable
    {
        private GL gl;
        private FontFamily? fontFamily;
        private Font? font;
        
        // Shader program
        private uint shader;
        private uint vao;
        private uint vbo;
        
        // Texture atlas
        private uint texture;
        private Dictionary<char, CharInfo> characters = new Dictionary<char, CharInfo>();
        
        // Font size and settings
        private int fontSize = 20;
        private int atlasWidth = 4096;
        private int atlasHeight = 4096;
        private bool useAntiAliasing = true;
        
        public TextRenderer(GL gl)
        {
            this.gl = gl;
            
            // Initialize text rendering
            InitializeTextRenderer();
        }
        
        private void InitializeTextRenderer()
        {
            Console.WriteLine("Initializing text renderer...");
            
            // Create shader for text rendering
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
                
                uniform sampler2D text;
                uniform vec4 textColor;
                
                void main()
                {    
                    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
                    color = textColor * sampled;
                }
            ";
            
            // Compile shaders
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader, "Text Vertex Shader");
            
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader, "Text Fragment Shader");
            
            // Create shader program
            shader = gl.CreateProgram();
            gl.AttachShader(shader, vertexShader);
            gl.AttachShader(shader, fragmentShader);
            gl.LinkProgram(shader);
            CheckProgramLinking(shader, "Text Shader Program");
            
            // Delete shaders after linking
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            
            // Create VAO and VBO for text rendering
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
            
            // Generate a font texture atlas
            GenerateFontAtlas();
            
            Console.WriteLine("Text renderer initialized successfully.");
            
            // Test shader and texture
            gl.UseProgram(shader);
            int textureLocation = gl.GetUniformLocation(shader, "text");
            gl.Uniform1(textureLocation, 0); // Set text sampler to texture unit 0
            gl.UseProgram(0);
        }
        
        private unsafe void GenerateFontAtlas()
        {
            Console.WriteLine("Generating font texture atlas...");
            
            // Load the font
            try
            {
                // Try to load Arial first, then fall back to any installed sans-serif font
                var fontCollection = new FontCollection();
                
                // Look for fonts in the system fonts directory and the local directory
                string fontsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
                
                // Try to load common fonts that should be available on most systems
                string[] fontPaths = {
                    Path.Combine(fontsPath, "arial.ttf"),
                    Path.Combine(fontsPath, "segoeui.ttf"),   // Windows
                    Path.Combine(fontsPath, "calibri.ttf"),   // Windows
                    Path.Combine(fontsPath, "verdana.ttf"),   // Windows (better readability)
                    Path.Combine(fontsPath, "DejaVuSans.ttf"),
                    "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",  // Linux
                    "/System/Library/Fonts/Helvetica.ttc"     // macOS
                };
                
                bool fontLoaded = false;
                foreach (var fontPath in fontPaths)
                {
                    if (File.Exists(fontPath))
                    {
                        try
                        {
                            fontFamily = fontCollection.Add(fontPath);
                            fontLoaded = true;
                            Console.WriteLine($"Successfully loaded font from {fontPath}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading font {fontPath}: {ex.Message}");
                        }
                    }
                }
                
                if (!fontLoaded)
                {
                    Console.WriteLine("Could not load any system fonts. Falling back to procedural font.");
                    GenerateProceduralFontFallback();
                    return;
                }
                
                // Create the font with the specified size and optional bold style for better readability
                // Use a slightly heavier weight for better readability
                if (fontFamily != null)
                {
                    // Cast to non-nullable since we've checked for null
                    FontFamily nonNullableFamily = (FontFamily)fontFamily;
                    font = new SixLabors.Fonts.Font(nonNullableFamily, fontSize, SixLabors.Fonts.FontStyle.Regular);
                }
                else
                {
                    Console.WriteLine("Error: fontFamily is null, cannot create font");
                    return;
                }

                // Use smaller texture atlas to avoid memory issues
                // Reduce atlas size for better memory usage
                int smallerAtlasWidth = 2048;
                int smallerAtlasHeight = 2048;
                
                // Create a texture atlas
                using var image = new Image<Rgba32>(smallerAtlasWidth, smallerAtlasHeight);
                
                // Fill with transparent black
                image.Mutate(ctx => ctx.Fill(Color.Transparent));
                
                // Set up a text renderer to use with ImageSharp
                var textOptions = new TextOptions(font)
                {
                    // Add anti-aliasing when enabled
                    TabWidth = 4,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Dpi = 96 // Standard DPI
                };
                
                // Calculate metrics for each ASCII character
                int x = 0;
                int y = 0;
                int rowHeight = 0;
                
                // Process a limited character set to save memory (32-126 is basic ASCII)
                for (char c = ' '; c <= '~'; c++)
                {
                    // Get the glyph metrics for this character
                    var glyphMetrics = TextMeasurer.MeasureSize(c.ToString(), textOptions);
                    
                    int charWidth = (int)Math.Ceiling(glyphMetrics.Width);
                    int charHeight = (int)Math.Ceiling(glyphMetrics.Height);
                    
                    // If we've run out of space on this row, move to the next row
                    if (x + charWidth + 2 >= smallerAtlasWidth)
                    {
                        y += rowHeight + 2;
                        rowHeight = 0;
                        x = 0;
                    }
                    
                    rowHeight = Math.Max(rowHeight, charHeight);
                    
                    // Draw the character
                    if (useAntiAliasing)
                    {
                        // Draw with anti-aliasing - simpler approach to reduce memory usage
                        image.Mutate(ctx => {
                            // Draw the main character
                            ctx.DrawText(c.ToString(), font, Color.White, new PointF(x, y));
                        });
                    }
                    else
                    {
                        // Simple rendering without anti-aliasing
                        image.Mutate(ctx => ctx.DrawText(c.ToString(), font, Color.White, new PointF(x, y)));
                    }
                    
                    // Store character info
                    FontRectangle fontRect = TextMeasurer.MeasureSize(c.ToString(), textOptions);
                    
                    // FIXED APPROACH: Use a consistent baseline for all characters
                    // Instead of calculating a bearing from the top, we'll position everything
                    // from the bottom up with a fixed baseline height
                    CharInfo info = new CharInfo
                    {
                        Size = new Vector2(charWidth, charHeight),
                        // No bearing - we'll position all characters at the same baseline Y
                        Bearing = new Vector2(0, 0),
                        // Use full character width plus a small padding to prevent horizontal squishing
                        Advance = charWidth + 2,
                        TextureCoords = new Vector4(
                            (float)x / smallerAtlasWidth,
                            (float)y / smallerAtlasHeight,
                            (float)(x + charWidth) / smallerAtlasWidth,
                            (float)(y + charHeight) / smallerAtlasHeight
                        )
                    };
                    
                    characters[c] = info;
                    
                    // Move to the next position
                    x += charWidth + 2; // Add a little padding
                }
                
                // Convert the RGBA image to a single-channel alpha image to save memory
                byte[] textureData = new byte[smallerAtlasWidth * smallerAtlasHeight];
                
                // Create and upload the texture
                texture = gl.GenTexture();
                gl.BindTexture(TextureTarget.Texture2D, texture);
                
                // Set texture parameters
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                
                // Extract just the alpha channel to save memory
                image.ProcessPixelRows(accessor => 
                {
                    for (int rows = 0; rows < accessor.Height; rows++)
                    {
                        var pixelRow = accessor.GetRowSpan(rows);
                        for (int col = 0; col < pixelRow.Length; col++)
                        {
                            // Use red channel as alpha
                            textureData[rows * smallerAtlasWidth + col] = pixelRow[col].R;
                        }
                    }
                });
                
                // Upload data
                fixed (byte* ptr = textureData)
                {
                    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, 
                        (uint)smallerAtlasWidth, (uint)smallerAtlasHeight, 0, PixelFormat.Red, 
                        PixelType.UnsignedByte, ptr);
                }
                
                gl.BindTexture(TextureTarget.Texture2D, 0);
                Console.WriteLine("Font texture atlas generated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fonts: {ex.Message}");
                // Fall back to procedural font if font loading fails
                GenerateProceduralFontFallback();
                return;
            }
        }
        
        // Fallback method in case font loading fails
        private unsafe void GenerateProceduralFontFallback()
        {
            Console.WriteLine("Falling back to procedural font texture...");
            
            // Generate texture
            texture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, texture);
            
            // Set texture parameters
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Create a better procedural font texture with basic character shapes
            byte[] data = new byte[atlasWidth * atlasHeight];
            
            // Fill with black (0)
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
            
            // Set up character info for a simple monospace font - increase size for better readability
            int charWidth = fontSize * 3 / 2;  // Increased from fontSize
            int charHeight = fontSize * 2;     // Keep the same
            int charsPerRow = atlasWidth / (charWidth + 4);  // Increased spacing
            int charSpacing = 4;               // Increased from 2
            int lineWidth = Math.Max(3, fontSize / 4);  // Thicker lines for better readability
            
            // Define a consistent baseline
            int baselineOffset = charHeight * 3 / 4; // Position baseline at 3/4 of the character height
            
            // Process standard ASCII characters (32-126)
            for (int charCode = 32; charCode < 127; charCode++)
            {
                char c = (char)charCode;
                
                // Calculate position in the texture atlas
                int row = (charCode - 32) / charsPerRow;
                int col = (charCode - 32) % charsPerRow;
                
                int x = col * (charWidth + charSpacing);
                int y = row * (charHeight + charSpacing);
                
                // Generate character
                switch (c)
                {
                    case ' ': // Space - leave empty
                        break;
                        
                    case '!': // Exclamation mark
                        DrawVerticalLine(data, x + charWidth/2, y + 2, charHeight - 8, lineWidth);
                        DrawDot(data, x + charWidth/2, y + charHeight - 4, lineWidth + 1);
                        break;
                    
                    case '"': // Double quote
                        DrawVerticalLine(data, x + charWidth/3, y + 2, charHeight/4, lineWidth);
                        DrawVerticalLine(data, x + 2*charWidth/3, y + 2, charHeight/4, lineWidth);
                        break;
                    
                    case '#': // Hash
                        DrawHorizontalLine(data, x + 2, y + charHeight/3, charWidth - 4, lineWidth);
                        DrawHorizontalLine(data, x + 2, y + 2*charHeight/3, charWidth - 4, lineWidth);
                        DrawVerticalLine(data, x + charWidth/3, y + 2, charHeight - 4, lineWidth);
                        DrawVerticalLine(data, x + 2*charWidth/3, y + 2, charHeight - 4, lineWidth);
                        break;
                    
                    case '$': // Dollar
                        DrawVerticalLine(data, x + charWidth/2, y + 2, charHeight - 4, lineWidth);
                        DrawHorizontalLine(data, x + 2, y + charHeight/4, charWidth - 4, lineWidth);
                        DrawHorizontalLine(data, x + 2, y + 3*charHeight/4, charWidth - 4, lineWidth);
                        DrawHorizontalLine(data, x + 2, y + charHeight/2, charWidth - 4, lineWidth);
                        break;

                    case '0': // Zero
                        DrawCircle(data, x + charWidth/2, y + charHeight/2, charHeight/3, lineWidth);
                        DrawLine(data, x + charWidth/3, y + charHeight/3, x + 2*charWidth/3, y + 2*charHeight/3, lineWidth);
                        break;
                    
                    case '1': // One
                        DrawVerticalLine(data, x + charWidth/2, y + 2, charHeight - 4, lineWidth);
                        DrawLine(data, x + charWidth/4, y + charWidth/3, x + charWidth/2, y + 2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 4, charWidth/2, lineWidth);
                        break;
                    
                    case '2': // Two
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + charWidth/4, y + charHeight/2, charHeight/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        break;
                    
                    case '3': // Three
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        break;
                    
                    case '4': // Four
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawVerticalLine(data, x + charWidth/4, y + 2, charHeight/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        break;
                    
                    case '5': // Five
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + charWidth/4, y + 2, charHeight/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + charHeight/2, charHeight/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        break;
                    
                    case '6': // Six
                        DrawVerticalLine(data, x + charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + charHeight/2, charHeight/2, lineWidth);
                        break;
                    
                    case '7': // Seven
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight - 4, lineWidth);
                        break;
                    
                    case '8': // Eight
                        DrawVerticalLine(data, x + charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        break;
                    
                    case '9': // Nine
                        DrawVerticalLine(data, x + 3*charWidth/4, y + 2, charHeight - 4, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + 2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight/2, charWidth/2, lineWidth);
                        DrawHorizontalLine(data, x + charWidth/4, y + charHeight - 2, charWidth/2, lineWidth);
                        DrawVerticalLine(data, x + charWidth/4, y + 2, charHeight/2, lineWidth);
                        break;
                        
                    // Default case for letters - create a simple letter-like shape based on character code
                    default:
                        // Draw a box outline for most characters
                        int margin = Math.Max(2, fontSize / 6); // Increased margin
                        
                        // For letters, create more distinctive shapes
                        if (c >= 'A' && c <= 'Z')
                        {
                            // Capital letters
                            if (c == 'A')
                            {
                                // Letter A
                                DrawLine(data, x + charWidth/2, y + 2, x + 2, y + charHeight - 2, lineWidth);
                                DrawLine(data, x + charWidth/2, y + 2, x + charWidth - 2, y + charHeight - 2, lineWidth);
                                DrawHorizontalLine(data, x + 4, y + charHeight/2, charWidth - 8, lineWidth);
                            }
                            else if (c == 'B')
                            {
                                // Letter B
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight/2 - margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + charHeight/2, charHeight/2 - margin, lineWidth);
                            }
                            else if (c == 'C')
                            {
                                // Letter C
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'D')
                            {
                                // Letter D
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth/2, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth/2, lineWidth);
                                DrawArc(data, x + charWidth/2, y + charHeight/2, charHeight/3, lineWidth, 270, 450);
                            }
                            else if (c == 'E')
                            {
                                // Letter E
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'F')
                            {
                                // Letter F
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'G')
                            {
                                // Letter G
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + charHeight/2, charHeight/2 - margin, lineWidth);
                                DrawHorizontalLine(data, x + charWidth/2, y + charHeight/2, charWidth/2 - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'H')
                            {
                                // Letter H
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'I')
                            {
                                // Letter I
                                DrawVerticalLine(data, x + charWidth/2, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'J')
                            {
                                // Letter J
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - margin - charHeight/3, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth/2, lineWidth);
                                DrawVerticalLine(data, x + margin, y + charHeight - margin - lineWidth - charHeight/3, charHeight/3, lineWidth);
                            }
                            else if (c == 'K')
                            {
                                // Letter K
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawLine(data, x + margin, y + charHeight/2, x + charWidth - margin - lineWidth, y + margin, lineWidth);
                                DrawLine(data, x + margin, y + charHeight/2, x + charWidth - margin - lineWidth, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'L')
                            {
                                // Letter L
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'M')
                            {
                                // Letter M
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - 2*margin, lineWidth);
                                DrawLine(data, x + margin, y + margin, x + charWidth/2, y + charHeight/2, lineWidth);
                                DrawLine(data, x + charWidth - margin - lineWidth, y + margin, x + charWidth/2, y + charHeight/2, lineWidth);
                            }
                            else if (c == 'N')
                            {
                                // Letter N
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - 2*margin, lineWidth);
                                DrawLine(data, x + margin, y + margin, x + charWidth - margin - lineWidth, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'O')
                            {
                                // Letter O
                                DrawCircle(data, x + charWidth/2, y + charHeight/2, charHeight/3, lineWidth);
                            }
                            else if (c == 'P')
                            {
                                // Letter P
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight/2 - margin, lineWidth);
                            }
                            else if (c == 'Q')
                            {
                                // Letter Q
                                DrawCircle(data, x + charWidth/2, y + charHeight/2, charHeight/3, lineWidth);
                                DrawLine(data, x + charWidth/2, y + charHeight/2, x + charWidth - margin - lineWidth, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'R')
                            {
                                // Letter R
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight/2 - margin, lineWidth);
                                DrawLine(data, x + margin + charWidth/4, y + charHeight/2, x + charWidth - margin - lineWidth, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'S')
                            {
                                // Letter S
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + margin, y + margin, charHeight/2 - margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight/2, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + charHeight/2, charHeight/2 - margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            }
                            else if (c == 'T')
                            {
                                // Letter T
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth/2, y + margin, charHeight - 2*margin, lineWidth);
                            }
                            else if (c == 'U')
                            {
                                // Letter U
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - margin - charHeight/4, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - margin - charHeight/4, lineWidth);
                                DrawArc(data, x + charWidth/2, y + charHeight - margin - charHeight/4, charWidth/2 - margin - lineWidth, lineWidth, 0, 180);
                            }
                            else if (c == 'V')
                            {
                                // Letter V
                                DrawLine(data, x + margin, y + margin, x + charWidth/2, y + charHeight - margin - lineWidth, lineWidth);
                                DrawLine(data, x + charWidth - margin - lineWidth, y + margin, x + charWidth/2, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'W')
                            {
                                // Letter W
                                DrawLine(data, x + margin, y + margin, x + charWidth/4, y + charHeight - margin - lineWidth, lineWidth);
                                DrawLine(data, x + charWidth/4, y + charHeight - margin - lineWidth, x + charWidth/2, y + charHeight/2, lineWidth);
                                DrawLine(data, x + charWidth/2, y + charHeight/2, x + 3*charWidth/4, y + charHeight - margin - lineWidth, lineWidth);
                                DrawLine(data, x + 3*charWidth/4, y + charHeight - margin - lineWidth, x + charWidth - margin - lineWidth, y + margin, lineWidth);
                            }
                            else if (c == 'X')
                            {
                                // Letter X
                                DrawLine(data, x + margin, y + margin, x + charWidth - margin - lineWidth, y + charHeight - margin - lineWidth, lineWidth);
                                DrawLine(data, x + charWidth - margin - lineWidth, y + margin, x + margin, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else if (c == 'Y')
                            {
                                // Letter Y
                                DrawLine(data, x + margin, y + margin, x + charWidth/2, y + charHeight/2, lineWidth);
                                DrawLine(data, x + charWidth - margin - lineWidth, y + margin, x + charWidth/2, y + charHeight/2, lineWidth);
                                DrawVerticalLine(data, x + charWidth/2, y + charHeight/2, charHeight/2 - margin, lineWidth);
                            }
                            else if (c == 'Z')
                            {
                                // Letter Z
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                                DrawLine(data, x + charWidth - margin - lineWidth, y + margin, x + margin, y + charHeight - margin - lineWidth, lineWidth);
                            }
                            else
                            {
                                // Other capital letters - simple rectangular representation
                                DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                                DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - 2*margin, lineWidth);
                            }
                        }
                        else if (c >= 'a' && c <= 'z')
                        {
                            // Lowercase letters - make them smaller than uppercase
                            int smallMargin = margin + fontSize/8; // Slightly smaller margin for more space
                            int smallHeight = charHeight - fontSize/3 - margin;
                            int yOffset = fontSize/3; // Move lowercase letters down a bit
                            
                            // Draw a simple shape for lowercase
                            if (c == 'a')
                            {
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - smallMargin - lineWidth, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                            }
                            else if (c == 'b')
                            {
                                DrawVerticalLine(data, x + smallMargin, y + margin, charHeight - 2*margin, lineWidth); // Full height b
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - smallMargin - lineWidth, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                            }
                            else if (c == 'c')
                            {
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                            }
                            else if (c == 'd')
                            {
                                DrawVerticalLine(data, x + charWidth - smallMargin - lineWidth, y + margin, charHeight - 2*margin, lineWidth); // Full height d
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                            }
                            else if (c == 'e')
                            {
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + charWidth - smallMargin - lineWidth, y + smallHeight/2 + yOffset, smallHeight/4, lineWidth);
                            }
                            else if (c == 'o')
                            {
                                DrawCircle(data, x + charWidth/2, y + smallHeight/2 + smallHeight/4 + yOffset, smallHeight/3, lineWidth);
                            }
                            else
                            {
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawHorizontalLine(data, x + smallMargin, y + smallHeight + yOffset, charWidth - 2*smallMargin, lineWidth);
                                DrawVerticalLine(data, x + smallMargin, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                                DrawVerticalLine(data, x + charWidth - smallMargin - lineWidth, y + smallHeight/2 + yOffset, smallHeight/2, lineWidth);
                            }
                        }
                        else
                        {
                            // Digits and other characters
                            DrawHorizontalLine(data, x + margin, y + margin, charWidth - 2*margin, lineWidth);
                            DrawHorizontalLine(data, x + margin, y + charHeight - margin - lineWidth, charWidth - 2*margin, lineWidth);
                            DrawVerticalLine(data, x + margin, y + margin, charHeight - 2*margin, lineWidth);
                            DrawVerticalLine(data, x + charWidth - margin - lineWidth, y + margin, charHeight - 2*margin, lineWidth);
                        }
                        break;
                }
                
                // Store character info
                CharInfo info = new CharInfo
                {
                    Size = new Vector2(charWidth, charHeight),
                    // No bearing - we'll position all characters at the same baseline Y
                    Bearing = new Vector2(0, 0),
                    // Use full character width plus a small padding to prevent horizontal squishing
                    Advance = charWidth + 2,
                    TextureCoords = new Vector4(
                        (float)x / atlasWidth,
                        (float)y / atlasHeight,
                        (float)(x + charWidth) / atlasWidth,
                        (float)(y + charHeight) / atlasHeight
                    )
                };
                
                characters[(char)charCode] = info;
            }
            
            // Upload data using Span
            fixed (byte* ptr = data)
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, 
                    (uint)atlasWidth, (uint)atlasHeight, 0, PixelFormat.Red, 
                    PixelType.UnsignedByte, ptr);
            }
            
            gl.BindTexture(TextureTarget.Texture2D, 0);
            Console.WriteLine("Procedural font texture generated successfully.");
        }
        
        private void DrawDot(byte[] data, int centerX, int centerY, int radius)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x*x + y*y <= radius*radius)
                    {
                        int px = centerX + x;
                        int py = centerY + y;
                        if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                            data[py * atlasWidth + px] = 255;
                    }
                }
            }
        }
        
        private void DrawHorizontalLine(byte[] data, int x, int y, int length, int thickness)
        {
            for (int j = 0; j < thickness; j++)
            {
                for (int i = 0; i < length; i++)
                {
                    int px = x + i;
                    int py = y + j - thickness/2;
                    if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                        data[py * atlasWidth + px] = 255;
                }
            }
        }
        
        private void DrawVerticalLine(byte[] data, int x, int y, int length, int thickness)
        {
            for (int j = 0; j < thickness; j++)
            {
                for (int i = 0; i < length; i++)
                {
                    int px = x + j - thickness/2;
                    int py = y + i;
                    if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                        data[py * atlasWidth + px] = 255;
                }
            }
        }
        
        private void DrawLine(byte[] data, int x1, int y1, int x2, int y2, int thickness)
        {
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                // Draw a thick point for better visibility
                for (int ty = -thickness/2; ty <= thickness/2; ty++)
                {
                    for (int tx = -thickness/2; tx <= thickness/2; tx++)
                    {
                        int px = x1 + tx;
                        int py = y1 + ty;
                        if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                            data[py * atlasWidth + px] = 255;
                    }
                }
                
                if (x1 == x2 && y1 == y2) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
        
        private void DrawCircle(byte[] data, int centerX, int centerY, int radius, int thickness)
        {
            for (int y = -radius-thickness/2; y <= radius+thickness/2; y++)
            {
                for (int x = -radius-thickness/2; x <= radius+thickness/2; x++)
                {
                    float distance = MathF.Sqrt(x*x + y*y);
                    if (distance <= radius + thickness/2.0f && distance >= radius - thickness/2.0f)
                    {
                        int px = centerX + x;
                        int py = centerY + y;
                        if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                            data[py * atlasWidth + px] = 255;
                    }
                }
            }
        }
        
        private void DrawArc(byte[] data, int centerX, int centerY, int radius, int thickness, int startAngle, int endAngle)
        {
            for (int i = startAngle; i <= endAngle; i++)
            {
                double angle = i * Math.PI / 180.0;
                int x = (int)(centerX + radius * Math.Cos(angle));
                int y = (int)(centerY + radius * Math.Sin(angle));
                
                for (int ty = -thickness/2; ty <= thickness/2; ty++)
                {
                    for (int tx = -thickness/2; tx <= thickness/2; tx++)
                    {
                        int px = x + tx;
                        int py = y + ty;
                        if (py < atlasHeight && px < atlasWidth && py >= 0 && px >= 0)
                            data[py * atlasWidth + px] = 255;
                    }
                }
            }
        }
        
        public unsafe void RenderText(string text, float x, float y, float scale, Vector4 color, Matrix4x4 projection)
        {
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
            
            int colorLocation = gl.GetUniformLocation(shader, "textColor");
            gl.Uniform4(colorLocation, color.X, color.Y, color.Z, color.W);
            
            int textLocation = gl.GetUniformLocation(shader, "text");
            gl.Uniform1(textLocation, 0);
            
            // Activate texture
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, texture);
            
            // Enable blending for text
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            // Bind VAO
            gl.BindVertexArray(vao);
            
            // Process each character
            float startX = x;
            float currentY = y; 
            
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    // Move to the next line with adequate spacing
                    currentY += fontSize * scale * 1.2f;
                    x = startX;
                    continue;
                }
                
                if (!characters.TryGetValue(c, out CharInfo charInfo))
                {
                    // Skip unknown characters
                    continue;
                }
                
                // FIXED APPROACH: Position all characters with their top at the same Y coordinate
                // This ensures they all align properly regardless of height differences
                float xpos = x;
                float ypos = currentY; 
                
                float w = charInfo.Size.X * scale;
                float h = charInfo.Size.Y * scale;
                
                // Update VBO
                float[] vertices = {
                    // pos            // tex
                    xpos,     ypos + h, charInfo.TextureCoords.X, charInfo.TextureCoords.W, // bottom left
                    xpos,     ypos,     charInfo.TextureCoords.X, charInfo.TextureCoords.Y, // top left
                    xpos + w, ypos,     charInfo.TextureCoords.Z, charInfo.TextureCoords.Y, // top right
                    
                    xpos,     ypos + h, charInfo.TextureCoords.X, charInfo.TextureCoords.W, // bottom left
                    xpos + w, ypos,     charInfo.TextureCoords.Z, charInfo.TextureCoords.Y, // top right
                    xpos + w, ypos + h, charInfo.TextureCoords.Z, charInfo.TextureCoords.W  // bottom right
                };
                
                // Update VBO
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                fixed (float* ptr = vertices)
                {
                    gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertices.Length * sizeof(float)), ptr);
                }
                
                // Render quad
                gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
                
                // Advance to next glyph
                x += charInfo.Advance * scale;
            }
            
            // Unbind
            gl.BindVertexArray(0);
            gl.BindTexture(TextureTarget.Texture2D, 0);
            gl.UseProgram(0);
            
            // Restore state
            gl.Disable(EnableCap.Blend);
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
        
        public void Dispose()
        {
            // Clean up resources
            gl.DeleteProgram(shader);
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteTexture(texture);
        }
        
        private struct CharInfo
        {
            public Vector2 Size;     // Size of glyph
            public Vector2 Bearing;  // Offset from baseline to left/top of glyph
            public float Advance;    // Horizontal offset to advance to next glyph
            public Vector4 TextureCoords; // UV coordinates in texture atlas (x, y, x2, y2)
        }
        
        // Public method to adjust font size at runtime
        public void SetFontSize(int newSize)
        {
            if (newSize != fontSize && newSize > 0 && newSize <= 72)
            {
                fontSize = newSize;
                // Regenerate font atlas with new size
                characters.Clear();
                gl.DeleteTexture(texture);
                GenerateFontAtlas();
            }
        }
        
        // Public method to toggle anti-aliasing
        public void SetAntiAliasing(bool enabled)
        {
            if (enabled != useAntiAliasing)
            {
                useAntiAliasing = enabled;
                // Regenerate font atlas with new anti-aliasing setting
                characters.Clear();
                gl.DeleteTexture(texture);
                GenerateFontAtlas();
            }
        }
    }
} 
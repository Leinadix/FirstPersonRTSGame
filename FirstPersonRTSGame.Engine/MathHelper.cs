using System;
using System.Numerics;

namespace FirstPersonRTSGame.Engine
{
    public static class MathHelper
    {
        /// <summary>
        /// Linearly interpolates between a and b by t.
        /// </summary>
        /// <param name="a">Start value.</param>
        /// <param name="b">End value.</param>
        /// <param name="t">Interpolation factor (0.0 to 1.0).</param>
        /// <returns>The interpolated value.</returns>
        public static float Lerp(float a, float b, float t)
        {
            // Ensure t is clamped between 0 and 1
            t = Math.Clamp(t, 0.0f, 1.0f);
            return a + (b - a) * t;
        }
        
        /// <summary>
        /// Smoothly interpolates between a and b by t using smoothstep function.
        /// </summary>
        /// <param name="a">Start value.</param>
        /// <param name="b">End value.</param>
        /// <param name="t">Interpolation factor (0.0 to 1.0).</param>
        /// <returns>The smoothly interpolated value.</returns>
        public static float SmoothStep(float a, float b, float t)
        {
            t = Math.Clamp(t, 0.0f, 1.0f);
            t = t * t * (3.0f - 2.0f * t); // Smoothstep formula
            return Lerp(a, b, t);
        }
        
        /// <summary>
        /// Maps a value from one range to another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromMin">Source range minimum.</param>
        /// <param name="fromMax">Source range maximum.</param>
        /// <param name="toMin">Target range minimum.</param>
        /// <param name="toMax">Target range maximum.</param>
        /// <returns>The mapped value.</returns>
        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = (value - fromMin) / (fromMax - fromMin);
            return Lerp(toMin, toMax, t);
        }
        
        /// <summary>
        /// Calculates a 2D distance function for various shapes like circles, boxes, etc.
        /// Useful for terrain generation.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <param name="center">Center of the shape.</param>
        /// <param name="radius">Size of the shape.</param>
        /// <returns>Distance from point to the shape's edge.</returns>
        public static float CircleDistance(Vector2 p, Vector2 center, float radius)
        {
            return Vector2.Distance(p, center) - radius;
        }

        // Linear interpolation between two vectors
        public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            float clampedT = Math.Clamp(t, 0.0f, 1.0f);
            return new Vector3(
                start.X + (end.X - start.X) * clampedT,
                start.Y + (end.Y - start.Y) * clampedT,
                start.Z + (end.Z - start.Z) * clampedT
            );
        }
        
        // Distance between two 2D points
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        
        // Perlin-like noise function (based on sine waves)
        // This is not true Perlin noise but provides a similar effect for terrain generation
        public static float GenerateNoise(float x, float y)
        {
            // Simple but effective noise function that combines multiple sine waves
            float noise = 0;
            
            noise += (float)Math.Sin(x * 1.0f) * (float)Math.Cos(y * 1.0f) * 0.5f;
            noise += (float)Math.Sin(x * 2.0f + 0.5f) * (float)Math.Cos(y * 2.0f + 0.5f) * 0.25f;
            noise += (float)Math.Sin(x * 4.0f + 1.0f) * (float)Math.Cos(y * 4.0f + 1.0f) * 0.125f;
            noise += (float)Math.Sin(x * 8.0f + 2.0f) * (float)Math.Cos(y * 8.0f + 2.0f) * 0.0625f;
            
            // Normalize to 0-1 range
            noise = (noise + 1) * 0.5f;
            
            return noise;
        }
    }
} 
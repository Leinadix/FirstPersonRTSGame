using System;
using System.Collections.Generic;
using System.Numerics;

namespace FirstPersonRTSGame.Game.UI
{
    public class UIAnimations
    {
        private Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        private Dictionary<string, float> values = new Dictionary<string, float>();
        
        public UIAnimations()
        {
            // Initialize with empty animations
        }
        
        public void CreateAnimation(string name, float startValue, float endValue, float duration, EasingType easingType = EasingType.EaseInOut)
        {
            animations[name] = new Animation
            {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                EasingType = easingType,
                CurrentTime = 0,
                IsPlaying = false,
                IsReversed = false
            };
            
            values[name] = startValue;
        }
        
        public void PlayAnimation(string name, bool reverse = false)
        {
            if (!animations.ContainsKey(name))
                return;
                
            var animation = animations[name];
            animation.IsPlaying = true;
            animation.IsReversed = reverse;
            
            if (reverse)
            {
                // If reversing, swap start and end values
                animation.CurrentTime = animation.Duration;
            }
            else
            {
                animation.CurrentTime = 0;
            }
            
            animations[name] = animation;
        }
        
        public void StopAnimation(string name)
        {
            if (!animations.ContainsKey(name))
                return;
                
            var animation = animations[name];
            animation.IsPlaying = false;
            animations[name] = animation;
        }
        
        public bool IsPlaying(string name)
        {
            return animations.ContainsKey(name) && animations[name].IsPlaying;
        }
        
        public float GetValue(string name)
        {
            if (!values.ContainsKey(name))
                return 0;
                
            return values[name];
        }
        
        public void Update(float deltaTime)
        {
            foreach (var key in animations.Keys)
            {
                var animation = animations[key];
                
                if (!animation.IsPlaying)
                    continue;
                    
                if (animation.IsReversed)
                {
                    animation.CurrentTime -= deltaTime;
                    
                    if (animation.CurrentTime <= 0)
                    {
                        animation.CurrentTime = 0;
                        animation.IsPlaying = false;
                        values[key] = animation.StartValue;
                    }
                    else
                    {
                        float progress = animation.CurrentTime / animation.Duration;
                        values[key] = Lerp(animation.EndValue, animation.StartValue, ApplyEasing(progress, animation.EasingType));
                    }
                }
                else
                {
                    animation.CurrentTime += deltaTime;
                    
                    if (animation.CurrentTime >= animation.Duration)
                    {
                        animation.CurrentTime = animation.Duration;
                        animation.IsPlaying = false;
                        values[key] = animation.EndValue;
                    }
                    else
                    {
                        float progress = animation.CurrentTime / animation.Duration;
                        values[key] = Lerp(animation.StartValue, animation.EndValue, ApplyEasing(progress, animation.EasingType));
                    }
                }
                
                animations[key] = animation;
            }
        }
        
        private float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }
        
        private Vector2 Lerp(Vector2 start, Vector2 end, float t)
        {
            return new Vector2(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t
            );
        }
        
        private float ApplyEasing(float t, EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.Linear:
                    return t;
                    
                case EasingType.EaseIn:
                    return t * t;
                    
                case EasingType.EaseOut:
                    return t * (2 - t);
                    
                case EasingType.EaseInOut:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                    
                case EasingType.Bounce:
                    if (t < (1 / 2.75f))
                    {
                        return 7.5625f * t * t;
                    }
                    else if (t < (2 / 2.75f))
                    {
                        t -= (1.5f / 2.75f);
                        return 7.5625f * t * t + 0.75f;
                    }
                    else if (t < (2.5f / 2.75f))
                    {
                        t -= (2.25f / 2.75f);
                        return 7.5625f * t * t + 0.9375f;
                    }
                    else
                    {
                        t -= (2.625f / 2.75f);
                        return 7.5625f * t * t + 0.984375f;
                    }
                    
                case EasingType.Elastic:
                    const float p = 0.3f;
                    return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
                    
                default:
                    return t;
            }
        }
        
        public void CreateColorTransition(string name, Vector4 startColor, Vector4 endColor, float duration, EasingType easingType = EasingType.EaseInOut)
        {
            // Create animations for each channel
            CreateAnimation(name + "_r", startColor.X, endColor.X, duration, easingType);
            CreateAnimation(name + "_g", startColor.Y, endColor.Y, duration, easingType);
            CreateAnimation(name + "_b", startColor.Z, endColor.Z, duration, easingType);
            CreateAnimation(name + "_a", startColor.W, endColor.W, duration, easingType);
        }
        
        public void PlayColorTransition(string name, bool reverse = false)
        {
            PlayAnimation(name + "_r", reverse);
            PlayAnimation(name + "_g", reverse);
            PlayAnimation(name + "_b", reverse);
            PlayAnimation(name + "_a", reverse);
        }
        
        public Vector4 GetColor(string name)
        {
            return new Vector4(
                GetValue(name + "_r"),
                GetValue(name + "_g"),
                GetValue(name + "_b"),
                GetValue(name + "_a")
            );
        }
    }
    
    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic
    }
    
    public struct Animation
    {
        public float StartValue;
        public float EndValue;
        public float Duration;
        public float CurrentTime;
        public bool IsPlaying;
        public bool IsReversed;
        public EasingType EasingType;
    }
} 
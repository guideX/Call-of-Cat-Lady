using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Plays skeletal animations from FBX models
    /// Handles bone transformations and keyframe interpolation
    /// </summary>
    public class AnimationPlayer
    {
        private AnimationClip currentClip;
        private TimeSpan currentTime;
        private int currentKeyframe;
        private Matrix[] boneTransforms;
        private Matrix[] worldTransforms;
        private Matrix[] skinTransforms;
        private bool isPlaying;
        private float playbackSpeed;

        public Matrix[] GetBoneTransforms() => skinTransforms;
        public bool IsPlaying => isPlaying;

        public AnimationPlayer(int boneCount)
        {
            boneTransforms = new Matrix[boneCount];
            worldTransforms = new Matrix[boneCount];
            skinTransforms = new Matrix[boneCount];
            playbackSpeed = 1.0f;
            
            // Initialize to identity
            for (int i = 0; i < boneCount; i++)
            {
                boneTransforms[i] = Matrix.Identity;
                worldTransforms[i] = Matrix.Identity;
                skinTransforms[i] = Matrix.Identity;
            }
        }

        public void StartClip(AnimationClip clip, float speed = 1.0f)
        {
            currentClip = clip;
            currentTime = TimeSpan.Zero;
            currentKeyframe = 0;
            isPlaying = true;
            playbackSpeed = speed;
        }

        public void StopClip()
        {
            isPlaying = false;
            currentTime = TimeSpan.Zero;
            currentKeyframe = 0;
        }

        public void Update(GameTime gameTime, bool loop, Matrix rootTransform)
        {
            if (!isPlaying || currentClip == null)
                return;

            // Update current time
            currentTime += TimeSpan.FromSeconds(
                gameTime.ElapsedGameTime.TotalSeconds * playbackSpeed);

            // Handle looping
            if (currentTime >= currentClip.Duration)
            {
                if (loop)
                {
                    currentTime = TimeSpan.Zero;
                    currentKeyframe = 0;
                }
                else
                {
                    currentTime = currentClip.Duration;
                    isPlaying = false;
                }
            }

            // Update bone transforms from animation
            if (currentClip.Bones != null && currentClip.Bones.Count > 0)
            {
                foreach (var bone in currentClip.Bones)
                {
                    if (bone.Index < boneTransforms.Length)
                    {
                        boneTransforms[bone.Index] = GetBoneTransform(bone);
                    }
                }
            }

            // Calculate world transforms
            CalculateWorldTransforms(rootTransform);

            // Calculate skin transforms for rendering
            CalculateSkinTransforms();
        }

        private Matrix GetBoneTransform(AnimationBone bone)
        {
            if (bone.Keyframes == null || bone.Keyframes.Count == 0)
                return Matrix.Identity;

            // Find current keyframe
            int keyframeIndex = 0;
            for (int i = 0; i < bone.Keyframes.Count; i++)
            {
                if (bone.Keyframes[i].Time <= currentTime)
                    keyframeIndex = i;
                else
                    break;
            }

            // Get current and next keyframe
            var currentKeyframe = bone.Keyframes[keyframeIndex];
            
            // If at the last keyframe, just use it
            if (keyframeIndex >= bone.Keyframes.Count - 1)
                return currentKeyframe.Transform;

            var nextKeyframe = bone.Keyframes[keyframeIndex + 1];

            // Interpolate between keyframes
            float t = (float)((currentTime - currentKeyframe.Time).TotalSeconds /
                             (nextKeyframe.Time - currentKeyframe.Time).TotalSeconds);
            t = MathHelper.Clamp(t, 0, 1);

            return Matrix.Lerp(currentKeyframe.Transform, nextKeyframe.Transform, t);
        }

        private void CalculateWorldTransforms(Matrix rootTransform)
        {
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            for (int i = 1; i < worldTransforms.Length; i++)
            {
                int parentIndex = 0; // Simplified - in real scenarios, you'd have parent indices
                worldTransforms[i] = boneTransforms[i] * worldTransforms[parentIndex];
            }
        }

        private void CalculateSkinTransforms()
        {
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = worldTransforms[i];
            }
        }
    }

    /// <summary>
    /// Represents an animation clip with keyframes
    /// </summary>
    public class AnimationClip
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public List<AnimationBone> Bones { get; set; }

        public AnimationClip()
        {
            Bones = new List<AnimationBone>();
        }
    }

    /// <summary>
    /// Represents a bone in the animation
    /// </summary>
    public class AnimationBone
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<AnimationKeyframe> Keyframes { get; set; }

        public AnimationBone()
        {
            Keyframes = new List<AnimationKeyframe>();
        }
    }

    /// <summary>
    /// Represents a keyframe in the animation
    /// </summary>
    public class AnimationKeyframe
    {
        public TimeSpan Time { get; set; }
        public Matrix Transform { get; set; }
    }

    /// <summary>
    /// Helper to extract animation data from Model
    /// </summary>
    public static class AnimationExtractor
    {
        public static AnimationClip ExtractAnimation(Model model, string clipName = null)
        {
            var clip = new AnimationClip();
            clip.Name = clipName ?? "Walk";

            // Try to get animation from model tag
            if (model.Tag is Dictionary<string, object> tagDict)
            {
                if (tagDict.TryGetValue("Animations", out object animationsObj))
                {
                    // Handle animation data if present
                    // This is a simplified version - actual FBX animation extraction
                    // would require more complex parsing
                }
            }

            // If no animation data found, create a simple procedural walk cycle
            // that matches the cat's leg animation system
            if (clip.Bones.Count == 0)
            {
                CreateProceduralWalkCycle(clip);
            }

            return clip;
        }

        private static void CreateProceduralWalkCycle(AnimationClip clip)
        {
            // Create a 1-second walk cycle
            clip.Duration = TimeSpan.FromSeconds(1.0);

            // We'll use the existing leg animation system
            // This is a fallback if the FBX doesn't have embedded animations
        }
    }
}

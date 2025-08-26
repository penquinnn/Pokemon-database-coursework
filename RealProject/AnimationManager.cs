using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    internal class AnimationManager
    {
        Animation currentAnimation;
        public Dictionary<string, Animation> animations;

        float animLength;
        float timer;

        bool isPlaying;

        public AnimationManager()
        {
            animations = new Dictionary<string, Animation>();
        }

        public void Update()
        {
            if(!isPlaying) return;

            if (timer >= animLength)
                timer = 0;
            else
                timer += Global.deltaTime;
        }

        public void Play(string anim)
        {
            if (currentAnimation != animations[anim])
            {
                currentAnimation = animations[anim];

                isPlaying = true;
                animLength = currentAnimation.frameDuration * currentAnimation.frames.Count();
                timer = 0;
            }
        }

        public void Stop()
        {
            isPlaying = false;
        }

        public Texture2D GetFrameTexture()
        {
            if (currentAnimation == null || currentAnimation.frames.Count == 0)
                return null;

            int index = (int)(timer / currentAnimation.frameDuration);
            index = Math.Clamp(index, 0, currentAnimation.frames.Count - 1);

            return currentAnimation.frames[index];
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    internal class Animation
    {
        public List<Texture2D> frames;
        public Texture2D animationSheet;
        public float frameDuration;

        public Animation(List<Rectangle> spriteBounds, Texture2D sheet, float frameDuration) 
        {
            animationSheet = sheet;
            SetFrames(spriteBounds);
            this.frameDuration = frameDuration;
        }

        void SetFrames(List<Rectangle> spriteBounds)
        {
            frames = new List<Texture2D>();

            foreach (Rectangle rect in spriteBounds)
            {
                frames.Add(Global.CropTexture(animationSheet, rect));
            }
        }
    }
}

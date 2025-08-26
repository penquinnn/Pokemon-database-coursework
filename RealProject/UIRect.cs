using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RealProject
{
    internal class UIRect
    {
        protected Texture2D image;

        int scaleFactor;

        public Vector2 pos;
        protected Vector2 renderSize;

        protected Color color;

        public bool enabled { get; private set; } = true;

        public List<UIRect> children = new List<UIRect> ();


        public UIRect(Texture2D image, float x, float y, UIRect parent = null)
        {
            this.image = image;

            pos = new Vector2(x, y);
            if (image != null)
                renderSize = new Vector2(image.Width, image.Height) * Global.pixelsPerUnit;
            else
                renderSize = Vector2.One;

            if(parent != null)
            {
                parent.AddChild(this);
            }

            color = Color.White;
            Debug.WriteLine(pos);
        }

        public void SetPosition(Vector2 position)
        {
            pos = position;
        }

        public void SetSize(Vector2 size)
        {
            renderSize = size;
        }

        public void SetSprite(Texture2D t)
        {
            image = t;
        }

        public virtual void SetColor(Color c)
        {
            color = c;
        }

        public void AddChild(UIRect child)
        {
            children.Add(child);
        }

        public void SetEnabled(bool en)
        {
            enabled = en;

            foreach (var v in children)
            {
                v.SetEnabled(en);
            }
        }

        public virtual void Update()
        {
            if (!enabled) return;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (image != null)
            {
                spriteBatch.Draw(image,
                    new Rectangle((int)(pos.X - renderSize.X / 2), (int)(pos.Y - renderSize.Y / 2), (int)renderSize.X, (int)renderSize.Y),
                    color);
            }

            if(UIManager.doDebug)
            {
                spriteBatch.Draw(UIManager.debugSquare, 
                    new Rectangle((int)(pos.X - renderSize.X / 2), (int)(pos.Y - renderSize.Y / 2), (int)renderSize.X, (int)renderSize.Y), 
                    new Color(255, 255, 255, 128));
            }
        }
    }
}

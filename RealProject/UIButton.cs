using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RealProject
{
    internal class UIButton : UIRect
    {
        Action onClick;
        bool isHovering;
        Vector2 buttonSize;
        bool interactable = true;

        public UIButton(Texture2D image, float x, float y, Action onClick, Vector2 buttonSize, UIRect parent = null) : base(image, x, y, parent)
        {
            this.onClick = onClick;
            this.buttonSize = buttonSize;
        }

        public override void Update()
        {
            base.Update();

            Vector2 mousepos = InputManager.GetMousePosition();

            float leftBound = pos.X - buttonSize.X / 2;
            float rightBound = pos.X + buttonSize.X / 2;
            float upBound = pos.Y - buttonSize.Y / 2;
            float downBound = pos.Y + buttonSize.Y / 2;

            if (mousepos.X >= leftBound && mousepos.X <= rightBound && mousepos.Y >= upBound && mousepos.Y <= downBound && interactable)
                isHovering = true;
            else
                isHovering = false;

            if (onClick != null)
            {
                if (InputManager.GetMouseButtonDown(0))
                {
                    if (isHovering)
                        onClick?.Invoke();
                }
            }
        }

        public bool CheckHover()
        {
            return isHovering;
        }

        public void RandomPos()
        {
            float x = (float)new Random().NextDouble()*Global.screenWidth;
            float y = (float)new Random().NextDouble()*Global.screenHeight;

            base.SetPosition(new Vector2(x, y));
        }

        public void SetInteractable(bool a)
        {
            interactable = a;
        }
    }
}

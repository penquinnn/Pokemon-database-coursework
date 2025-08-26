//using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    public static class InputManager
    {
        static MouseState previousMouseState;
        static MouseState currentMouseState;
        static KeyboardState previousKeyboardState;
        static KeyboardState currentKeyboardState;

        public static void Update()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
        }

        public static Vector2 GetMousePosition()
        {
            return new Vector2(currentMouseState.X, currentMouseState.Y);
        }

        public static bool GetMouseButtonDown(int button)
        {
            if (button == 0)
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    return true;
                }
            }
            else if (button == 1)
            {
                if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                {
                    return true;
                }
            } 
            else if(button == 2)
            {
                if (currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released)
                {
                    return true;
                }
            }
            else if (button == 3)
            {
                if (currentMouseState.XButton2 == ButtonState.Pressed && previousMouseState.XButton2 == ButtonState.Released)
                {
                    return true;
                }
            }
            else if (button == 4)
            {
                if (currentMouseState.XButton1 == ButtonState.Pressed && previousMouseState.XButton1 == ButtonState.Released)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetMouseButtonUp(int button)
        {
            if (button == 0)
            {
                if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 1)
            {
                if (currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 2)
            {
                if (currentMouseState.MiddleButton == ButtonState.Released && previousMouseState.MiddleButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 3)
            {
                if (currentMouseState.XButton2 == ButtonState.Released && previousMouseState.XButton2 == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 4)
            {
                if (currentMouseState.XButton1 == ButtonState.Released && previousMouseState.XButton1 == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetMouseButton(int button)
        {
            if (button == 0)
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 1)
            {
                if (currentMouseState.RightButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 2)
            {
                if (currentMouseState.MiddleButton == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 2)
            {
                if (currentMouseState.XButton2 == ButtonState.Pressed)
                {
                    return true;
                }
            }
            else if (button == 2)
            {
                if (currentMouseState.XButton1 == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetKeyDown(Keys keyCode)
        {
            if(currentKeyboardState.IsKeyDown(keyCode) && previousKeyboardState.IsKeyUp(keyCode))
            {
                return true;
            }

            return false;
        }
        public static bool GetKeyUp(Keys keyCode)
        {
            if (currentKeyboardState.IsKeyUp(keyCode) && previousKeyboardState.IsKeyDown(keyCode))
            {
                return true;
            }

            return false;
        }

        public static bool GetKey(Keys keyCode)
        {
            if (currentKeyboardState.IsKeyDown(keyCode))
            {
                return true;
            }

            return false;
        }

        public static IEnumerator<object> GetButtonDown_Continue()
        {
            do
            {
                yield return 1 / 60f;
            } while (!(InputManager.GetKeyDown(Keys.Space) || InputManager.GetMouseButtonDown(0)));
        }
    }
}

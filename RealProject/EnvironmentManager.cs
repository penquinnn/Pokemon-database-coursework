using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RealProject
{
    
    static class EnvironmentManager
    {
        public static Texture2D square;
        public static float timeOfDay; // 0 = midday, 1 = midnight
        static float dayLength = 64;
        static int timeDirection;
        static int alpha;

        public enum DayTime
        {
            Day,
            DuskDawn,
            Night
        }

        public static DayTime dayTime;

        public static void Initialize(ContentManager content)
        {
            square = content.Load<Texture2D>("Solid_white");
            timeDirection = 1;
            timeOfDay = 0.3f;
            alpha = 0;
            dayTime = DayTime.Day;
        }

        public static void Update()
        {
            timeOfDay += Global.deltaTime * timeDirection / dayLength;

            //if(InputManager.GetMouseButton(1))
            //    timeOfDay += Global.deltaTime * timeDirection / dayLength;

            if (timeOfDay >= 1)
                timeDirection = -1;
            else if(timeOfDay <= 0)
                timeDirection = 1;

            timeOfDay = MathHelper.Clamp(timeOfDay, 0, 1);

            //Debug.WriteLine(timeOfDay);

            SetAlpha();
        }

        static void SetAlpha()
        {
            if (timeOfDay < 0.05f)
            {
                alpha = 1;
                dayTime = DayTime.Night;
            }
            if (timeOfDay < 0.2f) // Midnight to dawn
            {
                alpha = (int)MathHelper.Lerp(200, 100, (timeOfDay - 0.05f) / 0.15f);
                dayTime = DayTime.Night;
            }
            else if (timeOfDay < 0.3f) // Dawn to day
            {
                alpha = (int)MathHelper.Lerp(100, 0, (timeOfDay - 0.2f) / 0.10f);
                dayTime = DayTime.DuskDawn;
            }
            else if (timeOfDay <= 0.7f) // Day
            {
                alpha = 0;
                dayTime = DayTime.Day;
            }
            else if (timeOfDay <= 0.8f) // Day to dusk
            {
                alpha = (int)MathHelper.Lerp(0, 100, (timeOfDay - 0.7f) / 0.10f);
                dayTime = DayTime.DuskDawn;
            }
            else if (timeOfDay < 0.95f) // Dusk to midnight
            {
                alpha = (int)MathHelper.Lerp(100, 200, (timeOfDay - 0.8f) / 0.15f);
                dayTime = DayTime.Night;
            }
            else if (timeOfDay <= 1)
            {
                alpha = 200;
                dayTime = DayTime.Night;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(square,
                new Rectangle(0, 0, Global.screenWidth, Global.screenHeight),
                new Color(0, 0, 32, alpha));
        }
    }
}

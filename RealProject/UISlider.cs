using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace RealProject
{
    internal class UISlider : UIRect
    {
        public float minValue;
        public float maxValue;
        public float rawValue;
        public float value;
        bool doInt;
        bool doGradient;
        Vector2 originalPosition;
        Vector2 maxSize;
        Color maxColor;
        Color minColor;
        Action onValueChanged = null;

        public UISlider(Texture2D image, float x, float y,
            float minValue, float maxValue, float value, 
            Vector2 size, Color baseColor, bool doInt, bool doGradient = false, 
            Color? minColor = null, UIRect parent = null) : base(image, x, y, parent)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.doInt = doInt;
            SetValue(value);
            originalPosition = new Vector2(x, y);
            maxSize = size;

            maxColor = baseColor;
            base.SetColor(maxColor);
            this.doGradient = doGradient;

            if(doGradient == true)
                if (minColor != null)
                    this.minColor = (Color)minColor;
        }

        public override void Update()
        {
            base.Update();

            SetSize();

            SetPosition();

            SetSliderColor();
        }

        void SetSize()
        {
            float range = maxValue - minValue;
            float percent = (value-minValue) / range;
            base.SetSize(new Vector2(maxSize.X*percent, maxSize.Y));
        }

        void SetPosition()
        {
            base.SetPosition(new Vector2(originalPosition.X - (maxSize.X-renderSize.X)/2, originalPosition.Y));
        }

        public void SetAction(Action doThis)
        {
            onValueChanged = doThis;

            Debug.WriteLine("YESSS");
        }

        float GetPercent()
        {
            return value / (maxValue - minValue);
        }

        public void SetSliderColor()
        {
            if(doGradient)
            {
                Color c = Color.Lerp(minColor, maxColor, GetPercent());
                base.SetColor(c);
            }
        }

        public void SetValue(float v)
        {
            rawValue = Math.Clamp(v, minValue, maxValue);

            if (doInt)
                value = MathF.Round(rawValue);
            else
                value = rawValue;

            onValueChanged?.Invoke();
        }
    }
}

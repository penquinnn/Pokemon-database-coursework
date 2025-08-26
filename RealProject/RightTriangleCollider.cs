using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RealProject
{
    class RightTriangleCollider : Collider
    {
        public Vector2 colliderSize;
        public int rotation;

        bool leftCollison, rightCollison;
        bool topCollision, bottomCollision;

        public RightTriangleCollider(Vector2 offset, bool trig, Vector2 size, int rot) : base(offset, trig)
        {
            colliderSize = size;
            rotation = rot;
        }

        public override bool CheckPlayerCollision(Vector2 tileCentre, Vector2 playerPos, Vector2 playerColliderSize, Vector2 playerColliderOffset)
        {
            bool colliding = false;

            bool verticalsInbetween = false;
            Vector2 playerColliderCentre = playerPos + playerColliderOffset;

            float top = playerColliderCentre.Y - playerColliderSize.Y / 2;
            float left = playerColliderCentre.X - playerColliderSize.X / 2;
            float right = playerColliderCentre.X + playerColliderSize.X / 2;
            float bottom = playerColliderCentre.Y + playerColliderSize.Y / 2;

            Vector2 colliderCentre = tileCentre + colliderOffset;

            if (top > colliderCentre.Y - colliderSize.Y / 2 && top < colliderCentre.Y + colliderSize.Y / 2)
            {
                verticalsInbetween = true;
                topCollision = true;
            }
            else if (bottom > colliderCentre.Y - colliderSize.Y / 2 && bottom < colliderCentre.Y + colliderSize.Y / 2)
            {
                verticalsInbetween = true;
                bottomCollision = true;
            }
            else if (colliderCentre.Y - colliderSize.Y / 2 > top && colliderCentre.Y + colliderSize.Y / 2 < bottom)
            {
                verticalsInbetween = true;
            }

            if (verticalsInbetween)
            {
                float tileLeft = colliderCentre.X - colliderSize.X / 2;

                float x = playerColliderCentre.Y - colliderCentre.Y;
                float tileRight = colliderCentre.X + x;
                Debug.WriteLine(tileRight);

                if (left > tileLeft && left < tileRight)
                {
                    //Debug.WriteLine("Intersecting");
                    colliding = true;
                    leftCollison = true;
                }
                else if (right > tileLeft && right < tileRight)
                {
                    //Debug.WriteLine("Intersecting");
                    colliding = true;
                    rightCollison = true;
                }
                else if (tileLeft > left && tileRight < right)
                {
                    //Debug.WriteLine("Intersecting");
                    colliding = true;
                }
            }

            return colliding;
        }

        public override Vector2 ResolveCollision(Vector2 tileCentre, Vector2 playerPos, Vector2 playerColliderSize, Vector2 playerColliderOffset)
        {
            Vector2 resolvedPosition = playerPos;
            Vector2 colliderPos = tileCentre + colliderOffset;

            Vector2 playerColliderPos = playerPos + playerColliderOffset;
            Vector2 dir = playerColliderPos - colliderPos;

            float x = playerColliderPos.Y - colliderPos.Y;
            float y = x;
            x = colliderPos.X + x;
            y = colliderPos.Y + y;

            if (MathF.Abs(dir.X) > MathF.Abs(dir.Y))
            {
                if (dir.X < 0)
                {
                    resolvedPosition.X = colliderPos.X - (colliderSize.X / 2) - (playerColliderSize.X / 2) - playerColliderOffset.X;
                }
                else
                {
                    resolvedPosition = colliderPos + new Vector2(x, y);// + (playerColliderSize / 2) - playerColliderOffset;
                }
            }
            if (MathF.Abs(dir.X) < MathF.Abs(dir.Y))
            {
                if (dir.Y < 0)
                {
                    //resolvedPosition.Y = colliderPos.Y - playerColliderPos.X - colliderPos.X - playerColliderOffset.Y; ;
                }
                else
                {
                    resolvedPosition.Y = colliderPos.Y + (colliderSize.Y / 2) + (playerColliderSize.Y / 2) - playerColliderOffset.Y;
                }
            }

            return resolvedPosition;
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using SharpDX.Direct2D1;

namespace RealProject
{
    internal class ColliderManager
    {
        public char[,] collisionMap;
        public Dictionary<char, Collider> colliderDictionary = new Dictionary<char, Collider>();

        Texture2D mapPNG;
        Player player;
        SpriteFont font;

        public ColliderManager(Texture2D texture, Player player, SpriteFont font) 
        {
            mapPNG = texture;
            this.player = player;
            this.font = font;
        }

        public void Initialize()
        {
            InitializeColliders();
            GetTileMapFromImage(mapPNG);
        }
        void InitializeColliders()
        {
            colliderDictionary.Add('x', new BoxCollider(Vector2.Zero, false, Vector2.One));
            colliderDictionary.Add('b', new BoxCollider(new Vector2(0, 0.2f), false, new Vector2(1, 0.5f)));
            colliderDictionary.Add('T', new BoxCollider(new Vector2(0, -0.2f), false, new Vector2(1, 0.5f)));
            colliderDictionary.Add('l', new BoxCollider(new Vector2(-0.2f, 0), false, new Vector2(0.5f, 1)));
            colliderDictionary.Add('R', new BoxCollider(new Vector2(0.2f, 0), false, new Vector2(0.5f, 1)));
            //colliderDictionary.Add('L', new RightTriangleCollider(Vector2.Zero, false, Vector2.One, 1));
        }

        void GetTileMapFromImage(Texture2D mapImg)
        {
            int width = mapImg.Width;
            int height = mapImg.Height;

            collisionMap = new char[width, height];

            Color[] pixelData = new Color[width * height];
            mapImg.GetData(pixelData);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixelData[y * width + x];

                    char collisionChar = (char)pixel.B;
                    if (pixel.B == 255)
                        collisionChar = ' ';
                    collisionMap[y, x] = collisionChar;
                }
            }
        }

        public void OverrideColliderMap(Char c, int x, int y)
        {
            collisionMap[y, x] = c;
        }

        public void Update()
        {
            if (GameStateManager.gameState != GameStateManager.GameState.Overworld) return;

            CheckPlayerCollision();
        }

        void CheckPlayerCollision()
        {
            char[,] colMap = collisionMap;
            int x = (int)MathF.Round(player.playerPos.X);
            x = (int)Math.Clamp(x, 1, colMap.GetLength(0) - 2);
            int y = (int)MathF.Round(player.playerPos.Y);
            y = (int)Math.Clamp(y, 1, colMap.GetLength(1) - 2);

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (colMap[y + j, x + i] != ' ')
                    {
                        Collider collider = colliderDictionary[colMap[y + j, x + i]];
                        Vector2 tileCenter = new Vector2(x + i, y + j);

                        bool intersecting = collider.CheckPlayerCollision(tileCenter, player.playerPos, player.playerCollider.colliderSize, player.playerCollider.colliderOffset);

                        if (intersecting)
                        {
                            Debug.WriteLine(player.playerPos);
                            player.playerPos = collider.ResolveCollision(tileCenter, player.playerPos, player.playerCollider.colliderSize, player.playerCollider.colliderOffset);
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int x = (int)MathF.Round(player.playerPos.X);
            x = (int)Math.Clamp(x, 1, collisionMap.GetLength(0) - 2);
            int y = (int)MathF.Round(player.playerPos.Y);
            y = (int)Math.Clamp(y, 1, collisionMap.GetLength(1) - 2);
            spriteBatch.DrawString(font, $"{x}, {y}, {player.playerPos + player.playerCollider.colliderOffset}", Vector2.Zero, Color.Black, 0f, Vector2.Zero, 4, SpriteEffects.None, 0);
        }
    }
}

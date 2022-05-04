﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpNGun
{
    public class Collider : Component
    {
        private Texture2D texture;//texture to be drawn by renderer

        private SpriteRenderer spriteRenderer;//spriterender for drawing

        /// <summary>
        /// Property for rectangle colisionbox  for sprite
        /// </summary>
        public Rectangle CollisionBox
        {
            get
            {
                return new Rectangle
                    (
                        (int)(GameObject.Transform.Position.X - spriteRenderer.Sprite.Width / 2),
                        (int)(GameObject.Transform.Position.Y - spriteRenderer.Sprite.Height / 2),
                        spriteRenderer.Sprite.Width,
                        spriteRenderer.Sprite.Height
                    );
            }
        }

        public override void Start()
        {
            //get spriterenderer component of GameObject
            spriteRenderer = (SpriteRenderer)GameObject.GetComponent<SpriteRenderer>();

            //load pixeltexture for texture. Texture used to create visible colisionbox for debugging
            texture = GameWorld.Instance.Content.Load<Texture2D>("Pixel");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawRectangle(CollisionBox, spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            CheckCollision();
        }

        /// <summary>
        /// Will draw rectangle for visible colisionbox for debugging
        /// </summary>
        /// <param name="collisionBox"></param>
        /// <param name="spriteBatch"></param>
        private void DrawRectangle(Rectangle collisionBox, SpriteBatch spriteBatch)
        {
            
            Rectangle topLine = new Rectangle(collisionBox.X, collisionBox.Y, collisionBox.Width, 1);
            Rectangle bottomLine = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 1);
            Rectangle rightLine = new Rectangle(collisionBox.X + collisionBox.Width, collisionBox.Y, 1, collisionBox.Height);
            Rectangle leftLine = new Rectangle(collisionBox.X, collisionBox.Y, 1, collisionBox.Height);

            spriteBatch.Draw(texture, topLine, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(texture, bottomLine, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(texture, rightLine, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(texture, leftLine, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
        }


        /// <summary>
        /// Checks if any colliders (collisionbox) intersects with each other and trigger event if they do
        /// </summary>
        public void CheckCollision()
        {
            foreach (Collider other in GameWorld.Instance.Colliders)
            {
                if (other != this && other.CollisionBox.Intersects(CollisionBox))
                {
                    EventManager.TriggerEvent("OnCollision", new Dictionary<string, object>
                    {

                        {"CollidedWith", other.GameObject},
                        {"CollidedFrom", this.GameObject}

                    });
                }
            }
        }
    }
}

